using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SharpDX;
using SharpDX.Direct2D1;
using SharpDX.WIC;

namespace FDK.メディア
{
	/// <summary>
	///		Direct2D の Bitmap を使って描画する画像。
	/// </summary>
	public class 画像 : Activity
	{
		/// <summary>
		///		画像の生成に成功していれば true 。
		/// </summary>
		public bool 生成成功
		{
			get
				=> ( null != this._Bitmap );
		}

		/// <summary>
		///		画像の生成に失敗していれば true 。
		/// </summary>
		public bool 生成失敗
		{
			get
				=> !( this.生成成功 );
		}

		public Size2F サイズ
		{
			get
			{
				if( this.生成成功 )
				{
					return new Size2F( this._Bitmap.PixelSize.Width, this._Bitmap.PixelSize.Height );
				}
				else
				{
					return Size2F.Zero;
				}
			}
		}

		public InterpolationMode 補正モード
		{
			get;
			set;
		} = InterpolationMode.Linear;

		public bool 加算合成
		{
			get;
			set;
		} = false;

		public Bitmap1 Bitmap
		{
			get
				=> this._Bitmap;
		}


		public 画像( string 画像ファイルパス )
		{
			this._画像ファイルパス = FDK.Folder.絶対パスに含まれるフォルダ変数を展開して返す( 画像ファイルパス );
		}

		protected override void On活性化( グラフィックデバイス gd )
		{
			this._画像を生成する( gd );
		}

		protected override void On非活性化( グラフィックデバイス gd )
		{
			FDKUtilities.解放する( ref this._Bitmap );
		}

		/// <summary>
		///		画像を描画する。
		/// </summary>
		/// <param name="gd">グラフィックデバイス。</param>
		/// <param name="左位置">画像の描画先範囲の左上隅X座標。</param>
		/// <param name="上位置">画像の描画先範囲の左上隅Y座標。</param>
		/// <param name="不透明度0to1">不透明度。(0:透明～1:不透明)</param>
		/// <param name="X方向拡大率">画像の横方向の拡大率。</param>
		/// <param name="Y方向拡大率">画像の縦方向の拡大率。</param>
		/// <param name="転送元矩形">画像の転送元範囲。</param>
		/// <param name="描画先矩形を整数境界に合わせる">true なら、描画先の転送先矩形の座標を float から int に丸める。</param>
		/// <param name="変換行列3D">射影行列。</param>
		/// <remarks>
		///		Direct2D の転送先矩形は float で指定できるが、非整数の値（＝物理ピクセル単位じゃない座標）を渡すと、表示画像がプラスマイナス1pxの範囲で乱れる。
		///		これにより、数px程度の大きさの画像を移動させるとチカチカする原因になる。
		///		それが困る場合には、<paramref name="描画先矩形を整数境界に合わせる"/> に true を指定すること。
		///		ただし、これを true にした場合、タイルのように並べて描画した場合に1pxずれる場合がある。この場合は false にすること。
		/// </remarks>
		public virtual void 描画する( グラフィックデバイス gd, float 左位置, float 上位置, float 不透明度0to1 = 1.0f, float X方向拡大率 = 1.0f, float Y方向拡大率 = 1.0f, RectangleF? 転送元矩形 = null, bool 描画先矩形を整数境界に合わせる = false, Matrix? 変換行列3D = null, LayerParameters1? レイヤーパラメータ = null )
		{
			Debug.Assert( this.活性化している );

			gd.D2DBatchDraw( ( dc ) => {
				this.描画する( dc, 左位置, 上位置, 不透明度0to1, X方向拡大率, Y方向拡大率, 転送元矩形, 描画先矩形を整数境界に合わせる, 変換行列3D, レイヤーパラメータ );
			} );
		}
		public virtual void 描画する( DeviceContext dc, float 左位置, float 上位置, float 不透明度0to1 = 1.0f, float X方向拡大率 = 1.0f, float Y方向拡大率 = 1.0f, RectangleF? 転送元矩形 = null, bool 描画先矩形を整数境界に合わせる = false, Matrix? 変換行列3D = null, LayerParameters1? レイヤーパラメータ = null )
		{
			Debug.Assert( this.活性化している );

			if( null == this._Bitmap )
				return;

			if( ( null == 転送元矩形 ) || ( false == 転送元矩形.HasValue ) )
			{
				転送元矩形 = new RectangleF( 0f, 0f, this._Bitmap.PixelSize.Width, this._Bitmap.PixelSize.Height );
			}

			RectangleF 転送先矩形;

			転送先矩形 = new RectangleF(
				x: 左位置,
				y: 上位置,
				width: 転送元矩形.Value.Width * X方向拡大率,
				height: 転送元矩形.Value.Height * Y方向拡大率 );

			if( 描画先矩形を整数境界に合わせる )
			{
				転送先矩形.X = (float) Math.Round( 転送先矩形.X );
				転送先矩形.Y = (float) Math.Round( 転送先矩形.Y );
				転送先矩形.Width = (float) Math.Round( 転送先矩形.Width );
				転送先矩形.Height = (float) Math.Round( 転送先矩形.Height );
			}

			// ブレンドモードをD2Dレンダーターゲットに設定する。
			dc.PrimitiveBlend = ( this.加算合成 ) ? PrimitiveBlend.Add : PrimitiveBlend.SourceOver;

			using( var layer = new Layer( dc ) )
			{
				// レイヤーパラメータの指定があれば、描画前に Push する。
				if( null != レイヤーパラメータ )
					dc.PushLayer( (LayerParameters1) レイヤーパラメータ, layer );

				// D2Dレンダーターゲットに Bitmap を描画する。
				dc.DrawBitmap(
					bitmap: this._Bitmap,
					destinationRectangle: 転送先矩形,
					opacity: 不透明度0to1,
					interpolationMode: this.補正モード,
					sourceRectangle: 転送元矩形,
					erspectiveTransformRef: 変換行列3D ); // null 指定可。

				// レイヤーパラメータの指定があれば、描画後に Pop する。
				if( null != レイヤーパラメータ )
					dc.PopLayer();
			}
		}

