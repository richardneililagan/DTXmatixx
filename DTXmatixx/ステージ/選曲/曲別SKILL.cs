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

namespace DTXmatixx.ステージ.選曲
{
	class 曲別SKILL : Activity
	{
		public 曲別SKILL()
		{
			this.子リスト.Add( this._数字画像 = new 画像フォント( @"$(System)images\パラメータ文字_大太斜.png", @"$(System)images\パラメータ文字_大太斜矩形.xml", 文字幅補正dpx: 0f ) );
			this.子リスト.Add( this._ロゴ画像 = new 画像( @"$(System)images\曲別SKILLアイコン2.png" ) );
		}

		protected override void On活性化( グラフィックデバイス gd )
		{
			using( Log.Block( FDKUtilities.現在のメソッド名 ) )
			{
				this._現在表示しているノード = null;
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
			var 描画領域 = new RectangleF( 10f, 340f, 275f, 98f );

			#region " ノードが変更されていたら情報を更新する。"
			//----------------
			if( App.曲ツリー.フォーカス曲ノード != this._現在表示しているノード )
			{
				this._現在表示しているノード = App.曲ツリー.フォーカス曲ノード; // MusicNode 以外は null が返される

				this._スキル値文字列 = null;
				var record = 曲DB.ユーザと曲ファイルのハッシュから成績を取得する( App.ユーザ設定.Id, this._現在表示しているノード.曲ファイルハッシュ );
				if( null != record )
				{
					this._スキル値文字列 = record.Skill.ToString( "0.00" ).PadLeft( 6 );  // 右詰め、余白は' '。
				}
			}
			//----------------
			#endregion

			if( this._スキル値文字列.Nullまたは空である() )
				return;

			gd.D2DBatchDraw( ( dc ) => {

				var pretrans = dc.Transform;

				// 曲別SKILLアイコンを描画する
				dc.Transform =
					Matrix3x2.Scaling( 0.5f, 0.4f ) *
					Matrix3x2.Translation( 描画領域.X, 描画領域.Y + 10f ) *
					pretrans;
				this._ロゴ画像.描画する( dc, 0f, 0f );

				// 小数部を描画する
				dc.Transform =
					Matrix3x2.Scaling( 0.8f, 0.8f ) *
					Matrix3x2.Translation( 描画領域.X + 130f + 175f, 描画領域.Y + ( 描画領域.Height * 0.2f ) ) *
					pretrans;
				this._数字画像.描画する( dc, 0f, 0f, _スキル値文字列.Substring( 4 ) );

				// 整数部を描画する（'.'含む）
				dc.Transform =
					Matrix3x2.Scaling( 1f, 1.0f ) *
					Matrix3x2.Translation( 描画領域.X + 130f, 描画領域.Y ) *
					pretrans;
				this._数字画像.描画する( dc, 0f, 0f, _スキル値文字列.Substring( 0, 4 ) );

			} );
		}

		private 画像フォント _数字画像 = null;
		private 画像 _ロゴ画像 = null;

		private MusicNode _現在表示しているノード = null;
		private string _スキル値文字列 = null;
	}
}
