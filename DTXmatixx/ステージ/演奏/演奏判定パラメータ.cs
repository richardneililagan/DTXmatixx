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
	class 演奏判定パラメータ : Activity
	{
		public int MaxCombo
		{
			get;
			protected set;
		} = 0;

		public 演奏判定パラメータ()
		{
			this.子リスト.Add( this._パラメータ文字 = new 画像フォント( @"$(System)images\パラメータ文字.png", @"$(System)images\パラメータ文字矩形.xml" ) );
		}

		protected override void On活性化( グラフィックデバイス gd )
		{
			this._判定toヒット数 = new Dictionary<判定種別, int>();
			foreach( 判定種別 judge in Enum.GetValues( typeof( 判定種別 ) ) )
				this._判定toヒット数.Add( judge, 0 );

			this.MaxCombo = 0;
		}

		public int 判定toヒット数( 判定種別 judge )
		{
			return this._判定toヒット数[ judge ];
		}
		public void ヒット数を加算する( 判定種別 judge )
		{
			this._判定toヒット数[ judge ]++;

			if( judge == 判定種別.OK || judge == 判定種別.MISS )
			{
				this.MaxCombo = 0;  // コンボ切れ
			}
			else
			{
				this.MaxCombo++;
			}
		}

		public void 描画する( DeviceContext1 dc, float x, float y )
		{
			float h = 40f;
			this.パラメータを描画する( dc, x, y, this._判定toヒット数[ 判定種別.PERFECT ], 4 ); y += h;
			this.パラメータを描画する( dc, x, y, this._判定toヒット数[ 判定種別.GREAT ], 4 ); y += h;
			this.パラメータを描画する( dc, x, y, this._判定toヒット数[ 判定種別.GOOD ], 4 ); y += h;
			this.パラメータを描画する( dc, x, y, this._判定toヒット数[ 判定種別.OK ], 4 ); y += h;
			this.パラメータを描画する( dc, x, y, this._判定toヒット数[ 判定種別.MISS ], 4 ); y += h;
			y += 3f;    // ちょっと間を開けて
			this.パラメータを描画する( dc, x, y, this.MaxCombo, 4 );
		}

		private Dictionary<判定種別, int> _判定toヒット数 = null;
		private 画像フォント _パラメータ文字 = null;

		private void パラメータを描画する( DeviceContext1 描画先dc, float x, float y, int 描画する数値, int 桁数 )
		{
			Debug.Assert( 1 <= 桁数 && 10 >= 桁数 );    // 最大10桁まで

			int 最大値 = (int) Math.Pow( 10, 桁数 ) - 1;     // 1桁なら9, 2桁なら99, 3桁なら999, ... でカンスト。
			int 判定数 = Math.Max( Math.Min( 描画する数値, 最大値 ), 0 );   // 丸める。
			var 判定数文字列 = 判定数.ToString().PadLeft( 桁数 ).Replace( ' ', 'o' );	// グレーの '0' は 'o' で描画できる（矩形リスト参照）。

			this._パラメータ文字.描画する( 描画先dc, x, y, 判定数文字列 );
		}
	}
}