		/// <summary>
		///		画像を描画する。
		/// </summary>
		/// <param name="gd">グラフィックデバイス。</param>
		/// <param name="変換行列2D">Transform に適用する行列。</param>
		/// <param name="変換行列3D">射影行列。</param>
		/// <param name="不透明度0to1">不透明度。(0:透明～1:不透明)</param>
		/// <param name="転送元矩形">描画する画像範囲。</param>
		public virtual void 描画する( グラフィックデバイス gd, Matrix3x2? 変換行列2D = null, Matrix? 変換行列3D = null, float 不透明度0to1 = 1.0f, RectangleF? 転送元矩形 = null, LayerParameters1? レイヤーパラメータ = null )
		{
			Debug.Assert( this.活性化している );

			if( null == this._Bitmap )
				return;

			gd.D2DBatchDraw( ( dc ) => {

				// 変換行列とブレンドモードをD2Dレンダーターゲットに設定する。
				dc.Transform = ( 変換行列2D ?? Matrix3x2.Identity ) * dc.Transform;
				dc.PrimitiveBlend = ( this.加算合成 ) ? PrimitiveBlend.Add : PrimitiveBlend.SourceOver;

				using( var layer = new Layer( dc ) )
				{
					// レイヤーパラメータの指定があれば、描画前に Push する。
					if( null != レイヤーパラメータ )
						dc.PushLayer( (LayerParameters1) レイヤーパラメータ, layer );

					// D2Dレンダーターゲットに this.Bitmap を描画する。
					dc.DrawBitmap(
						bitmap: this._Bitmap,
						destinationRectangle: null,
						opacity: 不透明度0to1,
						interpolationMode: this.補正モード,
						sourceRectangle: 転送元矩形,
						erspectiveTransformRef: 変換行列3D ); // null 指定可。

					// レイヤーパラメータの指定があれば、描画後に Pop する。
					if( null != レイヤーパラメータ )
						dc.PopLayer();
				}

			} );
		}


