using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SharpDX;
using SharpDX.Direct2D1;
using FDK;
using FDK.メディア;

namespace DTXmatixx.ステージ.演奏
{
	class 成績パネル : Activity
	{
		public 成績パネル()
		{
			this.子リスト.Add( this._パネル = new 画像( @"$(System)images\演奏画面_成績パネル.png" ) );
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

		public void 描画する( DeviceContext1 描画先dc, Bitmap1 描画先bmp )
		{
			描画先dc.DrawBitmap(
				this._パネル.Bitmap,
				destinationRectangle: new RectangleF( +12f, +80f, 225f, 848f ),
				opacity: 0.01f,
				interpolationMode: BitmapInterpolationMode.Linear );
		}

		private 画像 _パネル = null;
	}
}
