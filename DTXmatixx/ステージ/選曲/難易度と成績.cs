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
using DTXmatixx.設定;
using DTXmatixx.データベース.曲;

namespace DTXmatixx.ステージ.選曲
{
	class 難易度と成績 : Activity
	{
		// 外部接続アクション
		public Func<青い線> 青い線を取得する = null;

		public 難易度と成績()
		{
			this.子リスト.Add( this._数字画像 = new 画像フォント( @"$(System)images\パラメータ文字_大.png", @"$(System)images\パラメータ文字_大矩形.xml", 文字幅補正dpx: 0f ) );
		}

		protected override void On活性化( グラフィックデバイス gd )
		{
			using( Log.Block( FDKUtilities.現在のメソッド名 ) )
			{
				this._現在表示しているノード = null;
				this._見出し用TextFormat = new TextFormat( gd.DWriteFactory, "Century Gothic", 20f );
			}
		}
		protected override void On非活性化( グラフィックデバイス gd )
		{
			using( Log.Block( FDKUtilities.現在のメソッド名 ) )
			{
				FDKUtilities.解放する( ref this._見出し用TextFormat );
			}
		}

		/// <param name="選択している難易度">
		///		0:BASIC～4:ULTIMATE
		///	</param>
		public void 描画する( グラフィックデバイス gd, int 選択している難易度 )
		{
			#region " ノードが変更されていたら、情報を更新する。"
			//----------------
			if( App.曲ツリー.フォーカスノード != this._現在表示しているノード )
			{
				this._現在表示しているノード = App.曲ツリー.フォーカスノード;  // フォーカス曲ノードではない → このクラスではMusicNode以外も表示できる　というかSetNodeな。
			}
			//----------------
			#endregion

			var node = this._現在表示しているノード;

			bool 表示可能ノードである = ( node is MusicNode ) || ( node is SetNode );

			gd.D2DBatchDraw( ( dc ) => {

				var pretrans = dc.Transform;

				// 難易度パネルを描画する。

				var 領域dpx = new RectangleF( 642f, 529f, 338f, 508f );

				using( var 黒ブラシ = new SolidColorBrush( dc, Color4.Black ) )
				using( var 黒透過ブラシ = new SolidColorBrush( dc, new Color4( Color3.Black, 0.5f ) ) )
				using( var 白ブラシ = new SolidColorBrush( dc, Color4.White ) )
				using( var ULTIMATE色ブラシ = new SolidColorBrush( dc, Node.難易度色[ 4 ] ) )
				using( var MASTER色ブラシ = new SolidColorBrush( dc, Node.難易度色[ 3 ] ) )
				using( var EXTREME色ブラシ = new SolidColorBrush( dc, Node.難易度色[ 2 ] ) )
				using( var ADVANCED色ブラシ = new SolidColorBrush( dc, Node.難易度色[ 1 ] ) )
				using( var BASIC色ブラシ = new SolidColorBrush( dc, Node.難易度色[ 0 ] ) )
				{
					// 背景
					dc.FillRectangle( 領域dpx, 黒透過ブラシ );

					if( 表示可能ノードである )
					{
						// ULTIMATE 相当
						// todo: ULTIMATE の表示を実装する。
						
						// MASTER 相当
						this._難易度パネルを１つ描画する( dc, pretrans, 領域dpx.X, 領域dpx.Y, node.難易度[ 3 ].label, node.難易度[ 3 ].level, 白ブラシ, MASTER色ブラシ, 黒ブラシ );

						// EXTREME 相当
						this._難易度パネルを１つ描画する( dc, pretrans, 領域dpx.X, 領域dpx.Y + 120f, node.難易度[ 2 ].label, node.難易度[ 2 ].level, 白ブラシ, EXTREME色ブラシ, 黒ブラシ );

						// ADVANCED 相当
						this._難易度パネルを１つ描画する( dc, pretrans, 領域dpx.X, 領域dpx.Y + 240f, node.難易度[ 1 ].label, node.難易度[ 1 ].level, 白ブラシ, ADVANCED色ブラシ, 黒ブラシ );

						// BASIC 相当
						this._難易度パネルを１つ描画する( dc, pretrans, 領域dpx.X, 領域dpx.Y + 360f, node.難易度[ 0 ].label, node.難易度[ 0 ].level, 白ブラシ, BASIC色ブラシ, 黒ブラシ );
					}
				}

			} );

			// 選択枠を描画する。
			if( 表示可能ノードである )
			{
				var 青い線 = this.青い線を取得する();
				if( null != 青い線 )
				{
					var 領域dpx = new RectangleF( 642f + 10f, 529f + 10f + ( 3 - 選択している難易度 ) * 120f, 338f - 20f, 130f );
					var 太さdpx = 青い線.太さdpx;

					青い線.描画する( gd, new Vector2( 領域dpx.Left - 太さdpx / 4f, 領域dpx.Top ), 幅dpx: 領域dpx.Width + 太さdpx / 2f );      // 上辺
					青い線.描画する( gd, new Vector2( 領域dpx.Left, 領域dpx.Top - 太さdpx / 4f ), 高さdpx: 領域dpx.Height + 太さdpx / 2f );        // 左辺
					青い線.描画する( gd, new Vector2( 領域dpx.Left - 太さdpx / 4f, 領域dpx.Bottom ), 幅dpx: 領域dpx.Width + 太さdpx / 2f );       // 下辺
					青い線.描画する( gd, new Vector2( 領域dpx.Right, 領域dpx.Top - 太さdpx / 4f ), 高さdpx: 領域dpx.Height + 太さdpx / 2f );   // 右辺
				}
			}
		}

		private 画像フォント _数字画像 = null;
		private Node _現在表示しているノード = null;
		private TextFormat _見出し用TextFormat = null;

		private void _難易度パネルを１つ描画する( DeviceContext dc, Matrix3x2 pretrans, float 基点X, float 基点Y, string 難易度ラベル, float 難易度値, Brush 文字ブラシ, Brush 見出し背景ブラシ, Brush 数値背景ブラシ )
		{
			dc.Transform = pretrans;

			dc.FillRectangle( new RectangleF( 基点X + 156f, 基点Y + 29f, 157f, 24f ), 見出し背景ブラシ );
			dc.FillRectangle( new RectangleF( 基点X + 156f, 基点Y + 53f, 157f, 78f ), 数値背景ブラシ );

			this._見出し用TextFormat.TextAlignment = TextAlignment.Trailing;
			dc.DrawText( 難易度ラベル, this._見出し用TextFormat, new RectangleF( 基点X + 156f + 4f, 基点Y + 29f, 157f - 8f, 24f ), 文字ブラシ );

			if( 難易度ラベル.Nullでも空でもない() && 0.00 != 難易度値 )
			{
				var 難易度値文字列 = 難易度値.ToString( "0.00" ).PadLeft( 1 );	// 整数部は２桁を保証（１桁なら十の位は空白文字）

				// 小数部を描画する
				dc.Transform =
					Matrix3x2.Scaling( 0.5f, 0.5f ) *
					Matrix3x2.Translation( 基点X + 240f, 基点Y + 73f ) *
					pretrans;
				this._数字画像.描画する( dc, 0f, 0f, 難易度値文字列.Substring( 2 ) );

				// 整数部を描画する（'.'含む）
				dc.Transform =
					Matrix3x2.Scaling( 0.75f, 0.75f ) *
					Matrix3x2.Translation( 基点X + 176f, 基点Y + 53f ) *
					pretrans;
				this._数字画像.描画する( dc, 0f, 0f, 難易度値文字列.Substring( 0, 2 ) );
			}
		}
	}
}
