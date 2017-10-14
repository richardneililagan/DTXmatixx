using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using CSCore;
using FDK;
using FDK.メディア;
using FDK.メディア.サウンド.WASAPI;

namespace DTXmatixx.曲
{
	class WAV管理 : IDisposable
	{
		/// <param name="多重度">１サウンドの最大多重発声数。1以上。</param>
		public WAV管理( int 多重度 = 4 )
		{
			if( 1 >= 多重度 )
				throw new ArgumentException( $"多重度が1未満に設定されています。[{多重度}]" );

			this._多重度 = 多重度;

			this._サウンドリスト = new Dictionary<int, (IWaveSource source, Sound[] sounds)>();
		}
		public void Dispose()
		{
			if( null == this._サウンドリスト )
				return;

			foreach( var kvp in this._サウンドリスト )
			{
				for( int i = 0; i < kvp.Value.sounds.Length; i++ )
					kvp.Value.sounds[ i ]?.Dispose();
				kvp.Value.source?.Dispose();
			}
			this._サウンドリスト = null;
		}

		/// <summary>
		///		WAVファイルを登録する。
		/// </summary>
		/// <param name="WAV番号">登録する番号。0～1295。すでに登録されている場合は上書き更新される。</param>
		/// <param name="サウンドファイル">登録するサウンドファイルのパス。</param>
		public void 追加する( Device サウンドデバイス, int WAV番号, string サウンドファイル )
		{
			// パラメータチェック。
			if( null == サウンドデバイス )
				throw new ArgumentNullException();

			if( ( 0 > WAV番号 ) || ( 1295 < WAV番号 ) )
				throw new ArgumentOutOfRangeException( $"WAV番号が範囲を超えています。[{WAV番号}]" );

			var path = Folder.絶対パスに含まれるフォルダ変数を展開して返す( サウンドファイル );
			if( !( File.Exists( path ) ) )
			{
				Log.WARNING( $"サウンドファイルが存在しません。[{サウンドファイル}]" );
				return;
			}

			// すでに登録済みなら解放する。
			if( this._サウンドリスト.ContainsKey( WAV番号 ) )
			{
				for( int i = 0; i < this._サウンドリスト[ WAV番号 ].sounds.Length; i++ )
					this._サウンドリスト[ WAV番号 ].sounds[ i ].Dispose();
				this._サウンドリスト[ WAV番号 ].source.Dispose();
				this._サウンドリスト.Remove( WAV番号 );
			}

			// サウンドを生成して登録する。
			this._サウンドリスト.Add( WAV番号, (new DecodedWaveSource( path, サウンドデバイス.WaveFormat ), new Sound[ this._多重度 ]) );
			for( int i = 0; i < this._多重度; i++ )
				this._サウンドリスト[ WAV番号 ].sounds[ i ] = サウンドデバイス.サウンドを生成する( this._サウンドリスト[ WAV番号 ].source );

			Log.Info( $"サウンドを読み込みました。[{サウンドファイル}]" );
		}

		public void 再生する( int WAV番号 )
		{
			if( !( this._サウンドリスト.ContainsKey( WAV番号 ) ) )
				return;

			// todo: 多重再生未対応
			this._サウンドリスト[ WAV番号 ].sounds[ 0 ].Play();
		}

		private int _多重度 = 0;
		private Dictionary<int, (IWaveSource source, Sound[] sounds)> _サウンドリスト = null;
	}
}
