using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace FDK
{
	/// <summary>
	///		生成時にブロック開始ログ、破棄時にブロック終了ログを出力する。
	/// </summary>
	public class LogBlock : IDisposable
	{
		public LogBlock( string ブロック名 )
		{
			this._ブロック名 = ブロック名;
			Log.BeginInfo( this._ブロック名 );
		}

		public void Dispose()
		{
			Log.EndInfo( this._ブロック名 );
		}


		private string _ブロック名;
	}
}
