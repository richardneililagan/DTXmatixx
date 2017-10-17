using System;

namespace FDK
{
	class FDKException : Exception
	{
		public FDKException()
			: base()
		{
			Log.ERROR( "" );
		}

		public FDKException( string msg ) 
			: base( msg )
		{
			Log.ERROR( msg );
		}

		public FDKException( string msg, Exception inner )
			: base( msg, inner )
		{
			Log.ERROR( msg );
		}
	}
}
