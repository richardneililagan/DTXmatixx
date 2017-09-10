using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SharpDX;
using SharpDX.Animation;
using SharpDX.Direct2D1;
using FDK;
using FDK.メディア;

namespace DTXmatixx.画面遷移.ABC遷移
{
	class 回転幕 : Activity
	{
		public enum フェーズ
		{
			未定,
			クローズ,
			オープン,
			クローズ完了,
			オープン完了
		}
		public フェーズ 現在のフェーズ { get; protected set; }

		public 回転幕()
		{
			this.子リスト.Add( this._ロゴ = new 画像( @"$(System)images\タイトルロゴ.png" ) );
		}

		protected override void On活性化( グラフィックデバイス gd )
		{
			this.現在のフェーズ = フェーズ.未定;
		}

		protected override void On非活性化( グラフィックデバイス gd )
		{
			//FDKUtilities.解放する( ref this._ロゴ不透明度 );

			if( null != this._黒幕 )
			{
				foreach( var b in this._黒幕 )
					b.Dispose();
				this._黒幕 = null;
			}
			this._ロゴ不透明度?.Dispose();
			this._ロゴ不透明度 = null;
		}

		public void クローズする( グラフィックデバイス gd, float 速度倍率 = 1.0f )
		{
			this.現在のフェーズ = フェーズ.クローズ;

			if( null != this._黒幕 )
			{
				foreach( var b in this._黒幕 )
					b.Dispose();
			}

			this._黒幕 = new 黒幕[ 2 ] {
				// 上＆左
				new 黒幕() {
					中心位置X = new Variable( gd.Animation.Manager, initialValue: 1920.0/2.0 ),	// クローズ初期位置、以下同
					中心位置Y = new Variable( gd.Animation.Manager, initialValue: 0.0-500.0 ),
					回転角rad = new Variable( gd.Animation.Manager, initialValue: 0.0 ),
					太さ = new Variable( gd.Animation.Manager, initialValue: 1000.0 ),
					不透明度 = new Variable( gd.Animation.Manager, initialValue: 1.0 ),
					ストーリーボード = new Storyboard( gd.Animation.Manager ),
				},
				// 下＆右
				new 黒幕() {
					中心位置X = new Variable( gd.Animation.Manager, initialValue: 1920.0/2.0 ),
					中心位置Y = new Variable( gd.Animation.Manager, initialValue: 1080.0+500.0 ),
					回転角rad = new Variable( gd.Animation.Manager, initialValue: 0.0 ),
					太さ = new Variable( gd.Animation.Manager, initialValue: 1000.0 ),
					不透明度 = new Variable( gd.Animation.Manager, initialValue: 1.0 ),
					ストーリーボード = new Storyboard( gd.Animation.Manager ),
				},
			};

			this._ロゴ不透明度?.Dispose();
			this._ロゴ不透明度 = new Variable( gd.Animation.Manager, initialValue: 0.0 );

			double シーン1期間 = 0.3;
			double シーン2期間 = 0.4;
			double シーン3期間 = 0.2;

			#region " ストーリーボードの構築(1) 上→左の黒幕 + ロゴ "
			//----------------
			var 幕 = this._黒幕[ 0 ];

			// シーン1 細くなりつつ画面中央へ移動。
			using( var 中心位置Xの遷移 = gd.Animation.TrasitionLibrary.Constant( duration: シーン1期間 / 速度倍率 ) )
			using( var 中心位置Yの遷移 = gd.Animation.TrasitionLibrary.AccelerateDecelerate( duration: シーン1期間 / 速度倍率, finalValue: 1080.0 / 2.0, accelerationRatio: 0.1, decelerationRatio: 0.9 ) )
			using( var 回転radの遷移 = gd.Animation.TrasitionLibrary.Constant( duration: ( シーン1期間 - 0.1 ) / 速度倍率 ) )
			using( var 太さの遷移 = gd.Animation.TrasitionLibrary.AccelerateDecelerate( duration: シーン1期間 / 速度倍率, finalValue: 100.0, accelerationRatio: 0.1, decelerationRatio: 0.9 ) )
			using( var 不透明度の遷移1 = gd.Animation.TrasitionLibrary.AccelerateDecelerate( duration: ( シーン1期間 * 0.75 ) / 速度倍率, finalValue: 0.9, accelerationRatio: 0.1, decelerationRatio: 0.9 ) )
			using( var 不透明度の遷移2 = gd.Animation.TrasitionLibrary.AccelerateDecelerate( duration: ( シーン1期間 * 0.25 ) / 速度倍率, finalValue: 0.5, accelerationRatio: 0.1, decelerationRatio: 0.9 ) )
			using( var ロゴの不透明度の遷移 = gd.Animation.TrasitionLibrary.Discrete( delay: ( シーン1期間 + シーン2期間 + シーン3期間 * 0.24 ) / 速度倍率, finalValue: 1.0, hold: ( シーン3期間 ) / 速度倍率 ) )
			{
				幕.ストーリーボード.AddTransition( 幕.中心位置X, 中心位置Xの遷移 );
				幕.ストーリーボード.AddTransition( 幕.中心位置Y, 中心位置Yの遷移 );
				幕.ストーリーボード.AddTransition( 幕.回転角rad, 回転radの遷移 );
				幕.ストーリーボード.AddTransition( 幕.太さ, 太さの遷移 );
				幕.ストーリーボード.AddTransition( 幕.不透明度, 不透明度の遷移1 );
				幕.ストーリーボード.AddTransition( 幕.不透明度, 不透明度の遷移2 );
				幕.ストーリーボード.AddTransition( this._ロゴ不透明度, ロゴの不透明度の遷移 );	// 便乗
			}

			// シーン2 270°回転。
			using( var 中心位置Xの遷移 = gd.Animation.TrasitionLibrary.Constant( duration: ( シーン2期間 - 0.18 ) / 速度倍率 ) )
			using( var 中心位置Yの遷移 = gd.Animation.TrasitionLibrary.Constant( duration: ( シーン2期間 - 0.18 ) / 速度倍率 ) )
			using( var 回転radの遷移 = gd.Animation.TrasitionLibrary.AccelerateDecelerate( duration: シーン2期間 / 速度倍率, finalValue: Math.PI * 1.75, accelerationRatio: 0.5, decelerationRatio: 0.5 ) )
			using( var 太さの遷移 = gd.Animation.TrasitionLibrary.Constant( duration: ( シーン2期間 - 0.18 ) / 速度倍率 ) )
			using( var 不透明度の遷移 = gd.Animation.TrasitionLibrary.Constant( duration: ( シーン2期間 - 0.18 ) / 速度倍率 ) )
			{
				幕.ストーリーボード.AddTransition( 幕.中心位置X, 中心位置Xの遷移 );
				幕.ストーリーボード.AddTransition( 幕.中心位置Y, 中心位置Yの遷移 );
				幕.ストーリーボード.AddTransition( 幕.回転角rad, 回転radの遷移 );
				幕.ストーリーボード.AddTransition( 幕.太さ, 太さの遷移 );
				幕.ストーリーボード.AddTransition( 幕.不透明度, 不透明度の遷移 );
			}

			// シーン3 太くなりつつ画面左へ移動。
			using( var 中心位置Xの遷移 = gd.Animation.TrasitionLibrary.AccelerateDecelerate( duration: シーン3期間 / 速度倍率, finalValue: 0.0 - 200.0, accelerationRatio: 0.1, decelerationRatio: 0.9 ) )
			using( var 中心位置Yの遷移 = gd.Animation.TrasitionLibrary.Constant( duration: シーン3期間 / 速度倍率 ) )
			using( var 回転radの遷移 = gd.Animation.TrasitionLibrary.Constant( duration: シーン3期間 / 速度倍率 ) )
			using( var 太さの遷移 = gd.Animation.TrasitionLibrary.AccelerateDecelerate( duration: ( シーン3期間 + 0.05 ) / 速度倍率, finalValue: 800.0, accelerationRatio: 0.1, decelerationRatio: 0.9 ) )
			using( var 不透明度の遷移 = gd.Animation.TrasitionLibrary.AccelerateDecelerate( duration: ( シーン3期間 * 0.25 ) / 速度倍率, finalValue: 1.0, accelerationRatio: 0.1, decelerationRatio: 0.9 ) )
			{
				幕.ストーリーボード.AddTransition( 幕.中心位置X, 中心位置Xの遷移 );
				幕.ストーリーボード.AddTransition( 幕.中心位置Y, 中心位置Yの遷移 );
				幕.ストーリーボード.AddTransition( 幕.回転角rad, 回転radの遷移 );
				幕.ストーリーボード.AddTransition( 幕.太さ, 太さの遷移 );
				幕.ストーリーボード.AddTransition( 幕.不透明度, 不透明度の遷移 );
			}
			//----------------
			#endregion

			#region " ストーリーボードの構築(2) 下→右の黒幕 "
			//----------------
			幕 = this._黒幕[ 1 ];

			double ずれ = 0.03;

			// シーン1 細くなりつつ画面中央へ移動。
			double 期間 = シーン1期間 - ずれ;
			using( var 中心位置Xの遷移 = gd.Animation.TrasitionLibrary.Constant( duration: 期間 / 速度倍率 ) )
			using( var 中心位置Yの遷移 = gd.Animation.TrasitionLibrary.AccelerateDecelerate( duration: 期間 / 速度倍率, finalValue: 1080.0 / 2.0, accelerationRatio: 0.1, decelerationRatio: 0.9 ) )
			using( var 回転radの遷移 = gd.Animation.TrasitionLibrary.Constant( duration: ( 期間 - 0.1 ) / 速度倍率 ) )
			using( var 太さの遷移 = gd.Animation.TrasitionLibrary.AccelerateDecelerate( duration: 期間 / 速度倍率, finalValue: 100.0, accelerationRatio: 0.1, decelerationRatio: 0.9 ) )
			using( var 不透明度の遷移1 = gd.Animation.TrasitionLibrary.AccelerateDecelerate( duration: ( 期間 * 0.75 ) / 速度倍率, finalValue: 0.9, accelerationRatio: 0.1, decelerationRatio: 0.9 ) )
			using( var 不透明度の遷移2 = gd.Animation.TrasitionLibrary.AccelerateDecelerate( duration: ( 期間 * 0.25 ) / 速度倍率, finalValue: 0.5, accelerationRatio: 0.1, decelerationRatio: 0.9 ) )
			{
				幕.ストーリーボード.AddTransition( 幕.中心位置X, 中心位置Xの遷移 );
				幕.ストーリーボード.AddTransition( 幕.中心位置Y, 中心位置Yの遷移 );
				幕.ストーリーボード.AddTransition( 幕.回転角rad, 回転radの遷移 );
				幕.ストーリーボード.AddTransition( 幕.太さ, 太さの遷移 );
				幕.ストーリーボード.AddTransition( 幕.不透明度, 不透明度の遷移1 );
				幕.ストーリーボード.AddTransition( 幕.不透明度, 不透明度の遷移2 );
			}

			// シーン2 270°回転。
			期間 = シーン2期間 + ずれ;
			using( var 中心位置Xの遷移 = gd.Animation.TrasitionLibrary.Constant( duration: ( 期間 - 0.18 ) / 速度倍率 ) )
			using( var 中心位置Yの遷移 = gd.Animation.TrasitionLibrary.Constant( duration: ( 期間 - 0.18 ) / 速度倍率 ) )
			using( var 回転radの遷移 = gd.Animation.TrasitionLibrary.AccelerateDecelerate( duration: 期間 / 速度倍率, finalValue: Math.PI * 1.75, accelerationRatio: 0.5, decelerationRatio: 0.5 ) )
			using( var 太さの遷移 = gd.Animation.TrasitionLibrary.Constant( duration: ( 期間 - 0.18 ) / 速度倍率 ) )
			using( var 不透明度の遷移 = gd.Animation.TrasitionLibrary.Constant( duration: ( 期間 - 0.18 ) / 速度倍率 ) )
			{
				幕.ストーリーボード.AddTransition( 幕.中心位置X, 中心位置Xの遷移 );
				幕.ストーリーボード.AddTransition( 幕.中心位置Y, 中心位置Yの遷移 );
				幕.ストーリーボード.AddTransition( 幕.回転角rad, 回転radの遷移 );
				幕.ストーリーボード.AddTransition( 幕.太さ, 太さの遷移 );
				幕.ストーリーボード.AddTransition( 幕.不透明度, 不透明度の遷移 );
			}

			// シーン3 太くなりつつ画面右へ移動。
			using( var 中心位置Xの遷移 = gd.Animation.TrasitionLibrary.AccelerateDecelerate( duration: シーン3期間 / 速度倍率, finalValue: 1920.0 + 200.0, accelerationRatio: 0.1, decelerationRatio: 0.9 ) )
			using( var 中心位置Yの遷移 = gd.Animation.TrasitionLibrary.Constant( duration: シーン3期間 / 速度倍率 ) )
			using( var 回転radの遷移 = gd.Animation.TrasitionLibrary.Constant( duration: シーン3期間 / 速度倍率 ) )
			using( var 太さの遷移 = gd.Animation.TrasitionLibrary.AccelerateDecelerate( duration: ( シーン3期間 + 0.05 ) / 速度倍率, finalValue: 800.0, accelerationRatio: 0.1, decelerationRatio: 0.9 ) )
			using( var 不透明度の遷移 = gd.Animation.TrasitionLibrary.AccelerateDecelerate( duration: ( シーン3期間 * 0.25 ) / 速度倍率, finalValue: 1.0, accelerationRatio: 0.1, decelerationRatio: 0.9 ) )
			{
				幕.ストーリーボード.AddTransition( 幕.中心位置X, 中心位置Xの遷移 );
				幕.ストーリーボード.AddTransition( 幕.中心位置Y, 中心位置Yの遷移 );
				幕.ストーリーボード.AddTransition( 幕.回転角rad, 回転radの遷移 );
				幕.ストーリーボード.AddTransition( 幕.太さ, 太さの遷移 );
				幕.ストーリーボード.AddTransition( 幕.不透明度, 不透明度の遷移 );
			}
			//----------------
			#endregion

			// アニメ開始
			var start = gd.Animation.Timer.Time;
			foreach( var bs in this._黒幕 )
				bs.ストーリーボード.Schedule( start );
		}

