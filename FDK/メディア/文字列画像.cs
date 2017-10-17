using System;
using System.Collections.Generic;
using System.Diagnostics;
using SharpDX;
using SharpDX.Direct2D1;
using SharpDX.DirectWrite;
using FDK;

namespace FDK.メディア
{
	/// <summary>
	///		DirectWrite を使った Direct2D1ビットマップ。
	/// </summary>
	/// <remarks>
	///		「表示文字列」メンバを設定/更新すれば、次回の描画時にビットマップが生成される。
	/// </remarks>
	public class 文字列画像 : Activity
	{
		/// <summary>
		///		このメンバを set すれば、次回の進行描画時に画像が更新される。
		/// </summary>
		public string 表示文字列
		{
			get;
			set;
		} = null;
		public string フォント名
		{
			get;
			set;
		} = "メイリオ";
		public float フォントサイズpt
		{
			get;
			set;
		} = 20.0f;
		public FontWeight フォント幅
		{
			get;
			set;
		} = FontWeight.Normal;
		public FontStyle フォントスタイル
		{
			get;
			set;
		} = FontStyle.Normal;
		public InterpolationMode 補正モード
		{
			get;
			set;
		} = InterpolationMode.Linear;
		public RectangleF? 転送元矩形
		{
			get;
			set;
		} = null;
		public bool 加算合成
		{
			get;
			set;
		} = false;
		public Size2F サイズ
		{
			get;
			set;
		} = new Size2F( -1f, -1f );
		public Color4 前景色
		{
			get;
			set;
		} = Color4.White;
		public Color4 背景色
		{
			get;
			set;
		} = Color4.Black;
		public enum 効果
		{
			/// <summary>
			///		前景色で描画。
			/// </summary>
			通常,
			/// <summary>
			///		文字は前景色で、影は背景色で描画する。
			/// </summary>
			ドロップシャドウ,
			/// <summary>
			///		文字は前景色で、縁は背景色で描画する。
			/// </summary>
			縁取り,
		}
		public 効果 描画効果
		{
			get;
			set;
		} = 効果.通常;
		/// <summary>
		///		効果が縁取りのときのみ有効。
		/// </summary>
		public float 縁のサイズdpx
		{
			get;
			set;
		} = 6f;

		public 文字列画像()
		{
			// 必要なプロパティは呼び出し元で設定すること。
		}

		protected override void On活性化( グラフィックデバイス gd )
		{
			this._前回の表示文字列 = null;

			if( this.表示文字列.Nullでも空でもない() )
			{
				this.ビットマップを生成または更新する( gd );
				this._前回の表示文字列 = this.表示文字列; // 最初の構築完了。
			}
		}
		protected override void On非活性化( グラフィックデバイス gd )
		{
			FDKUtilities.解放する( ref this._背景色ブラシ );
			FDKUtilities.解放する( ref this._前景色ブラシ );
			FDKUtilities.解放する( ref this._Bitmap );
			FDKUtilities.解放する( ref this._TextLayout );
			FDKUtilities.解放する( ref this._TextFormat );
		}

		public void 描画する( グラフィックデバイス gd, float 左位置, float 上位置, float 不透明度0to1 = 1.0f, float X方向拡大率 = 1.0f, float Y方向拡大率 = 1.0f, Matrix? 変換行列3D = null )
		{
			var 変換行列2D =
				Matrix3x2.Scaling( X方向拡大率, Y方向拡大率 )     // スケーリング
				* Matrix3x2.Translation( 左位置, 上位置 );        // 平行移動

			this.描画する( gd, 変換行列2D, 変換行列3D, 不透明度0to1 );
		}
		public void 描画する( グラフィックデバイス gd, Matrix3x2? 変換行列2D = null, Matrix? 変換行列3D = null, float 不透明度0to1 = 1.0f )
		{
			Debug.Assert( this.活性化している );

			if( this.表示文字列.Nullまたは空である() )
				return;

			// 表示文字列が変更されているなら、ここで表示ビットマップレンダーターゲットの再構築を行う。
			if( false == string.Equals( this.表示文字列, this._前回の表示文字列 ) )
			{
				this.ビットマップを生成または更新する( gd );
			}

			if( null == this._Bitmap )
				return;

			gd.D2DBatchDraw( ( dc ) => {

				// 変換行列とブレンドモードをD2Dレンダーターゲットに設定する。
				dc.Transform = ( 変換行列2D ?? Matrix3x2.Identity ) * dc.Transform;
				dc.PrimitiveBlend = ( 加算合成 ) ? PrimitiveBlend.Add : PrimitiveBlend.SourceOver;

				// D2Dレンダーターゲットに this.Bitmap を描画する。
				using( var bmp = this._Bitmap.Bitmap )
				{
					dc.DrawBitmap(
						bitmap: bmp,
						destinationRectangle: null,
						opacity: 不透明度0to1,
						interpolationMode: this.補正モード,
						sourceRectangle: this.転送元矩形,
						erspectiveTransformRef: 変換行列3D );
				}
			} );
		}

