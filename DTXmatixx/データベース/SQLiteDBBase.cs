using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Data.Linq.Mapping;
using System.Data.SQLite;
using System.Diagnostics;
using System.Linq;
using FDK;

namespace DTXmatixx.データベース
{
	/// <summary>
	///		SQLiteのデータベースを操作するクラスの共通機能。
	/// </summary>
	abstract class SQLiteDBBase : IDisposable
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

		public SQLiteDBBase( VariablePath DBファイルパス, long Version )
		{
			if( 0 == Version )
				throw new Exception( "Version = 0 は予約されています。" );

			this._DBファイルパス = DBファイルパス;
			this._DB接続文字列 = new SQLiteConnectionStringBuilder() { DataSource = this._DBファイルパス.変数なしパス }.ToString();

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
				this.UserVersion = Version;
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
				this.UserVersion = Version;
			}
			else
			{
				// (D) 実DBが上位バージョンである　→　例外発出。上位互換はなし。
				throw new Exception( $"データベースが未知のバージョン({実DBのバージョン})です。" );
			}
		}
		public SQLiteDBBase( string DBファイルパス, long Version )
			: this( DBファイルパス?.ToVariablePath(), Version )
		{
		}
		public void Dispose()
		{
			//this.DataContext?.SubmitChanges();	--> Submit していいとは限らない。
			this.DataContext?.Dispose();

			this.Connection?.Close();
			this.Connection?.Dispose();
		}

		protected VariablePath _DBファイルパス;
		protected string _DB接続文字列;

		// 以下、派生クラスで実装する。

		protected virtual void テーブルがなければ作成する()
		{
			throw new NotImplementedException();
		}

		/// <summary>
		///		{移行元DBバージョン} から {移行元DBバージョン+1} へ、１つだけマイグレーションする。
		/// </summary>
		protected virtual void データベースのアップグレードマイグレーションを行う( long 移行元DBバージョン )
		{
			throw new NotImplementedException();
		}
	}
}
