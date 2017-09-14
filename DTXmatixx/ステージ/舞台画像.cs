using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SharpDX;
using SharpDX.Animation;
using SharpDX.Direct2D1;
using SharpDX.Direct2D1.Effects;
using FDK;
using FDK.メディア;

namespace DTXmatixx.ステージ
{
	class 舞台画像 : Activity
	{
		public Size2F サイズ
		{
			get
				=> this._背景画像.サイズ;
		}

		public bool ぼかしを適用中
		{
			get;
			protected set;
		} = false;
		public bool 縮小を適用中
		{
			get;
			protected set;
		} = false;

		public 舞台画像( string 背景画像ファイル名 = null, string 背景黒幕付き画像ファイル名 = null )
		{
			this.子リスト.Add( this._背景画像 = new 画像( 背景画像ファイル名 ?? @"$(System)images\舞台.jpg" ) );
			this.子リスト.Add( this._背景黒幕付き画像 = new 画像( 背景黒幕付き画像ファイル名 ?? @"$(System)images\舞台黒幕付き.jpg" ) );
		}

		public void ぼかしを適用する( グラフィックデバイス gd, double 完了までの最大時間sec = 1.0 )
		{
			Debug.Assert( this.活性化している );

			if( !( this.ぼかしを適用中 ) )
			{
				if( 0.0 == 完了までの最大時間sec )
				{
					this._ぼかし割合?.Dispose();
					this._ぼかし割合 = new Variable( gd.Animation.Manager, initialValue: 1.0 );
				}
				else
				{
					using( var 割合遷移 = gd.Animation.TrasitionLibrary.SmoothStop( 完了までの最大時間sec, finalValue: 1.0 ) )
					{
						this._ストーリーボード?.Abandon();
						this._ストーリーボード?.Dispose();
						this._ストーリーボード = new Storyboard( gd.Animation.Manager );
						this._ストーリーボード.AddTransition( this._ぼかし割合, 割合遷移 );
						this._ストーリーボード.Schedule( gd.Animation.Timer.Time );    // 今すぐ開始
					}
				}
				this.ぼかしを適用中 = true;
			}
		}
		public void ぼかしを解除する( グラフィックデバイス gd, double 完了までの最大時間sec = 1.0 )
		{
			Debug.Assert( this.活性化している );

			if( this.ぼかしを適用中 )
			{
				if( 0.0 == 完了までの最大時間sec )
				{
					this._ぼかし割合?.Dispose();
					this._ぼかし割合 = new Variable( gd.Animation.Manager, initialValue: 0.0 );
				}
				else
				{
					using( var 割合遷移 = gd.Animation.TrasitionLibrary.SmoothStop( 完了までの最大時間sec, finalValue: 0.0 ) )
					{
						this._ストーリーボード?.Abandon();
						this._ストーリーボード?.Dispose();
						this._ストーリーボード = new Storyboard( gd.Animation.Manager );
						this._ストーリーボード.AddTransition( this._ぼかし割合, 割合遷移 );
						this._ストーリーボード.Schedule( gd.Animation.Timer.Time );    // 今すぐ開始
					}
				}
				this.ぼかしを適用中 = false;
			}
		}

		public void 縮小を適用する( グラフィックデバイス gd, double 完了までの最大時間sec = 1.0 )
		{
			Debug.Assert( this.活性化している );

			if( !( this.縮小を適用中 ) )
			{
				if( 0.0 == 完了までの最大時間sec )
				{
					this._縮小割合?.Dispose();
					this._縮小割合 = new Variable( gd.Animation.Manager, initialValue: 1.0 );
				}
				else
				{
					using( var 割合遷移 = gd.Animation.TrasitionLibrary.SmoothStop( 完了までの最大時間sec, finalValue: 1.0 ) )
					{
						this._ストーリーボード?.Abandon();
						this._ストーリーボード?.Dispose();
						this._ストーリーボード = new Storyboard( gd.Animation.Manager );
						this._ストーリーボード.AddTransition( this._縮小割合, 割合遷移 );
						this._ストーリーボード.Schedule( gd.Animation.Timer.Time );    // 今すぐ開始
					}
				}
				this.縮小を適用中 = true;
			}
		}
		public void 縮小を解除する( グラフィックデバイス gd, double 完了までの最大時間sec = 1.0 )
		{
			Debug.Assert( this.活性化している );

			if( this.縮小を適用中 )
			{
				if( 0.0 == 完了までの最大時間sec )
				{
					this._縮小割合?.Dispose();
					this._縮小割合 = new Variable( gd.Animation.Manager, initialValue: 0.0 );
				}
				else
				{
					using( var 割合遷移 = gd.Animation.TrasitionLibrary.SmoothStop( 完了までの最大時間sec, finalValue: 0.0 ) )
					{
						this._ストーリーボード?.Abandon();
						this._ストーリーボード?.Dispose();
						this._ストーリーボード = new Storyboard( gd.Animation.Manager );
						this._ストーリーボード.AddTransition( this._縮小割合, 割合遷移 );
						this._ストーリーボード.Schedule( gd.Animation.Timer.Time );    // 今すぐ開始
					}
				}
				this.縮小を適用中 = false;
			}
		}

