using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SharpDX;
using SharpDX.Animation;
using SharpDX.Direct2D1;
using FDK;
using FDK.メディア;

namespace DTXmatixx.アイキャッチ
{
	class シャッター : アイキャッチBase
	{
		public シャッター()
		{
			this.子リスト.Add( this._ロゴ = new 画像( @"$(System)images\タイトルロゴ.png" ) );
		}

		protected override void On活性化( グラフィックデバイス gd )
		{
			this._明るいブラシ = new SolidColorBrush( gd.D2DDeviceContext, new Color4( 83f / 255f, 210f / 255f, 255f / 255f, 1f ) );
			this._ふつうのブラシ = new SolidColorBrush( gd.D2DDeviceContext, new Color4( 46f / 255f, 117f / 255f, 182f / 255f, 1f ) );
			this._濃いブラシ = new SolidColorBrush( gd.D2DDeviceContext, new Color4( 0f / 255f, 32f / 255f, 96f / 255f, 1f ) );
			this._黒ブラシ = new SolidColorBrush( gd.D2DDeviceContext, Color4.Black );
			this._白ブラシ = new SolidColorBrush( gd.D2DDeviceContext, Color4.White );

			this._シャッター情報 = new シャッター情報[ シャッター枚数 ] {
				#region " *** "
				//----------------
				new シャッター情報() {		// 1
					ブラシ = this._明るいブラシ,
					矩形サイズ = new Size2F( 910f, 908f ),
					角度rad = (float) Math.PI / 4f,
					閉じ中心位置 = new Vector2( 0f, 1080f ),
					開き中心位置 = new Vector2( 0f-450, 1080f+450f ),
					完全開き時刻sec = 0.0,
					開閉時間sec = 0.2,
				},
				new シャッター情報() {		// 2
					ブラシ = this._濃いブラシ,
					矩形サイズ = new Size2F( 1000f, 992f ),
					角度rad = (float) Math.PI / 4f,
					閉じ中心位置 = new Vector2( 1920f, 0f ),
					開き中心位置 = new Vector2( 1920f+500f, 0f-500f ),
					完全開き時刻sec = 0.03,
					開閉時間sec = 0.2,
				},
				new シャッター情報() {		// 3
					ブラシ = this._黒ブラシ,
					矩形サイズ = new Size2F( 915f, 911f ),
					角度rad = (float) Math.PI / -4f,
					閉じ中心位置 = new Vector2( 0f, 0f ),
					開き中心位置 = new Vector2( 0f-450f, 0f-450f ),
					完全開き時刻sec = 0.075,
					開閉時間sec = 0.2,
				},
				new シャッター情報() {		// 4
					ブラシ = this._ふつうのブラシ,
					矩形サイズ = new Size2F( 884, 885f ),
					角度rad = (float) Math.PI / -4f,
					閉じ中心位置 = new Vector2( 1920f, 1080f ),
					開き中心位置 = new Vector2( 1920f+450f, 1080f+450f ),
					完全開き時刻sec = 0.06,
					開閉時間sec = 0.2,
				},
				new シャッター情報() {		// 5
					ブラシ = this._濃いブラシ,
					矩形サイズ = new Size2F( 370f, 740f ),
					角度rad = (float) 0f,
					閉じ中心位置 = new Vector2( 104f+185f, 541f ),
					開き中心位置 = new Vector2( 104f+185f-500f, 541f ),
					完全開き時刻sec = 0.16,
					開閉時間sec = 0.2,
				},
				new シャッター情報() {		// 6
					ブラシ = this._黒ブラシ,
					矩形サイズ = new Size2F( 280f, 560f ),
					角度rad = (float) 0f,
					閉じ中心位置 = new Vector2( 1519+140f, 570f ),
					開き中心位置 = new Vector2( 1519+140f+400f, 570f ),
					完全開き時刻sec = 0.2,
					開閉時間sec = 0.2,
				},
				new シャッター情報() {		// 7
					ブラシ = this._明るいブラシ,
					矩形サイズ = new Size2F( 780f, 788f ),
					角度rad = (float) Math.PI / 4f,
					閉じ中心位置 = new Vector2( 1521f, 1080f ),
					開き中心位置 = new Vector2( 1521f+390f, 1080f+390f ),
					完全開き時刻sec = 0.2,
					開閉時間sec = 0.2,
				},
				new シャッター情報() {		// 8
					ブラシ = this._黒ブラシ,
					矩形サイズ = new Size2F( 1114f, 495f ),
					角度rad = (float) Math.PI / 4f,
					閉じ中心位置 = new Vector2( 1236f, 178f ),
					開き中心位置 = new Vector2( 1236f+400f, 178f-400f ),
					完全開き時刻sec = 0.23,
					開閉時間sec = 0.2,
				},
				new シャッター情報() {		// 9
					ブラシ = this._黒ブラシ,
					矩形サイズ = new Size2F( 652f, 312f ),
					角度rad = (float) 0f,
					閉じ中心位置 = new Vector2( 479f+323f, 1080f ),
					開き中心位置 = new Vector2( 479f+323f, 1080f+160f ),
					完全開き時刻sec = 0.3,
					開閉時間sec = 0.2,
				},
				new シャッター情報() {		// 10
					ブラシ = this._ふつうのブラシ,
					矩形サイズ = new Size2F( 412f, 288f ),
					角度rad = (float) 0f,
					閉じ中心位置 = new Vector2( 666f, 0f ),
					開き中心位置 = new Vector2( 666f, 0f-200f ),
					完全開き時刻sec = 0.33,
					開閉時間sec = 0.2,
				},
				new シャッター情報() {		// 11
					ブラシ = this._黒ブラシ,
					矩形サイズ = new Size2F( 630f, 630f ),
					角度rad = (float) Math.PI / 4f,
					閉じ中心位置 = new Vector2( 460f, 930f ),
					開き中心位置 = new Vector2( 460f-330f, 930f+330f ),
					完全開き時刻sec = 0.36,
					開閉時間sec = 0.2,
				},
				new シャッター情報() {		// 12
					ブラシ = this._明るいブラシ,
					矩形サイズ = new Size2F( 875f, 884f ),
					角度rad = (float) Math.PI / -4f,
					閉じ中心位置 = new Vector2( 461f, 138f ),
					開き中心位置 = new Vector2( 461f-438f, 138f-438f ),
					完全開き時刻sec = 0.36,
					開閉時間sec = 0.2,
				},
				new シャッター情報() {		// 13
					ブラシ = this._濃いブラシ,
					矩形サイズ = new Size2F( 460f, 690f ),
					角度rad = (float) 0f,
					閉じ中心位置 = new Vector2( 915f+230f, 253f+325f ),
					開き中心位置 = new Vector2( 915f+230f+480f, 253f+325f ),
					完全開き時刻sec = 0.4,
					開閉時間sec = 0.2,
				},
				new シャッター情報() {		// 14
					ブラシ = this._ふつうのブラシ,
					矩形サイズ = new Size2F( 340f, 620f ),
					角度rad = (float) 0f,
					閉じ中心位置 = new Vector2( 614f+150f, 620f ),
					開き中心位置 = new Vector2( 614f+150f-670f, 620f ),
					完全開き時刻sec = 0.40,
					開閉時間sec = 0.2,
				},
				//----------------
				#endregion
			};

			//for( int i = 0; i < シャッター枚数; i++ )
			//	this._シャッター情報[ i ].開to閉割合 = new Variable( gd.Animation.Manager, initialValue: 0.0 );
			//this._ロゴ不透明度 = new Variable( gd.Animation.Manager, initialValue: 0.0 );
			// 
			// --> クローズかオープンかで初期値が変わるので、ここでは設定しない。

			this.現在のフェーズ = フェーズ.未定;
		}
		protected override void On非活性化( グラフィックデバイス gd )
		{
			FDKUtilities.解放する( ref this._白ブラシ );
			FDKUtilities.解放する( ref this._黒ブラシ );
			FDKUtilities.解放する( ref this._濃いブラシ );
			FDKUtilities.解放する( ref this._ふつうのブラシ );
			FDKUtilities.解放する( ref this._明るいブラシ );

			if( null != this._シャッター情報 )
			{
				foreach( var s in this._シャッター情報 )
					s.Dispose();
				this._シャッター情報 = null;
			}
			FDKUtilities.解放する( ref this._ロゴボード );
			FDKUtilities.解放する( ref this._ロゴ不透明度 );
		}

