using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SharpDX;

namespace FDK
{
	public static class Extensions
	{
		// SharpDX.IUnknown 

		/// <summary>
		///		COM オブジェクトの参照カウントを取得して返す。
		/// </summary>
		/// <param name="unknownObject">COMオブジェクト。</param>
		/// <returns>現在の参照カウントの値。</returns>
		public static int GetRefferenceCount( this IUnknown unknownObject )
		{
			try
			{
				unknownObject.AddReference();
			}
			catch( InvalidOperationException )
			{
				// すでに Dispose されている。
				return 0;
			}

			return unknownObject.Release();
		}


		// System.String

		/// <summary>
		///		文字列が Null でも空でもないなら true を返す。
		/// </summary>
		public static bool Nullでも空でもない( this string 検査対象 )
			=> !( string.IsNullOrEmpty( 検査対象 ) );
		
		/// <summary>
		///		文字列が Null または空なら true を返す。
		/// </summary>
		public static bool Nullまたは空である( this string 検査対象 )
			=> string.IsNullOrEmpty( 検査対象 );


		// SharpDX.Size2F

		/// <summary>
		///		SharpDX.Size2F を System.Drawing.SizeF へ変換する。
		/// </summary>
		public static System.Drawing.SizeF ToDrawingSizeF( this SharpDX.Size2F size )
			=> new System.Drawing.SizeF( size.Width, size.Height );

		/// <summary>
		///		SharpDX.Size2F を System.Drawing.Size へ変換する。
		/// </summary>
		public static System.Drawing.Size ToDrawingSize( this SharpDX.Size2F size )
			=> new System.Drawing.Size( (int) size.Width, (int) size.Height );


		// SharpDX.Size2

		/// <summary>
		///		SharpDX.Size2 を System.Drawing.SizeF へ変換する。
		/// </summary>
		public static System.Drawing.SizeF ToDrawingSizeF( this SharpDX.Size2 size )
			=> new System.Drawing.SizeF( size.Width, size.Height );

		/// <summary>
		///		SharpDX.Size2 を System.Drawing.Size へ変換する。
		/// </summary>
		public static System.Drawing.Size ToDrawingSize( this SharpDX.Size2 size )
			=> new System.Drawing.Size( size.Width, size.Height );
	}
}
