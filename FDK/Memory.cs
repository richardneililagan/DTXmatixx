using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace FDK
{
	/// <summary>
	/// ガベージコレクション対象外のメモリの確保と解放。
	/// （引用元: https://msdn.microsoft.com/ja-jp/library/aa664786(v=vs.71).aspx ）
	/// </summary>
	public unsafe class Memory
	{
		// Handle for the process heap. This handle is used in all calls to the
		// HeapXXX APIs in the methods below.
		static int ph = GetProcessHeap();

		// Private instance constructor to prevent instantiation.
		private Memory()
		{
		}

		// Allocates a memory block of the given size. The allocated memory is
		// automatically initialized to zero.
		public static void* Alloc( int size )
		{
			void* result = HeapAlloc( ph, HEAP_ZERO_MEMORY, size );
			if( result == null ) throw new OutOfMemoryException();

			IntPtr pr = new IntPtr( result );
			//Debug.WriteLine( $"HeapAlloc, {size}bytes, address={pr.ToString()}" );

			return result;
		}

		// Copies count bytes from src to dst. The source and destination
		// blocks are permitted to overlap.
		public static void Copy( void* src, void* dst, int count )
		{
			byte* ps = (byte*) src;
			byte* pd = (byte*) dst;
			if( ps > pd )
			{
				for( ; count != 0; count-- ) *pd++ = *ps++;
			}
			else if( ps < pd )
			{
				for( ps += count, pd += count; count != 0; count-- ) *--pd = *--ps;
			}
		}

		// ゼロで埋める。
		public static void Zero( void* dst, int count )
		{
			byte* pd = (byte*) dst;
			for( ; count != 0; count-- ) *pd++ = 0;
		}

		// Frees a memory block.
		public static void Free( void* block )
		{
			IntPtr pr = new IntPtr( block );
			//Debug.WriteLine( $"HeapFree, address={pr.ToString()}" );

			if( !HeapFree( ph, 0, block ) ) throw new InvalidOperationException();
		}

		// Re-allocates a memory block. If the reallocation request is for a
		// larger size, the additional region of memory is automatically
		// initialized to zero.
		public static void* ReAlloc( void* block, int size )
		{
			void* result = HeapReAlloc( ph, HEAP_ZERO_MEMORY, block, size );
			if( result == null ) throw new OutOfMemoryException();
			return result;
		}

		// Returns the size of a memory block.
		public static int SizeOf( void* block )
		{
			int result = HeapSize( ph, 0, block );
			if( result == -1 ) throw new InvalidOperationException();
			return result;
		}

		// Heap API flags
		const int HEAP_ZERO_MEMORY = 0x00000008;

		// Heap API functions
		[DllImport( "kernel32" )]
		static extern int GetProcessHeap();

		[DllImport( "kernel32" )]
		static extern void* HeapAlloc( int hHeap, int flags, int size );

		[DllImport( "kernel32" )]
		static extern bool HeapFree( int hHeap, int flags, void* block );

		[DllImport( "kernel32" )]
		static extern void* HeapReAlloc( int hHeap, int flags, void* block, int size );

		[DllImport( "kernel32" )]
		static extern int HeapSize( int hHeap, int flags, void* block );
	}
}
