using System;
using System.Collections.Generic;
using System.Data.Linq.Mapping;
using System.Diagnostics;
using System.Linq;

namespace DTXmatixx.データベース.曲
{
	/// <summary>
	///		曲テーブルのエンティティクラス。
	/// </summary>
	[Table( Name = "Songs" )]   // テーブル名は複数形
	class Song : ICloneable
	{
		/// <summary>
		///		曲譜面ファイルのハッシュ値。
		///		正確には一意じゃないけど、主キーとして扱う。
		/// </summary>
		[Column( DbType = "NVARCHAR", CanBeNull = false, IsPrimaryKey = true )]
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
		///		曲の難易度。0.00～9.99。
		/// </summary>
		[Column( DbType = "REAL", CanBeNull = false )]
		public double Level { get; set; }

		/// <summary>
		///		最小BPM。null なら未取得。
		/// </summary>
		[Column( DbType = "REAL", CanBeNull = true )]
		public double? MinBPM { get; set; }

		/// <summary>
		///		最大BPM。null なら未取得。
		/// </summary>
		[Column( DbType = "REAL", CanBeNull = true )]
		public double? MaxBPM { get; set; }

		#region " TotalNotes "
		//----------------
		/// <summary>
		///		左シンバルの総ノーツ数。
		/// </summary>
		[Column( DbType = "INT", CanBeNull = false )]
		public int TotalNotes_LeftCymbal { get; set; }

		/// <summary>
		///		ハイハットの総ノーツ数。
		/// </summary>
		[Column( DbType = "INT", CanBeNull = false )]
		public int TotalNotes_HiHat { get; set; }

		/// <summary>
		///		左ペダルまたは左バスの総ノーツ数。
		/// </summary>
		[Column( DbType = "INT", CanBeNull = false )]
		public int TotalNotes_LeftPedal { get; set; }

		/// <summary>
		///		スネアの総ノーツ数。
		/// </summary>
		[Column( DbType = "INT", CanBeNull = false )]
		public int TotalNotes_Snare { get; set; }

		/// <summary>
		///		バスの総ノーツ数。
		/// </summary>
		[Column( DbType = "INT", CanBeNull = false )]
		public int TotalNotes_Bass { get; set; }

		/// <summary>
		///		ハイタムの総ノーツ数。
		/// </summary>
		[Column( DbType = "INT", CanBeNull = false )]
		public int TotalNotes_HighTom { get; set; }

		/// <summary>
		///		ロータムの総ノーツ数。
		/// </summary>
		[Column( DbType = "INT", CanBeNull = false )]
		public int TotalNotes_LowTom { get; set; }

		/// <summary>
		///		フロアタムの総ノーツ数。
		/// </summary>
		[Column( DbType = "INT", CanBeNull = false )]
		public int TotalNotes_FloorTom { get; set; }

		/// <summary>
		///		右シンバルの総ノーツ数。
		/// </summary>
		[Column( DbType = "INT", CanBeNull = false )]
		public int TotalNotes_RightCymbal { get; set; }
		//----------------
		#endregion

		/// <summary>
		///		曲のプレビュー画像。
		/// </summary>
		[Column( DbType = "NVARCHAR" )]
		public string PreImage { get; set; }

		/// <summary>
		///		曲のアーティスト名。
		/// </summary>
		[Column( DbType = "NVARCHAR" )]
		public string Artist { get; set; }

		///////////////////////////

		/// <summary>
		///		規定値で初期化。
		/// </summary>
		public Song()
		{
			this.HashId = "";
			this.Title = "(no title)";
			this.Path = "";
			this.LastWriteTime = DateTime.Now.ToString( "G" );
			this.Level = 5.00;
			this.MinBPM = null;
			this.MaxBPM = null;
			this.TotalNotes_LeftCymbal = 0;
			this.TotalNotes_HiHat = 0;
			this.TotalNotes_LeftPedal = 0;
			this.TotalNotes_Snare = 0;
			this.TotalNotes_Bass = 0;
			this.TotalNotes_HighTom = 0;
			this.TotalNotes_LowTom = 0;
			this.TotalNotes_FloorTom = 0;
			this.TotalNotes_RightCymbal = 0;
			this.PreImage = "";
			this.Artist = "";
		}

		// ICloneable 実装
		public Song Clone()
		{
			return (Song) this.MemberwiseClone();
		}
		object ICloneable.Clone()
		{
			return this.Clone();
		}

		///////////////////////////

		/// <summary>
		///		テーブルのカラム部分を列挙したSQL。
		/// </summary>
		public static readonly string ColumsList =
			@"( HashId NVARCHAR NOT NULL PRIMARY KEY" +
			@", Title NVARCHAR NOT NULL" +
			@", Path NVARCHAR NOT NULL UNIQUE" +
			@", LastWriteTime NVARCHAR NOT NULL" +
			@", Level REAL NOT NULL CHECK(0.0 <= Level AND Level < 10.0)" +
			@", MinBPM REAL" +
			@", MaxBPM REAL" +
			@", TotalNotes_LeftCymbal INTEGER NOT NULL" +
			@", TotalNotes_HiHat INTEGER NOT NULL" +
			@", TotalNotes_LeftPedal INTEGER NOT NULL" +
			@", TotalNotes_Snare INTEGER NOT NULL" +
			@", TotalNotes_Bass INTEGER NOT NULL" +
			@", TotalNotes_HighTom INTEGER NOT NULL" +
			@", TotalNotes_LowTom INTEGER NOT NULL" +
			@", TotalNotes_FloorTom INTEGER NOT NULL" +
			@", TotalNotes_RightCymbal INTEGER NOT NULL" +
			@", PreImage NVARCHAR" +
			@", Artist NVARCHAR" +
			@")";
	}
}
