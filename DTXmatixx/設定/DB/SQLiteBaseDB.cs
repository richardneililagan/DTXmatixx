using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Data.Linq.Mapping;
using System.Data.SQLite;
using System.Diagnostics;
using System.Linq;
using FDK;

namespace DTXmatixx.設定.DB
{
	/// <summary>
	///		SQLiteのデータベースを操作するクラスの共通機能。
	/// </summary>
	abstract class SQLiteBaseDB : IDisposable
	{
		public long UserVersion
		{
			get
				=> this.DataContext.ExecuteQuery<long>( @"PRAGMA user_version" ).FirstOrDefault();

			set
				=> this.DataContext.ExecuteCommand( $"PRAGMA user_version = {value}" );
		}
		public SQLiteConnection Connection
		{
			get;
			protected set;
		} = null;
		public DataContext DataContext
		{
			get;
			protected set;
		} = null;

		public SQLiteBaseDB( string DBファイルパス, long Version )
		{
			if( 0 == Version )
				throw new Exception( "Version = 0 は予約されています。" );

			this._DBファイルパス = DBファイルパス;

			this._DB接続文字列 = new SQLiteConnectionStringBuilder() { DataSource = Folder.絶対パスに含まれるフォルダ変数を展開して返す( this._DBファイルパス ) }.ToString();

			this.Connection = new SQLiteConnection( this._DB接続文字列 );
			this.Connection.Open();

			this.DataContext = new DataContext( this.Connection );

			// マイグレーションが必要？

			var 実DBのバージョン = this.UserVersion;	// DBが存在しない場合は 0 。

			if( 実DBのバージョン == Version )
			{
				// (A) マイグレーション不要。
			}
			else if( 実DBのバージョン == 0 )
			{
				// (B) 実DBが存在していない　→　作成する。
				this.テーブルがなければ作成する();
			}
			else if( 実DBのバージョン < Version )
			{
				// (C) 実DBが下位バージョンである　→　アップグレードする。
				while( 実DBのバージョン < Version )
				{
					// 1バージョンずつアップグレード。
					this.データベースのアップグレードマイグレーションを行う( 実DBのバージョン );
					実DBのバージョン++;
				}
			}
			else
			{
				// (D) 実DBが上位バージョンである　→　例外発出。上位互換はなし。
				throw new Exception( $"データベースが未知のバージョン({実DBのバージョン})です。" );
			}
		}
		public void Dispose()
		{
			//this.DataContext?.SubmitChanges();	--> Submit していいとは限らない。
			this.DataContext?.Dispose();

			this.Connection?.Close();
			this.Connection?.Dispose();
		}

		protected string _DBファイルパス;
		protected string _DB接続文字列;

		// 以下、派生クラスで実装する。

		protected virtual void テーブルがなければ作成する()
		{
			throw new NotImplementedException();
		}
		protected virtual void データベースのアップグレードマイグレーションを行う( long 移行元DBバージョン )
		{
			throw new NotImplementedException();
		}
	}
}
