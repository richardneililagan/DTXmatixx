using System;
using System.Collections.Generic;
using SharpDX;
using SharpDX.DirectInput;
using FDK.カウンタ;

namespace FDK.入力
{
	public class Keyboard : IInputDevice, IDisposable
	{
		public InputDeviceType 入力デバイス種別
			=> InputDeviceType.Keyboard;

		/// <summary>
		///		ポーリング時に、前回のポーリング以降の状態と比べて生成された入力イベントのリスト。
		/// </summary>
		public List<InputEvent> 入力イベントリスト
		{
			get;
			protected set;
		}


		public Keyboard( IntPtr hWindow )
		{
			using( Log.Block( FDKUtilities.現在のメソッド名 ) )
			{
				this._Window = hWindow;
				this.入力イベントリスト = new List<InputEvent>();

				// キーの押下状態配列を初期化する。
				for( int i = 0; i < 256; i++ )
					this._現在のキーの押下状態[ i ] = false;

				// DirectInput を生成する。
				var di = new DirectInput();

				// キーボードが接続されていないなら、this.Device = null のままとする。
				if( 0 == di.GetDevices( DeviceType.Keyboard, DeviceEnumerationFlags.AttachedOnly ).Count )
				{
					this._Device = null; // これは、エラーではない。
					return;
				}

				// デバイスを生成する。
				this._Device = new SharpDX.DirectInput.Keyboard( di );

				// デバイスの協調モードを設定する。
				this._Device.SetCooperativeLevel(
					this._Window,
					CooperativeLevel.NoWinKey |
					CooperativeLevel.Foreground |
					CooperativeLevel.NonExclusive );

				// デバイスの入力バッファサイズを設定する。
				this._Device.Properties.BufferSize = Keyboard._デバイスの入力バッファサイズ;
			}
		}

		public void Dispose()
		{
			using( Log.Block( FDKUtilities.現在のメソッド名 ) )
			{
				FDKUtilities.解放する( ref this._Device );
			}
		}

		public void ポーリングする()
		{
			if( null == this._Device )
				return; // 準正常。

			this.入力イベントリスト.Clear(); // Acquire 前にクリアしておく（Acquire の失敗時にリストが空であるように）。

			// Acquire する。失敗（非アクティブ、ウィンドウ終了時など）したら、何もしない。 
			try
			{
				this._Device.Acquire();
			}
			catch
			{
				//Log.WARNING( "キーボードデバイスの Acquire に失敗しました。" );
				return;
			}

			try
			{
				// ポーリングを行う。
				this._Device.Poll();

				// ポーリング結果から状態配列を更新する。
				var timeStamp = QPCTimer.生カウント;    // なるべく早くに取得しておく。
				foreach( var k in this._Device.GetBufferedData() )
				{
					if( k.IsPressed )
					{
						#region " (a) 押された "
						//-----------------
						this.入力イベントリスト.Add(
							new InputEvent() {
								DeviceID = 0,			// 固定
								Key = (int) k.Key,
								Velocity = 255,
								TimeStamp = timeStamp,	// DirectInput の TimeStamp は使わない。
								押された = true,
								離された = false,
							} );

						this._現在のキーの押下状態[ (int) k.Key ] = true;
						//-----------------
						#endregion
					}
					else if( k.IsReleased )
					{
						#region " (b) 離された "
						//-----------------
						this.入力イベントリスト.Add(
							new InputEvent() {
								DeviceID = 0,           // 固定
								Key = (int) k.Key,
								Velocity = 255,
								TimeStamp = timeStamp,	// DirectInput の TimeStamp は使わない。
								押された = false,
								離された = true,
							} );

						this._現在のキーの押下状態[ (int) k.Key ] = true;
						//-----------------
						#endregion
					}
				}
			}
			catch( SharpDXException )
			{
				// たまに DIERR_INPUTLOST が発生するが、再度 Acquire すればいいだけなので無視する。
			}
		}

		public bool キーが押された( int deviceID, int key )
		{
			return this.キーが押された( deviceID, key, out _ );
		}

		public bool キーが押された( int deviceID, int key, out InputEvent ev )
		{
			ev = null;

			if( null == this._Device )
				return false;   // 準正常。

			ev = this.入力イベントリスト.Find( ( item ) => ( item.Key == key && item.押された ) );
			return ( null != ev ) ? true : false;
		}

		public bool キーが押された( int deviceID, Key key )
		{
			return this.キーが押された( deviceID, (int) key, out _ );
		}

		public bool キーが押された( int deviceID, Key key, out InputEvent ev )
		{
			return this.キーが押された( deviceID, (int) key, out ev );
		}

		public bool キーが押されている( int deviceID, int key )
		{
			if( null == this._Device )   // 準正常。
				return false;

			return this._現在のキーの押下状態[ key ];
		}

		public bool キーが押されている( int deviceID, Key key )
		{
			return this.キーが押されている( deviceID, (int) key );
		}

		public bool キーが離された( int deviceID, int key )
		{
			return this.キーが離された( deviceID, key, out _ );
		}

		public bool キーが離された( int deviceID, int key, out InputEvent ev )
		{
			ev = null;

			if( null == this._Device )
				return false;   // 準正常。

			ev = this.入力イベントリスト.Find( ( item ) => ( item.Key == key && item.離された ) );
			return ( null != ev ) ? true : false;
		}

		public bool キーが離された( int deviceID, Key key )
		{
			return this.キーが離された( deviceID, (int) key, out _ );
		}

		public bool キーが離された( int deviceID, Key key, out InputEvent ev )
		{
			return this.キーが離された( deviceID, (int) key, out ev );
		}

		public bool キーが離されている( int deviceID, int key )
		{
			if( null == this._Device )   // 準正常。
				return false;

			return !( this._現在のキーの押下状態[ key ] );
		}

		public bool キーが離されている( int deviceID, Key key )
		{
			return this.キーが離されている( deviceID, (int) key );
		}


		private const int _デバイスの入力バッファサイズ = 32;

		private SharpDX.DirectInput.Keyboard _Device = null; // キーボードがアタッチされていない場合は null 。

		private IntPtr _Window = IntPtr.Zero;
		
		/// <summary>
		///		ポーリングごとに累積更新された最終の結果。
		/// </summary>
		private readonly bool[] _現在のキーの押下状態 = new bool[ 256 ];
	}
}
