using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SharpDX;
using SharpDX.DirectInput;
using FDK;
using FDK.メディア;
using DTXmatixx.アイキャッチ;

namespace DTXmatixx.ステージ.結果
{
	class 結果ステージ : ステージ
	{
		public enum フェーズ
		{
			表示,
			フェードアウト,
			確定,
		}
		public フェーズ 現在のフェーズ
		{
			get;
			protected set;
		}

		public 結果ステージ()
		{
			this.子リスト.Add( this._背景 = new 舞台画像() );
		}

		protected override void On活性化( グラフィックデバイス gd )
		{
			using( Log.Block( FDKUtilities.現在のメソッド名 ) )
			{
				this.現在のフェーズ = フェーズ.表示;
				this._初めての進行描画 = true;
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
			if( this._初めての進行描画 )
			{
				this._背景.ぼかしと縮小を適用する( gd, 0.0 );	// 即時適用
				this._初めての進行描画 = false;
			}

			this._背景.進行描画する( gd );

			App.Keyboard.ポーリングする();

			switch( this.現在のフェーズ )
			{
				case フェーズ.表示:
					if( App.Keyboard.キーが押された( 0, Key.Return ) )
					{
						App.ステージ管理.アイキャッチを選択しクローズする( gd, nameof( シャッター ) );
						this.現在のフェーズ = フェーズ.フェードアウト;
					}
					break;

				case フェーズ.フェードアウト:
					App.ステージ管理.現在のアイキャッチ.進行描画する( gd );
					if( App.ステージ管理.現在のアイキャッチ.現在のフェーズ == アイキャッチ.フェーズ.クローズ完了 )
						this.現在のフェーズ = フェーズ.確定;
					break;

				case フェーズ.確定:
					break;
			}
		}

		private bool _初めての進行描画 = true;
		private 舞台画像 _背景 = null;
	}
}
