using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using CSCore;
using FDK;

namespace FDK.メディア.サウンド.WASAPI
{
	public static class SampleSourceFactory
	{
		/// <summary>
		///		指定されたファイルから <see cref="ISampleSource"/> を生成して返す。
		///		失敗すれば例外発生。
		/// </summary>
		public static ISampleSource Create( SoundDevice device, string ファイルパス )
		{
			var path = Folder.絶対パスに含まれるフォルダ変数を展開して返す( ファイルパス );
			var ext = Path.GetExtension( path ).ToLower();

			#region " NVorbis を試みる "
			//----------------
			if( ".ogg" == ext )
			{
				try
				{
					using( var audioStream = new FileStream( path, FileMode.Open ) )
					{
						return new NVorbisResampledWaveSource( audioStream, device.WaveFormat )
							.ToSampleSource();
					}
				}
				catch
				{
					// ダメだったので次へ。
				}
			}
			//----------------
			#endregion

			#region " XA を試みる "
			//----------------
			if( ".xa" == ext )
			{
				try
				{
					return new XaResampledWaveSource( path, device.WaveFormat )
						.ToSampleSource();
				}
				catch
				{
					// ダメだったので次へ。
				}
			}
			//----------------
			#endregion

			#region " MediaFoundation を試みる "
			//----------------
			try
			{
				return new MediaFoundationWaveSource( path, device.WaveFormat )
					.ToSampleSource();
			}
			catch
			{
				// ダメだったので次へ。
			}
			//----------------
			#endregion

			throw new InvalidDataException( $"未対応のオーディオファイルです。[{ファイルパス}]" );
		}
	}
}
