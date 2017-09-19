using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SharpDX;
using FDK;
using FDK.メディア;

namespace DTXmatixx.ステージ.演奏
{
	class 成績パネル : Activity
	{
		public 成績パネル()
		{
			this.子リスト.Add( this._パネル = new テクスチャ( @"$(System)images\演奏画面_成績パネル.png" ) );
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

		public void 進行描画する( グラフィックデバイス gd )
		{
			// テクスチャは画面中央が (0,0,0) で、Xは右がプラス方向, Yは上がプラス方向, Zは奥がプラス方向+。

			var 画面左上dpx = new Vector3(  // 3D視点で見る画面左上の座標。
				-gd.設計画面サイズ.Width / 2f,
				+gd.設計画面サイズ.Height / 2f,
				0f );

			var 変換行列 =
				Matrix.Scaling( this._パネル.サイズ.Width, this._パネル.サイズ.Height, 1f ) *
				Matrix.RotationY( MathUtil.DegreesToRadians( +48f ) ) *
				Matrix.Translation( 画面左上dpx.X + 1506f, 画面左上dpx.Y - 530f, 0f );

			this._パネル.描画する( gd, 変換行列 );
		}

		private テクスチャ _パネル = null;
	}
}
