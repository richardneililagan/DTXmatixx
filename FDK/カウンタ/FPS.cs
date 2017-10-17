using System;
using System.Collections.Generic;
using System.Diagnostics;
using FDK.メディア;

namespace FDK.カウンタ
{
	/// <summary>
	///		FPS（１秒間の進行処理回数）と VPS（１秒間の描画処理回数）を計測する。
	/// </summary>
	/// <remarks>
	///		計測するだけで、表示はしない。
	///		FPSをカウントする() を呼び出さないと、VPS も更新されないので注意。
	/// </remarks>
	public class FPS : Activity
	{
		public int 現在のFPS
		{
			get
			{
				lock( this._スレッド間同期 )
				{
					return this._現在のFPS;
				}
			}
		}

		public int 現在のVPS
		{
			get
			{
				lock( this._スレッド間同期 )
				{
					return this._現在のVPS;
				}
			}
		}

		public FPS()
		{
			this.子リスト.Add( this._FPSパラメータ = new 文字列画像() );

			this._初めてのFPS更新 = true;
			this._現在のFPS = 0;
			this._現在のVPS = 0;
		}

		/// <summary>
		///		FPSをカウントUPして、「現在のFPS, 現在のVPS」プロパティに現在の値をセットする。
		///		VPSはカウントUPされない。
		/// </summary>
		public void FPSをカウントしプロパティを更新する()
		{
			lock( this._スレッド間同期 )
			{
				if( this._初めてのFPS更新 )
				{
					this._初めてのFPS更新 = false;
					this._fps用カウンタ = 0;
					this._vps用カウンタ = 0;
					this._定間隔進行 = new 定間隔進行();
					this._定間隔進行.経過時間の計測を開始する();
				}
				else
				{
					// FPS 更新。
					this._fps用カウンタ++;

					// 1秒ごとに FPS, VPS プロパティの値を更新。
					this._定間隔進行.経過時間の分だけ進行する( 1000, () => {

						this._現在のFPS = this._fps用カウンタ;
						this._現在のVPS = this._vps用カウンタ;
						this._fps用カウンタ = 0;
						this._vps用カウンタ = 0;

					} );
				}
			}
		}

		/// <summary>
		///		VPSをカウントUPする。
		///		「現在のFPS, 現在のVPS」プロパティは更新しない。
		///		FPSはカウントUPされない。
		/// </summary>
		public void VPSをカウントする()
		{
			lock( this._スレッド間同期 )
			{
				// VPS 更新。
				this._vps用カウンタ++;
			}
		}

		public void 描画する( グラフィックデバイス gd, float x = 0f, float y =0f )
		{
			Debug.Assert( this.活性化している );

			this._FPSパラメータ.表示文字列 = $"VPS: {this._現在のVPS.ToString()} / FPS: {this._現在のFPS.ToString()}";
			this._FPSパラメータ.描画する( gd, x, y );
		}


		private int _現在のFPS = 0;

		private int _現在のVPS = 0;

		private int _fps用カウンタ = 0;

		private int _vps用カウンタ = 0;

		private 定間隔進行 _定間隔進行 = null;

		private bool _初めてのFPS更新 = true;

		private 文字列画像 _FPSパラメータ = null;

		private readonly object _スレッド間同期 = new object();
	}
}