		protected SharpDX.Direct2D1.BitmapRenderTarget _Bitmap = null;

		protected void ビットマップを生成または更新する( グラフィックデバイス gd )
		{
			this._前回の表示文字列 = this.表示文字列;

			#region " テキストフォーマットの作成がまだなら作成する。"
			//----------------
			if( null == this._TextFormat )
			{
				this._TextFormat = new TextFormat( gd.DWriteFactory, this.フォント名, this.フォント幅, this.フォントスタイル, this.フォントサイズpt ) {
					TextAlignment = TextAlignment.Leading,
				};
			}
			//----------------
			#endregion

			#region " サイズを計算する。"
			//----------------
			var 最大サイズ = new Size2F( gd.設計画面サイズ.Width, gd.設計画面サイズ.Height );

			this._TextLayout?.Dispose();
			this._TextLayout = new TextLayout(
				gd.DWriteFactory,
				this.表示文字列,
				this._TextFormat,
				最大サイズ.Width,
				最大サイズ.Height );

			this.サイズ = new Size2F(
				this._TextLayout.Metrics.WidthIncludingTrailingWhitespace,
				this._TextLayout.Metrics.Height );
			//----------------
			#endregion

			#region " 古いビットマップレンダーターゲットを解放し、新しく生成する。"
			//----------------
			using( var target = gd.D2DDeviceContext.Target )	// Target を get すると COM参照カウンタが増えるので注意。
			{
				// D2DContext1.Target が設定済みであること。さもなきゃ例外も出さずに落ちる。
				Debug.Assert( null != target );
			}
			this._Bitmap?.Dispose();
			this._Bitmap = new SharpDX.Direct2D1.BitmapRenderTarget(
				gd.D2DDeviceContext,
				CompatibleRenderTargetOptions.None,
				this.サイズ );
			//----------------
			#endregion

			#region " ブラシを作成または更新する。"
			//----------------
			this._前景色ブラシ?.Dispose();
			this._前景色ブラシ = new SolidColorBrush( this._Bitmap, this.前景色 );

			this._背景色ブラシ?.Dispose();
			this._背景色ブラシ = new SolidColorBrush( this._Bitmap, this.背景色 );
			//----------------
			#endregion

			// ビットマップレンダーターゲットにテキストを描画する。
			gd.D2DBatchDraw( this._Bitmap, ( rt ) => {

				rt.Transform = Matrix3x2.Identity;  // ここではDPXtoPX変換は行わない。（ビットマップの描画時に行うので。）

				rt.Clear( Color.Transparent );

				switch( this.描画効果 )
				{
					case 効果.通常:
						rt.DrawTextLayout(
							new Vector2( 0.0f, 0.0f ),
							this._TextLayout,
							this._前景色ブラシ,
							DrawTextOptions.Clip );
						break;

					case 効果.ドロップシャドウ:
						rt.DrawTextLayout(		// 影
							new Vector2( 1.0f, 1.0f ),
							this._TextLayout,
							this._背景色ブラシ,
							DrawTextOptions.Clip );
						rt.DrawTextLayout(		// 本体
							new Vector2( 0.0f, 0.0f ),
							this._TextLayout,
							this._前景色ブラシ,
							DrawTextOptions.Clip );
						break;

					case 効果.縁取り:
						using( var tr = new 縁取りTextRenderer( gd.D2DFactory, rt, this._背景色ブラシ, this._前景色ブラシ, this.縁のサイズdpx ) )
						{
							this._TextLayout.Draw( tr, 0f, 0f );
						}
						break;
				}
			} );
		}

		private string _前回の表示文字列 = null;
		private TextFormat _TextFormat = null;
		private TextLayout _TextLayout = null;
		private SolidColorBrush _前景色ブラシ = null;
		private SolidColorBrush _背景色ブラシ = null;
	}
}
