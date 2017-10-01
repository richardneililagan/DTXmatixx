using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SharpDX;
using SharpDX.Direct2D1;
using FDK;
using FDK.メディア;

namespace DTXmatixx.ステージ.選曲
{
	class 難易度と成績 : Activity
	{
		public 難易度と成績()
		{
		}

		protected override void On活性化( グラフィックデバイス gd )
		{
			using( Log.Block( FDKUtilities.現在のメソッド名 ) )
			{
			}
		}
		protected override void On非活性化( グラフィックデバイス gd )
		{
			using( Log.Block( FDKUtilities.現在のメソッド名 ) )
			{
			}
		}

		public void 描画する( グラフィックデバイス gd )
		{
			gd.D2DBatchDraw( ( dc ) => {

				var 領域dpx = new RectangleF( 642f, 529f, 338f, 508f );

				using( var 黒ブラシ = new SolidColorBrush( dc, Color4.Black ) )
				using( var 黒透過ブラシ = new SolidColorBrush( dc, new Color4( Color3.Black, 0.5f ) ) )
				using( var MASTER色ブラシ = new SolidColorBrush( dc, new Color4( 0xfffe55c6 ) ) )
				using( var EXTREME色ブラシ = new SolidColorBrush( dc, new Color4( 0xff7d5cfe ) ) )
				using( var ADVANCED色ブラシ = new SolidColorBrush( dc, new Color4( 0xff00aaeb ) ) )
				using( var BASIC色ブラシ = new SolidColorBrush( dc, new Color4( 0xfffe9551 ) ) )
				{
					// 背景
					dc.FillRectangle( 領域dpx, 黒透過ブラシ );

					// MASTER
					dc.FillRectangle( new RectangleF( 領域dpx.X + 156f, 領域dpx.Y + 29f, 157f, 24f ), MASTER色ブラシ );
					dc.FillRectangle( new RectangleF( 領域dpx.X + 156f, 領域dpx.Y + 53f, 157f, 78f ), 黒ブラシ );

					// EXTREME
					dc.FillRectangle( new RectangleF( 領域dpx.X + 156f, 領域dpx.Y + 149f, 157f, 24f ), EXTREME色ブラシ );
					dc.FillRectangle( new RectangleF( 領域dpx.X + 156f, 領域dpx.Y + 173f, 157f, 78f ), 黒ブラシ );

					// ADVANCED
					dc.FillRectangle( new RectangleF( 領域dpx.X + 156f, 領域dpx.Y + 269f, 157f, 24f ), ADVANCED色ブラシ );
					dc.FillRectangle( new RectangleF( 領域dpx.X + 156f, 領域dpx.Y + 293f, 157f, 78f ), 黒ブラシ );

					// BASIC
					dc.FillRectangle( new RectangleF( 領域dpx.X + 156f, 領域dpx.Y + 389f, 157f, 24f ), BASIC色ブラシ );
					dc.FillRectangle( new RectangleF( 領域dpx.X + 156f, 領域dpx.Y + 413f, 157f, 78f ), 黒ブラシ );
				}

			} );
		}
	}
}
