using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using FDK;
using FDK.メディア;
using FDK.カウンタ;

namespace DTXmatixx.ステージ.選曲
{
	class 選択曲枠ランナー : Activity
	{
		public 選択曲枠ランナー()
		{
			this.子リスト.Add( this._ランナー画像 = new 画像( @"$(System)images\選曲画面_枠ランナー.png" ) );
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

		public void リセットする()
		{
			this._カウンタ = new LoopCounter( 0, 2300, 1 );	// 2秒ごとに300ms のループ
		}
		public void 進行描画する( グラフィックデバイス gd )
		{
			if( null == this._カウンタ )
				return;

			if( 2000 <= this._カウンタ.現在値 )
			{
				float 割合 = ( this._カウンタ.現在値 - 2000 ) / 300f;    // 0→1

				// 上
				this._ランナー画像.描画する(
					gd,
					左位置: 1920f - 割合 * ( 1920f - 1044f ),
					上位置: 485f - this._ランナー画像.サイズ.Height / 2f );

				// 下
				this._ランナー画像.描画する(
					gd,
					左位置: 1920f - 割合 * ( 1920f - 1044f ),
					上位置: 598f - this._ランナー画像.サイズ.Height / 2f );
			}
		}

		private 画像 _ランナー画像 = null;
		private LoopCounter _カウンタ = null;
	}
}
