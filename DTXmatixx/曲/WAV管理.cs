using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using CSCore;
using FDK;
using FDK.メディア.サウンド.WASAPI;
using SSTFormat.v3;
using DTXmatixx.ステージ;

namespace DTXmatixx.曲
{
	/// <summary>
	///		主にDTXファイルの #WAV サウンドを管理する。
	///		SSTファイルのドラムサウンドは、ここではなく <see cref="DTXmatixx.ステージ.ドラムサウンド"/> で管理する。
	/// </summary>
	class WAV管理 : IDisposable
	{
		/// <param name="多重度">１サウンドの最大多重発声数。1以上。</param>
		public WAV管理( int 多重度 = 4 )
		{
			if( 1 > 多重度 )
				throw new ArgumentException( $"多重度が1未満に設定されています。[{多重度}]" );

			this._既定の多重度 = 多重度;

			this.初期化する();
		}

		public void 初期化する()
		{
			this._WavContexts = new Dictionary<int, WavContext>();
		}

		public void Dispose()
		{
			foreach( var kvp in this._WavContexts )
			{
				var context = kvp.Value;

				// Sound[] を解放。
				if( null != context.Sounds )
				{
					foreach( var sd in context.Sounds )
						sd.Stop();
					context.Sounds = null;
				}

				// Source を解放。
				context.SampleSource?.Dispose();
			}
			this._WavContexts.Clear();
			this._WavContexts = null;
		}

		/// <summary>
		///		WAVファイルを登録する。
		/// </summary>
		/// <param name="wav番号">登録する番号。0～1295。すでに登録されている場合は上書き更新される。</param>
		/// <param name="サウンドファイル">登録するサウンドファイルのパス。</param>
		public void 登録する( SoundDevice device, int wav番号, VariablePath サウンドファイル, bool 多重再生する )
		{
			#region " パラメータチェック。"
			//----------------
			if( null == device )
				throw new ArgumentNullException();

			if( ( 0 > wav番号 ) || ( 1295 < wav番号 ) )
				throw new ArgumentOutOfRangeException( $"WAV番号が範囲を超えています。[{wav番号}]" );

			if( !( File.Exists( サウンドファイル.変数なしパス ) ) )
			{
				Log.WARNING( $"サウンドファイルが存在しません。[{サウンドファイル.変数付きパス}]" );
				return;
			}
			//----------------
			#endregion

			// 先に SampleSource を生成する。
			var sampleSource = (ISampleSource) null;
			try
			{
				sampleSource = SampleSourceFactory.Create( device, サウンドファイル.変数なしパス );
			}
			catch
			{
				Log.WARNING( $"サウンドのデコードに失敗しました。[{サウンドファイル.変数付きパス}" );
				return;
			}

			// すでに登録済みなら解放する。
			if( this._WavContexts.ContainsKey( wav番号 ) )
			{
				this._WavContexts[ wav番号 ].Dispose();
				this._WavContexts.Remove( wav番号 );
			}

			// 新しいContextを生成して登録する。
			var context = new WavContext( wav番号, ( 多重再生する ) ? this._既定の多重度 : 1 );

			context.SampleSource = sampleSource;

			for( int i = 0; i < context.Sounds.Length; i++ )
				context.Sounds[ i ] = new Sound( device, context.SampleSource );

			this._WavContexts.Add( wav番号, context );

			Log.Info( $"サウンドを読み込みました。[{サウンドファイル.変数付きパス}]" );
		}
		
		/// <summary>
		///		指定した番号のWAVを、指定したチップ種別として発声する。
		/// </summary>
		/// <param name="音量">0:無音～1:原音</param>
		public void 発声する( int WAV番号, チップ種別 chipType, float 音量 = 1f )
		{
			if( !( this._WavContexts.ContainsKey( WAV番号 ) ) )
				return;

			// 現在発声中のサウンドを全部止めるチップ種別の場合は止める。
			if( 0 != chipType.排他発声グループID() ) // グループID = 0 は対象外。
			{
				// 消音対象のコンテキストの Sounds[] を select する。
				var 停止するサウンドs =
					from kvp in this._WavContexts
					where ( chipType.直前のチップを消音する( kvp.Value.最後に発声したときのチップ種別 ) )
					select kvp.Value.Sounds;

				// 集めた Sounds[] をすべて停止する。
				foreach( var sounds in 停止するサウンドs )
				{
					foreach( var sound in sounds )
						sound.Stop();
				}
			}

			// 発声する。
			this._WavContexts[ WAV番号 ].発声する( chipType, 音量 );
		}

		public void すべての発声を停止する()
		{
			foreach( var kvp in this._WavContexts )
				kvp.Value.Dispose();
		}

		/// <summary>
		///		１つの WAV に相当する管理情報。
		/// </summary>
		private class WavContext : IDisposable
		{
			/// <summary>
			///		0～1295。
			/// </summary>
			public int WAV番号;

			/// <summary>
			///		この WAV に対応するサンプルソース（デコード済みサウンドデータ）。
			/// </summary>
			public ISampleSource SampleSource;

			/// <summary>
			///		この WAV に対応するサウンド。
			///		サウンドデータとして <see cref="SampleSource"/> を使用し、多重度の数だけ存在することができる。
			/// </summary>
			public Sound[] Sounds;

			public チップ種別 最後に発声したときのチップ種別
			{
				get;
				protected set;
			} = チップ種別.Unknown;

			public WavContext( int wav番号, int 多重度 )
			{
				if( ( 0 > wav番号 ) || ( 1295 < wav番号 ) )
					throw new ArgumentOutOfRangeException( "WAV番号が不正です。" );

				this.WAV番号 = wav番号;
				this.SampleSource = null;
				this.Sounds = new Sound[ 多重度 ];
			}
			public void Dispose()
			{
				foreach( var sd in this.Sounds )
					sd.Dispose();
				this.Sounds = null;

				this.SampleSource?.Dispose();
				this.SampleSource = null;
			}
			/// <summary>
			///		指定したチップ種別扱いでWAVを発声する。
			/// </summary>
			/// <param name="音量">0:無音～1:原音</param>
			public void 発声する( チップ種別 chipType, float 音量 )
			{
				this.最後に発声したときのチップ種別 = chipType;

				// 発声。
				音量 = ( 0f > 音量 ) ? 0f : ( 1f < 音量 ) ? 1f : 音量;
				this.Sounds[ this.次に再生するSound番号 ].Volume = 音量;
				this.Sounds[ this.次に再生するSound番号 ].Play( 0 );

				// サウンドローテーション。
				this.次に再生するSound番号 = ( this.次に再生するSound番号 + 1 ) % this.Sounds.Length;
			}

			private int 次に再生するSound番号 = 0;
		}
		/// <summary>
		///		全WAVの管理DB。KeyはWAV番号。
		/// </summary>
		private Dictionary<int, WavContext> _WavContexts = null;

		private readonly int _既定の多重度;
	}
}
