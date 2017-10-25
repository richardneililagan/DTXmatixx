using System;
using System.Collections.Generic;
using System.Data.Linq.Mapping;
using System.Diagnostics;
using System.Linq;
using DTXmatixx.ステージ.演奏;

namespace DTXmatixx.データベース
{
	/// <summary>
	///		ユーザテーブルのエンティティクラス。
	/// </summary>
	[Table( Name = "Users" )]   // テーブル名は複数形
	class User
	{
		/// <summary>
		///		ユーザを一意に識別する文字列。主キー。
		///		変更不可。
		/// </summary>
		[Column( DbType = "NVARCHAR", CanBeNull = true, IsPrimaryKey = true )]
		public string Id { get; set; }

		/// <summary>
		///		ユーザ名。
		///		変更可。
		/// </summary>
		[Column( DbType = "NVARCHAR", CanBeNull = false )]
		public string Name { get; set; }

		/// <summary>
		///		左シンバルレーンの AutoPlay 。
		///		0: OFF, その他: ON。
		/// </summary>
		[Column( DbType = "INT", CanBeNull = false )]
		public int AutoPlayLeftCymbal { get; set; }

		/// <summary>
		///		ハイハットレーンの AutoPlay 。
		///		0: OFF, その他: ON。
		/// </summary>
		[Column( DbType = "INT", CanBeNull = false )]
		public int AutoPlayHiHat { get; set; }

		/// <summary>
		///		左ペダルレーンの AutoPlay 。
		///		0: OFF, その他: ON。
		/// </summary>
		[Column( DbType = "INT", CanBeNull = false )]
		public int AutoPlayLeftPedal { get; set; }

		/// <summary>
		///		スネアレーンの AutoPlay 。
		///		0: OFF, その他: ON。
		/// </summary>
		[Column( DbType = "INT", CanBeNull = false )]
		public int AutoPlaySnare { get; set; }

		/// <summary>
		///		バスレーンの AutoPlay 。
		///		0: OFF, その他: ON。
		/// </summary>
		[Column( DbType = "INT", CanBeNull = false )]
		public int AutoPlayBass { get; set; }

		/// <summary>
		///		ハイタムレーンの AutoPlay 。
		///		0: OFF, その他: ON。
		/// </summary>
		[Column( DbType = "INT", CanBeNull = false )]
		public int AutoPlayHighTom { get; set; }

		/// <summary>
		///		ロータムレーンの AutoPlay 。
		///		0: OFF, その他: ON。
		/// </summary>
		[Column( DbType = "INT", CanBeNull = false )]
		public int AutoPlayLowTom { get; set; }

		/// <summary>
		///		フロアタムレーンの AutoPlay 。
		///		0: OFF, その他: ON。
		/// </summary>
		[Column( DbType = "INT", CanBeNull = false )]
		public int AutoPlayFloorTom { get; set; }

		/// <summary>
		///		右シンバルレーンの AutoPlay 。
		///		0: OFF, その他: ON。
		/// </summary>
		[Column( DbType = "INT", CanBeNull = false )]
		public int AutoPlayRightCymbal { get; set; }

		/// <summary>
		///		Perfect の最大ヒット距離[秒]。
		/// </summary>
		[Column( DbType = "REAL", CanBeNull = false )]
		public double MaxRangePerfect { get; set; }

		/// <summary>
		///		Great の最大ヒット距離[秒]。
		/// </summary>
		[Column( DbType = "REAL", CanBeNull = false )]
		public double MaxRangeGreat { get; set; }

		/// <summary>
		///		Good の最大ヒット距離[秒]。
		/// </summary>
		[Column( DbType = "REAL", CanBeNull = false )]
		public double MaxRangeGood { get; set; }

		/// <summary>
		///		Ok の最大ヒット距離[秒]。
		/// </summary>
		[Column( DbType = "REAL", CanBeNull = false )]
		public double MaxRangeOk { get; set; }

		/// <summary>
		///		譜面スクロール速度の倍率。1.0で等倍。
		/// </summary>
		[Column( DbType = "REAL", CanBeNull = false )]
		public double ScrollSpeed { get; set; }

		/// <summary>
		///		起動直後の表示モード。
		///		0: ウィンドウモード、その他: 全画面モードで。
		/// </summary>
		[Column( DbType = "INT", CanBeNull = false )]
		public int Fullscreen { get; set; }

		///////////////////////////

		/// <summary>
		///		既定値で初期化。
		/// </summary>
		public User()
		{
			this.Id = "Anonymous";
			this.Name = "Anonymous";
			this.AutoPlayLeftCymbal = 0;
			this.AutoPlayHiHat = 0;
			this.AutoPlayLeftPedal = 0;
			this.AutoPlaySnare = 0;
			this.AutoPlayBass = 0;
			this.AutoPlayHighTom = 0;
			this.AutoPlayLowTom = 0;
			this.AutoPlayFloorTom = 0;
			this.AutoPlayRightCymbal = 0;
			this.MaxRangePerfect = 0.034;
			this.MaxRangeGreat = 0.067;
			this.MaxRangeGood = 0.084;
			this.MaxRangeOk = 0.117;
			this.ScrollSpeed = 1.0;
			this.Fullscreen = 0;
		}

		///////////////////////////

		/// <summary>
		///		テーブル作成用のSQL。
		/// </summary>
		public static readonly string CreateTableSQL =
			@"CREATE TABLE IF NOT EXISTS Users " +
			@"( Id NVARCHAR NOT NULL PRIMARY KEY" +
			@", Name NVARCHAR NOT NULL" +
			@", AutoplayLeftCymbal INTEGER NOT NULL" +
			@", AutoPlayHiHat INTEGER NOT NULL" +
			@", AutoPlayLeftPedal INTEGER NOT NULL" +
			@", AutoPlaySnare INTEGER NOT NULL" +
			@", AutoPlayBass INTEGER NOT NULL" +
			@", AutoPlayHighTom INTEGER NOT NULL" +
			@", AutoPlayLowTom INTEGER NOT NULL" +
			@", AutoPlayFloorTom INTEGER NOT NULL" +
			@", AutoPlayRightCymbal INTEGER NOT NULL" +
			@", MaxRangePerfect REAL NOT NULL" +
			@", MaxRangeGreat REAL NOT NULL" +
			@", MaxRangeGood REAL NOT NULL" +
			@", MaxRangeOk REAL NOT NULL" +
			@", ScrollSpeed REAL NOT NULL" +
			@", Fullscreen INTEGER NOT NULL" +
			@");";
	}
}
