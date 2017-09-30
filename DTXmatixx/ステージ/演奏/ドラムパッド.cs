using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SharpDX;
using FDK;
using FDK.メディア;
using FDK.カウンタ;

namespace DTXmatixx.ステージ.演奏
{
	class ドラムパッド : Activity
	{
		public ドラムパッド()
		{
			this.子リスト.Add( this._パッド絵 = new 画像( @"$(System)images\ドラムパッド.png" ) );
		}

		protected override void On活性化( グラフィックデバイス gd )
		{
			using( Log.Block( FDKUtilities.現在のメソッド名 ) )
			{
				this._パッド絵の矩形リスト = new 矩形リスト( @"$(System)images\ドラムパッド矩形.xml" );

				this._レーンtoパッドContext = new Dictionary<表示レーン種別, パッドContext>();

				foreach( 表示レーン種別 lane in Enum.GetValues( typeof( 表示レーン種別 ) ) )
				{
					this._レーンtoパッドContext.Add( lane, new パッドContext() {
						左上位置dpx = new Vector2(
							x: レーンフレーム.領域.X + レーンフレーム.レーンtoチップの左端位置dpx[ lane ],
							y: 840f ),
						転送元矩形 = (RectangleF) this._パッド絵の矩形リスト[ lane.ToString() ],
						転送元矩形Flush = (RectangleF) this._パッド絵の矩形リスト[ lane.ToString() + "_Flush" ],
						アニメカウンタ = new Counter(),
					} );
				}
			}
		}
		protected override void On非活性化( グラフィックデバイス gd )
		{
			using( Log.Block( FDKUtilities.現在のメソッド名 ) )
			{
				this._レーンtoパッドContext.Clear();
			}
		}

		public void ヒットする( 表示レーン種別 lane )
		{
			this._レーンtoパッドContext[ lane ].アニメカウンタ.開始する( 0, 100, 1 );
		}
		public void 進行描画する( グラフィックデバイス gd )
		{
			foreach( 表示レーン種別 lane in Enum.GetValues( typeof( 表示レーン種別 ) ) )
			{
				var drumContext = this._レーンtoパッドContext[ lane ];

				// ドラム側アニメーション
				float Yオフセットdpx = 0f;
				float フラッシュ画像の不透明度 = 0f;

				if( drumContext.アニメカウンタ.動作中である )
				{
					フラッシュ画像の不透明度 = (float) Math.Sin( Math.PI * drumContext.アニメカウンタ.現在値の割合 );    // 0 → 1 → 0
					Yオフセットdpx = (float) Math.Sin( Math.PI * drumContext.アニメカウンタ.現在値の割合 ) * 18f;     // 0 → 18 → 0
				}

				// ドラムパッド本体表示
				this._パッド絵.描画する( gd, drumContext.左上位置dpx.X, drumContext.左上位置dpx.Y + Yオフセットdpx, 1f, 転送元矩形: drumContext.転送元矩形 );

				// ドラムフラッシュ表示
				this._パッド絵.描画する( gd, drumContext.左上位置dpx.X, drumContext.左上位置dpx.Y + Yオフセットdpx, フラッシュ画像の不透明度, 転送元矩形: drumContext.転送元矩形Flush );
			}
		}

		private 画像 _パッド絵 = null;
		private 矩形リスト _パッド絵の矩形リスト = null;

		private struct パッドContext
		{
			public Vector2 左上位置dpx;
			public RectangleF 転送元矩形;
			public RectangleF 転送元矩形Flush;
			public Counter アニメカウンタ;
		};
		private Dictionary<表示レーン種別, パッドContext> _レーンtoパッドContext = null;
	}
}
