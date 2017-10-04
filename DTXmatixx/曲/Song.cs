using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Data.Linq.Mapping;

namespace DTXmatixx.曲
{
	/// <summary>
	///		Songs テーブルの Linq 用スキーマ。
	///		SongDB 内で、曲の静的情報をキャッシュする。
	/// </summary>
	[Table( Name = "Songs" )]	// テーブル名は複数形
	public class Song	// クラスは単数形
	{
		/// <summary>
		///		一意な ID。
		///		値はDB側で自動生成されるので、INSERT 時は null を設定しておくこと。
		/// </summary>
		[Column( Name = "id", DbType = "INT", CanBeNull = false, IsPrimaryKey = true )] // Linq で自動増加させたい場合は、IsDbGenerate を指定してはならない。
		public Int32? Id { get; set; } = null;

		/// <summary>
		///		曲譜面ファイルへの絶対パス。
		///		これも一意とする。（テーブル生成SQLで UNIQUE を付与している。）
		/// </summary>
		[Column( Name = "path", DbType = "NVARCHAR", CanBeNull = false, UpdateCheck = UpdateCheck.Never )]
		public String Path { get; set; }

		/// <summary>
		///		曲譜面ファイルの最終更新時刻の文字列表記。
		///		文字列の書式は、System.DateTime.ToString("G") と同じ。（例: "08/17/2000 16:32:32"）
		///		カルチャはシステム既定のものとする。
		/// </summary>
		[Column( Name = "last_write_time", DbType = "NVARCHAR", CanBeNull = false, UpdateCheck = UpdateCheck.Never )]
		public String LastWriteTime { get; set; }

		///////////////////////////

		/// <summary>
		///		テーブル作成用のSQL。
		/// </summary>
		public static readonly string CreateTable = 
			@"CREATE TABLE IF NOT EXISTS Songs ( " +
			@"id INTEGER NOT NULL PRIMARY KEY, " +  // Linq で自動増加させたい場合は、AUTOINCREMENT は使ってはならない。（生SQLからなら、使わないといけない。）
			@"path NVARCHAR NOT NULL UNIQUE, " +
			@"last_write_time NVARCHAR NOT NULL " +
			@");";

		/// <summary>
		///		テーブルのインデックス作成用のSQL。
		/// </summary>
		public static readonly string CreateIndex =
			@"CREATE INDEX IF NOT EXISTS SongsIndex ON Songs(path);";
	}
}
