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
				this._見出し用TextFormat = new TextFormat( gd.DWriteFactory, "Century Gothic", 50f );
			}
		}
		protected override void On非活性化( グラフィックデバイス gd )
		{
			using( Log.Block( FDKUtilities.現在のメソッド名 ) )
			{
				FDKUtilities.解放する( ref this._見出し用TextFormat );
			}
		}

		public void 描画する( グラフィックデバイス gd )
		{
			var 見出し描画領域 = new RectangleF( 783f, 117f, 414f, 63f );
			var 数値描画領域 = new RectangleF( 783f, 180f, 414f, 213f );

			var node = App.曲ツリー.フォーカス曲ノード;
			var anker = App.曲ツリー.フォーカス難易度;

			(string label, float level) 難易度;
			if( node.親ノード is SetNode )
			{
				// 親が SetNode なら、難易度はそっちから取得する。
				難易度 = node.親ノード.難易度[ anker ];
			}
			else
			{
				難易度 = node.難易度[ anker ];
			}

			gd.D2DBatchDraw( ( dc ) => {

				var pretrans = dc.Transform;

				using( var 見出し背景ブラシ = new SolidColorBrush( dc, Node.難易度色[ anker ] ) )
				using( var 黒ブラシ = new SolidColorBrush( dc, Color4.Black ) )
				using( var 黒透過ブラシ = new SolidColorBrush( dc, new Color4( Color3.Black, 0.5f ) ) )
				using( var 白ブラシ = new SolidColorBrush( dc, Color4.White ) )
				{
					dc.Transform = pretrans;

					// 背景領域を塗りつぶす。
					dc.FillRectangle( 見出し描画領域, 見出し背景ブラシ );
					dc.FillRectangle( 数値描画領域, 黒ブラシ );

					// 見出し文字列を描画する。
					this._見出し用TextFormat.TextAlignment = TextAlignment.Trailing;
					var 見出し文字領域 = 見出し描画領域;
					見出し文字領域.Width -= 8f;	// 右マージン
					dc.DrawText( 難易度.label, this._見出し用TextFormat, 見出し文字領域, 白ブラシ );

					// 小数部を描画する。
					var 数値文字列 = 難易度.level.ToString( "0.00" ).PadLeft( 1 );
					dc.Transform =
						Matrix3x2.Scaling( 2.2f, 2.2f ) *
						Matrix3x2.Translation( 数値描画領域.X + 175f, 数値描画領域.Y ) *
						pretrans;
					this._数字画像.描画する( dc, 0f, 0f, 数値文字列.Substring( 2 ) );

					// 整数部と小数点を描画する。
					dc.Transform =
						Matrix3x2.Scaling( 2.2f, 2.2f ) *
						Matrix3x2.Translation( 数値描画領域.X + 15f, 数値描画領域.Y ) *
						pretrans;
					this._数字画像.描画する( dc, 0f, 0f, 数値文字列.Substring( 0, 2 ) );
				}

			} );
		}

		private 画像フォント _数字画像 = null;
		//private string _難易度文字列 = "5.00";
		private TextFormat _見出し用TextFormat = null;
	}
}
