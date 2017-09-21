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
	class 判定文字列 : Activity
	{
		public 判定文字列()
		{
			this.子リスト.Add( this._判定文字列画像 = new 画像( @"$(System)images\判定文字列.png" ) );
		}

		protected override void On活性化( グラフィックデバイス gd )
		{
			using( Log.Block( FDKUtilities.現在のメソッド名 ) )
			{
				this._判定文字列の矩形リスト = new 矩形リスト( @"$(System)images\判定文字列矩形.xml" );

				this._アニメカウンタ = new Dictionary<表示レーン種別, (判定種別 type, Counter counter)>();
				foreach( 表示レーン種別 レーン in Enum.GetValues( typeof( 表示レーン種別 ) ) )
					this._アニメカウンタ.Add( レーン, (判定種別.PERFECT, null) ); // 初期値 counter == null
			}
		}
		protected override void On非活性化( グラフィックデバイス gd )
		{
			using( Log.Block( FDKUtilities.現在のメソッド名 ) )
			{
			}
		}

		public void 開始( 表示レーン種別 lane, 判定種別 judge )
		{
			this._アニメカウンタ[ lane ] = (judge, new Counter( 0, 99, 2 ));
		}

		public void 進行描画する( グラフィックデバイス gd )
		{
			foreach( 表示レーン種別 レーン in Enum.GetValues( typeof( 表示レーン種別 ) ) )
			{
				var 状態 = this._アニメカウンタ[ レーン ];
				var 転送元矩形 = this._判定文字列の矩形リスト[ 状態.type.ToString() ];
				Debug.Assert( null != 転送元矩形 );

				// カウンタが無効であるか、アニメが修了しているならスキップ。
				if( null == 状態.counter || 状態.counter.終了値に達した )
				{
					this._アニメカウンタ[ レーン ] = (状態.type, null);
					continue;
				}

				this._判定文字列画像.描画する(
					gd,
					左位置: レーンフレーム.領域.Left + レーンフレーム.レーンto左端位置dpx[ レーン ] + レーンフレーム.レーンtoレーン幅dpx[ レーン ] / 2f - 転送元矩形.Value.Width / 2f,
					上位置: this._レーンto縦中央位置dpx[ レーン ] - 転送元矩形.Value.Height / 2f,
					転送元矩形: 転送元矩形 );
			}
		}

		private 画像 _判定文字列画像 = null;
		private 矩形リスト _判定文字列の矩形リスト = null;
		/// <summary>
		///		表示レーンごとの、判定文字列とそのアニメ用カウンタ。
		///		counter が null の場合、判定文字列は表示されない。
		/// </summary>
		private Dictionary<表示レーン種別, (判定種別 type, Counter counter)> _アニメカウンタ = null;

		private Dictionary<表示レーン種別, float> _レーンto縦中央位置dpx = new Dictionary<表示レーン種別, float>() {
			{ 表示レーン種別.Unknown, -100f },
			{ 表示レーン種別.LeftCrash, 530f },
			{ 表示レーン種別.HiHat, 597f },
			{ 表示レーン種別.Foot, 636f },
			{ 表示レーン種別.Snare, 597f },
			{ 表示レーン種別.Bass, 635f },
			{ 表示レーン種別.Tom1, 561f },
			{ 表示レーン種別.Tom2, 561f },
			{ 表示レーン種別.Tom3, 600f },
			{ 表示レーン種別.RightCrash, 533f },
		};
	}
}
