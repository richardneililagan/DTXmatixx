using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using SharpDX.Direct2D1;

namespace FDK.メディア
{
	/// <summary>
	///		レンダーターゲットとしても描画可能なビットマップを扱うクラス。
	/// </summary>
	public class 描画可能画像 : 画像
	{
		public 描画可能画像( string 画像ファイルパス )
			: base( 画像ファイルパス )
		{
		}
		public 描画可能画像( Size2 サイズ )
			: base( null )
		{
			this._サイズ = サイズ;
		}
		public 描画可能画像( int width, int height )
			: base( null )
		{
			this._サイズ = new Size2( width, height );
		}

		protected override void On活性化( グラフィックデバイス gd )
		{
			if( this._画像ファイルパス.Nullでも空でもない() )
			{
				// (A) ファイルから生成する。
				this._画像を生成する( gd, new BitmapProperties1() { BitmapOptions = BitmapOptions.Target } );
			}
			else
			{
				// (B) 空のビットマップを生成する。
				this._Bitmap?.Dispose();
				this._Bitmap = new Bitmap1( gd.D2DDeviceContext, this._サイズ, new BitmapProperties1() {
					PixelFormat = new PixelFormat( gd.D2DDeviceContext.PixelFormat.Format, AlphaMode.Premultiplied ),
					BitmapOptions = BitmapOptions.Target,
				} );
			}
		}

		/// <summary>
		///		生成済み画像（ビットマップ）に対するユーザアクションによる描画を行う。
		/// </summary>
		/// <remarks>
		///		活性化状態であれば、進行描画() 中でなくても、任意のタイミングで呼び出して良い。
		///		ユーザアクション内では BeginDraw(), EndDraw() の呼び出しは（呼び出しもとでやるので）不要。
		/// </remarks>
		/// <param name="gd">グラフィックデバイス。</param>
		/// <param name="描画アクション">Bitmap に対して行いたい操作。</param>
		public void 画像へ描画する( グラフィックデバイス gd, Action<DeviceContext1> 描画アクション )
		{
			gd.D2DBatchDraw( ( dc ) => {

				dc.Transform = Matrix3x2.Identity;	// DPX から PX への拡大は、ここではなく画面への描画時に行う。

				using( var 旧ターゲット = dc.Target )
				{
					try
					{
						dc.Target = this._Bitmap;
						描画アクション( dc );
					}
					finally
					{
						dc.Target = 旧ターゲット;
					}
				}

			} );
		}


		private Size2 _サイズ;
	}
}