		protected string _画像ファイルパス = null;

		protected Bitmap1 _Bitmap = null;


		protected void _画像を生成する( グラフィックデバイス gd, BitmapProperties1 bitmapProperties1 = null )
		{
			var decoder = (BitmapDecoder) null;
			var sourceFrame = (BitmapFrameDecode) null;
			var converter = (FormatConverter) null;

			string 変数付きファイルパス = Folder.絶対パスをフォルダ変数付き絶対パスに変換して返す( this._画像ファイルパス ); // Log出力用
			try
			{
				#region " 画像ファイルパスの有効性を確認する。"
				//-----------------
				if( this._画像ファイルパス.Nullまたは空である() )
				{
					Log.ERROR( $"画像ファイルパスが null または空文字列です。[{変数付きファイルパス}]" );
					return;
				}
				if( false == System.IO.File.Exists( this._画像ファイルパス ) )
				{
					Log.ERROR( $"画像ファイルが存在しません。[{変数付きファイルパス}]" );
					return;
				}
				//-----------------
				#endregion

				#region " 画像ファイルに対応できるデコーダを見つける。"
				//-----------------
				try
				{
					decoder = new BitmapDecoder(
						gd.WicImagingFactory,
						this._画像ファイルパス,
						SharpDX.IO.NativeFileAccess.Read,
						DecodeOptions.CacheOnLoad );
				}
				catch( SharpDXException e )
				{
					Log.ERROR( $"画像ファイルに対応するコーデックが見つかりません。(0x{e.HResult:x8})[{変数付きファイルパス}]" );
					return;
				}
				//-----------------
				#endregion

				#region " 最初のフレームをデコードし、取得する。"
				//-----------------
				try
				{
					sourceFrame = decoder.GetFrame( 0 );
				}
				catch( SharpDXException e )
				{
					Log.ERROR( $"画像ファイルの最初のフレームのデコードに失敗しました。(0x{e.HResult:x8})[{変数付きファイルパス}]" );
					return;
				}
				//-----------------
				#endregion

				#region " 32bitPBGRA へのフォーマットコンバータを生成する。"
				//-----------------
				try
				{
					// WICイメージングファクトリから新しいコンバータを生成。
					converter = new FormatConverter( gd.WicImagingFactory );

					// コンバータに変換元フレームや変換後フォーマットなどを設定。
					converter.Initialize(
						sourceRef: sourceFrame,
						dstFormat: SharpDX.WIC.PixelFormat.Format32bppPBGRA,	// Premultiplied BGRA
						dither: BitmapDitherType.None,
						paletteRef: null,
						alphaThresholdPercent: 0.0,
						paletteTranslate: BitmapPaletteType.MedianCut );
				}
				catch( SharpDXException e )
				{
					Log.ERROR( $"32bitPBGRA へのフォーマットコンバータの生成または初期化に失敗しました。(0x{e.HResult:x8})[{変数付きファイルパス}]" );
					return;
				}
				//-----------------
				#endregion

				#region " コンバータを使って、フレームを WICビットマップ経由で D2D ビットマップに変換する。"
				//-----------------
				try
				{
					// WIC ビットマップを D2D ビットマップに変換する。
					this._Bitmap?.Dispose();
					this._Bitmap = Bitmap1.FromWicBitmap(
						gd.D2DDeviceContext,
						converter,
						bitmapProperties1 );
				}
				catch( SharpDXException e )
				{
					Log.ERROR( $"Direct2D1.Bitmap1 への変換に失敗しました。(0x{e.HResult:x8})[{変数付きファイルパス}]" );
					return;
				}
				//-----------------
				#endregion

				//Log.Info( $"{FDKUtilities.現在のメソッド名}: 画像を生成しました。[{変数付きファイルパス}]" );
			}
			finally
			{
				converter?.Dispose();
				sourceFrame?.Dispose();
				decoder?.Dispose();
			}
		}
	}
}
