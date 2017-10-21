using System;
using System.Collections.Generic;
using System.Data.Linq.Mapping;
using System.Diagnostics;
using System.Linq;

namespace DTXmatixx.設定.DB
{
	/// <summary>
	///		曲テーブルのエンティティクラス。
	/// </summary>
	[Table( Name = "Songs" )]   // テーブル名は複数形
	class Song
	{
		/// <summary>
		///		一意な ID。
		///		値はDB側で自動生成されるので、INSERT 時は null を設定しておくこと。
		/// </summary>
		[Column( DbType = "INT", CanBeNull = false, IsPrimaryKey = true )] // Linq で自動増加させたい場合は、IsDbGenerate を指定してはならない。
		public int? Id { get; set; } = null;

		/// <summary>
		///		曲譜面ファイルのハッシュ値。
		///		正確には一意じゃないけど、主キーとして扱う。
		/// </summary>
		[Column( DbType = "NVARCHAR", CanBeNull = false )]
		public string HashId { get; set; }

		/// <summary>
		///		曲のタイトル。
		/// </summary>
		[Column( DbType = "NVARCHAR", CanBeNull = false )]
		public string Title { get; set; }

		/// <summary>
		///		曲譜面ファイルへの絶対パス。
		///		これも一意とする。（テーブル生成SQLで UNIQUE を付与している。）
		/// </summary>
		[Column( DbType = "NVARCHAR", CanBeNull = false )]
		public string Path { get; set; }

		/// <summary>
		///		曲譜面ファイルの最終更新時刻の文字列表記。
		///		文字列の書式は、System.DateTime.ToString("G") と同じ。（例: "08/17/2000 16:32:32"）
		///		カルチャはシステム既定のものとする。
		/// </summary>
		[Column( DbType = "NVARCHAR", CanBeNull = false )]
		public string LastWriteTime { get; set; }

		/// <summary>
		///		左シンバルの総ノーツ数。
		/// </summary>
		[Column( DbType = "INT", CanBeNull = false )]
		public int LeftCymbalNotes { get; set; }
		/// <summary>
		///		ハイハットの総ノーツ数。
		/// </summary>
		[Column( DbType = "INT", CanBeNull = false )]
		public int HiHatNotes { get; set; }
		/// <summary>
		///		左ペダルまたは左バスの総ノーツ数。
		/// </summary>
		[Column( DbType = "INT", CanBeNull = false )]
		public int LeftPedalNotes { get; set; }
		/// <summary>
		///		スネアの総ノーツ数。
		/// </summary>
		[Column( DbType = "INT", CanBeNull = false )]
		public int SnareNotes { get; set; }
		/// <summary>
		///		バスの総ノーツ数。
		/// </summary>
		[Column( DbType = "INT", CanBeNull = false )]
		public int BassNotes { get; set; }
		/// <summary>
		///		ハイタムの総ノーツ数。
		/// </summary>
		[Column( DbType = "INT", CanBeNull = false )]
		public int HighTomNotes { get; set; }
		/// <summary>
		///		ロータムの総ノーツ数。
		/// </summary>
		[Column( DbType = "INT", CanBeNull = false )]
		public int LowTomNotes { get; set; }
		/// <summary>
		///		フロアタムの総ノーツ数。
		/// </summary>
		[Column( DbType = "INT", CanBeNull = false )]
		public int FloorTomNotes { get; set; }
		/// <summary>
		///		右シンバルの総ノーツ数。
		/// </summary>
		[Column( DbType = "INT", CanBeNull = false )]
		public int RightCymbalNotes { get; set; }

		/// <summary>
		///		曲の難易度。0.00～9.99。
		/// </summary>
		[Column( DbType = "REAL", CanBeNull = false )]
		public double Level { get; set; }

		/// <summary>
		///		最小BPM。
		/// </summary>
		[Column( DbType = "REAL", CanBeNull = false )]
		public double MinBPM { get; set; }

		/// <summary>
		///		最大BPM。
		/// </summary>
		[Column( DbType = "REAL", CanBeNull = false )]
		public double MaxBPM { get; set; }

		///////////////////////////

		/// <summary>
		///		規定値で初期化。
		/// </summary>
		public Song()
		{
			this.Id = null;
			this.HashId = "";
			this.Title = "(no title)";
			this.Path = "";
			this.LastWriteTime = DateTime.Now.ToString( "G" );
			this.LeftCymbalNotes = 0;
			this.HiHatNotes = 0;
			this.LeftPedalNotes = 0;
			this.SnareNotes = 0;
			this.BassNotes = 0;
			this.HighTomNotes = 0;
			this.LowTomNotes = 0;
			this.FloorTomNotes = 0;
			this.RightCymbalNotes = 0;
			this.Level = 5.00;
			this.MinBPM = 120.0;
			this.MaxBPM = 120.0;
		}

		///////////////////////////

		/// <summary>
		///		テーブル作成用のSQL。
		/// </summary>
		public static readonly string CreateTableSQL =
			@"CREATE TABLE IF NOT EXISTS Songs " +
			@"( Id INTEGER NOT NULL PRIMARY KEY" +  // Linq で自動増加させたい場合は、AUTOINCREMENT は使ってはならない。（生SQLからなら、使わないといけない。）
			@", HashId NVARCHAR NOT NULL" +
			@", Title NVARCHAR NOT NULL" +
			@", Path NVARCHAR NOT NULL UNIQUE" +
			@", LastWriteTime NVARCHAR NOT NULL" +
			@", LeftCymbalNotes INTEGER NOT NULL" +
			@", HiHatNotes INTEGER NOT NULL" +
			@", LeftPedalNotes INTEGER NOT NULL" +
			@", SnareNotes INTEGER NOT NULL" +
			@", BassNotes INTEGER NOT NULL" +
			@", HighTomNotes INTEGER NOT NULL" +
			@", LowTomNotes INTEGER NOT NULL" +
			@", FloorTomNotes INTEGER NOT NULL" +
			@", RightCymbalNotes INTEGER NOT NULL" +
			@", Level REAL NOT NULL CHECK(0.0 <= Level AND Level < 10.0)" +
			@", MinBPM REAL NOT NULL" +
			@", MaxBPM REAL NOT NULL" +
			@");";
	}
}
