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
	class 曲ステータスパネル : Activity
	{
		public 曲ステータスパネル()
		{
			this.子リスト.Add( this._背景画像 = new 画像( @"$(System)images\選曲画面_曲ステータスパネル.png" ) );
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
			var 領域dpx = new RectangleF( 320f, 532f, 239f, 505f );

			this._背景画像.描画する( gd, 左位置: 領域dpx.X, 上位置: 領域dpx.Y );
		}

		private 画像 _背景画像 = null;
	}
}
