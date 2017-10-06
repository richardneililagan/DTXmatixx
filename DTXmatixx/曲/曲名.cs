using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SharpDX;
using SharpDX.Direct2D1;
using SharpDX.DirectWrite;
using FDK;
using FDK.メディア;

namespace DTXmatixx.曲
{
	class 曲名 : ビットマップ付きテクスチャ
	{
		/// <summary>
		///		このメンバを set すれば、次回の進行描画時に画像が更新される。
		/// </summary>
		public string タイトル
		{
			get;
			set;
		} = null;

		/// <summary>
		///		このメンバを set すれば、次回の進行描画時に画像が更新される。
		/// </summary>
		public string サブタイトル
		{
			get;
			set;
		} = null;


		public 曲名()
			: base( new Size2( (int) Node.全体サイズ.Width, (int) Node.全体サイズ.Height ) )
		{
			this._fontName = "Arial";
			this._titleFontColor = Color4.White;
			this._subtitleFontColor = new Color4( 0.5f, 0.5f, 0.5f, 1.0f );	// 灰色
			this._backColor = new Color4( 0f, 0f, 0f, 0.75f );	// 半透明の黒
			this._fontWeight = FontWeight.Normal;
			this._fontStyle = FontStyle.Normal;
			this._fontSizePt = 20f;
			this._textAlignment = TextAlignment.Leading;
		}

		protected override void On活性化( グラフィックデバイス gd )
		{
			base.On活性化( gd );  // 忘れずに。サイズメンバを確定させるために、先に呼び出す。

			this._前回のタイトル = null;

			this._textFormat = new TextFormat(
				gd.DWriteFactory,
				this._fontName,
				this._fontWeight,
				this._fontStyle,
				this._fontSizePt ) {
				TextAlignment = this._textAlignment
			};
			this._titleFontBrush = new SolidColorBrush( gd.D2DDeviceContext, this._titleFontColor );
			this._subtitleFontBrush = new SolidColorBrush( gd.D2DDeviceContext, this._subtitleFontColor );
			this._backBrush = new SolidColorBrush( gd.D2DDeviceContext, this._backColor );
		}

		protected override void On非活性化( グラフィックデバイス gd )
		{
			FDKUtilities.解放する( ref this._backBrush );
			FDKUtilities.解放する( ref this._subtitleFontBrush );
			FDKUtilities.解放する( ref this._titleFontBrush );
			FDKUtilities.解放する( ref this._textFormat );

			base.On非活性化( gd ); // 忘れずに。
		}

		public new void 描画する( グラフィックデバイス gd, Matrix ワールド行列変換, RectangleF? レイアウト矩形 = null )
		{
			Debug.Assert( this.活性化している );

			if( this.タイトル.Nullまたは空である() )
				return;

			var 全体矩形 = レイアウト矩形 ?? new RectangleF( 0f, 0f, this.サイズ.Width, this.サイズ.Height );

			var マージン = 4f;
			var タイトル文字矩形 = new RectangleF( 全体矩形.X + マージン, 全体矩形.Y + マージン, 全体矩形.Width - マージン * 2f, 全体矩形.Height - マージン * 2f );

			// タイトルまたはサブタイトルが変更されているなら、ここでビットマップの更新を行う。
			if( !( string.Equals( this.タイトル, this._前回のタイトル ) ) ||
				!( string.Equals( this.サブタイトル, this._前回のサブタイトル ) ) )
			{
				this._前回のタイトル = this.タイトル;
				this._前回のサブタイトル = this.サブタイトル;

				using( var タイトルレイアウト = new TextLayout(
					gd.DWriteFactory,
					this.タイトル,
					this._textFormat,
					タイトル文字矩形.Width,
					タイトル文字矩形.Height ) )
				{
					var タイトルサイズ = new Size2F(
						タイトルレイアウト.Metrics.WidthIncludingTrailingWhitespace,
						タイトルレイアウト.Metrics.Height );

					var サブタイトル文字矩形 = タイトル文字矩形;
					サブタイトル文字矩形.Y += タイトルサイズ.Height;	// サブタイトルの位置は、タイトルの縦幅分だけ下へ移動。

					using( var サブタイトルレイアウト = new TextLayout(
						gd.DWriteFactory,
						this.サブタイトル,
						this._textFormat,
						サブタイトル文字矩形.Width,
						サブタイトル文字矩形.Height ) )
					{
						this.ビットマップへ描画する( gd, ( dc, bmp ) => {

							// dc は最終的に bmp をテクスチャに描画するので、DPXとPXの拡大率を考慮する必要はない。よって、Transform は Identity に上書きする。
							dc.Transform = Matrix3x2.Identity;

							// 背景色で塗りつぶす。
							dc.FillRectangle( 全体矩形, this._backBrush );

							// タイトルを描画。
							dc.DrawTextLayout(
								new Vector2( タイトル文字矩形.X, タイトル文字矩形.Y ),
								タイトルレイアウト,
								this._titleFontBrush,
								DrawTextOptions.Clip );

							// サブタイトルを描画。
							dc.DrawTextLayout(
								new Vector2( サブタイトル文字矩形.X, サブタイトル文字矩形.Y ),
								サブタイトルレイアウト,
								this._subtitleFontBrush,
								DrawTextOptions.Clip );
						} );
					}
				}
			}

			// テクスチャを描画する。
			base.描画する( gd, ワールド行列変換 );
		}


		private string _前回のタイトル = null;

		private string _前回のサブタイトル = null;

		private TextFormat _textFormat = null;

		private string _fontName;

		private Color4 _titleFontColor;

		private Color4 _subtitleFontColor;

		private Color4 _backColor;

		private FontWeight _fontWeight;

		private FontStyle _fontStyle;

		private float _fontSizePt;

		private TextAlignment _textAlignment;

		private Brush _titleFontBrush = null;

		private Brush _subtitleFontBrush = null;

		private Brush _backBrush = null;
	}
}
