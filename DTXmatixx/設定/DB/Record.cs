using System;
using System.Collections.Generic;
using System.Data.Linq.Mapping;
using System.Diagnostics;
using System.Linq;

namespace DTXmatixx.設定.DB
{
	/// <summary>
	///		Records テーブルの Linq 用スキーマ。
	///		UsersDB 内で、ユーザの演奏成績を管理する。
	/// </summary>
	[Table( Name = "Records" )]   // テーブル名は複数形
	public class Record	// クラス名は単数形
	{
		/// <summary>
		///		ユーザを一意に識別するID。
		/// </summary>
		[Column( Name = "user_id", DbType = "NVARCHAR", CanBeNull = false )]
		public String UserId { get; set; }

		/// <summary>
		///		曲譜面ファイルのハッシュ値。
		/// </summary>
		[Column( Name = "song_hash_id", DbType = "NVARCHAR", CanBeNull = false, UpdateCheck = UpdateCheck.Never )]
		public String SongHashId { get; set; }

		/// <summary>
		///		スコア。
		/// </summary>
		[Column( Name = "score", DbType = "INT", CanBeNull = false )]
		public Int32 Score { get; set; }

		/// <summary>
		///		カウントマップラインのデータ。
		///		１ブロックを１文字（'0':0～'C':12）で表し、<see cref="DTXmatixx.ステージ.演奏.カウントマップライン.カウントマップの最大要素数"/> 個の文字が並ぶ。
		///		もし不足分があれば、'0' とみなされる。
		/// </summary>
		[Column( Name = "count_map", DbType = "NVARCHAR", CanBeNull = true, UpdateCheck = UpdateCheck.Never )]
		public String CountMap { get; set; } = null;

		///////////////////////////

		/// <summary>
		///		テーブル作成用のSQL。
		/// </summary>
		public static readonly string CreateTable =
			@"CREATE TABLE IF NOT EXISTS Records " +
			@"( user_id NVARCHAR NOT NULL" +
			@", song_hash_id NVARCHAR NOT NULL" +
			@", score INTEGER NOT NULL" +
			@", count_map NVARCHAR" +
			@");";
	}
}