		public void オープンする( グラフィックデバイス gd, float 速度倍率 = 1.0f )
		{
			this.現在のフェーズ = フェーズ.オープン;

			if( null != this._黒幕 )
			{
				foreach( var b in this._黒幕 )
					b.Dispose();
			}

			this._黒幕 = new 黒幕[ 2 ] {
				// 上＆左
				new 黒幕() {
					中心位置X = new Variable( gd.Animation.Manager, initialValue: 0.0 - 200.0 ),	// オープン初期位置、以下同
					中心位置Y = new Variable( gd.Animation.Manager, initialValue: 1080.0 / 2.0 ),
					回転角rad = new Variable( gd.Animation.Manager, initialValue: Math.PI * 1.75 ),
					太さ = new Variable( gd.Animation.Manager, initialValue: 800.0 ),
					不透明度 = new Variable( gd.Animation.Manager, initialValue: 1.0 ),
					ストーリーボード = new Storyboard( gd.Animation.Manager ),
				},
				// 下＆右
				new 黒幕() {
					中心位置X = new Variable( gd.Animation.Manager, initialValue: 1920.0 + 200.0 ),
					中心位置Y = new Variable( gd.Animation.Manager, initialValue: 1080.0 / 2.0 ),
					回転角rad = new Variable( gd.Animation.Manager, initialValue: Math.PI * 1.75 ),
					太さ = new Variable( gd.Animation.Manager, initialValue: 800.0 ),
					不透明度 = new Variable( gd.Animation.Manager, initialValue: 1.0 ),
					ストーリーボード = new Storyboard( gd.Animation.Manager ),
				},
			};

			this._ロゴ不透明度?.Dispose();
			this._ロゴ不透明度 = new Variable( gd.Animation.Manager, initialValue: 1.0 );

			double シーン3期間 = 0.2;
			double シーン2期間 = 0.4;
			double シーン1期間 = 0.3;

			#region " ストーリーボードの構築(1) 上＆左の黒幕 + ロゴ "
			//----------------
			var 幕 = this._黒幕[ 0 ];

			// シーン3 細くなりつつ画面中央へ移動。
			using( var 中心位置Xの遷移 = gd.Animation.TrasitionLibrary.AccelerateDecelerate( duration: シーン3期間 / 速度倍率, finalValue: 1920.0 / 2.0, accelerationRatio: 0.9, decelerationRatio: 0.1 ) )
			using( var 中心位置Yの遷移 = gd.Animation.TrasitionLibrary.Constant( duration: シーン3期間 / 速度倍率 ) )
			using( var 回転radの遷移 = gd.Animation.TrasitionLibrary.Constant( duration: ( シーン3期間 - 0.08 ) / 速度倍率 ) )
			using( var 太さの遷移 = gd.Animation.TrasitionLibrary.AccelerateDecelerate( duration: シーン3期間 / 速度倍率, finalValue: 100.0, accelerationRatio: 0.9, decelerationRatio: 0.1 ) )
			using( var 不透明度の遷移1 = gd.Animation.TrasitionLibrary.AccelerateDecelerate( duration: ( シーン3期間 * 0.75 ) / 速度倍率, finalValue: 0.9, accelerationRatio: 0.9, decelerationRatio: 0.1 ) )
			using( var 不透明度の遷移2 = gd.Animation.TrasitionLibrary.AccelerateDecelerate( duration: ( シーン3期間 * 0.25 ) / 速度倍率, finalValue: 0.5, accelerationRatio: 0.9, decelerationRatio: 0.1 ) )
			{
				幕.ストーリーボード.AddTransition( 幕.中心位置X, 中心位置Xの遷移 );
				幕.ストーリーボード.AddTransition( 幕.中心位置Y, 中心位置Yの遷移 );
				幕.ストーリーボード.AddTransition( 幕.回転角rad, 回転radの遷移 );
				幕.ストーリーボード.AddTransition( 幕.太さ, 太さの遷移 );
				幕.ストーリーボード.AddTransition( 幕.不透明度, 不透明度の遷移1 );
				幕.ストーリーボード.AddTransition( 幕.不透明度, 不透明度の遷移2 );
			}

			// シーン2 -270°回転。
			using( var 中心位置Xの遷移 = gd.Animation.TrasitionLibrary.Constant( duration: シーン2期間 / 速度倍率 ) )
			using( var 中心位置Yの遷移 = gd.Animation.TrasitionLibrary.Constant( duration: ( シーン2期間 - 0.18 ) / 速度倍率 ) )
			using( var 回転radの遷移 = gd.Animation.TrasitionLibrary.AccelerateDecelerate( duration: シーン2期間 / 速度倍率, finalValue: 0.0, accelerationRatio: 0.5, decelerationRatio: 0.5 ) )
			using( var 太さの遷移 = gd.Animation.TrasitionLibrary.Constant( duration: ( シーン2期間 - 0.18 ) / 速度倍率 ) )
			using( var 不透明度の遷移 = gd.Animation.TrasitionLibrary.Constant( duration: ( シーン2期間 - 0.18 ) / 速度倍率 ) )
			{
				幕.ストーリーボード.AddTransition( 幕.中心位置X, 中心位置Xの遷移 );
				幕.ストーリーボード.AddTransition( 幕.中心位置Y, 中心位置Yの遷移 );
				幕.ストーリーボード.AddTransition( 幕.回転角rad, 回転radの遷移 );
				幕.ストーリーボード.AddTransition( 幕.太さ, 太さの遷移 );
				幕.ストーリーボード.AddTransition( 幕.不透明度, 不透明度の遷移 );
			}

			// シーン1 太くなりつつ画面上方へ移動。
			using( var 中心位置Xの遷移 = gd.Animation.TrasitionLibrary.Constant( duration: シーン1期間 / 速度倍率 ) )
			using( var 中心位置Yの遷移 = gd.Animation.TrasitionLibrary.AccelerateDecelerate( duration: シーン1期間 / 速度倍率, finalValue: 0.0 - 500.0, accelerationRatio: 0.9, decelerationRatio: 0.1 ) )
			using( var 回転radの遷移 = gd.Animation.TrasitionLibrary.Constant( duration: シーン1期間 / 速度倍率 ) )
			using( var 太さの遷移 = gd.Animation.TrasitionLibrary.AccelerateDecelerate( duration: シーン1期間 / 速度倍率, finalValue: 1000.0, accelerationRatio: 0.9, decelerationRatio: 0.1 ) )
			using( var 不透明度の遷移 = gd.Animation.TrasitionLibrary.AccelerateDecelerate( duration: ( シーン1期間 * 0.25 ) / 速度倍率, finalValue: 1.0, accelerationRatio: 0.9, decelerationRatio: 0.1 ) )
			using( var ロゴの不透明度の遷移 = gd.Animation.TrasitionLibrary.Discrete( delay: ( シーン3期間 * ( 1.0 - 0.24 ) ) / 速度倍率, finalValue: 0.0, hold: ( シーン3期間 ) / 速度倍率 ) )
			{
				幕.ストーリーボード.AddTransition( 幕.中心位置X, 中心位置Xの遷移 );
				幕.ストーリーボード.AddTransition( 幕.中心位置Y, 中心位置Yの遷移 );
				幕.ストーリーボード.AddTransition( 幕.回転角rad, 回転radの遷移 );
				幕.ストーリーボード.AddTransition( 幕.太さ, 太さの遷移 );
				幕.ストーリーボード.AddTransition( 幕.不透明度, 不透明度の遷移 );
				幕.ストーリーボード.AddTransition( this._ロゴ不透明度, ロゴの不透明度の遷移 );   // 便乗
			}
			//----------------
			#endregion

			#region " ストーリーボードの構築(2) 下＆右の黒幕 "
			//----------------
			幕 = this._黒幕[ 1 ];

			double ずれ = 0.03;

			// シーン3 細くなりつつ画面中央へ移動。
			using( var 中心位置Xの遷移 = gd.Animation.TrasitionLibrary.AccelerateDecelerate( duration: シーン3期間 / 速度倍率, finalValue: 1920.0 / 2.0, accelerationRatio: 0.9, decelerationRatio: 0.1 ) )
			using( var 中心位置Yの遷移 = gd.Animation.TrasitionLibrary.Constant( duration: シーン3期間 / 速度倍率 ) )
			using( var 回転radの遷移 = gd.Animation.TrasitionLibrary.Constant( duration: ( シーン3期間 - 0.08 ) / 速度倍率 ) )
			using( var 太さの遷移 = gd.Animation.TrasitionLibrary.AccelerateDecelerate( duration: シーン3期間 / 速度倍率, finalValue: 100.0, accelerationRatio: 0.9, decelerationRatio: 0.1 ) )
			using( var 不透明度の遷移1 = gd.Animation.TrasitionLibrary.AccelerateDecelerate( duration: ( シーン3期間 * 0.75 ) / 速度倍率, finalValue: 0.9, accelerationRatio: 0.9, decelerationRatio: 0.1 ) )
			using( var 不透明度の遷移2 = gd.Animation.TrasitionLibrary.AccelerateDecelerate( duration: ( シーン3期間 * 0.25 ) / 速度倍率, finalValue: 0.5, accelerationRatio: 0.9, decelerationRatio: 0.1 ) )
			{
				幕.ストーリーボード.AddTransition( 幕.中心位置X, 中心位置Xの遷移 );
				幕.ストーリーボード.AddTransition( 幕.中心位置Y, 中心位置Yの遷移 );
				幕.ストーリーボード.AddTransition( 幕.回転角rad, 回転radの遷移 );
				幕.ストーリーボード.AddTransition( 幕.太さ, 太さの遷移 );
				幕.ストーリーボード.AddTransition( 幕.不透明度, 不透明度の遷移1 );
				幕.ストーリーボード.AddTransition( 幕.不透明度, 不透明度の遷移2 );
			}
			// シーン2 -270°回転。
			double 期間 = シーン2期間 + ずれ;
			using( var 中心位置Xの遷移 = gd.Animation.TrasitionLibrary.Constant( duration: 期間 / 速度倍率 ) )
			using( var 中心位置Yの遷移 = gd.Animation.TrasitionLibrary.Constant( duration: ( 期間 - 0.18 ) / 速度倍率 ) )
			using( var 回転radの遷移 = gd.Animation.TrasitionLibrary.AccelerateDecelerate( duration: 期間 / 速度倍率, finalValue: 0.0, accelerationRatio: 0.5, decelerationRatio: 0.5 ) )
			using( var 太さの遷移 = gd.Animation.TrasitionLibrary.Constant( duration: ( 期間 - 0.18 ) / 速度倍率 ) )
			using( var 不透明度の遷移 = gd.Animation.TrasitionLibrary.Constant( duration: ( 期間 - 0.18 ) / 速度倍率 ) )
			{
				幕.ストーリーボード.AddTransition( 幕.中心位置X, 中心位置Xの遷移 );
				幕.ストーリーボード.AddTransition( 幕.中心位置Y, 中心位置Yの遷移 );
				幕.ストーリーボード.AddTransition( 幕.回転角rad, 回転radの遷移 );
				幕.ストーリーボード.AddTransition( 幕.太さ, 太さの遷移 );
				幕.ストーリーボード.AddTransition( 幕.不透明度, 不透明度の遷移 );
			}

			// シーン1 太くなりつつ画面下方へ移動。
			期間 = シーン1期間 - ずれ;
			using( var 中心位置Xの遷移 = gd.Animation.TrasitionLibrary.Constant( duration: 期間 / 速度倍率 ) )
			using( var 中心位置Yの遷移 = gd.Animation.TrasitionLibrary.AccelerateDecelerate( duration: 期間 / 速度倍率, finalValue: 1080.0 + 500.0, accelerationRatio: 0.9, decelerationRatio: 0.1 ) )
			using( var 回転radの遷移 = gd.Animation.TrasitionLibrary.Constant( duration: 期間 / 速度倍率 ) )
			using( var 太さの遷移 = gd.Animation.TrasitionLibrary.AccelerateDecelerate( duration: 期間 / 速度倍率, finalValue: 1000.0, accelerationRatio: 0.9, decelerationRatio: 0.1 ) )
			using( var 不透明度の遷移 = gd.Animation.TrasitionLibrary.AccelerateDecelerate( duration: ( 期間 * 0.25 ) / 速度倍率, finalValue: 1.0, accelerationRatio: 0.9, decelerationRatio: 0.1 ) )
			{
				幕.ストーリーボード.AddTransition( 幕.中心位置X, 中心位置Xの遷移 );
				幕.ストーリーボード.AddTransition( 幕.中心位置Y, 中心位置Yの遷移 );
				幕.ストーリーボード.AddTransition( 幕.回転角rad, 回転radの遷移 );
				幕.ストーリーボード.AddTransition( 幕.太さ, 太さの遷移 );
				幕.ストーリーボード.AddTransition( 幕.不透明度, 不透明度の遷移 );
			}
			//----------------
			#endregion

			// アニメ開始
			var start = gd.Animation.Timer.Time;
			foreach( var bs in this._黒幕 )
				bs.ストーリーボード.Schedule( start );
		}

