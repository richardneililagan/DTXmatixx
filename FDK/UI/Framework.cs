using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FDK;
using FDK.メディア;

namespace FDK.UI
{
	public class Framework : IDisposable
	{
		public Element Root
		{
			get;
			set;
		} = null;


		public Framework()
		{
		}

		public void Dispose()
		{
			this.Clear();
		}

		public void Clear()
		{
			this.Root?.Dispose();
			this.Root = null;
		}

		public void Render( グラフィックデバイス gd )
		{
			this.Root?.Render( gd, new PointF( 0f, 0f ) );	// Root == null は OK
		}
	}
}
