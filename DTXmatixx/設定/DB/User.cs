using System;
using System.Collections.Generic;
using System.Data.Linq.Mapping;
using System.Diagnostics;
using System.Linq;
using DTXmatixx.ステージ.演奏;

namespace DTXmatixx.設定.DB
{
	/// <summary>
	///		Users テーブルの Linq 用スキーマ。
	///		UsersDB 内で、ユーザの情報やオプション設定などを管理する。
	/// </summary>
	[Table( Name = "Users" )]   // テーブル名は複数形
	public class User	// クラスは単数形
	{
		// !!! 整数型は、SQLなら "INTEGER" だが、DbTypeプロパティは "INT" としないとエラーになるので注意。 !!!

		/// <summary>
		///		ユーザを一意に識別するID。
		///		変更不可。
		/// </summary>
		[Column( Name = "id", DbType = "NVARCHAR", CanBeNull = false, IsPrimaryKey = true )]
		public String Id { get; set; }

		/// <summary>
		///		ユーザ名。
		///		変更可。
		/// </summary>
		[Column( Name = "name", DbType = "NVARCHAR", CanBeNull = false )]
		public String Name { get; set; }

		/// <summary>
		///		左シンバルレーンの AutoPlay 。
		///		0: OFF, その他: ON。
		/// </summary>
		[Column( Name = "autoplay_left_cymbal", DbType = "INT", CanBeNull = false )]
		public Int32 AutoPlayLeftCymbal { get; set; }

		/// <summary>
		///		ハイハットレーンの AutoPlay 。
		///		0: OFF, その他: ON。
		/// </summary>
		[Column( Name = "autoplay_hihat", DbType = "INT", CanBeNull = false )]
		public Int32 AutoPlayHiHat { get; set; }

		/// <summary>
		///		左ペダルレーンの AutoPlay 。
		///		0: OFF, その他: ON。
		/// </summary>
		[Column( Name = "autoplay_left_pedal", DbType = "INT", CanBeNull = false )]
		public Int32 AutoPlayLeftPedal { get; set; }

		/// <summary>
		///		スネアレーンの AutoPlay 。
		///		0: OFF, その他: ON。
		/// </summary>
		[Column( Name = "autoplay_snare", DbType = "INT", CanBeNull = false )]
		public Int32 AutoPlaySnare { get; set; }

		/// <summary>
		///		バスレーンの AutoPlay 。
		///		0: OFF, その他: ON。
		/// </summary>
		[Column( Name = "autoplay_bass", DbType = "INT", CanBeNull = false )]
		public Int32 AutoPlayBass { get; set; }

		/// <summary>
		///		ハイタムレーンの AutoPlay 。
		///		0: OFF, その他: ON。
		/// </summary>
		[Column( Name = "autoplay_high_tom", DbType = "INT", CanBeNull = false )]
		public Int32 AutoPlayHighTom { get; set; }

		/// <summary>
		///		ロータムレーンの AutoPlay 。
		///		0: OFF, その他: ON。
		/// </summary>
		[Column( Name = "autoplay_low_tom", DbType = "INT", CanBeNull = false )]
		public Int32 AutoPlayLowTom { get; set; }

		/// <summary>
		///		フロアタムレーンの AutoPlay 。
		///		0: OFF, その他: ON。
		/// </summary>
		[Column( Name = "autoplay_floor_tom", DbType = "INT", CanBeNull = false )]
		public Int32 AutoPlayFloorTom { get; set; }

		/// <summary>
		///		右シンバルレーンの AutoPlay 。
		///		0: OFF, その他: ON。
		/// </summary>
		[Column( Name = "autoplay_right_cymbal", DbType = "INT", CanBeNull = false )]
		public Int32 AutoPlayRightCymbal { get; set; }

		/// <summary>
		///		Perfect の最大ヒット距離[秒]。
		/// </summary>
		[Column( Name = "max_range_perfect", DbType = "REAL", CanBeNull = false )]
		public Double MaxRangePerfect { get; set; }

		/// <summary>
		///		Great の最大ヒット距離[秒]。
		/// </summary>
		[Column( Name = "max_range_great", DbType = "REAL", CanBeNull = false )]
		public Double MaxRangeGreat { get; set; }

		/// <summary>
		///		Good の最大ヒット距離[秒]。
		/// </summary>
		[Column( Name = "max_range_good", DbType = "REAL", CanBeNull = false )]
		public Double MaxRangeGood { get; set; }

		/// <summary>
		///		Ok の最大ヒット距離[秒]。
		/// </summary>
		[Column( Name = "max_range_ok", DbType = "REAL", CanBeNull = false )]
		public Double MaxRangeOk { get; set; }

		/// <summary>
		///		譜面スクロール速度の倍率。1.0で等倍。
		/// </summary>
		[Column( Name = "scroll_speed", DbType = "REAL", CanBeNull = false )]
		public Double ScrollSpeed { get; set; }

		/// <summary>
		///		起動直後の表示モード。
		///		0: ウィンドウモード、その他: 全画面モードで。
		/// </summary>
		[Column( Name = "fullscreen", DbType = "INT", CanBeNull = false )]
		public Int32 Fullscreen { get; set; }

		///////////////////////////

		/// <summary>
		///		テーブル作成用のSQL。
		/// </summary>
		public static readonly string CreateTable =
			@"CREATE TABLE IF NOT EXISTS Users " +
			@"( id NVARCHAR NOT NULL PRIMARY KEY" +
			@", name NVARCHAR NOT NULL" +
			@", autoplay_left_cymbal INTEGER NOT NULL" +
			@", autoplay_hihat INTEGER NOT NULL" +
			@", autoplay_left_pedal INTEGER NOT NULL" +
			@", autoplay_snare INTEGER NOT NULL" +
			@", autoplay_bass INTEGER NOT NULL" +
			@", autoplay_high_tom INTEGER NOT NULL" +
			@", autoplay_low_tom INTEGER NOT NULL" +
			@", autoplay_floor_tom INTEGER NOT NULL" +
			@", autoplay_right_cymbal INTEGER NOT NULL" +
			@", max_range_perfect REAL NOT NULL" +
			@", max_range_great REAL NOT NULL" +
			@", max_range_good REAL NOT NULL" +
			@", max_range_ok REAL NOT NULL" +
			@", scroll_speed REAL NOT NULL" +
			@", fullscreen INTEGER NOT NULL" +
			@");";

		/// <summary>
		///		テーブルのインデックス作成用のSQL。
		/// </summary>
		public static readonly string CreateIndex =
			@"CREATE INDEX IF NOT EXISTS UsersIndex ON Users(id);";

		///////////////////////////

		internal static readonly Dictionary<判定種別, double> 最大ヒット距離secの規定値 = new Dictionary<判定種別, double>() {
			{ 判定種別.PERFECT, 0.034 },
			{ 判定種別.GREAT, 0.067 },
			{ 判定種別.GOOD, 0.084 },
			{ 判定種別.OK, 0.117 },
		};
	}
}
