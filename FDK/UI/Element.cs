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
	public class Element : IDisposable
	{
		public System.Drawing.PointF Location
		{
			get;
			set;
		} = new System.Drawing.PointF( 0f, 0f );


		public Element()
		{
		}

		public void Dispose()
		{
			// (1) 自分を解放する。
			this.OnDispose();

			// (2) 子を解放する。
			foreach( var child in this._子要素リスト )
				child.Dispose();
			this._子要素リスト.Clear();
		}

		public void AddChild( Element child, bool isAbove, Element refChild = null )
		{
			if( isAbove )
			{
				this._子要素リスト.Insert( 0, child );
			}
			else if( null != refChild )
			{
				int n = this._子要素リスト.IndexOf( refChild );

				if( 0 <= n )
				{
					this._子要素リスト.Insert( n + 1, child );
				}
				else
				{
					Log.ERRORandTHROW( "指定された要素が存在しません。", new ArgumentException() );
				}
			}
			else
			{
				Log.ERRORandTHROW( "指定された要素が無効です。", new ArgumentNullException() );
			}
		}

		internal void Render( グラフィックデバイス gd, PointF upperLeft )
		{
			// (1) 自分を描画する。
			this.OnRender( gd, upperLeft );

			// (2) 子要素を昇順に描画する。
			upperLeft += new SizeF( this.Location );
			for( int i = 0; i < this._子要素リスト.Count; i++ )
				this._子要素リスト[ i ].Render( gd, upperLeft );
		}


		protected readonly List<Element> _子要素リスト = new List<Element>();

		// 以下、派生クラスで実装する。

		protected virtual void OnRender( グラフィックデバイス gd, PointF upperLeft ) { }

		protected virtual void OnDispose() { }
	}
}
