using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SharpDX;
using SharpDX.Animation;
using SharpDX.DirectInput;
using SharpDX.Direct2D1;
using SharpDX.Direct2D1.Effects;
using FDK;
using FDK.メディア;
using DTXmatixx.ステージ.アイキャッチ;

namespace DTXmatixx.ステージ.タイトル
{
	class タイトルステージ : ステージ
	{
		public enum フェーズ
		{
			表示,
			フェードアウト,
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
			this.子リスト.Add( this._背景画像 = new 画像( @"$(System)images\舞台.jpg" ) );
			this.子リスト.Add( this._タイトルロゴ = new 画像( @"$(System)images\タイトルロゴ（読みあり）.png" ) );
			this.子リスト.Add( this._パッドを叩いてください = new 文字列画像( "パッドを叩いてください", 40f ) );
			this.子リスト.Add( this._フェードアウト = new シャッター() );
		}
		protected override void On活性化( グラフィックデバイス gd )
		{
			using( Log.Block( FDKUtilities.現在のメソッド名 ) )
			{
				this._帯ブラシ = new SolidColorBrush( gd.D2DDeviceContext, new Color4( 0f, 0f, 0f, 0.8f ) );
				this.現在のフェーズ = フェーズ.表示;
			}
		}
		protected override void On非活性化( グラフィックデバイス gd )
		{
			using( Log.Block( FDKUtilities.現在のメソッド名 ) )
			{
				FDKUtilities.解放する( ref this._帯ブラシ );
			}
		}
		public override void 進行描画する( グラフィックデバイス gd )
		{
			App.Keyboard.ポーリングする();

			switch( this.現在のフェーズ )
			{
				case フェーズ.表示:

					this._背景を描画する( gd, null );
					this._タイトルロゴ.描画する( gd, ( gd.設計画面サイズ.Width - this._タイトルロゴ.サイズ.Width ) / 2f, ( gd.設計画面サイズ.Height - this._タイトルロゴ.サイズ.Height ) / 2f - 100f );
					this._帯メッセージを描画する( gd );

					if( App.Keyboard.キーが押された( 0, Key.Return ) )
					{
						this._フェードアウト.クローズする( gd );
						this.現在のフェーズ = フェーズ.フェードアウト;
					}
					else if( App.Keyboard.キーが押された( 0, Key.Escape ) )
					{
						this.現在のフェーズ = フェーズ.キャンセル;
					}
					break;

				case フェーズ.フェードアウト:

					this._背景を描画する( gd, null );
					this._タイトルロゴ.描画する( gd, ( gd.設計画面サイズ.Width - this._タイトルロゴ.サイズ.Width ) / 2f, ( gd.設計画面サイズ.Height - this._タイトルロゴ.サイズ.Height ) / 2f - 100f );
					this._帯メッセージを描画する( gd );

					this._フェードアウト.進行描画する( gd );

					if( this._フェードアウト.現在のフェーズ == シャッター.フェーズ.クローズ完了 )
						this.現在のフェーズ = フェーズ.確定;
					break;

				case フェーズ.確定:
				case フェーズ.キャンセル:
					break;
			}
		}

		private 画像 _背景画像 = null;
		private 画像 _タイトルロゴ = null;
		private Brush _帯ブラシ = null;
		private 文字列画像 _パッドを叩いてください = null;
		private シャッター _フェードアウト = null;

		private void _背景を描画する( グラフィックデバイス gd, SharpDX.Direct2D1.Effect effect = null )
		{
			if( effect is null )
			{
				this._背景画像.描画する( gd, 0f, 0f );
			}
			else
			{
				gd.D2DBatchDraw( ( dc ) => {
					dc.DrawImage( effect, new Vector2( 0f, 0f ) );
				} );
			}
		}
		private void _帯メッセージを描画する( グラフィックデバイス gd )
		{
			var 領域 = new RectangleF( 0f, 800f, gd.設計画面サイズ.Width, 80f );

			gd.D2DBatchDraw( ( dc ) => {
				dc.FillRectangle( 領域, this._帯ブラシ );
			} );

			this._パッドを叩いてください.描画する( gd, 720f, 810f );
		}
	}
}
