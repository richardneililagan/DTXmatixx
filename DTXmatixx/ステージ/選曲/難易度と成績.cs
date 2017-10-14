using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SharpDX;
using SharpDX.Direct2D1;
using FDK;
using FDK.メディア;
using DTXmatixx.曲;
using DTXmatixx.設定;
using DTXmatixx.設定.DB;

namespace DTXmatixx.ステージ.選曲
{
	class 難易度と成績 : Activity
	{
		public 難易度と成績()
		{
			this.子リスト.Add( this._数字画像 = new 画像フォント( @"$(System)images\パラメータ文字_大.png", @"$(System)images\パラメータ文字_大矩形.xml", 文字幅補正dpx: 0f ) );
		}

		protected override void On活性化( グラフィックデバイス gd )
		{
			using( Log.Block( FDKUtilities.現在のメソッド名 ) )
			{
				this._現在表示しているノード = null;
				this._Level_MASTER = "";
				//this._Level_EXTREME = "";
				//this._Level_ADVANCED = "";
				//this._Level_BASIC = "";
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
			#region " ノードが変更されていたら情報を更新する。"
			//----------------
			if( App.曲ツリー.フォーカスノード != this._現在表示しているノード )
			{
				this._現在表示しているノード = App.曲ツリー.フォーカスノード;

				if( this._現在表示しているノード is MusicNode musicNode )
				{
					var song = 曲DB.曲を取得する( musicNode.曲ファイルパス );

					if( null != song )
					{
						this._Level_MASTER = song.Level.ToString( "0.00" ); // todo: 今のところ MASTER だけ。
						//this._Level_EXTREME = "";
						//this._Level_ADVANCED = "";
						//this._Level_BASIC = "";
					}
				}
				else
				{
					this._Level_MASTER = "";
					//this._Level_EXTREME = "";
					//this._Level_ADVANCED = "";
					//this._Level_BASIC = "";
				}
			}
			//----------------
			#endregion

			gd.D2DBatchDraw( ( dc ) => {

				var 領域dpx = new RectangleF( 642f, 529f, 338f, 508f );

				using( var 黒ブラシ = new SolidColorBrush( dc, Color4.Black ) )
				using( var 黒透過ブラシ = new SolidColorBrush( dc, new Color4( Color3.Black, 0.5f ) ) )
				using( var MASTER色ブラシ = new SolidColorBrush( dc, new Color4( 0xfffe55c6 ) ) )
				using( var EXTREME色ブラシ = new SolidColorBrush( dc, new Color4( 0xff7d5cfe ) ) )
				using( var ADVANCED色ブラシ = new SolidColorBrush( dc, new Color4( 0xff00aaeb ) ) )
				using( var BASIC色ブラシ = new SolidColorBrush( dc, new Color4( 0xfffe9551 ) ) )
				{
					// 背景
					dc.Transform = Matrix3x2.Identity;
					dc.FillRectangle( 領域dpx, 黒透過ブラシ );

					// MASTER
					if( this._Level_MASTER.Nullでも空でもない() )
					{
						dc.FillRectangle( new RectangleF( 領域dpx.X + 156f, 領域dpx.Y + 29f, 157f, 24f ), MASTER色ブラシ );
						dc.FillRectangle( new RectangleF( 領域dpx.X + 156f, 領域dpx.Y + 53f, 157f, 78f ), 黒ブラシ );

						// 小数部を描画する
						dc.Transform =
							Matrix3x2.Scaling( 0.5f, 0.5f ) *
							Matrix3x2.Translation( 領域dpx.X + 240f, 領域dpx.Y + 73f );
						this._数字画像.描画する( dc, 0f, 0f, this._Level_MASTER.Substring( 2 ) );

						// 整数部を描画する（'.'含む）
						dc.Transform =
							Matrix3x2.Scaling( 0.75f, 0.75f ) *
							Matrix3x2.Translation( 領域dpx.X + 176f, 領域dpx.Y + 53f );
						this._数字画像.描画する( dc, 0f, 0f, this._Level_MASTER.Substring( 0, 2 ) );
					}

					// EXTREME
					dc.Transform = Matrix3x2.Identity;
					dc.FillRectangle( new RectangleF( 領域dpx.X + 156f, 領域dpx.Y + 149f, 157f, 24f ), EXTREME色ブラシ );
					dc.FillRectangle( new RectangleF( 領域dpx.X + 156f, 領域dpx.Y + 173f, 157f, 78f ), 黒ブラシ );

					// ADVANCED
					dc.Transform = Matrix3x2.Identity;
					dc.FillRectangle( new RectangleF( 領域dpx.X + 156f, 領域dpx.Y + 269f, 157f, 24f ), ADVANCED色ブラシ );
					dc.FillRectangle( new RectangleF( 領域dpx.X + 156f, 領域dpx.Y + 293f, 157f, 78f ), 黒ブラシ );

					// BASIC
					dc.Transform = Matrix3x2.Identity;
					dc.FillRectangle( new RectangleF( 領域dpx.X + 156f, 領域dpx.Y + 389f, 157f, 24f ), BASIC色ブラシ );
					dc.FillRectangle( new RectangleF( 領域dpx.X + 156f, 領域dpx.Y + 413f, 157f, 78f ), 黒ブラシ );
				}

			} );
		}

		private 画像フォント _数字画像 = null;
		private Node _現在表示しているノード = null;
		private string _Level_MASTER;
		//private string _Level_EXTREME;
		//private string _Level_ADVANCED;
		//private string _Level_BASIC;
	}
}