		public override void クローズする( グラフィックデバイス gd, float 速度倍率 = 1.0f )
		{
			double 秒( double v ) => ( v / 速度倍率 );
			var start = gd.Animation.Timer.Time;

			for( int i = 0; i < シャッター枚数; i++ )
			{
				using( var 開閉遷移 = gd.Animation.TrasitionLibrary.SmoothStop( maximumDuration: 秒( this._シャッター情報[ i ].開閉時間sec ), finalValue: 1.0 ) )   // 終了値 1.0(完全閉じ)
				{
					this._シャッター情報[ i ].開to閉割合?.Dispose();
					this._シャッター情報[ i ].開to閉割合 = new Variable( gd.Animation.Manager, initialValue: 0.0 );    // 初期値 0.0(完全開き)
					this._シャッター情報[ i ].ストーリーボード?.Abandon();
					this._シャッター情報[ i ].ストーリーボード?.Dispose();
					this._シャッター情報[ i ].ストーリーボード = new Storyboard( gd.Animation.Manager );
					this._シャッター情報[ i ].ストーリーボード.AddTransition( this._シャッター情報[ i ].開to閉割合, 開閉遷移 );
					this._シャッター情報[ i ].ストーリーボード.Schedule( start + 秒( this._シャッター情報[ i ].完全開き時刻sec ) );    // 開始時刻: 完全開き時刻
				}
			}

			using( var _不透明度遷移 = gd.Animation.TrasitionLibrary.Linear( duration: 秒( 0.75f ), finalValue: 1.0 ) )    // 終了値 1.0(完全不透明)
			{
				this._ロゴ不透明度?.Dispose();
				this._ロゴ不透明度 = new Variable( gd.Animation.Manager, initialValue: 0.0 ); // 初期値 0.0(完全透明)
				this._ロゴボード = new Storyboard( gd.Animation.Manager );
				this._ロゴボード.AddTransition( this._ロゴ不透明度, _不透明度遷移 );
				this._ロゴボード.Schedule( start );
			}

			this.現在のフェーズ = フェーズ.クローズ;
		}
		public override void オープンする( グラフィックデバイス gd, float 速度倍率 = 1.0f )
		{
			double 秒( double v ) => ( v / 速度倍率 );

			double 最も遅い時刻sec = 0.0;
			foreach( var s in this._シャッター情報 )
			{
				if( 最も遅い時刻sec < s.完全閉じ時刻sec )
					最も遅い時刻sec = s.完全閉じ時刻sec;
			}
			var start = gd.Animation.Timer.Time;
			var end = start + 秒( 最も遅い時刻sec );

			for( int i = 0; i < シャッター枚数; i++ )
			{
				using( var 開閉遷移 = gd.Animation.TrasitionLibrary.SmoothStop( maximumDuration: 秒( this._シャッター情報[ i ].開閉時間sec ), finalValue: 0.0 ) )  // 終了値: 0.0(完全開き)
				{
					this._シャッター情報[ i ].開to閉割合?.Dispose();
					this._シャッター情報[ i ].開to閉割合 = new Variable( gd.Animation.Manager, initialValue: 1.0 );    // 初期値 1.0(完全閉じ)
					this._シャッター情報[ i ].ストーリーボード?.Abandon();
					this._シャッター情報[ i ].ストーリーボード?.Dispose();
					this._シャッター情報[ i ].ストーリーボード = new Storyboard( gd.Animation.Manager );
					this._シャッター情報[ i ].ストーリーボード.AddTransition( this._シャッター情報[ i ].開to閉割合, 開閉遷移 );
					this._シャッター情報[ i ].ストーリーボード.Schedule( end - 秒( this._シャッター情報[ i ].完全閉じ時刻sec ) );   // 開始時刻: 完全閉じ時刻
				}
			}

			this._ロゴ不透明度?.Dispose();
			this._ロゴ不透明度 = new Variable( gd.Animation.Manager, initialValue: 1.0 ); // 初期値 0.0(完全不透明)
			using( var _不透明度遷移 = gd.Animation.TrasitionLibrary.Linear( duration: 秒( 0.75 ), finalValue: 0.0 ) )    // 終了値 0.0(完全透明)
			{
				this._ロゴボード = new Storyboard( gd.Animation.Manager );
				this._ロゴボード.AddTransition( this._ロゴ不透明度, _不透明度遷移 );
				this._ロゴボード.Schedule( start );
			}

			this.現在のフェーズ = フェーズ.オープン;
		}

