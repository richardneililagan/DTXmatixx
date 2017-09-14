using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SharpDX.DirectInput;
using FDK;
using FDK.メディア;

namespace DTXmatixx.ステージ.曲ツリー構築
{
	class 曲ツリー構築ステージ : ステージ
	{
		public enum フェーズ
		{
			開始,
			構築中,
			確定,
			キャンセル,
		}
		public フェーズ 現在のフェーズ
		{
			get;
			protected set;
		}

		public 曲ツリー構築ステージ()
		{
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
			App.Keyboard.ポーリングする();

			switch( this.現在のフェーズ )
			{
				case フェーズ.開始:
					this.現在のフェーズ = フェーズ.構築中;
					break;

				case フェーズ.構築中:
					this.現在のフェーズ = フェーズ.確定;
					break;

				case フェーズ.確定:
				case フェーズ.キャンセル:
					break;
			}

			if( App.Keyboard.キーが押された( 0, Key.Escape ) )
			{
				this.現在のフェーズ = フェーズ.キャンセル;
			}

		}
	}
}
