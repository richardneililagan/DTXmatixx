using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SharpDX;
using SharpDX.Animation;
using FDK;
using FDK.メディア;

namespace DTXmatixx.ステージ.演奏
{
	class チップ光 : Activity
	{
		public チップ光()
		{
			this.子リスト.Add( this._放射光 = new 画像( @"$(System)images\チップ光.png" ) { 加算合成 = true } );
		}

		protected override void On活性化( グラフィックデバイス gd )
		{
			using( Log.Block( FDKUtilities.現在のメソッド名 ) )
			{
				this._放射光の矩形リスト = new 矩形リスト( @"$(System)images\チップ光矩形.xml" );      // デバイスリソースは持たないので、子Activityではない。

				this._レーンtoステータス = new Dictionary<表示レーン種別, 表示レーンステータス>() {
					{ 表示レーン種別.Unknown, new 表示レーンステータス( 表示レーン種別.Unknown ) },
					{ 表示レーン種別.LeftCrash, new 表示レーンステータス( 表示レーン種別.LeftCrash ) },
					{ 表示レーン種別.HiHat, new 表示レーンステータス( 表示レーン種別.HiHat ) },
					{ 表示レーン種別.Foot, new 表示レーンステータス( 表示レーン種別.Foot ) },
					{ 表示レーン種別.Snare, new 表示レーンステータス( 表示レーン種別.Snare ) },
					{ 表示レーン種別.Bass, new 表示レーンステータス( 表示レーン種別.Bass ) },
					{ 表示レーン種別.Tom1, new 表示レーンステータス( 表示レーン種別.Tom1 ) },
					{ 表示レーン種別.Tom2, new 表示レーンステータス( 表示レーン種別.Tom2 ) },
					{ 表示レーン種別.Tom3, new 表示レーンステータス( 表示レーン種別.Tom3 ) },
					{ 表示レーン種別.RightCrash, new 表示レーンステータス( 表示レーン種別.RightCrash ) },
				};
			}
		}
		protected override void On非活性化( グラフィックデバイス gd )
		{
			using( Log.Block( FDKUtilities.現在のメソッド名 ) )
			{
				foreach( var kvp in this._レーンtoステータス )
					kvp.Value.Dispose();

				this._レーンtoステータス = null;
			}
		}

		public void 表示を開始する( 表示レーン種別 lane )
		{
			var status = this._レーンtoステータス[ lane ];

			status.現在の状態 = 表示レーンステータス.状態.表示開始;  // 描画スレッドへ通知。
		}
		public void 進行描画する( グラフィックデバイス gd )
		{
			foreach( 表示レーン種別 レーン in Enum.GetValues( typeof( 表示レーン種別 ) ) )
			{
				var status = this._レーンtoステータス[ レーン ];

				switch( status.現在の状態 )
				{
					case 表示レーンステータス.状態.表示開始:
						#region " 表示開始 "
						//----------------
						{
							status.アニメ用メンバを解放する();

							// 初期状態
							status.回転角 = new Variable( gd.Animation.Manager, initialValue: 0.0 );
							status.拡大率 = new Variable( gd.Animation.Manager, initialValue: 1.0 );
							status.ストーリーボード = new Storyboard( gd.Animation.Manager );

							double 期間sec;

							// シーン1. 回転しつつ縮小しつつ透明化
							期間sec = 0.1;
							using( var 回転角の遷移 = gd.Animation.TrasitionLibrary.Linear( duration: 期間sec, finalValue: 45.0 ) )
							using( var 拡大率の遷移1 = gd.Animation.TrasitionLibrary.Linear( duration: 期間sec/2.0, finalValue: 1.5 ) )
							using( var 拡大率の遷移2 = gd.Animation.TrasitionLibrary.Linear( duration: 期間sec/2.0, finalValue: 0.7 ) )
							{
								status.ストーリーボード.AddTransition( status.回転角, 回転角の遷移 );
								status.ストーリーボード.AddTransition( status.拡大率, 拡大率の遷移1 );
								status.ストーリーボード.AddTransition( status.拡大率, 拡大率の遷移2 );
							}

							// 開始
							status.ストーリーボード.Schedule( gd.Animation.Timer.Time );
							status.現在の状態 = 表示レーンステータス.状態.表示中;
						}
						//----------------
						#endregion
						break;

					case 表示レーンステータス.状態.表示中:
						#region " 表示中 "
						//----------------
						{
							var 転送元矩形dpx = (RectangleF) this._放射光の矩形リスト[ レーン.ToString() ].Value;
							var 転送元矩形の中心dpx = new Vector2( 転送元矩形dpx.Width / 2f, 転送元矩形dpx.Height / 2f );

							var 変換行列2D =
								Matrix3x2.Scaling( (float)status.拡大率.Value, (float) status.拡大率.Value, center: 転送元矩形の中心dpx ) *
								Matrix3x2.Rotation( MathUtil.DegreesToRadians( (float) status.回転角.Value ), center: 転送元矩形の中心dpx ) *
								Matrix3x2.Translation( status.表示中央位置dpx.X - 転送元矩形の中心dpx.X, status.表示中央位置dpx.Y - 転送元矩形の中心dpx.Y );

							this._放射光.描画する( gd, 変換行列2D, 転送元矩形: 転送元矩形dpx );
						}

						// 全部終わったら非表示へ。
						if( ( ( null == status.ストーリーボード ) || ( status.ストーリーボード.Status == StoryboardStatus.Ready ) ) )
						{
							status.現在の状態 = 表示レーンステータス.状態.非表示;
						}
						//----------------
						#endregion
						break;

					default:
						break;
				}
			}
		}

		private 画像 _放射光 = null;
		private 矩形リスト _放射光の矩形リスト = null;

		/// <summary>
		///		以下の画像のアニメ＆表示管理を行うクラス。
		///		・放射光
		///		・フレア（輪）
		/// </summary>
		private class 表示レーンステータス : IDisposable
		{
			public enum 状態
			{
				非表示,
				表示開始,   // 高速進行スレッドが設定
				表示中,     // 描画スレッドが設定
			}
			public 状態 現在の状態 = 状態.非表示;

			public readonly Vector2 表示中央位置dpx;

			public Variable 回転角 = null;
			public Variable 拡大率 = null;
			public Storyboard ストーリーボード = null;

			public 表示レーンステータス( 表示レーン種別 lane )
			{
				this.現在の状態 = 状態.非表示;

				// 表示中央位置は、レーンごとに固定。
				this.表示中央位置dpx = new Vector2(
					レーンフレーム.領域.Left + レーンフレーム.レーンto左端位置dpx[ lane ] + レーンフレーム.レーンtoレーン幅dpx[ lane ] / 2f,
					演奏ステージ.ヒット判定バーの中央Y座標dpx );
			}
			public void Dispose()
			{
				this.アニメ用メンバを解放する();
				this.現在の状態 = 状態.非表示;
			}
			public void アニメ用メンバを解放する()
			{
				this.ストーリーボード?.Abandon();
				FDKUtilities.解放する( ref this.ストーリーボード );
				FDKUtilities.解放する( ref this.回転角 );
				FDKUtilities.解放する( ref this.拡大率 );
			}
		}
		private Dictionary<表示レーン種別, 表示レーンステータス> _レーンtoステータス = null;
	}
}
