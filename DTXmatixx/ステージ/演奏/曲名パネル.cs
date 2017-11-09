using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SharpDX;
using SharpDX.DirectWrite;
using FDK;
using FDK.メディア;
using DTXmatixx.曲;

namespace DTXmatixx.ステージ.演奏
{
	class 曲名パネル : Activity
	{
		public 曲名パネル()
		{
			this.子リスト.Add( this._パネル = new 画像( @"$(System)images\演奏画面_曲名パネル.png" ) );
			this.子リスト.Add( this._曲名画像 = new 文字列画像() {
				フォント名 = "HGMaruGothicMPRO",
				フォントサイズpt = 26f,
				フォント幅 = FontWeight.Regular,
				フォントスタイル = FontStyle.Normal,
				描画効果 = 文字列画像.効果.縁取り,
				縁のサイズdpx = 4f,
				前景色 = Color4.Black,
				背景色 = Color4.White,
			} );
			this.子リスト.Add( this._サブタイトル画像 = new 文字列画像() {
				フォント名 = "HGMaruGothicMPRO",
				フォントサイズpt = 18f,
				フォント幅 = FontWeight.Regular,
				フォントスタイル = FontStyle.Normal,
				描画効果 = 文字列画像.効果.縁取り,
				縁のサイズdpx = 3f,
				前景色 = Color4.Black,
				背景色 = Color4.White,
			} );
		}

		protected override void On活性化( グラフィックデバイス gd )
		{
			using( Log.Block( FDKUtilities.現在のメソッド名 ) )
			{
				var 選択曲 = App.曲ツリー.フォーカス曲ノード;
				Debug.Assert( null != 選択曲 );

				this._曲名画像.表示文字列 = 選択曲.タイトル;
				this._サブタイトル画像.表示文字列 = 選択曲.サブタイトル;
			}
		}
		protected override void On非活性化( グラフィックデバイス gd )
		{
			using( Log.Block( FDKUtilities.現在のメソッド名 ) )
			{
			}
		}

		public void 描画する( グラフィックデバイス gd )
		{
			this._パネル.描画する( gd, 1458f, 3f );
			this._サムネイルを描画する( gd );
			this._曲名を描画する( gd );
			this._サブタイトルを描画する( gd );
		}

		private 画像 _パネル = null;
		private 文字列画像 _曲名画像 = null;
		private 文字列画像 _サブタイトル画像 = null;

		private readonly Vector3 _サムネイル画像表示位置dpx = new Vector3( 1477f, 19f, 0f );
		private readonly Vector3 _サムネイル画像表示サイズdpx = new Vector3( 91f, 91f, 0f );
		private readonly Vector2 _曲名表示位置dpx = new Vector2( 1576f + 4f, 43f + 10f );
		private readonly Vector2 _曲名表示サイズdpx = new Vector2( 331f - 8f - 4f, 70f - 10f );

		private void _サムネイルを描画する( グラフィックデバイス gd )
		{
			var 選択曲 = App.曲ツリー.フォーカス曲ノード;
			Debug.Assert( null != 選択曲 );

			var サムネイル画像 = 選択曲.ノード画像 ?? Node.既定のノード画像;
			Debug.Assert( null != サムネイル画像 );

			// テクスチャは画面中央が (0,0,0) で、Xは右がプラス方向, Yは上がプラス方向, Zは奥がプラス方向+。

			var 画面左上dpx = new Vector3(  // 3D視点で見る画面左上の座標。
				-gd.設計画面サイズ.Width / 2f,
				+gd.設計画面サイズ.Height / 2f,
				0f );

			var 変換行列 =
				Matrix.Scaling( this._サムネイル画像表示サイズdpx ) *
				Matrix.Translation(
					画面左上dpx.X + this._サムネイル画像表示位置dpx.X + this._サムネイル画像表示サイズdpx.X / 2f,
					画面左上dpx.Y - this._サムネイル画像表示位置dpx.Y - this._サムネイル画像表示サイズdpx.Y / 2f,
					0f );

			サムネイル画像.描画する( gd, 変換行列 );
		}
		private void _曲名を描画する( グラフィックデバイス gd )
		{
			// 拡大率を計算して描画する。

			this._曲名画像.描画する(
				gd,
				this._曲名表示位置dpx.X,
				this._曲名表示位置dpx.Y,
				X方向拡大率: ( this._曲名画像.サイズ.Width <= this._曲名表示サイズdpx.X ) ? 1f : this._曲名表示サイズdpx.X / this._曲名画像.サイズ.Width );
		}
		private void _サブタイトルを描画する( グラフィックデバイス gd )
		{
			// 拡大率を計算して描画する。

			this._サブタイトル画像.描画する(
				gd,
				this._曲名表示位置dpx.X,
				this._曲名表示位置dpx.Y + 30f,
				X方向拡大率: ( this._サブタイトル画像.サイズ.Width <= this._曲名表示サイズdpx.X ) ? 1f : this._曲名表示サイズdpx.X / this._サブタイトル画像.サイズ.Width );
		}
	}
}
