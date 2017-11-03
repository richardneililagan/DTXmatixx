using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using FDK;
using FDK.メディア;
using FDK.カウンタ;

namespace DTXmatixx.ステージ.終了
{
	class 終了ステージ : ステージ
	{
		public enum フェーズ
		{
			開始,
			表示中,
			確定,
		}
		public フェーズ 現在のフェーズ
		{
			get;
			protected set;
		}

		public 終了ステージ()
		{
			this.子リスト.Add( this._背景画像 = new 画像( @"$(System)images\終了画面.jpg" ) );
		}

		protected override void On活性化( グラフィックデバイス gd )
		{
			using( Log.Block( FDKUtilities.現在のメソッド名 ) )
			{
				this.現在のフェーズ = フェーズ.開始;
			}
		}
		protected override void On非活性化( グラフィックデバイス gd )
		{
			using( Log.Block( FDKUtilities.現在のメソッド名 ) )
			{
			}
		}

		public override void 進行描画する( グラフィックデバイス gd )
		{
			switch( this.現在のフェーズ )
			{
				case フェーズ.開始:
					this._カウンタ = new Counter( 0, 1, 値をひとつ増加させるのにかける時間ms: 1000 );
					this.現在のフェーズ = フェーズ.表示中;
					break;

				case フェーズ.表示中:
					this._背景画像.描画する( gd );

					if( this._カウンタ.終了値に達した )
					{
						this.現在のフェーズ = フェーズ.確定;
					}
					break;

				case フェーズ.確定:
					break;
			}
		}

		private 画像 _背景画像 = null;
		private Counter _カウンタ = null;
	}
}
