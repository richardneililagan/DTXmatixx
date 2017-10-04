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
	/// <summary>
	///		曲の情報をキャッシュしておくデータベースを管理するためのクラス。
	/// </summary>
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
					// Songs テーブルを作成。
					command.CommandText = Song.CreateTable;
					command.ExecuteNonQuery();

					// Songs テーブルにインデックスを作成。
					command.CommandText = Song.CreateIndex;
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
					var table = context.GetTable<Song>();
					var query = from s in table where ( s.Path == songFile ) select s;

					// (A) 同じパスのレコードが存在しなかったら、追加する。
					if( 0 == query.Count() )
					{
						table.InsertOnSubmit( new Song() {
							Path = songFile,
							LastWriteTime = File.GetLastWriteTime( songFile ).ToString( "G" ),
						} );
						Log.Info( $"DBに曲を追加しました。{曲ファイルパス}" );
					}
					else
					{
						// (B) 同じパスのレコードが存在する場合、最終更新日時が違ったら、レコードを更新する。
						foreach( var song in query )	// 1個しかないはずだが First() が使えないので MSDN のマネ。
						{
							string 既存レコードの最終更新日時 = song.LastWriteTime;
							string 曲ファイルの最終更新日時 = File.GetLastWriteTime( songFile ).ToString( "G" );

							if( 既存レコードの最終更新日時 != 曲ファイルの最終更新日時 )
							{
								song.LastWriteTime = 曲ファイルの最終更新日時;
								Log.Info( $"DBの曲の情報を更新しました。{曲ファイルパス}" );
							}
						}
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
