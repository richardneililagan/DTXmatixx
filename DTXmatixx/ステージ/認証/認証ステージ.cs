using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SharpDX;
using SharpDX.Direct2D1.Effects;
using FDK;
using FDK.メディア;
using FDK.カウンタ;
using DTXmatixx.アイキャッチ;

namespace DTXmatixx.ステージ.認証
{
	class 認証ステージ : ステージ
	{
		public enum フェーズ
		{
			フェードイン,
			表示,
			フェードアウト,
			アイキャッチ,
			確定,
			キャンセル,
		}
		public フェーズ 現在のフェーズ
		{
			get;
			protected set;
		}

		public 認証ステージ()
		{
			this.子リスト.Add( this._舞台画像 = new 舞台画像() );
			this.子リスト.Add( this._タッチアイコン = new 画像( @"$(System)images\タッチアイコン.png" ) );
			this.子リスト.Add( this._確認できませんでした = new 文字列画像( "確認できませんでした。", 40f ) );
		}
		protected override void On活性化( グラフィックデバイス gd )
		{
			using( Log.Block( FDKUtilities.現在のメソッド名 ) )
			{
				this.現在のフェーズ = フェーズ.フェードイン;
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
			var fadeIn = App.ステージ管理.シャッター;
			var fadeOut = App.ステージ管理.回転幕;

			if( this._初めての進行描画 )
			{
				fadeIn.オープンする( gd );
				this._初めての進行描画 = false;
			}

			App.Keyboard.ポーリングする();

			switch( this.現在のフェーズ )
			{
				case フェーズ.フェードイン:
					this._舞台画像.進行描画する( gd, true );
					this._タッチアイコン.描画する( gd, this._タッチアイコン表示行列 );
					fadeIn.進行描画する( gd );

					if( fadeIn.現在のフェーズ == シャッター.フェーズ.オープン完了 )
					{
						this._表示フェーズカウンタ = new Counter( 0, 5000, 1 );	// 全5秒
						this.現在のフェーズ = フェーズ.表示;
					}
					break;

				case フェーズ.表示:
					this._舞台画像.進行描画する( gd, true );
					this._タッチアイコン.描画する( gd, this._タッチアイコン表示行列 );
					if( 1000 < this._表示フェーズカウンタ.現在値 )
						this._確認できませんでした.描画する( gd, this._確認できませんでした表示行列 );

					if( 2000 < this._表示フェーズカウンタ.現在値 )
					{
						fadeOut.クローズする( gd );
						this.現在のフェーズ = フェーズ.フェードアウト;
					}
					break;

				case フェーズ.フェードアウト:
					this._舞台画像.進行描画する( gd, true );
					fadeOut.進行描画する( gd );

					if( fadeOut.現在のフェーズ == 回転幕.フェーズ.クローズ完了 )
					{
						this._待機フェーズカウンタ = new Counter( 0, 500, 1 );   // 全0.5秒
						this.現在のフェーズ = フェーズ.アイキャッチ;
					}
					break;

				case フェーズ.アイキャッチ:
					//this._舞台画像.進行描画する( gd );		フェードアウトで全画面上書きされるため、描画不要。
					fadeOut.進行描画する( gd );

					if( this._待機フェーズカウンタ.終了値に達した )
					{
						this.現在のフェーズ = フェーズ.確定;
					}
					break;

				case フェーズ.確定:
				case フェーズ.キャンセル:
					break;
			}
		}

		private bool _初めての進行描画 = true;
		private 舞台画像 _舞台画像 = null;
		private 画像 _タッチアイコン = null;
		private 文字列画像 _確認できませんでした = null;
		private Counter _表示フェーズカウンタ = null;
		private Counter _待機フェーズカウンタ = null;

		private readonly Matrix3x2 _タッチアイコン表示行列 = Matrix3x2.Translation( 960f - 128f, 540f - 128f );
		private readonly Matrix3x2 _確認できませんでした表示行列 = Matrix3x2.Translation( 960f - 180f, 540f - 200f );
	}
}
