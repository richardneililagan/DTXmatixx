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
		public const long VERSION = 2;

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
						this.DataContext.ExecuteCommand( $"CREATE TABLE IF NOT EXISTS Users {User.ColumnsList};" );
						this.DataContext.ExecuteCommand( $"CREATE TABLE IF NOT EXISTS Records {Record.ColumnList};" );
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
				case 1:
					#region " 1 → 2 "
					//----------------
					// 変更点:
					// ・Users テーブルから SongFolders カラムを削除。
					this.DataContext.ExecuteCommand( "PRAGMA foreign_keys = OFF" );
					this.DataContext.SubmitChanges();
					using( var transaction = this.Connection.BeginTransaction() )
					{
						try
						{
							// テータベースをアップデートしてデータを移行する。
							this.DataContext.ExecuteCommand( $"CREATE TABLE new_Users {User.ColumnsList}" );
							this.DataContext.ExecuteCommand( "INSERT INTO new_Users SELECT Id,Name,ScrollSpeed,Fullscreen,AutoPlay_LeftCymbal,AutoPlay_HiHat,AutoPlay_LeftPedal,AutoPlay_Snare,AutoPlay_Bass,AutoPlay_HighTom,AutoPlay_LowTom,AutoPlay_FloorTom,AutoPlay_RightCymbal,MaxRange_Perfect,MaxRange_Great,MaxRange_Good,MaxRange_Ok,CymbalFree FROM Users" );
							this.DataContext.ExecuteCommand( "DROP TABLE Users" );
							this.DataContext.ExecuteCommand( "ALTER TABLE new_Users RENAME TO Users" );
							this.DataContext.ExecuteCommand( "PRAGMA foreign_keys = ON" );
							this.DataContext.SubmitChanges();

							// 成功。
							transaction.Commit();
							Log.Info( "Users テーブルをアップデートしました。[1→2]" );
						}
						catch
						{
							// 失敗。
							transaction.Rollback();
							Log.ERROR( "Users テーブルのアップデートに失敗しました。[1→2]" );
						}
					}
					//----------------
					#endregion
					break;

				default:
					throw new Exception( $"移行元DBのバージョン({移行元DBバージョン})がマイグレーションに未対応です。" );
			}
		}
	}
}
