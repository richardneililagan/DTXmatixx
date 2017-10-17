using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using CSCore;

namespace FDK.メディア.サウンド.WASAPI
{
	public class Sound : ISampleSource
	{
		public bool 再生中である
		{
			get
				=> this._DeviceRef.TryGetTarget( out SoundDevice device ) && device.Mixer.Contains( this );
		}
		public bool 再生中ではない
		{
			get
				=> !( this.再生中である );
		}

		/// <summary>
		///		シークが可能なら true。
		/// </summary>
		public bool CanSeek
			=> this._BaseSampleSource.CanSeek;

		/// <summary>
		///		このサウンドのフォーマット。
		/// </summary>
		public WaveFormat WaveFormat
			=> this._BaseSampleSource.WaveFormat;
		
		/// <remarks>
		///		１つの <see cref="SampleSource"/>を複数の<see cref="Sound"/>インスタンスで共有できるように、
		///		このプロパティは<see cref="Sound"/>インスタンスごとに独立して管理する。
		/// </remarks>
		public long Position
		{
			get
				=> this._Position;
			set
				=> this._Position = Math.Min( Math.Max( value, 0 ), this.Length );
		}

		public long Length
			=> this._BaseSampleSource.Length;

		/// <summary>
		///		音量。0.0(無音)～1.0(原音)～...上限なし
		/// </summary>
		/// <remarks>
		///		このクラスではなく、<see cref="Mixer"/>クラスから参照して使用する。
		/// </remarks>
		public float Volume
		{
			get
				=> this._Volume;
			set
				=> this._Volume = Math.Max( value, 0 );
		}

		protected Sound( SoundDevice device )
		{
			Debug.Assert( null != device );
			this._DeviceRef = new WeakReference<SoundDevice>( device );
		}
		public Sound( SoundDevice device, ISampleSource sampleSource )
			: this( device )
		{
			this._BaseSampleSource = sampleSource;
		}
		public void Dispose()
		{
			this.Stop();

			//this._SampleSource?.Dispose();	Dispose は外部で。（SampleSource は複数の Sound で共有されている可能性があるため。）
			this._BaseSampleSource = null;

			this._DeviceRef = null;
		}
		public void Play( long 再生開始位置frame = 0 )
		{
			if( this._DeviceRef.TryGetTarget( out SoundDevice device ) )
			{
				// BaseSampleSource の位置を、再生開始位置へ移動。
				if( this._BaseSampleSource.CanSeek )
				{
					this._Position = 再生開始位置frame * this.WaveFormat.Channels;
					//this._BaseSampleSource.Position = this._Position;		--> ここではまだ設定しない。Read() で設定する。
				}
				else
				{
					Log.ERROR( $"このサンプルソースの再生位置を変更することができません。既定の位置から再生を開始します。" );
				}

				// ミキサーに追加（＝再生開始）。
				device.Mixer.AddSound( this );
			}
		}
		public void Play( double 再生開始位置sec )
			=> this.Play( this._秒ToFrame( 再生開始位置sec ) );
		public int Read( float[] buffer, int offset, int count )
		{
			if( this._BaseSampleSource.Length == this._Position )
				return 0;	// 同じ場合でも Read() が 2 とか返してきて永遠に終わらないことがあるので、ここで阻止する。

			// １つの BaseSampleSource を複数の Sound で共有するために、Position は Sound ごとに管理している。
			this._BaseSampleSource.Position = this._Position;
			var readCount = this._BaseSampleSource.Read( buffer, offset, count );	// 読み込み。
			this._Position = this._BaseSampleSource.Position;

			return readCount;
		}
		public void Stop()
		{
			if( (null != this._DeviceRef ) && this._DeviceRef.TryGetTarget( out SoundDevice device ) )
			{
				device.Mixer?.RemoveSound( this );
			}
		}

		private WeakReference<SoundDevice> _DeviceRef = null;
		private ISampleSource _BaseSampleSource = null;
		private long _Position = 0;
		private float _Volume = 1.0f;

		private long _秒ToFrame( double 時間sec )
		{
			var wf = this._BaseSampleSource.WaveFormat;
			return (long) ( 時間sec * wf.SampleRate + 0.5 ); // +0.5 で四捨五入ができる
		}
		private double _FrameTo秒( long 時間frame )
		{
			var wf = this._BaseSampleSource.WaveFormat;
			return (double) 時間frame / wf.SampleRate;
		}
	}
}
