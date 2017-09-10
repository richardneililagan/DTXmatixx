using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SharpDX;
using SharpDX.Animation;
using SharpDX.Direct2D1.Effects;
using FDK;
using FDK.メディア;

namespace DTXmatixx.ステージ
{
	class 舞台画像 : Activity
	{
		public bool ぼかしと縮小を適用中
		{
			get;
			protected set;
		} = false;

		public 舞台画像()
		{
			this.子リスト.Add( this._背景画像 = new 画像( @"$(System)images\舞台.jpg" ) );
			this.子リスト.Add( this._背景黒幕付き画像 = new 画像( @"$(System)images\舞台黒幕付き.jpg" ) );
		}

		public void ぼかしと縮小を適用する( グラフィックデバイス gd )
		{
			Debug.Assert( this.活性化している );

			if( !( this.ぼかしと縮小を適用中 ) )
			{
				using( var 割合遷移 = gd.Animation.TrasitionLibrary.SmoothStop( 1, finalValue: 1.0 ) )	// 1秒以内に1.0へ遷移せよ
				{
					this._ストーリーボード?.Abandon();
					this._ストーリーボード?.Dispose();
					this._ストーリーボード = new Storyboard( gd.Animation.Manager );
					this._ストーリーボード.AddTransition( this._ぼかしと縮小割合, 割合遷移 );
					this._ストーリーボード.Schedule( gd.Animation.Timer.Time );    // 今すぐ開始
				}
				this.ぼかしと縮小を適用中 = true;
			}
		}
		public void ぼかしと縮小を解除する( グラフィックデバイス gd )
		{
			Debug.Assert( this.活性化している );

			if( this.ぼかしと縮小を適用中 )
			{
				using( var 割合遷移 = gd.Animation.TrasitionLibrary.SmoothStop( 1, finalValue: 0.0 ) )  // 1秒以内に0.0へ遷移せよ
				{
					this._ストーリーボード?.Abandon();
					this._ストーリーボード?.Dispose();
					this._ストーリーボード = new Storyboard( gd.Animation.Manager );
					this._ストーリーボード.AddTransition( this._ぼかしと縮小割合, 割合遷移 );
					this._ストーリーボード.Schedule( gd.Animation.Timer.Time );    // 今すぐ開始
				}
				this.ぼかしと縮小を適用中 = false;
			}
		}

		protected override void On活性化( グラフィックデバイス gd )
		{
			this._ガウスぼかしエフェクト = new GaussianBlur( gd.D2DDeviceContext );
			this._ガウスぼかしエフェクト黒幕付き用 = new GaussianBlur( gd.D2DDeviceContext );

			this._拡大エフェクト = new Scale( gd.D2DDeviceContext ) {
				CenterPoint = new Vector2( 960f, 540f ),
			};
			this._拡大エフェクト黒幕付き用 = new Scale( gd.D2DDeviceContext ) {
				CenterPoint = new Vector2( 960f, 540f ),
			};

			this._ぼかしと縮小割合 = new Variable( gd.Animation.Manager, initialValue: 0.0 );
			this._ストーリーボード = null;

			this._初めての進行描画 = true;
		}
		protected override void On非活性化( グラフィックデバイス gd )
		{
			this._ストーリーボード?.Abandon();

			FDKUtilities.解放する( ref this._ガウスぼかしエフェクト );
			FDKUtilities.解放する( ref this._ガウスぼかしエフェクト黒幕付き用 );
			FDKUtilities.解放する( ref this._拡大エフェクト );
			FDKUtilities.解放する( ref this._拡大エフェクト黒幕付き用 );
			FDKUtilities.解放する( ref this._ストーリーボード );
			FDKUtilities.解放する( ref this._ぼかしと縮小割合 );
		}

		public void 進行描画する( グラフィックデバイス gd, bool 黒幕付き = false )
		{
			if( this._初めての進行描画 )
			{
				if( null != this._背景画像 )
				{
					this._拡大エフェクト.SetInput( 0, this._背景画像.Bitmap, true );			// (1) 画像を拡大
					this._ガウスぼかしエフェクト.SetInputEffect( 0, this._拡大エフェクト );		// (2) 拡大結果にぼかし
				}
				if( null != this._背景黒幕付き画像 )
				{
					this._拡大エフェクト黒幕付き用.SetInput( 0, this._背景黒幕付き画像.Bitmap, true );			// (1) 画像を拡大
					this._ガウスぼかしエフェクト黒幕付き用.SetInputEffect( 0, this._拡大エフェクト黒幕付き用 );	// (2) 拡大結果にぼかし
				}

				this._初めての進行描画 = false;
			}

			double 割合 = this._ぼかしと縮小割合?.Value ?? 0.0;

			if( 黒幕付き )
			{
				this._拡大エフェクト黒幕付き用.ScaleAmount = new Vector2( (float) ( 1f + ( 1.0 - 割合 ) * 0.04 ) );    // 1.04 ～ 1
				this._ガウスぼかしエフェクト黒幕付き用.StandardDeviation = (float) ( 割合 * 10.0 );       // 0～10
				gd.D2DBatchDraw( ( dc ) => {
					dc.DrawImage( this._ガウスぼかしエフェクト黒幕付き用, new Vector2( 0f, 0f ) );
				} );
			}
			else
			{
				this._拡大エフェクト.ScaleAmount = new Vector2( (float) ( 1f + ( 1.0 - 割合 ) * 0.04 ) );    // 1.04 ～ 1
				this._ガウスぼかしエフェクト.StandardDeviation = (float) ( 割合 * 10.0 );       // 0～10
				gd.D2DBatchDraw( ( dc ) => {
					dc.DrawImage( this._ガウスぼかしエフェクト, new Vector2( 0f, 0f ) );
				} );
			}
		}

		private bool _初めての進行描画 = true;
		private 画像 _背景画像 = null;
		private 画像 _背景黒幕付き画像 = null;
		private GaussianBlur _ガウスぼかしエフェクト = null;
		private GaussianBlur _ガウスぼかしエフェクト黒幕付き用 = null;
		private Scale _拡大エフェクト = null;
		private Scale _拡大エフェクト黒幕付き用 = null;

		/// <summary>
		///		くっきり＆拡大: 0 ～ 1 :ぼかし＆縮小
		/// </summary>
		private Variable _ぼかしと縮小割合 = null;
		private Storyboard _ストーリーボード = null;
	}
}
