using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SharpDX;
using FDK;
using FDK.メディア;
using DTXmatixx.曲;
using DTXmatixx.設定;

namespace DTXmatixx.ステージ.選曲
{
	class BPMパネル : Activity
	{
		public BPMパネル()
		{
			this.子リスト.Add( this._BPMパネル = new 画像( @"$(System)images\選曲画面_BPMパネル.png" ) );
			this.子リスト.Add( this._パラメータ文字 = new 画像フォント( @"$(System)images\パラメータ文字_小.png", @"$(System)images\パラメータ文字_小矩形.xml", 文字幅補正dpx: 0f ) );
		}

		protected override void On活性化( グラフィックデバイス gd )
		{
		}
		protected override void On非活性化( グラフィックデバイス gd )
		{
		}

		public void 描画する( グラフィックデバイス gd )
		{
			var 領域 = new RectangleF( 78f, 455f, 357f, 55f );

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
						this._最小BPM = song.MinBPM ?? 120.0;
						this._最大BPM = song.MaxBPM ?? 120.0;
					}
				}
				else
				{
					this._最小BPM = 120.0;
					this._最大BPM = 120.0;
				}
			}
			//----------------
			#endregion

			this._BPMパネル.描画する( gd, 領域.X - 5f, 領域.Y - 4f );

			if( 10.0 >= Math.Abs( this._最大BPM - this._最小BPM ) )
			{
				// (A) 「最小値」だけ描画。差が10以下なら同一値とみなす。
				this._パラメータ文字.描画する( gd, 領域.X + 120f, 領域.Y, this._最小BPM.ToString( "0" ).PadLeft( 3 ) );
			}
			else
			{
				// (B) 「最小～最大」を描画。
				this._パラメータ文字.描画する( gd, 領域.X + 80f, 領域.Y, this._最小BPM.ToString( "0" ) + "~" + this._最大BPM.ToString("0") );
			}
		}

		private 画像 _BPMパネル = null;
		private 画像フォント _パラメータ文字 = null;

		private Node _現在表示しているノード = null;
		private double _最小BPM;
		private double _最大BPM;
	}
}
