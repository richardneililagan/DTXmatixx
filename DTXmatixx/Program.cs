using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
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

			#region " ログファイルへのログの複製出力開始 "
			//----------------
			Trace.AutoFlush = true;
			try
			{
				var AppDataフォルダ名 = Path.Combine( Environment.GetFolderPath( Environment.SpecialFolder.ApplicationData, Environment.SpecialFolderOption.Create ), @"DTXMatixx\" );
				if( !( Directory.Exists( AppDataフォルダ名 ) ) )
					Directory.CreateDirectory( AppDataフォルダ名 );   // なければ作成。

				var ログフォルダ名 = Path.Combine( AppDataフォルダ名, "Logs" );
				if( !( Directory.Exists( ログフォルダ名 ) ) )
					Directory.CreateDirectory( ログフォルダ名 );   // なければ作成。

				var ログファイル名 = Path.Combine( ログフォルダ名, "Log." + DateTime.Now.ToString( "yyyyMMdd.HHmmss" ) + ".txt" );

				Trace.Listeners.Add( new TraceLogListener( new StreamWriter( ログファイル名, false, Encoding.GetEncoding( "utf-8" ) ) ) );
			}
			catch( UnauthorizedAccessException )
			{
				MessageBox.Show( "Failed to create log file.", "DTXMania boot error", MessageBoxButtons.OK, MessageBoxIcon.Error );
				Environment.Exit( 1 );
			}
			//----------------
			#endregion

			Log.現在のスレッドに名前をつける( "描画" );
			Log.Header( "アプリケーションを起動します。" );

			using( var app = new App() )
			{
				app.Run();
				Log.Header( "アプリケーションを終了します。" );
			}
			Log.Header( "アプリケーションを終了しました。" );

			Trace.WriteLine( "" );
			Trace.WriteLine( "遊んでくれてありがとう！" );
		}
	}
}
