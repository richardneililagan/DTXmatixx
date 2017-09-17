using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SharpDX;
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

		public void 描画する( グラフィックデバイス gd )
		{
			var 選択曲 = App.曲ツリー.フォーカスノード as MusicNode;
			Debug.Assert( null != 選択曲 );

			var サムネイル画像 = 選択曲.ノード画像 ?? Node.既定のノード画像;
			Debug.Assert( null != サムネイル画像 );

			this._パネル.描画する( gd, 1458f, 3f );

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

		private 画像 _パネル = null;
		private readonly Vector3 _サムネイル画像表示位置dpx = new Vector3( 1477f, 19f, 0f );
		private readonly Vector3 _サムネイル画像表示サイズdpx = new Vector3( 91f, 91f, 0f );
	}
}
