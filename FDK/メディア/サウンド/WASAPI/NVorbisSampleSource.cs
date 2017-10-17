using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using CSCore;
using NVorbis;

namespace FDK.メディア.サウンド.WASAPI
{
	/// <summary>
	///		指定されたメディアファイル（動画, 音楽）を Vorbis としてデコードして、CSCore.ISampleSource オブジェクトを生成する。
	///		リサンプラーなし版。
	///		参照：<seealso cref="https://cscore.codeplex.com/SourceControl/latest#Samples/NVorbisIntegration/Program.cs"/>
	/// </summary>
	public class NVorbisSampleSource : ISampleSource
	{
		private Stream _stream;
		private VorbisReader _vorbisReader;
		private WaveFormat _waveFormat;

		public bool CanSeek
			=> this._stream.CanSeek;

		public WaveFormat WaveFormat
			=> this._waveFormat;

		public long Position
		{
			get
				=> ( this.CanSeek ) ? this._vorbisReader.DecodedPosition : 0;
			set
				=> this._vorbisReader.DecodedPosition = ( this.CanSeek ) ? 
					value : throw new InvalidOperationException( "DecodedNVorbisSource is not seekable." );
		}

		public long Length
			=> ( this.CanSeek ) ? this._vorbisReader.TotalSamples * this.WaveFormat.Channels : 0;	// TotalSamples はフレーム数を返す。


		public NVorbisSampleSource( Stream stream, WaveFormat deviceFormat )
		{
			if( null == stream )
				throw new ArgumentException( "stream" );
			if( !( stream.CanRead ) )
				throw new ArgumentException( "Stream is not readable.", "stream" );

			this._stream = stream;
			this._vorbisReader = new VorbisReader( stream, false );

			this._waveFormat = new WaveFormat(
				this._vorbisReader.SampleRate,
				32,								// 32bit 固定
				this._vorbisReader.Channels, 
				AudioEncoding.IeeeFloat );      // IeeeFloat 固定
		}

		public int Read( float[] buffer, int offset, int count )
		{
			return this._vorbisReader.ReadSamples( buffer, offset, count );
		}

		public void Dispose()
		{
			this._vorbisReader?.Dispose();
			this._vorbisReader = null;
		}
	}
}
