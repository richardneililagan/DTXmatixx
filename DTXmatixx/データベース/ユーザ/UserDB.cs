using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Diagnostics;
using System.Linq;
using FDK;
using DTXmatixx.ステージ.演奏;

namespace DTXmatixx.データベース.ユーザ
{
	/// <summary>
	///		ユーザデータベースに対応するエンティティクラス。
	/// </summary>
	class UserDB : SQLiteDBBase
	{
		public const long VERSION = 1;

		public Table<User> Users
			=> base.DataContext.GetTable<User>();

		public Table<Record> Records
			=> base.DataContext.GetTable<Record>();

		public UserDB()
			: base( @"$(AppData)UserDB.sqlite3", VERSION )
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
						// テーブルを作成する。
						this.DataContext.ExecuteCommand( User.CreateTableSQL );
						this.DataContext.ExecuteCommand( Record.CreateTableSQL );
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
