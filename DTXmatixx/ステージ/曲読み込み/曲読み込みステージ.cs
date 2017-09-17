using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SharpDX;
using SharpDX.Direct2D1;
using SharpDX.DirectWrite;
using FDK;
using FDK.メディア;
using DTXmatixx.曲;

namespace DTXmatixx.ステージ.曲読み込み
{
	class 曲読み込みステージ : ステージ
	{
		private readonly string _フォント名 =
			//"HGMaruGothicMPRO";	なんかグリフがバグる……
			"メイリオ";
		private readonly float _フォントサイズ = 80.0f;

		public enum フェーズ
		{
			フェードイン,
			表示,
			完了,
			キャンセル,
		}
		public フェーズ 現在のフェーズ
		{
			get;
			protected set;
		}

		public 曲読み込みステージ()
		{
			this.子リスト.Add( this._舞台画像 = new 舞台画像() );
			this.子リスト.Add( this._注意文 = new 画像( @"$(System)images\ご注意ください.png" ) );
		}

		protected override void On活性化( グラフィックデバイス gd )
		{
			using( Log.Block( FDKUtilities.現在のメソッド名 ) )
			{
				this._曲名フォーマット = new TextFormat( gd.DWriteFactory, this._フォント名, FontWeight.UltraBlack, FontStyle.Normal, this._フォントサイズ );
				this._曲名の輪郭の色 = new SolidColorBrush( gd.D2DDeviceContext, Color4.White );
				this._曲名の塗りつぶしの色 = new SolidColorBrush( gd.D2DDeviceContext, Color4.Black );

				this.現在のフェーズ = フェーズ.フェードイン;
				this._初めての進行描画 = true;
			}
		}
		protected override void On非活性化( グラフィックデバイス gd )
		{
			using( Log.Block( FDKUtilities.現在のメソッド名 ) )
			{
				FDKUtilities.解放する( ref this._曲名の塗りつぶしの色 );
				FDKUtilities.解放する( ref this._曲名の輪郭の色 );
			}
		}

		public override void 進行描画する( グラフィックデバイス gd )
		{
			var fadeIn = App.ステージ管理.GO;

			if( this._初めての進行描画 )
			{
				this._舞台画像.ぼかしを適用する( gd, 0.0 );
				fadeIn.オープンする( gd );
				this._初めての進行描画 = false;
			}

			this._舞台画像.進行描画する( gd );
			this._注意文.描画する( gd, 0f, 760f );
			this._プレビュー画像を描画する( gd );
			this._曲名を描画する( gd );

			switch( this.現在のフェーズ )
			{
				case フェーズ.フェードイン:
					fadeIn.進行描画する( gd );
					if( fadeIn.現在のフェーズ == アイキャッチ.GO.フェーズ.オープン完了 )
						this.現在のフェーズ = フェーズ.表示;
					break;

				case フェーズ.表示:
					
					// todo: ここで曲データを読み込む。

					this.現在のフェーズ = フェーズ.完了;
					break;

				case フェーズ.完了:
				case フェーズ.キャンセル:
					break;
			}
		}

		private bool _初めての進行描画 = true;
		private 舞台画像 _舞台画像 = null;
		private 画像 _注意文 = null;
		private readonly Vector3 _プレビュー画像表示位置dpx = new Vector3( 150f, 117f, 0f );
		private readonly Vector3 _プレビュー画像表示サイズdpx = new Vector3( 576f, 576f, 0f );

		private void _プレビュー画像を描画する( グラフィックデバイス gd )
		{
			var 選択曲 = App.曲ツリー.フォーカスノード as MusicNode;
			Debug.Assert( null != 選択曲 );

			var プレビュー画像 = 選択曲.ノード画像 ?? Node.既定のノード画像;
			Debug.Assert( null != プレビュー画像 );

			// テクスチャは画面中央が (0,0,0) で、Xは右がプラス方向, Yは上がプラス方向, Zは奥がプラス方向+。

			var 画面左上dpx = new Vector3(  // 3D視点で見る画面左上の座標。
				-gd.設計画面サイズ.Width / 2f,
				+gd.設計画面サイズ.Height / 2f,
				0f );

			var 変換行列 =
				Matrix.Scaling( this._プレビュー画像表示サイズdpx ) *
				Matrix.Translation(
					画面左上dpx.X + this._プレビュー画像表示位置dpx.X + this._プレビュー画像表示サイズdpx.X / 2f,
					画面左上dpx.Y - this._プレビュー画像表示位置dpx.Y - this._プレビュー画像表示サイズdpx.Y / 2f,
					0f );

			プレビュー画像.描画する( gd, 変換行列 );
		}

		private TextFormat _曲名フォーマット = null;
		private SolidColorBrush _曲名の輪郭の色 = null;
		private SolidColorBrush _曲名の塗りつぶしの色 = null;

		private void _曲名を描画する( グラフィックデバイス gd )
		{
			var 選択曲 = App.曲ツリー.フォーカスノード as MusicNode;
			Debug.Assert( null != 選択曲 );

			gd.D2DBatchDraw( ( dc ) => {

				using( var textLayout = new TextLayout( gd.DWriteFactory, 選択曲.タイトル, this._曲名フォーマット, 1920f, 1080f ) )
				using( var textRenderer = new 縁取りTextRenderer( gd.D2DFactory, dc, this._曲名の輪郭の色, this._曲名の塗りつぶしの色, 8f ) )
				{
					textLayout.Draw( textRenderer, 782f, 409f );
				}

			} );
		}
	}
}
