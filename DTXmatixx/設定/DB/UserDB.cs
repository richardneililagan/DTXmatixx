using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Diagnostics;
using System.Linq;
using FDK;
using DTXmatixx.ステージ.演奏;

namespace DTXmatixx.設定.DB
{
	/// <summary>
	///		ユーザデータベースに対応するエンティティクラス。
	/// </summary>
	class UserDB : SQLiteBaseDB
	{
		public const long VERSION = 1;

		public Table<User> Users
		{
			get
				=> base.DataContext.GetTable<User>();
		}
		public Table<Record> Records
		{
			get
				=> base.DataContext.GetTable<Record>();
		}

		public UserDB()
			: base( Folder.絶対パスに含まれるフォルダ変数を展開して返す( @"$(AppData)UserDB.sqlite3" ), VERSION )
		{
		}

		protected override void テーブルがなければ作成する()
		{
			using( Log.Block( FDKUtilities.現在のメソッド名 ) )
			{
				using( var transaction = this.Connection.BeginTransaction() )
				{
					try
					{
						// DBのバージョンを設定する。
						this.UserVersion = VERSION;

						// テーブルを作成する。
						this.DataContext.ExecuteCommand( User.CreateTableSQL );
						this.DataContext.ExecuteCommand( Record.CreateTableSQL );

						#region " Users テーブルに "AutoPlayer" ユーザがいないなら、追加する。"
						//----------------
						{
							var queryAutoPlayer = from user in this.Users where ( user.Id == "AutoPlayer" ) select user;

							if( 0 == queryAutoPlayer.Count() )
							{
								this.Users.InsertOnSubmit( new User() {
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
									// 他は規定値。
								} );
							}
						}
						//----------------
						#endregion

						this.DataContext.SubmitChanges();

						// 成功。
						transaction.Commit();
					}
					catch
					{
						// 失敗。
						transaction.Rollback();
					}
				}
			}
		}
		protected override void データベースのアップグレードマイグレーションを行う( long 移行元DBバージョン )
		{
			switch( 移行元DBバージョン )
			{
				default:
					throw new Exception( $"移行元DBのバージョン({移行元DBバージョン})がマイグレーションに未対応です。" );
			}
		}
	}
}
