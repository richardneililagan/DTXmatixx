using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FDK.メディア.サウンド.WASAPI
{
	public class SoundTimer : IDisposable
	{
		/// <summary>
		///		コンストラクタまたはリセットの時点からの相対経過時間[sec]。
		/// </summary>
		public double 現在時刻sec
		{
			get
			{
				lock( this._スレッド間同期 )
				{
					if( 0 < this._停止回数 )
					{
						// 一時停止中。
						return ( this._停止位置sec - this._開始位置sec );
					}
					else if( this._DeviceRef.TryGetTarget( out SoundDevice device ) )
					{
						// 稼働中。
						return ( device.GetDevicePosition() - this._開始位置sec );
					}
					else
						throw new InvalidOperationException( "サウンドデバイスが無効です。" );
				}
			}
		}

		public SoundTimer( SoundDevice device )
		{
			this._DeviceRef = new WeakReference<SoundDevice>( device );
			this.リセットする();
		}
		public void Dispose()
		{
			lock( this._スレッド間同期 )
			{
				this._DeviceRef = null;
			}
		}
		public void リセットする( double 新しい現在時刻sec = 0.0 )
		{
			lock( this._スレッド間同期 )
			{
				if( this._DeviceRef.TryGetTarget( out SoundDevice device ) )
				{
					this._開始位置sec = device.GetDevicePosition() - 新しい現在時刻sec;
					this._停止回数 = 0;
					this._停止位置sec = 0;
				}
			}
		}
		public void 一時停止する()
		{
			lock( this._スレッド間同期 )
			{
				if( this._DeviceRef.TryGetTarget( out SoundDevice device ) )
				{
					if( 0 == this._停止回数 )
						this._停止位置sec = device.GetDevicePosition();

					this._停止回数++;
				}
			}
		}
		public void 再開する()
		{
			lock( this._スレッド間同期 )
			{
				this._停止回数--;

				if( 0 == this._停止回数 )
					this.リセットする( this._停止位置sec - this._開始位置sec );
			}
		}

		private WeakReference<SoundDevice> _DeviceRef = null;
		private int _停止回数 = 0;
		private double _開始位置sec = 0.0;
		private double _停止位置sec = 0.0;
		private readonly object _スレッド間同期 = new object();
	}
}
