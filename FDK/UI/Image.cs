using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using SharpDX.Direct2D1;
using FDK;
using FDK.メディア;

namespace FDK.UI
{
	public class Image : Element
	{
		public Size2F Size
		{
			get
				=> this._画像.サイズ;
		}


		public Image( グラフィックデバイス gd, string imagePath )
		{
			this._画像 = new 描画可能画像( imagePath );
			this._画像.活性化する( gd );
		}
		public Image( グラフィックデバイス gd, Size2F size )
		{
			this._画像 = new 描画可能画像( new Size2( (int) size.Width, (int) size.Height ) );
			this._画像.活性化する( gd );
		}

		protected override void OnDispose()
		{
			this._画像?.非活性化する( null );   // このクラスは null でも OK
			this._画像 = null;
		}

		public void DrawToImage( グラフィックデバイス gd, Action<DeviceContext1> action )
		{
			this._画像.画像へ描画する( gd, action );
		}

		protected override void OnRender( グラフィックデバイス gd, PointF upperLeft )
		{
			upperLeft += new SizeF( this.Location );
			this._画像.描画する( gd, upperLeft.X, upperLeft.Y );
		}


		protected 描画可能画像 _画像 = null;
	}
}
