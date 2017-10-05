using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Linq;
using System.Data.SQLite;
using System.Diagnostics;
using System.IO;
using System.Linq;
using FDK;
using DTXmatixx.ステージ.演奏;

namespace DTXmatixx.設定.DB
{
	class UsersDB : IDisposable
	{
		public UsersDB( string DBファイルパス )
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
					// Users テーブルを作成。
					command.CommandText = User.CreateTable;
					command.ExecuteNonQuery();

					// Users テーブルにインデックスを作成。
					command.CommandText = User.CreateIndex;
					command.ExecuteNonQuery();
				}

				// Users テーブルに "AutoPlayer" ユーザがいないなら、追加する。
				using( var context = new DataContext( DB接続 ) )
				{
					var table = context.GetTable<User>();
					var queryAutoPlayer = from user in table where ( user.Id == "AutoPlayer" ) select user;

					if( 0 == queryAutoPlayer.Count() )
					{
						table.InsertOnSubmit( new User() {
							Id = "AutoPlayer",
							Name = "AutoPlayer",
							AutoPlayLeftCymbal = 1,
							AutoPlayHiHat = 1,
							AutoPlayLeftPedal = 1,
							AutoPlaySnare = 1,
							AutoPlayBass = 1,
							AutoPlayHighTom = 1,
							AutoPlayLowTom = 1,
							AutoPlayFloorTom = 1,
							AutoPlayRightCymbal = 1,
							MaxRangePerfect = User.最大ヒット距離secの規定値[ 判定種別.PERFECT ],
							MaxRangeGreat = User.最大ヒット距離secの規定値[ 判定種別.GREAT ],
							MaxRangeGood = User.最大ヒット距離secの規定値[ 判定種別.GOOD ],
							MaxRangeOk = User.最大ヒット距離secの規定値[ 判定種別.OK ],
							ScrollSpeed = 1.0,
							Fullscreen = 0,
						} );
					}

					context.SubmitChanges();
				}
			}
		}

		/// <summary>
		///		データベースからユーザの情報を検索して返す。
		///		存在しなかったら null を返す。
		/// </summary>
		public User ユーザの情報を返す( string id )
		{
			var DC接続文字列 = new SQLiteConnectionStringBuilder { DataSource = this._DBファイルパス };
			using( var DB接続 = new SQLiteConnection( DC接続文字列.ToString() ) )
			{
				DB接続.Open();

				using( var context = new DataContext( DB接続 ) )
				{
					var table = context.GetTable<User>();
					var query = from u in table where ( u.Id == id ) select u;

					foreach( var u in query )
						return u;   // あっても1個しかないはずなので即返す。
				}
			}

			return null;
		}


		/// <summary>
		///		コンストラクタで渡された、データベースファイルの絶対パス。
		///		フォルダ変数は展開済み。
		/// </summary>
		private string _DBファイルパス;
	}
}