		protected override void 進行描画する( グラフィックデバイス gd, StoryboardStatus 描画しないStatus )
		{
			bool すべて完了 = true;

			gd.D2DBatchDraw( ( dc ) => {

				var pretrans = dc.Transform;

				for( int i = シャッター枚数 - 1; i >= 0; i-- )
				{
					var context = this._シャッター情報[ i ];

					if( context.ストーリーボード.Status != StoryboardStatus.Ready )
						すべて完了 = false;

					if( context.ストーリーボード.Status == 描画しないStatus )
						continue;

					dc.Transform =
						Matrix3x2.Rotation( context.角度rad )
						* Matrix3x2.Translation( context.開き中心位置 + ( context.閉じ中心位置 - context.開き中心位置 ) * new Vector2( (float) context.開to閉割合.Value ) )
						* pretrans;
					float w = context.矩形サイズ.Width;
					float h = context.矩形サイズ.Height;
					var rc = new RectangleF( -w / 2f, -h / 2f, w, h );
					dc.FillRectangle( rc, context.ブラシ );
					dc.DrawRectangle( rc, this._白ブラシ, 3.0f );
				}

			} );

			if( null != this._ロゴ不透明度 )
			{
				if( this._ロゴボード.Status != StoryboardStatus.Ready )
					すべて完了 = false;

				this._ロゴ.描画する(
					gd,
					this._ロゴ表示領域.Left,
					this._ロゴ表示領域.Top,
					不透明度0to1: (float) this._ロゴ不透明度.Value,
					X方向拡大率: ( this._ロゴ表示領域.Width / this._ロゴ.サイズ.Width ),
					Y方向拡大率: ( this._ロゴ表示領域.Height / this._ロゴ.サイズ.Height ) );
			}

			if( すべて完了 )
			{
				if( this.現在のフェーズ == フェーズ.クローズ )
				{
					this.現在のフェーズ = フェーズ.クローズ完了;
				}
				else if( this.現在のフェーズ == フェーズ.オープン )
				{
					this.現在のフェーズ = フェーズ.オープン完了;
				}
			}
		}

