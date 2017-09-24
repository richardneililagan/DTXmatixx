using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SharpDX;
using FDK;
using FDK.メディア;

namespace DTXmatixx.ステージ.演奏
{
	class コンボ : Activity
	{
		public int 現在値
		{
			get;
			set;
		} = 1234567;

		public コンボ()
		{
			this.子リスト.Add( this._コンボ文字画像 = new テクスチャ( @"$(System)images\コンボ文字.png" ) );
		}

		protected override void On活性化( グラフィックデバイス gd )
		{
			using( Log.Block( FDKUtilities.現在のメソッド名 ) )
			{
				this._コンボ文字画像の矩形 = new 矩形リスト( @"$(System)images\コンボ文字矩形.xml" );
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

			var 数字 = this.現在値.ToString();

			float 数字全体の幅dpx = 0f;
			for( int i = 0; i < 数字.Length; i++ )
				数字全体の幅dpx += this._コンボ文字画像の矩形[ 数字[ i ].ToString() ].Value.Size.Width;

			float Xdpx = -( 数字全体の幅dpx / 2f );
			for( int i = 0; i < 数字.Length; i++ )
			{
				var 矩形 = this._コンボ文字画像の矩形[ 数字[ i ].ToString() ].Value;

				//矩形.Width *= 0.5f;   // 修正

				var 変換行列 =
					Matrix.Scaling( 矩形.Width, 矩形.Height, 1f ) *
					Matrix.Translation( Xdpx, 0f, 0f ) *  // ローカル座標での移動
					Matrix.RotationY( MathUtil.DegreesToRadians( +48f ) ) *
					Matrix.Translation( 画面左上dpx.X + 1506f, 画面左上dpx.Y - 530f, 0f );  // ワールド座標での移動

				this._コンボ文字画像.描画する( gd, 変換行列, 矩形 );

				Xdpx += 矩形.Width;// - 10f; // ちょっと詰める
			}
		}

		private テクスチャ _コンボ文字画像 = null;
		private 矩形リスト _コンボ文字画像の矩形 = null;
	}
}
