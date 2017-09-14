using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SharpDX;
using SharpDX.Direct2D1;
using SharpDX.DirectInput;
using FDK;
using FDK.メディア;
using DTXmatixx.アイキャッチ;

namespace DTXmatixx.ステージ.選曲
{
	class 選曲ステージ : ステージ
	{
		public enum フェーズ
		{
			フェードイン,
			表示,
			確定,
			キャンセル,
		}
		public フェーズ 現在のフェーズ
		{
			get;
			protected set;
		}

		public 選曲ステージ()
		{
			this.子リスト.Add( this._舞台画像 = new 舞台画像() );
			this.子リスト.Add( this._曲リスト = new 曲リスト() );
			this.子リスト.Add( this._ステージタイマー = new 画像( @"$(System)images\ステージタイマー.png" ) );
		}
		protected override void On活性化( グラフィックデバイス gd )
		{
			using( Log.Block( FDKUtilities.現在のメソッド名 ) )
			{
				this._白 = new SolidColorBrush( gd.D2DDeviceContext, Color4.White );
				this._黒 = new SolidColorBrush( gd.D2DDeviceContext, Color4.Black );
				this._黒透過 = new SolidColorBrush( gd.D2DDeviceContext, new Color4( Color3.Black, 0.5f ) );
				this._灰透過 = new SolidColorBrush( gd.D2DDeviceContext, new Color4( 0x80535353 ) );
				this._ソートタブ上色 = new SolidColorBrush( gd.D2DDeviceContext, new Color4( 0xFF121212 ) );
				this._ソートタブ下色 = new SolidColorBrush( gd.D2DDeviceContext, new Color4( 0xFF1f1f1f ) );

				this.現在のフェーズ = フェーズ.フェードイン;
				this._初めての進行描画 = true;
			}
		}
		protected override void On非活性化( グラフィックデバイス gd )
		{
			using( Log.Block( FDKUtilities.現在のメソッド名 ) )
			{
				FDKUtilities.解放する( ref this._白 );
				FDKUtilities.解放する( ref this._黒 );
				FDKUtilities.解放する( ref this._黒透過 );
				FDKUtilities.解放する( ref this._灰透過 );
				FDKUtilities.解放する( ref this._ソートタブ上色 );
				FDKUtilities.解放する( ref this._ソートタブ下色 );
			}
		}
		public override void 進行描画する( グラフィックデバイス gd )
		{
			var fadeIn = App.ステージ管理.回転幕;

			if( this._初めての進行描画 )
			{
				fadeIn.オープンする( gd );
				this._初めての進行描画 = false;
			}

			App.Keyboard.ポーリングする();

			switch( this.現在のフェーズ )
			{
				case フェーズ.フェードイン:
					this._舞台画像.進行描画する( gd );
					this._曲リスト.進行描画する( gd );
					this._その他パネルを描画する( gd );
					fadeIn.進行描画する( gd );

					if( fadeIn.現在のフェーズ == 回転幕.フェーズ.オープン完了 )
					{
						this.現在のフェーズ = フェーズ.表示;
					}
					break;

				case フェーズ.表示:
					this._舞台画像.進行描画する( gd );
					this._曲リスト.進行描画する( gd );
					this._その他パネルを描画する( gd );
					break;

				case フェーズ.確定:
				case フェーズ.キャンセル:
					break;
			}
		}

		private bool _初めての進行描画 = true;
		private 舞台画像 _舞台画像 = null;
		private 曲リスト _曲リスト = null;

		private void _その他パネルを描画する( グラフィックデバイス gd )
		{
			gd.D2DBatchDraw( ( dc ) => {

				// 曲リストソートタブ
				dc.FillRectangle( new RectangleF( 927f, 50f, 993f, 138f ), this._ソートタブ上色 );
				dc.FillRectangle( new RectangleF( 927f, 142f, 993f, 46f ), this._ソートタブ下色 );

				// インフォメーションバー
				dc.FillRectangle( new RectangleF( 0f, 0f, 1920f, 50f ), this._黒 );
				dc.DrawLine( new Vector2( 0f, 50f ), new Vector2( 1920f, 50f ), this._白, strokeWidth: 1f );

				// ボトムバー
				dc.FillRectangle( new RectangleF( 0f, 1080f - 43f, 1920f, 1080f ), this._黒 );

				// プレビュー領域
				dc.FillRectangle( new RectangleF( 0f, 52f, 927f, 476f ), this._黒透過 );
				dc.DrawRectangle( new RectangleF( 0f, 52f, 927f, 476f ), this._灰透過, strokeWidth: 1f );
				dc.DrawLine( new Vector2( 1f, 442f ), new Vector2( 925f, 442f ), this._灰透過, strokeWidth: 1f );

			} );

			this._ステージタイマー.描画する( gd, 1689f, 37f );
		}

		private SolidColorBrush _白 = null;
		private SolidColorBrush _黒 = null;
		private SolidColorBrush _ソートタブ上色 = null;
		private SolidColorBrush _ソートタブ下色 = null;
		private SolidColorBrush _黒透過 = null;
		private SolidColorBrush _灰透過 = null;
		private 画像 _ステージタイマー = null;
	}
}
