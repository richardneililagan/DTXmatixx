using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SharpDX;
using SharpDX.Direct2D1;
using FDK;
using FDK.メディア;

namespace DTXmatixx.ステージ.演奏
{
	class 右サイドクリアパネル : Activity
	{
		public ビットマップ付きテクスチャ クリアパネル
		{
			get;
			protected set;
		} = null;

		public 右サイドクリアパネル()
		{
			this.子リスト.Add( this._背景 = new 画像( @"$(System)images\右サイドクリアパネル.png" ) );
			this.子リスト.Add( this.クリアパネル = new ビットマップ付きテクスチャ( new Size2( 500, 990 ) ) );  // this._背景.サイズはまだ設定されていない。
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

		/// <summary>
		///		クリアパネルに初期背景を上書きすることで、それまで描かれていた内容を消去する。
		/// </summary>
		public void クリアする( グラフィックデバイス gd )
		{
			this.クリアパネル.ビットマップへ描画する( gd, ( dc, bmp ) => {
				dc.PrimitiveBlend = PrimitiveBlend.Copy;
				dc.DrawBitmap( this._背景.Bitmap, opacity: 1f, interpolationMode: InterpolationMode.Linear );
			} );
		}
		public void 描画する( グラフィックデバイス gd )
		{
			// テクスチャは画面中央が (0,0,0) で、Xは右がプラス方向, Yは上がプラス方向, Zは奥がプラス方向+。

			var 画面左上dpx = new Vector3(  // 3D視点で見る画面左上の座標。
				-gd.設計画面サイズ.Width / 2f,
				+gd.設計画面サイズ.Height / 2f,
				0f );

			var 変換行列 =
				Matrix.Scaling( this.クリアパネル.サイズ.Width, this.クリアパネル.サイズ.Height, 1f ) *
				Matrix.RotationY( MathUtil.DegreesToRadians( +48f ) ) *
				Matrix.Translation( 画面左上dpx.X + 1630f, 画面左上dpx.Y - 530f, 0f );

			this.クリアパネル.描画する( gd, 変換行列 );
		}

		private 画像 _背景 = null;
	}
}
