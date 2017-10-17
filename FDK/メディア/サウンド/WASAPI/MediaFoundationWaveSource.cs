using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using CSCore;
using CSCore.MediaFoundation;
using SharpDX.MediaFoundation;

namespace FDK.メディア.サウンド.WASAPI
{
	/// <summary>
	///		指定されたメディアファイル（動画, 音楽）をデコードして、<see cref="CSCore.IWaveSource"/> オブジェクトを生成する。
	/// </summary>
	public class MediaFoundationWaveSource : IWaveSource
	{
		/// <summary>
		///		シーク能力があるなら true 。
		/// </summary>
		public bool CanSeek => true;    // オンメモリなので常にサポートできる。

		/// <summary>
		///		デコード後のオーディオデータのフォーマット。
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
		///		指定されたファイルを指定されたフォーマットでデコードし、内部にオンメモリで保管する。
		/// </summary>
		public MediaFoundationWaveSource( string ファイルパス, WaveFormat deviceFormat )
		{
			var path = Folder.絶対パスに含まれるフォルダ変数を展開して返す( ファイルパス );

			this.WaveFormat = new WaveFormat(
				deviceFormat.SampleRate,		// 指定されたレート
				32,							// 32bit 固定
				deviceFormat.Channels,		// 指定されたチャンネル数
				AudioEncoding.IeeeFloat );  // IeeeFloat 固定

			//using( var sourceReader = new MFSourceReader( path ) )  // SourceReader は、SharpDX ではなく CSCore のものを使う。（WaveFormat から MediaType に一発で変換できるので。）
			//	→ CSCore.Win32.Comobject のファイナライザに不具合があるので、SourceReader には CSCore ではなく SharpDX のものを使う。
			//	  _MediaType, WaveFormat フィールドは CSCore のものなので注意。
			using( var sourceReader1 = new SourceReader( path ) )
			using( var sourceReader = sourceReader1.QueryInterfaceOrNull<SourceReaderEx>() )
			using( var waveStream = new MemoryStream() )
			{
				// (1) 最初のオーディオストリームを選択し、その他のすべてのストリームを非選択にする。
				sourceReader.SetStreamSelection( SourceReaderIndex.AllStreams, false );
				sourceReader.SetStreamSelection( SourceReaderIndex.FirstAudioStream, true );

				// (2) デコード後フォーマットを持つメディアタイプを作成し、SourceReader に登録する。

				// CSCore の場合。WaveFormatEx にも対応。
				//using( var partialMediaType = MFMediaType.FromWaveFormat( this.WaveFormat ) )

				// SharpDX の場合。
				var wf = SharpDX.Multimedia.WaveFormat.CreateIeeeFloatWaveFormat( deviceFormat.SampleRate, deviceFormat.Channels );
				MediaFactory.CreateAudioMediaType( ref wf, out AudioMediaType partialMediaType );

				using( partialMediaType )
				{
					// 作成したメディアタイプを sourceReader にセットする。必要なデコーダが見つからなかったら、ここで例外が発生する。
					sourceReader.SetCurrentMediaType( SourceReaderIndex.FirstAudioStream, partialMediaType );

					// 完成されたメディアタイプを取得する。
					this._MediaType = new MFMediaType( sourceReader.GetCurrentMediaType( SourceReaderIndex.FirstAudioStream ).NativePointer ); // ネイティブポインタを使って、SharpDX → CSCore へ変換。SharpDX側オブジェクトは解放したらダメ。

					// メディアタイプからフォーマットを取得する。（同じであるはずだが念のため）
					this.WaveFormat = this._MediaType.ToWaveFormat( MFWaveFormatExConvertFlags.Normal );

					// 最初のオーディオストリームが選択されていることを保証する。
					sourceReader.SetStreamSelection( (int) SourceReaderIndex.FirstAudioStream, true );
				}

				// (3) sourceReader からサンプルを取得してデコードし、waveStream へ書き込む。
				while( true )
				{
					// 次のサンプルを読み込む。
					using( var sample = sourceReader.ReadSample(
						(int) SourceReaderIndex.FirstAudioStream,
						(int) CSCore.MediaFoundation.SourceReaderControlFlags.None,
						out int actualStreamIndexRef,
						out var dwStreamFlagsRef,
						out Int64 llTimestampRef ) )
					{
						if( null == sample )
							break;      // EndOfStream やエラーのときも null になる。

						// sample をロックし、オーディオデータへのポインタを取得する。
						using( var mediaBuffer = sample.ConvertToContiguousBuffer() )
						{
							// オーディオデータをメモリストリームに書き込む。
							var audioData = mediaBuffer.Lock( out int cbMaxLengthRef, out int cbCurrentLengthRef );
							try
							{
								byte[] dstData = new byte[ cbCurrentLengthRef ];
								Marshal.Copy( audioData, dstData, 0, cbCurrentLengthRef );
								waveStream.Write( dstData, 0, cbCurrentLengthRef );
							}
							finally
							{
								mediaBuffer.Unlock();
							}
						}
					}
				}

				// (4) ストリームの内容を byte 配列に出力する。
				this._DecodedWaveData = waveStream.ToArray();
			}
		}

		/// <summary>
		///		解放する。
		/// </summary>
		public void Dispose()
		{
			this._DecodedWaveData = null;
			FDKUtilities.解放する( ref this._MediaType );
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

		private MFMediaType _MediaType = null;
		private byte[] _DecodedWaveData = null;
		private long _Position = 0;
	}
}
