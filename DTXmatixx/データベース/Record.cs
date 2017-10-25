using System;
using System.Collections.Generic;
using System.Data.Linq.Mapping;
using System.Data.SQLite;
using System.Diagnostics;
using System.Linq;

namespace DTXmatixx.データベース
{
	/// <summary>
	///		成績テーブルのエンティティクラス。
	/// </summary>
	[Table( Name = "Records" )]   // テーブル名は複数形
	public class Record	// クラス名は単数形
	{
		/// <summary>
		///		一意な ID。
		///		値はDB側で自動生成されるので、INSERT 時は null を設定しておくこと。
		/// </summary>
		[Column( DbType = "INT", CanBeNull = false, IsPrimaryKey = true )] // Linq で自動増加させたい場合は、IsDbGenerate を指定してはならない。
		public int? Id { get; set; } = null;

		/// <summary>
		///		ユーザを一意に識別するID。
		/// </summary>
		[Column( DbType = "NVARCHAR", CanBeNull = false )]
		public string UserId { get; set; }

		/// <summary>
		///		曲譜面ファイルのハッシュ値。
		/// </summary>
		[Column( DbType = "NVARCHAR", CanBeNull = false )]
		public string SongHashId { get; set; }

		/// <summary>
		///		スコア。
		/// </summary>
		[Column( DbType = "INT", CanBeNull = false )]
		public int Score { get; set; }

		/// <summary>
		///		カウントマップラインのデータ。
		///		１ブロックを１文字（'0':0～'C':12）で表し、<see cref="DTXmatixx.ステージ.演奏.カウントマップライン.カウントマップの最大要素数"/> 個の文字が並ぶ。
		///		もし不足分があれば、'0' とみなされる。
		/// </summary>
		[Column( DbType = "NVARCHAR", CanBeNull = false )]
		public string CountMap { get; set; }

		/// <summary>
		///		曲別SKILL。
		/// </summary>
		[Column( DbType = "REAL", CanBeNull = false )]
		public double Skill { get; set; }

		/// <summary>
		///		達成率。
		/// </summary>
		[Column( DbType = "REAL", CanBeNull = false )]
		public double Achievement { get; set; }

		///////////////////////////

		/// <summary>
		///		規定値で初期化。
		/// </summary>
		public Record()
		{
			this.Id = null;
			this.UserId = "Anonymous";
			this.SongHashId = "";
			this.Score = 0;
			this.CountMap = "";
			this.Skill = 0.0;
			this.Achievement = 0.0;
		}

		///////////////////////////

		/// <summary>
		///		テーブル作成用のSQL。
		/// </summary>
		public static readonly string CreateTableSQL =
			@"CREATE TABLE IF NOT EXISTS Records " +
			@"( Id INTEGER NOT NULL PRIMARY KEY" +
			@", UserId NVARCHAR NOT NULL" +
			@", SongHashId NVARCHAR NOT NULL" +
			@", Score INTEGER NOT NULL" +
			@", CountMap NVARCHAR NOT NULL" +
			@", Skill REAL NOT NULL"+
			@", Achievement REAL NOT NULL" +
			@");";
	}
}
