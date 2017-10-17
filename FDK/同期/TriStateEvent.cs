using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace FDK.同期
{
	/// <summary>
	///		ON, OFF, 無効 の３状態を持つイベント。
	/// </summary>
	/// <remarks>
	///		状態が 無効 にされると、ON待ち／OFF待ちスレッドのブロックはいずれも解除され、
	///		またそれ以降、状態を変更することはできなくなる（リセットすると戻る）。
	/// </remarks>
	public class TriStateEvent
	{
		public enum 状態種別 { ON, OFF, 無効 }

		public 状態種別 現在の状態
		{
			get
			{
				lock( this._スレッド間同期 )
				{
					return this._状態;
				}
			}
			set
			{
				lock( this._スレッド間同期 )
				{
					if( this._状態 == 状態種別.無効 )
						return;     // 一度無効になったら、以降は変更不可。

					this._状態 = value;

					switch( value )
					{
						case 状態種別.ON:
							this._無効イベント?.Reset();
							this._OFFイベント?.Reset();
							this._ONイベント?.Set();
							break;

						case 状態種別.OFF:
							this._無効イベント?.Reset();
							this._ONイベント?.Reset();
							this._OFFイベント?.Set();
							break;

						case 状態種別.無効:
							this._ONイベント?.Set();
							this._OFFイベント?.Set();
							this._無効イベント?.Set();
							break;
					}
				}
			}
		}


		public TriStateEvent( 状態種別 初期状態 = 状態種別.OFF )
		{
			this.リセットする( 初期状態 );
		}

		/// <returns>
		///		解除後の状態（ON または 無効）。
		///	</returns>
		public 状態種別 ONになるまでブロックする()
		{
			int h = EventWaitHandle.WaitAny( new EventWaitHandle[] { this._ONイベント, this._無効イベント } );
			return ( h == 0 ) ? 状態種別.ON : 状態種別.無効;
		}
		
		/// <returns>
		///		解除後の状態（OFF または 無効）。
		///	</returns>
		public 状態種別 OFFになるまでブロックする()
		{
			int h = EventWaitHandle.WaitAny( new EventWaitHandle[] { this._OFFイベント, this._無効イベント } );
			return ( h == 0 ) ? 状態種別.OFF : 状態種別.無効;
		}

		public void 無効になるまでブロックする()
		{
			this._無効イベント.WaitOne();
		}

		/// <summary>
		///		状態をリセットする。
		///		すでに無効になった後でも可。
		/// </summary>
		public void リセットする( 状態種別 初期状態 = 状態種別.OFF )
		{
			lock( this._スレッド間同期 )
			{
				this.現在の状態 = 初期状態;

				this._無効イベント = new ManualResetEvent( false );
				this._ONイベント = new ManualResetEvent( 初期状態 == 状態種別.ON );
				this._OFFイベント = new ManualResetEvent( 初期状態 == 状態種別.OFF );
			}
		}


		private 状態種別 _状態 = 状態種別.OFF;

		private ManualResetEvent _無効イベント = null;

		private ManualResetEvent _ONイベント = null;

		private ManualResetEvent _OFFイベント = null;

		private readonly object _スレッド間同期 = new object();
	}
}
