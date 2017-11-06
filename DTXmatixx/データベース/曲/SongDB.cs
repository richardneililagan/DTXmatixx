using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Diagnostics;
using System.Linq;
using FDK;

namespace DTXmatixx.データベース.曲
{
	/// <summary>
	///		曲データベースに対応するエンティティクラス。
	/// </summary>
	class SongDB : SQLiteDBBase
	{
		public const long VERSION = 1;

		public Table<Song> Songs
			=> base.DataContext.GetTable<Song>();

		public SongDB()
			: base(  @"$(AppData)SongDB.sqlite3", VERSION )
		{
		}

		protected override void テーブルがなければ作成する()
		{
			using( var transaction = this.Connection.BeginTransaction() )
			{
				try
				{
					// テーブルを作成する。
					this.DataContext.ExecuteCommand( Song.CreateTableSQL );
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
