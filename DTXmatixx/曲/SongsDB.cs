using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Linq;
using System.Data.Linq.Mapping;
using System.Data.SQLite;
using System.Diagnostics;
using System.IO;
using System.Linq;
using FDK;

namespace DTXmatixx.曲
{
	class SongsDB : IDisposable
	{
		public SongsDB( string DBファイルパス )
		{
			this.初期化する( DBファイルパス );
		}

		public void Dispose()
		{
		}

		/// <summary>
		///		指定されたデータベースへ接続し、もし存在していないなら、テーブルとインデックスを作成する。
		/// </summary>
		/// <param name="DBファイルパス">データベースファイルの絶対パス。フォルダ変数使用可。</param>
		public void 初期化する( string DBファイルパス )
		{
			this._DBファイルパス = Folder.絶対パスに含まれるフォルダ変数を展開して返す( DBファイルパス );

			var DC接続文字列 = new SQLiteConnectionStringBuilder { DataSource = this._DBファイルパス };
			using( var DB接続 = new SQLiteConnection( DC接続文字列.ToString() ) )
			{
				DB接続.Open();

				using( var command = new SQLiteCommand( DB接続 ) )
				{
					// Songs テーブルを作成（Songsテーブルクラスの定義と対応すること）
					command.CommandText =
						$@"CREATE TABLE IF NOT EXISTS Songs ( " +
						$@"id INTEGER NOT NULL PRIMARY KEY," +	// Linq で自動増加させたい場合は、AUTOINCREMENT は使ってはならない。（生SQLからなら、使わないといけない。）
						$@"path NVARCHAR NOT NULL UNIQUE" +
						$@");";
					command.ExecuteNonQuery();

					// Songs テーブルにインデックスを作成
					command.CommandText =
						$@"CREATE INDEX IF NOT EXISTS SongsIndex ON Songs(path);";
					command.ExecuteNonQuery();
				}
			}
		}

		/// <summary>
		///		曲ファイルをデータベースに追加する。
		///		すでに存在している場合は、必要があればレコードを更新する。
		/// </summary>
		/// <param name="曲ファイルパス">曲ファイルへの絶対パス。フォルダ変数使用可。</param>
		public void 曲を追加する( string 曲ファイルパス )
		{
			string songFile = Folder.絶対パスに含まれるフォルダ変数を展開して返す( 曲ファイルパス );

			var DC接続文字列 = new SQLiteConnectionStringBuilder { DataSource = this._DBファイルパス };
			using( var DB接続 = new SQLiteConnection( DC接続文字列.ToString() ) )
			{
				DB接続.Open();

				using( var context = new DataContext( DB接続 ) )
				{
					var table = context.GetTable<Songs>();

					// 同じパスのレコードが存在しなかったら、追加する。
					var found = from s in table where ( s.Path == songFile ) select s;
					if( 0 == found.Count() )
					{
						table.InsertOnSubmit( new Songs() { Path = songFile } );
						Log.Info( $"DBに曲を追加しました。{曲ファイルパス}" );
					}

					context.SubmitChanges();
				}
			}
		}

		/// <summary>
		///		コンストラクタで渡された、データベースファイルの絶対パス。
		///		フォルダ変数は展開済み。
		/// </summary>
		private string _DBファイルパス;
	}
}