		public void 進行描画する( グラフィックデバイス gd )
		{
			switch( this.現在のフェーズ )
			{
				case フェーズ.未定:
					break;

				case フェーズ.クローズ:
				case フェーズ.クローズ完了:
					this.進行描画する( gd, StoryboardStatus.Scheduled );
					break;

				case フェーズ.オープン:
				case フェーズ.オープン完了:
					this.進行描画する( gd, StoryboardStatus.Ready );
					break;
			}
		}

		protected void 進行描画する( グラフィックデバイス gd, StoryboardStatus 描画しないStatus )
		{
			bool すべて完了 = true;

			this._ロゴ.描画する(
				gd,
				this._ロゴ表示領域.Left,
				this._ロゴ表示領域.Top,
				不透明度0to1: (float) this._ロゴ不透明度.Value,
				X方向拡大率: ( this._ロゴ表示領域.Width / this._ロゴ.サイズ.Width ),
				Y方向拡大率: ( this._ロゴ表示領域.Height / this._ロゴ.サイズ.Height ) );

			gd.D2DBatchDraw( ( dc ) => {

				var pretrans = dc.Transform;

				for( int i = 0; i < 2; i++ )
				{
					var context = this._黒幕[ i ];

					if( context.ストーリーボード.Status != StoryboardStatus.Ready )
						すべて完了 = false;

					if( context.ストーリーボード.Status == 描画しないStatus )
						continue;

					dc.Transform =
						Matrix3x2.Rotation( (float) context.回転角rad.Value )
						* Matrix3x2.Translation( (float) context.中心位置X.Value, (float) context.中心位置Y.Value )
						* pretrans;

					using( var brush = new SolidColorBrush( dc, new Color4( 0f, 0f, 0f, (float) context.不透明度.Value ) ) )
					{
						float w = 2800.0f;
						float h = (float) context.太さ.Value;
						var rc = new RectangleF( -w / 2f, -h / 2f, w, h );
						dc.FillRectangle( rc, brush );
					}
				}

			} );

			if( すべて完了 )
			{
				if( this.現在のフェーズ == フェーズ.クローズ )
					this.現在のフェーズ = フェーズ.クローズ完了;

				if( this.現在のフェーズ == フェーズ.オープン )
					this.現在のフェーズ = フェーズ.オープン完了;
			}
		}

		protected class 黒幕 : IDisposable
		{
			public Variable 中心位置X = null;
			public Variable 中心位置Y = null;
			public Variable 回転角rad = null;
			public Variable 太さ = null;
			public Variable 不透明度 = null;
			public Storyboard ストーリーボード = null;

			public void Dispose()
			{
				FDKUtilities.解放する( ref this.ストーリーボード );
				FDKUtilities.解放する( ref this.不透明度 );
				FDKUtilities.解放する( ref this.太さ );
				FDKUtilities.解放する( ref this.回転角rad );
				FDKUtilities.解放する( ref this.中心位置Y );
				FDKUtilities.解放する( ref this.中心位置X );
			}
		}
		protected 黒幕[] _黒幕 = null;

		protected 画像 _ロゴ = null;
		protected Variable _ロゴ不透明度 = null;
		protected readonly RectangleF _ロゴ表示領域 = new RectangleF( ( 1920f - 600f ) / 2f, ( 1080f - 350f ) / 2f, 600f, 350f );
	}
}
