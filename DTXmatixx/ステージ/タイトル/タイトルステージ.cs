using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SharpDX;
using SharpDX.Animation;
using SharpDX.DirectInput;
using FDK;
using FDK.メディア;

namespace DTXmatixx.ステージ.タイトル
{
	class タイトルステージ : ステージ
	{
		public enum フェーズ
		{
			表示,
			フェードアウト,
			フェードイン,
			確定,
			キャンセル,
		}
		public フェーズ 現在のフェーズ
		{
			get;
			protected set;
		}

		public タイトルステージ()
		{
			this.子リスト.Add( this._背景画像 = new 画像( @"D:\作業場\開発\@DTXMania\DTXmatixx\2017-06-16.jpg" ) );
			this.子リスト.Add( this._シャッター = new シャッター() );
		}

		protected override void On活性化( グラフィックデバイス gd )
		{
			using( Log.Block( FDKUtilities.現在のメソッド名 ) )
			{
				this.現在のフェーズ = フェーズ.表示;
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

			float 速度倍率 = 1.0f;

			switch( this.現在のフェーズ )
			{
				case フェーズ.表示:
					this._背景画像.描画する( gd, 0.0f, 0.0f );
					if( App.Keyboard.キーが押された( 0, Key.Return ) )
					{
						this.現在のフェーズ = フェーズ.フェードアウト;
						this._シャッター.クローズする( gd, 速度倍率 );
					}
					break;

				case フェーズ.フェードイン:
					this._背景画像.描画する( gd, 0.0f, 0.0f );
					this._シャッター.進行描画する( gd );
					if( App.Keyboard.キーが押された( 0, Key.Return ) )
					{
						this.現在のフェーズ = フェーズ.フェードアウト;
						this._シャッター.クローズする( gd, 速度倍率 );
					}
					break;

				case フェーズ.フェードアウト:
					this._背景画像.描画する( gd, 0.0f, 0.0f );
					this._シャッター.進行描画する( gd );
					if( App.Keyboard.キーが押された( 0, Key.Return ) )
					{
						this.現在のフェーズ = フェーズ.フェードイン;
						this._シャッター.オープンする( gd, 速度倍率 );
					}
					break;

				case フェーズ.確定:
				case フェーズ.キャンセル:
					break;
			}

			if( App.Keyboard.キーが離された( 0, Key.Escape ) )
			{
				this.現在のフェーズ = フェーズ.キャンセル;
			}
		}


		private 画像 _背景画像 = null;

		private シャッター _シャッター = null;
	}
}
