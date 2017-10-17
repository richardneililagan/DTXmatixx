using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using CSCore;
using CSCore.DSP;

namespace FDK.メディア.サウンド.WASAPI
{
	/// <summary>
	///		指定されたメディアファイルを XA としてデコードして、CSCore.IWaveSource オブジェクトを生成する。
	///		リサンプラーあり版。
	/// </summary>
	class XaResampledWaveSource : IWaveSource
	{
		public bool CanSeek => true;    // オンメモリなので常にサポートできる。

		/// <summary>
		/// 	デコード＆リサンプル後のオーディオデータのフォーマット。
		/// </summary>
		public WaveFormat WaveFormat
		{
			get;
			protected set;
		} = null;

		/// <summary>
		///		デコード後のオーディオデータのすべての長さ[byte]。
		/// </summary>
		public long Length
			=> this._DecodedWaveData.Length;

		/// <summary>
		///		現在の再生位置[byte]。
		/// </summary>
		public long Position
		{
			get
				=> this._Position;
			set
				=> this._Position = FDKUtilities.位置をブロック境界単位にそろえて返す( value, this.WaveFormat.BlockAlign );
		}

		/// <summary>
		///		コンストラクタ。
		///		指定されたXAファイルを指定されたフォーマットでデコードし、内部にオンメモリで保管する。
		/// </summary>
		public XaResampledWaveSource( string ファイルパス, WaveFormat deviceFormat )
		{
			this.WaveFormat = new WaveFormat(
				deviceFormat.SampleRate,
				32,
				deviceFormat.Channels,
				AudioEncoding.IeeeFloat );

			// リサンプルなし版で生成して、それを this.WaveFormat に合わせてリサンプルしたデータ(byte[])を保管する。
			using( var xaSource = new XAWaveSource( ファイルパス, deviceFormat ) )
			using( var resampler = new DmoResampler( xaSource, this.WaveFormat ) )
			{
				// resampler.Length はサンプル単位ではなくフレーム単位。
				var サイズbyte = resampler.Length * resampler.WaveFormat.Channels; // 実際のサイズはチャンネル倍ある。

				this._DecodedWaveData = new byte[ サイズbyte ];
				resampler.Read( this._DecodedWaveData, 0, (int) サイズbyte );  // でもこっちはバイト単位。
			}
		}

		/// <summary>
		///		連続したデータを読み込み、<see cref="Position"/> を読み込んだ数だけ進める。
		/// </summary>
		/// <param name="buffer">読み込んだデータを格納するための配列。</param>
		/// <param name="offset"><paramref name="buffer"/> に格納を始める位置。</param>
		/// <param name="count">読み込む最大のデータ数。</param>
		/// <returns><paramref name="buffer"/> に読み込んだデータの総数。</returns>
		public int Read( byte[] buffer, int offset, int count )
		{
			// ※ 音がめちゃくちゃになるとうざいので、このメソッド内では例外を出さないこと。
			if( ( null == this._DecodedWaveData ) || ( null == buffer ) )
				return 0;

			long 読み込み可能な最大count = ( this.Length - this._Position );
			if( count > 読み込み可能な最大count )
				count = (int) 読み込み可能な最大count;

			if( 0 < count )
			{
				Buffer.BlockCopy(
					src: this._DecodedWaveData,
					srcOffset: (int) this._Position,
					dst: buffer,
					dstOffset: offset,
					count: count );

				this._Position += count;
			}

			return count;
		}

		/// <summary>
		///		解放する。
		/// </summary>
		public void Dispose()
		{
			this._DecodedWaveData = null;
		}

		private byte[] _DecodedWaveData = null;
		private long _Position = 0;
	}
}