		protected override void On活性化( グラフィックデバイス gd )
		{
			this._ガウスぼかしエフェクト = new GaussianBlur( gd.D2DDeviceContext );
			this._ガウスぼかしエフェクト黒幕付き用 = new GaussianBlur( gd.D2DDeviceContext );

			this._拡大エフェクト = new Scale( gd.D2DDeviceContext ) {
				CenterPoint = new Vector2( gd.設計画面サイズ.Width / 2.0f, gd.設計画面サイズ.Height / 2.0f ),
			};
			this._拡大エフェクト黒幕付き用 = new Scale( gd.D2DDeviceContext ) {
				CenterPoint = new Vector2( gd.設計画面サイズ.Width / 2.0f, gd.設計画面サイズ.Height / 2.0f ),
			};

			this._切り取りエフェクト = new Crop( gd.D2DDeviceContext );
			this._切り取りエフェクト黒幕付き用 = new Crop( gd.D2DDeviceContext );

			this._ぼかし割合 = new Variable( gd.Animation.Manager, initialValue: 0.0 );
			this._縮小割合 = new Variable( gd.Animation.Manager, initialValue: 1.0 );
			this._ストーリーボード = null;

			this._初めての進行描画 = true;
		}
		protected override void On非活性化( グラフィックデバイス gd )
		{
			this._ストーリーボード?.Abandon();

			FDKUtilities.解放する( ref this._ストーリーボード );
			FDKUtilities.解放する( ref this._ぼかし割合 );
			FDKUtilities.解放する( ref this._縮小割合 );
			FDKUtilities.解放する( ref this._切り取りエフェクト黒幕付き用 );
			FDKUtilities.解放する( ref this._切り取りエフェクト );
			FDKUtilities.解放する( ref this._拡大エフェクト黒幕付き用 );
			FDKUtilities.解放する( ref this._拡大エフェクト );
			FDKUtilities.解放する( ref this._ガウスぼかしエフェクト黒幕付き用 );
			FDKUtilities.解放する( ref this._ガウスぼかしエフェクト );
		}

		public void 進行描画する( グラフィックデバイス gd, bool 黒幕付き = false, Vector4? 表示領域 = null, LayerParameters1? layerParameters1 = null )
		{
			#region " 初めての進行描画 "
			//----------------
			if( this._初めての進行描画 )
			{
				if( null != this._背景画像 )
				{
					this._拡大エフェクト.SetInput( 0, this._背景画像.Bitmap, true );          // (1) 拡大
					this._ガウスぼかしエフェクト.SetInputEffect( 0, this._拡大エフェクト );    // (2) ぼかし
					this._切り取りエフェクト.SetInputEffect( 0, this._ガウスぼかしエフェクト ); // (3) クリッピング
				}
				if( null != this._背景黒幕付き画像 )
				{
					this._拡大エフェクト黒幕付き用.SetInput( 0, this._背景黒幕付き画像.Bitmap, true );             // (1) 拡大
					this._ガウスぼかしエフェクト黒幕付き用.SetInputEffect( 0, this._拡大エフェクト黒幕付き用 );     // (2) ぼかし
					this._切り取りエフェクト黒幕付き用.SetInputEffect( 0, this._ガウスぼかしエフェクト黒幕付き用 ); // (3) クリッピング
				}

				this._初めての進行描画 = false;
			}
			//----------------
			#endregion

			double ぼかし割合 = this._ぼかし割合?.Value ?? 0.0;
			double 縮小割合 = this._縮小割合?.Value ?? 1.0;

			if( 黒幕付き )
			{
				this._拡大エフェクト黒幕付き用.ScaleAmount = new Vector2( (float) ( 1f + ( 1.0 - 縮小割合 ) * 0.04 ) );    // 1.04 ～ 1
				this._ガウスぼかしエフェクト黒幕付き用.StandardDeviation = (float) ( ぼかし割合 * 10.0 );       // 0～10
				this._切り取りエフェクト黒幕付き用.Rectangle = ( null != 表示領域 ) ? ( (Vector4) 表示領域 ) : new Vector4( 0f, 0f, this._背景黒幕付き画像.サイズ.Width, this._背景黒幕付き画像.サイズ.Height );

				gd.D2DBatchDraw( ( dc ) => {

					if( null == layerParameters1 )
					{
						dc.DrawImage( this._切り取りエフェクト黒幕付き用 );
					}
					else
					{
						using( var layer = new Layer( dc ) )
						{
							dc.PushLayer( (LayerParameters1) layerParameters1, layer );
							dc.DrawImage( this._切り取りエフェクト黒幕付き用 );
							dc.PopLayer();
						}
					}

				} );
			}
			else
			{
				this._拡大エフェクト.ScaleAmount = new Vector2( (float) ( 1f + ( 1.0 - 縮小割合 ) * 0.04 ) );    // 1.04 ～ 1
				this._ガウスぼかしエフェクト.StandardDeviation = (float) ( ぼかし割合 * 10.0 );       // 0～10
				this._切り取りエフェクト.Rectangle = ( null != 表示領域 ) ? ( (Vector4) 表示領域 ) : new Vector4( 0f, 0f, this._背景画像.サイズ.Width, this._背景画像.サイズ.Height );

				gd.D2DBatchDraw( ( dc ) => {

					if( null == layerParameters1 )
					{
						dc.DrawImage( this._切り取りエフェクト );
					}
					else
					{
						using( var layer = new Layer( dc ) )
						{
							dc.PushLayer( (LayerParameters1) layerParameters1, layer );
							dc.DrawImage( this._切り取りエフェクト );
							dc.PopLayer();
						}
					}

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
		private Crop _切り取りエフェクト = null;
		private Crop _切り取りエフェクト黒幕付き用 = null;

		/// <summary>
		///		くっきり: 0 ～ 1 :ぼかし
		/// </summary>
		private Variable _ぼかし割合 = null;
		/// <summary>
		///		拡大: 0 ～ 1 :標準
		/// </summary>
		private Variable _縮小割合 = null;
		private Storyboard _ストーリーボード = null;
	}
}
