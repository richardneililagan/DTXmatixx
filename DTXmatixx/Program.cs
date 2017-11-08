using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Windows.Forms;
using FDK;
using DTXmatixx.Viewer;

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
				string serviceUri = "net.pipe://localhost/DTXMania";
				string endPointName = "Viewer";
				string endPointUri = $"{serviceUri}/{endPointName}";

				// アプリのWCFサービスホストを生成する。
				var serviceHost = new ServiceHost( app, new Uri( serviceUri ) );

				// 名前付きパイプにバインドしたエンドポイントをサービスホストへ追加する。
				serviceHost.AddServiceEndpoint(
					typeof( IDTXManiaService ),
					new NetNamedPipeBinding( NetNamedPipeSecurityMode.None ),
					endPointName );

				// サービスの受付を開始する。
				try
				{
					serviceHost.Open();
				}
				catch( AddressAlreadyInUseException )
				{
					MessageBox.Show( "DTXMania はすでに起動しています。多重起動はできません。", "DTXMania Runtime Error", MessageBoxButtons.OK, MessageBoxIcon.Error );
					return;
				}

				// アプリを実行する。
				try
				{
					app.Run();
				}
				finally
				{
					// サービスの受付を終了する。
					serviceHost.Close( new TimeSpan( 0, 0, 2 ) );   // 最大2sec待つ
				}

				Log.Header( "アプリケーションを終了します。" );
			}
			Log.Header( "アプリケーションを終了しました。" );

			Trace.WriteLine( "" );
			Trace.WriteLine( "遊んでくれてありがとう！" );
		}
	}
}
