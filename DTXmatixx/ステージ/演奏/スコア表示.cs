using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SharpDX;
using SharpDX.Animation;
using SharpDX.Direct2D1;
using FDK;
using FDK.メディア;
using FDK.カウンタ;

namespace DTXmatixx.ステージ.演奏
{
	/// <summary>
	///		スコアの描画を行う。
	///		スコアの計算については、<see cref="成績"/> クラスにて実装する。
	/// </summary>
	class スコア表示 : Activity
	{
		public スコア表示()
		{
			this.子リスト.Add( this._スコア数字画像 = new 画像( @"$(System)images\スコア数字.png" ) );
		}

		protected override void On活性化( グラフィックデバイス gd )
		{
			using( Log.Block( FDKUtilities.現在のメソッド名 ) )
			{
				this._スコア数字画像の矩形リスト = new 矩形リスト( @"$(System)images\スコア数字矩形.xml" );

				// 表示用
				this._現在表示中のスコア = 0;
				this._前回表示したスコア = 0;
				this._前回表示した数字 = "        0";
				this._各桁のアニメ = new 各桁のアニメ[ 9 ];
				for( int i = 0; i < this._各桁のアニメ.Length; i++ )
					this._各桁のアニメ[ i ] = new 各桁のアニメ();

				// スコア計算用
				this._判定toヒット数 = new Dictionary<判定種別, int>();
				foreach( 判定種別 judge in Enum.GetValues( typeof( 判定種別 ) ) )
					this._判定toヒット数.Add( judge, 0 );
			}
		}
		protected override void On非活性化( グラフィックデバイス gd )
		{
			using( Log.Block( FDKUtilities.現在のメソッド名 ) )
			{
				for( int i = 0; i < this._各桁のアニメ.Length; i++ )
					this._各桁のアニメ[ i ].Dispose();
				this._各桁のアニメ = null;
			}
		}

		/// <param name="全体の中央位置">
		///		パネル(dc)の左上を原点とする座標。
		/// </param>
		public void 進行描画する( DeviceContext dc, アニメーション管理 am, Vector2 全体の中央位置, 成績 現在の成績 )
		{
			// 追っかけ
			if( this._現在表示中のスコア < 現在の成績.Score )
			{
				int 増分 = 現在の成績.Score - this._現在表示中のスコア;
				int 追っかけ分 = Math.Max( (int) ( 増分 * 0.75 ), 1 );	// VPS に依存するけどまあいい

				this._現在表示中のスコア = Math.Min( this._現在表示中のスコア + 追っかけ分, 現在の成績.Score );
			}

			int スコア値 = Math.Min( Math.Max( this._現在表示中のスコア, 0 ), 999999999 );  // プロパティには制限はないが、表示は999999999（9桁）でカンスト。

			string 数字 = スコア値.ToString().PadLeft( 9 ); // 右詰め9桁、余白は ' '。
			var 全体のサイズ = new Vector2( 62f * 9f, 99f );  // 固定とする

			// 1桁ずつ表示。

			var 文字間隔補正 = -10f;
			var 文字の位置 = new Vector2( -( 全体のサイズ.X / 2f ), 0f );

			for( int i = 0; i < 数字.Length; i++ )
			{
				// 前回の文字と違うなら、桁アニメーション開始。
				if( 数字[ i ] != this._前回表示した数字[ i ] )
				{
					this._各桁のアニメ[ i ].跳ね開始( am, 0.0 );
				}

				var 転送元矩形 = (RectangleF) this._スコア数字画像の矩形リスト[ 数字[ i ].ToString() ];

				dc.Transform =
					//Matrix3x2.Scaling( 画像矩形から表示矩形への拡大率 ) *
					Matrix3x2.Translation( 文字の位置.X, 文字の位置.Y + (float) ( this._各桁のアニメ[ i ].Yオフセット?.Value ?? 0.0f ) ) *
					//Matrix3x2.Scaling( 全体の拡大率.X, 全体の拡大率.Y, center: new Vector2( 0f, 全体のサイズ.Y / 2f ) ) *
					Matrix3x2.Translation( 全体の中央位置 );

				dc.DrawBitmap( this._スコア数字画像.Bitmap, 1f, BitmapInterpolationMode.Linear, 転送元矩形 );

				文字の位置.X += ( 転送元矩形.Width + 文字間隔補正 ) * 1f;// 画像矩形から表示矩形への拡大率.X;
			}

			// 更新。
			this._前回表示したスコア = this._現在表示中のスコア;
			this._前回表示した数字 = 数字;
		}

		/// <summary>
		///		<see cref="進行描画する(DeviceContext1, Vector2)"/> で更新される。
		/// </summary>
		private int _現在表示中のスコア = 0;
		/// <summary>
		///		<see cref="進行描画する(DeviceContext1, Vector2)"/> で更新される。
		/// </summary>
		private int _前回表示したスコア = 0;

		private 画像 _スコア数字画像 = null;
		private 矩形リスト _スコア数字画像の矩形リスト = null;

		private Dictionary<判定種別, int> _判定toヒット数 = null;

		private class 各桁のアニメ : IDisposable
		{
			public Storyboard ストーリーボード = null;
			public Variable Yオフセット = null;

			public 各桁のアニメ()
			{
			}
			public void Dispose()
			{
				this.ストーリーボード?.Abandon();
				this.ストーリーボード?.Dispose();
				this.Yオフセット?.Dispose();
			}
			public void 跳ね開始( アニメーション管理 am, double 遅延sec )
			{
				this.Dispose();

				this.ストーリーボード = new Storyboard( am.Manager );
				this.Yオフセット = new Variable( am.Manager, initialValue: 0.0 );

				var Yオフセットの遷移 = new List<Transition>() {
					am.TrasitionLibrary.Constant( 遅延sec ),
					am.TrasitionLibrary.Linear( 0.05, finalValue: -10.0 ),	// 上へ移動
					am.TrasitionLibrary.Linear( 0.05, finalValue: 0.0 ),	// 下へ戻る
				};
				for( int i = 0; i < Yオフセットの遷移.Count; i++ )
				{
					this.ストーリーボード.AddTransition( this.Yオフセット, Yオフセットの遷移[ i ] );
					Yオフセットの遷移[ i ].Dispose();
				}
				this.ストーリーボード.Schedule( am.Timer.Time );
			}
		};
		private 各桁のアニメ[] _各桁のアニメ = null;
		private string _前回表示した数字 = "        0";
	}
}
