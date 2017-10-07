using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SharpDX;
using FDK;
using FDK.メディア;

namespace DTXmatixx.ステージ.演奏
{
	class 譜面スクロール速度表示 : Activity
	{
		public 譜面スクロール速度表示()
		{
			this.子リスト.Add( this._文字画像 = new 画像フォント( @"$(System)images\パラメータ文字_小.png", @"$(System)images\パラメータ文字_小矩形.xml", 文字幅補正dpx: -3f ) );
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

		public void 進行描画する( グラフィックデバイス gd, double 速度 )
		{
			var 表示領域 = new RectangleF( 482, 985f, 48f, 24f );

			this._文字画像.描画する( gd, 表示領域.X, 表示領域.Y, 速度.ToString( "0.0" ) );
		}

		private 画像フォント _文字画像 = null;
	}
}