		private class シャッター情報 : IDisposable
		{
			public シャッター情報()
			{
			}
			public void Dispose()
			{
				FDKUtilities.解放する( ref this.ストーリーボード );
				FDKUtilities.解放する( ref this.開to閉割合 );
			}

			public Brush ブラシ = null;
			public Size2F 矩形サイズ;
			public float 角度rad;
			public Vector2 閉じ中心位置;
			public Vector2 開き中心位置;

			/// <summary>
			///		開き: 0.0 → 1.0: 閉じ
			/// </summary>
			public Variable 開to閉割合 = null;
			public double 完全開き時刻sec = 0.0;
			public double 開閉時間sec = 1.0;
			public double 完全閉じ時刻sec => this.完全開き時刻sec + 開閉時間sec;
			public Storyboard ストーリーボード = null;
		}

		private const int シャッター枚数 = 14;
		private シャッター情報[] _シャッター情報 = null;

		private Brush _明るいブラシ = null;
		private Brush _ふつうのブラシ = null;
		private Brush _濃いブラシ = null;
		private Brush _黒ブラシ = null;
		private Brush _白ブラシ = null;

		private 画像 _ロゴ = null;
		private Variable _ロゴ不透明度 = null;
		private Storyboard _ロゴボード = null;
		private readonly RectangleF _ロゴ表示領域 = new RectangleF( 1100f, 700f, 730f, 300f );
	}
}
