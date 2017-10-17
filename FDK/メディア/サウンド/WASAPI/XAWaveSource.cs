using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using CSCore;
using CSCore.DSP;

namespace FDK.メディア.サウンド.WASAPI
{
	/// <summary>
	///		指定されたメディアファイルを XA としてデコードして、CSCore.IWaveSource オブジェクトを生成する。
	///		リサンプラーなし版。
	/// </summary>
	unsafe class XAWaveSource : IWaveSource
	{
		public bool CanSeek => true; // オンメモリなので常にサポートする。

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
		public XAWaveSource( string ファイルパス, WaveFormat deviceFormat )
		{
			var path = Folder.絶対パスに含まれるフォルダ変数を展開して返す( ファイルパス );

			var xaheader = new XAHEADER();
			var srcBuf = (byte[]) null;

			#region " XAHEADER と XAデータ を読み込みむ。"
			//----------------
			using( var br = new BinaryReader( new FileStream( path, FileMode.Open ) ) )
			{
				xaheader.id = br.ReadUInt32();
				xaheader.nDataLen = br.ReadUInt32();
				xaheader.nSamples = br.ReadUInt32();
				xaheader.nSamplesPerSec = br.ReadUInt16();
				xaheader.nBits = br.ReadByte();
				xaheader.nChannels = br.ReadByte();
				xaheader.nLoopPtr = br.ReadUInt32();
				xaheader.befL = new short[ 2 ];
				xaheader.befL[ 0 ] = br.ReadInt16();
				xaheader.befL[ 1 ] = br.ReadInt16();
				xaheader.befR = new short[ 2 ];
				xaheader.befR[ 0 ] = br.ReadInt16();
				xaheader.befR[ 1 ] = br.ReadInt16();
				xaheader.pad = new byte[ 4 ];
				xaheader.pad = br.ReadBytes( 4 );

				srcBuf = br.ReadBytes( (int) xaheader.nDataLen );
			}
			//----------------
			#endregion

			var waveformatex = new WAVEFORMATEX();
			var handlePtr = IntPtr.Zero;

			#region " XAファイルをオープンし、Waveフォーマットとハンドルを取得。"
			//----------------
			handlePtr = xaDecodeOpen( ref xaheader, out waveformatex );

			if( null == handlePtr || IntPtr.Zero == handlePtr )
				throw new Exception( $"xaDecodeOpen に失敗しました。[{ファイルパス}]" );
			//----------------
			#endregion

			#region " Waveフォーマットを WaveFormat プロパティに設定。"
			//----------------
			if( 0 == waveformatex.cbSize )
			{
				this.WaveFormat = new WaveFormat(
					(int) waveformatex.nSamplesPerSec,
					(int) waveformatex.wBitsPerSample,
					(int) waveformatex.nChannels,
					(AudioEncoding) waveformatex.wFormatTag );
			}
			else
			{
				var msg = $"デコード後のフォーマットが WAVEFORMATEX 型になる XA には未対応です。[{ファイルパス}]";
				Log.ERROR( msg );
				throw new Exception( msg );
			}
			//----------------
			#endregion

			#region " デコード後のPCMサイズ[byte]を取得し、バッファを確保する。"
			//----------------
			if( !( xaDecodeSize( handlePtr, xaheader.nDataLen, out uint decodedWaveDataLength ) ) )
			{
				var msg = $"xaDecodeSize に失敗しました。[{ファイルパス}]";
				Log.ERROR( msg );
				throw new Exception( msg );
			}
			this._DecodedWaveData = new byte[ decodedWaveDataLength ];
			//----------------
			#endregion

			#region " デコードする。"
			//----------------
			unsafe
			{
				fixed ( byte* pXaBuf = srcBuf )
				fixed ( byte* pPcmBuf = this._DecodedWaveData )
				{
					var xastreamheader = new XASTREAMHEADER() {
						pSrc = pXaBuf,
						nSrcLen = xaheader.nDataLen,
						nSrcUsed = 0,
						pDst = pPcmBuf,
						nDstLen = decodedWaveDataLength,
						nDstUsed = 0,
					};
					if( !( xaDecodeConvert( handlePtr, ref xastreamheader ) ) )
					{
						var msg = $"xaDecodeConvert に失敗しました。[{ファイルパス}]";
						Log.ERROR( msg );
						throw new Exception( msg );
					}
				}
			}
			//----------------
			#endregion

			#region " XAファイルを閉じる。"
			//----------------
			if( !( xaDecodeClose( handlePtr ) ) )
			{
				var msg = $"xaDecodeClose に失敗しました。[{ファイルパス}]";
				Log.ERROR( msg );
				throw new Exception( msg );
			}
			//----------------
			#endregion
		}

		/// <summary>
		///		解放する。
		/// </summary>
		public void Dispose()
		{
			this._DecodedWaveData = null;
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

		private byte[] _DecodedWaveData = null;
		private long _Position = 0;

		#region " Win32(xsdec.dll) "
		//----------------
		[StructLayout( LayoutKind.Sequential )]
		public struct WAVEFORMATEX
		{
			public ushort wFormatTag;
			public ushort nChannels;
			public uint nSamplesPerSec;
			public uint nAvgBytesPerSec;
			public ushort nBlockAlign;
			public ushort wBitsPerSample;
			public ushort cbSize;
		}

		[StructLayout( LayoutKind.Sequential )]
		public struct XASTREAMHEADER
		{
			public byte* pSrc;
			public uint nSrcLen;
			public uint nSrcUsed;
			public byte* pDst;
			public uint nDstLen;
			public uint nDstUsed;
		}

		[StructLayout( LayoutKind.Sequential )]
		public struct XAHEADER
		{
			public uint id;
			public uint nDataLen;
			public uint nSamples;
			public ushort nSamplesPerSec;
			public byte nBits;
			public byte nChannels;
			public uint nLoopPtr;
			[MarshalAs( UnmanagedType.ByValArray, SizeConst = 2 )]
			public short[] befL;
			[MarshalAs( UnmanagedType.ByValArray, SizeConst = 2 )]
			public short[] befR;
			[MarshalAs( UnmanagedType.ByValArray, SizeConst = 4 )]
			public byte[] pad;
		}

		[DllImport( "xadec.dll", EntryPoint = "xaDecodeOpen", CallingConvention = CallingConvention.Cdecl )]
		public extern static IntPtr xaDecodeOpen( ref XAHEADER pxah, out WAVEFORMATEX pwfx );

		[DllImport( "xadec.dll", EntryPoint = "xaDecodeClose", CallingConvention = CallingConvention.Cdecl )]
		public extern static bool xaDecodeClose( IntPtr hxas );

		[DllImport( "xadec.dll", EntryPoint = "xaDecodeSize", CallingConvention = CallingConvention.Cdecl )]
		public extern static bool xaDecodeSize( IntPtr hxas, uint slen, out uint pdlen );

		[DllImport( "xadec.dll", EntryPoint = "xaDecodeConvert", CallingConvention = CallingConvention.Cdecl )]
		public extern static bool xaDecodeConvert( IntPtr hxas, ref XASTREAMHEADER psh );
		//----------------
		#endregion
	}
}
