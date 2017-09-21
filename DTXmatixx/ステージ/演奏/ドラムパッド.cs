using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using FDK;
using FDK.メディア;

namespace DTXmatixx.ステージ.演奏
{
	class ドラムパッド : Activity
	{
		public ドラムパッド()
		{
			this.子リスト.Add( this._パッド絵 = new 画像( @"$(System)images\ドラムパッド.png" ) );
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

		public void 進行描画する( グラフィックデバイス gd )
		{
			this._パッド絵.描画する( gd, 445f, 840f );
		}

		private 画像 _パッド絵 = null;
	}
}
