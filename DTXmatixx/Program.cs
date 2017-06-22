using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using FDK;

namespace DTXmatixx
{
	static class Program
	{
		/// <summary>
		/// アプリケーションのメイン エントリ ポイントです。
		/// </summary>
		[STAThread]
		static void Main()
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault( false );

			Log.現在のスレッドに名前をつける( "描画" );
			Log.Header( "アプリケーションを起動します。" );

			using( var app = new App() )
			{
				app.Run();
				Log.Header( "アプリケーションを終了します。" );
			}
			Log.Header( "アプリケーションを終了しました。" );
		}
	}
}
