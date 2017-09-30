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
	class レーンフラッシュ : Activity
	{
		public レーンフラッシュ()
		{
			this.子リスト.Add( this._レーンフラッシュ画像 = new 画像( @"$(System)images\レーンフラッシュ.png" ) { 加算合成 = true } );
		}

		protected override void On活性化( グラフィックデバイス gd )
		{
			using( Log.Block( FDKUtilities.現在のメソッド名 ) )
			{
				this._レーンフラッシュの矩形リスト = new 矩形リスト( @"$(System)images\レーンフラッシュ矩形.xml" );

				this._レーンtoレーンContext = new Dictionary<表示レーン種別, レーンContext>();

				foreach( 表示レーン種別 lane in Enum.GetValues( typeof( 表示レーン種別 ) ) )
				{
					this._レーンtoレーンContext.Add( lane, new レーンContext() {
						開始位置dpx = new Vector2(
							x: レーンフレーム.領域.X + レーンフレーム.レーンtoチップの左端位置dpx[ lane ],
							y: レーンフレーム.領域.Bottom ),
						転送元矩形 = (RectangleF) this._レーンフラッシュの矩形リスト[ lane.ToString() ],
						アニメカウンタ = new Counter(),
					} );
				}
			}
		}
		protected override void On非活性化( グラフィックデバイス gd )
		{
			using( Log.Block( FDKUtilities.現在のメソッド名 ) )
			{
				this._レーンtoレーンContext.Clear();
			}
		}

		public void 開始する( 表示レーン種別 lane )
		{
			this._レーンtoレーンContext[ lane ].アニメカウンタ.開始する( 0, 250, 1 );
		}
		public void 進行描画する( グラフィックデバイス gd )
		{
			foreach( 表示レーン種別 lane in Enum.GetValues( typeof( 表示レーン種別 ) ) )
			{
				var laneContext = this._レーンtoレーンContext[ lane ];

				if( laneContext.アニメカウンタ.動作中である && laneContext.アニメカウンタ.終了値に達していない )
				{
					this._レーンフラッシュ画像.描画する(
						gd, 
						laneContext.開始位置dpx.X,
						laneContext.開始位置dpx.Y - laneContext.アニメカウンタ.現在値の割合 * レーンフレーム.領域.Height,
						不透明度0to1: 1f - laneContext.アニメカウンタ.現在値の割合,
						転送元矩形: laneContext.転送元矩形 );
				}
			}
		}

		private struct レーンContext
		{
			public Vector2 開始位置dpx;
			public RectangleF 転送元矩形;
			public Counter アニメカウンタ;
		};
		private Dictionary<表示レーン種別, レーンContext> _レーンtoレーンContext = null;

		private 画像 _レーンフラッシュ画像 = null;
		private 矩形リスト _レーンフラッシュの矩形リスト = null;
	}
}
