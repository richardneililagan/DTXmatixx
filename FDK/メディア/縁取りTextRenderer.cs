using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SharpDX;
using SharpDX.Direct2D1;
using SharpDX.DirectWrite;
using SharpDX.Mathematics.Interop;

namespace FDK.メディア
{
	/// <summary>
	///		文字列を好きなサイズで縁取りできるカスタムテキストレンダラー。
	///		<see cref="SharpDX.DirectWrite.TextLayout"/>.Draw() の引数に渡して使う。
	/// </summary>
	public class 縁取りTextRenderer : TextRendererBase
	{
		public 縁取りTextRenderer( SharpDX.Direct2D1.Factory d2dFactory, RenderTarget renderTarget, Brush 輪郭ブラシ, Brush 塗りつぶしブラシ, float 輪郭の太さ = 1.0f )
		{
			this._d2dFactory = d2dFactory;
			this._renderTarget = renderTarget;
			this._輪郭ブラシ = 輪郭ブラシ;
			this._塗りつぶしブラシ = 塗りつぶしブラシ;
			this._輪郭の太さ = 輪郭の太さ;
		}

		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				// コンストラクタで受け取ったこれらのオブジェクトは、呼び出し元で解放すること。（参照カウンタをAddしてないので）
				//FDKUtilities.解放する( ref this._塗りつぶしブラシ );
				//FDKUtilities.解放する( ref this._輪郭ブラシ );
				//FDKUtilities.解放する( ref this._renderTarget );
				//FDKUtilities.解放する( ref this._d2dFactory );
			}
		}

		public override Result DrawGlyphRun( object clientDrawingContext, float baselineOriginX, float baselineOriginY, MeasuringMode measuringMode, GlyphRun glyphRun, GlyphRunDescription glyphRunDescription, ComObject clientDrawingEffect )
		{
			using( var パスジオメトリ = new PathGeometry( this._d2dFactory ) )
			{
				using( var sink = パスジオメトリ.Open() )
				{
					glyphRun.FontFace.GetGlyphRunOutline(
						glyphRun.FontSize,
						glyphRun.Indices,
						glyphRun.Advances,
						glyphRun.Offsets,
						glyphRun.IsSideways,
						( 1 == ( glyphRun.BidiLevel % 2 ) ),    // 奇数ならtrue
						sink );

					sink.Close();
				}

				// グリフ実行の原点を、適切なベースラインの原点からレンダリングされるように移動する行列。
				var matrix = new Matrix3x2(
					1.0f, 0.0f,
					0.0f, 1.0f,
					baselineOriginX, baselineOriginY );

				using( var 変換済みジオメトリ = new TransformedGeometry( this._d2dFactory, パスジオメトリ, matrix ) )
				{
					this._renderTarget.DrawGeometry( 変換済みジオメトリ, this._輪郭ブラシ, this._輪郭の太さ );
					this._renderTarget.FillGeometry( 変換済みジオメトリ, this._塗りつぶしブラシ );
				}
			}
			return Result.Ok;
		}

		public override bool IsPixelSnappingDisabled( object clientDrawingContext )
			=> false;

		public override RawMatrix3x2 GetCurrentTransform( object clientDrawingContext )
			=> this._renderTarget.Transform;

		public override float GetPixelsPerDip( object clientDrawingContext )
			=> this._renderTarget.DotsPerInch.Width / 96f;

		private SharpDX.Direct2D1.Factory _d2dFactory = null;
		private RenderTarget _renderTarget = null;
		private Brush _輪郭ブラシ = null;
		private Brush _塗りつぶしブラシ = null;
		private float _輪郭の太さ = 1.0f;
	}
}
