using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SharpDX;
using SharpDX.Direct2D1;
using SharpDX.Direct3D11;

namespace FDK.メディア
{
	/// <summary>
	///		D3Dテクスチャとメモリを共有するD2Dビットマップを持つテクスチャ。
	///		D2Dビットマップに対して描画を行えば、それをD3Dテクスチャとして表示することができる。
	/// </summary>
	public class ビットマップ付きテクスチャ : テクスチャ
	{
		public ビットマップ付きテクスチャ( string 画像ファイルパス )
			: base( 画像ファイルパス, BindFlags.RenderTarget | BindFlags.ShaderResource )
		{
		}

		public ビットマップ付きテクスチャ( Size2 サイズ )
			: base( サイズ, BindFlags.RenderTarget | BindFlags.ShaderResource )
		{
		}

		protected override void On活性化( グラフィックデバイス gd )
		{
			// テクスチャを作成する。
			base.On活性化( gd );

			// 作成したテクスチャとデータを共有するビットマップターゲットを作成する。
			using( var dxgiSurface = this.Texture.QueryInterfaceOrNull<SharpDX.DXGI.Surface1>() )
			{
				var bmpProp = new BitmapProperties1() {
					PixelFormat = new PixelFormat( dxgiSurface.Description.Format, AlphaMode.Premultiplied ),
					BitmapOptions = BitmapOptions.Target | BitmapOptions.CannotDraw,
				};
				this._BitmapTarget = new Bitmap1( gd.D2DDeviceContext, dxgiSurface, bmpProp );
			}
		}

		protected override void On非活性化( グラフィックデバイス gd )
		{
			// ビットマップターゲットを解放する。
			FDKUtilities.解放する( ref this._BitmapTarget );

			// テクスチャを解放する。
			base.On非活性化( gd );
		}

		public void ビットマップへ描画する( グラフィックデバイス gd, Action<SharpDX.Direct2D1.DeviceContext1, Bitmap1> 描画アクション )
		{
			gd.D2DBatchDraw( ( dc ) => {

				using( var 旧ターゲット = dc.Target )
				{
					try
					{
						dc.Target = this._BitmapTarget;
						dc.Transform = Matrix3x2.Identity;	// 等倍描画（DPX to DPX）

						描画アクション( dc, this._BitmapTarget );
					}
					finally
					{
						dc.Target = 旧ターゲット;
					}
				}

			} );
		}


		private Bitmap1 _BitmapTarget = null;
	}
}
