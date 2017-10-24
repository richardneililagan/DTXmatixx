using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SharpDX;
using SharpDX.Direct2D1;
using FDK;
using FDK.メディア;
using DTXmatixx.曲;

namespace DTXmatixx.ステージ.曲読み込み
{
	class 難易度 : Activity
	{
		public 難易度()
		{
			this.子リスト.Add( this._数字画像 = new 画像フォント( @"$(System)images\パラメータ文字_大.png", @"$(System)images\パラメータ文字_大矩形.xml", 文字幅補正dpx: 0f ) );
		}

		protected override void On活性化( グラフィックデバイス gd )
		{
			using( Log.Block( FDKUtilities.現在のメソッド名 ) )
			{
				var 選択曲 = App.曲ツリー.フォーカス曲ノード;
				Debug.Assert( null != 選択曲 );

				this._難易度文字列 = 選択曲.難易度.ToString( "0.00" );
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
			var ヘッダ描画領域 = new RectangleF( 783f, 117f, 414f, 63f );
			var ボディ描画領域 = new RectangleF( 783f, 180f, 414f, 213f );

			// todo: 今はMASTERで固定。
			gd.D2DBatchDraw( ( dc ) => {

				var pretrans = dc.Transform;

				using( var 黒ブラシ = new SolidColorBrush( dc, Color4.Black ) )
				using( var MASTER色ブラシ = new SolidColorBrush( dc, new Color4( 0xfffe55c6 ) ) )
				{
					// MASTER
					{
						dc.FillRectangle( ヘッダ描画領域, MASTER色ブラシ );
						dc.FillRectangle( ボディ描画領域, 黒ブラシ );

						// 小数部を描画する
						dc.Transform =
							Matrix3x2.Scaling( 2.2f, 2.2f ) *
							Matrix3x2.Translation( ボディ描画領域.X + 175f, ボディ描画領域.Y ) *
							pretrans;
						this._数字画像.描画する( dc, 0f, 0f, this._難易度文字列.Substring( 2 ) );

						// 整数部を描画する（'.'含む）
						dc.Transform =
							Matrix3x2.Scaling( 2.2f, 2.2f ) *
							Matrix3x2.Translation( ボディ描画領域.X + 15f, ボディ描画領域.Y ) *
							pretrans;
						this._数字画像.描画する( dc, 0f, 0f, this._難易度文字列.Substring( 0, 2 ) );
					}
				}

			} );
		}

		private 画像フォント _数字画像 = null;
		private string _難易度文字列 = "5.00";
	}
}
