using System;
using System.Collections.Generic;
using System.Data.Linq.Mapping;
using System.Diagnostics;
using System.Linq;
using FDK;
using DTXmatixx.ステージ.演奏;

namespace DTXmatixx.データベース.ユーザ
{
	/// <summary>
	///		ユーザテーブルのエンティティクラス。
	/// </summary>
	[Table( Name = "Users" )]   // テーブル名は複数形
	class User : ICloneable
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

		#region " AutoPlay "
		//----------------
		/// <summary>
		///		左シンバルレーンの AutoPlay 。
		///		0: OFF, その他: ON。
		/// </summary>
		[Column( DbType = "INT", CanBeNull = false )]
		public int AutoPlay_LeftCymbal { get; set; }

		/// <summary>
		///		ハイハットレーンの AutoPlay 。
		///		0: OFF, その他: ON。
		/// </summary>
		[Column( DbType = "INT", CanBeNull = false )]
		public int AutoPlay_HiHat { get; set; }

		/// <summary>
		///		左ペダルレーンの AutoPlay 。
		///		0: OFF, その他: ON。
		/// </summary>
		[Column( DbType = "INT", CanBeNull = false )]
		public int AutoPlay_LeftPedal { get; set; }

		/// <summary>
		///		スネアレーンの AutoPlay 。
		///		0: OFF, その他: ON。
		/// </summary>
		[Column( DbType = "INT", CanBeNull = false )]
		public int AutoPlay_Snare { get; set; }

		/// <summary>
		///		バスレーンの AutoPlay 。
		///		0: OFF, その他: ON。
		/// </summary>
		[Column( DbType = "INT", CanBeNull = false )]
		public int AutoPlay_Bass { get; set; }

		/// <summary>
		///		ハイタムレーンの AutoPlay 。
		///		0: OFF, その他: ON。
		/// </summary>
		[Column( DbType = "INT", CanBeNull = false )]
		public int AutoPlay_HighTom { get; set; }

		/// <summary>
		///		ロータムレーンの AutoPlay 。
		///		0: OFF, その他: ON。
		/// </summary>
		[Column( DbType = "INT", CanBeNull = false )]
		public int AutoPlay_LowTom { get; set; }

		/// <summary>
		///		フロアタムレーンの AutoPlay 。
		///		0: OFF, その他: ON。
		/// </summary>
		[Column( DbType = "INT", CanBeNull = false )]
		public int AutoPlay_FloorTom { get; set; }

		/// <summary>
		///		右シンバルレーンの AutoPlay 。
		///		0: OFF, その他: ON。
		/// </summary>
		[Column( DbType = "INT", CanBeNull = false )]
		public int AutoPlay_RightCymbal { get; set; }
		//----------------
		#endregion

		#region " 最大ヒット距離 "
		//----------------
		/// <summary>
		///		Perfect の最大ヒット距離[秒]。
		/// </summary>
		[Column( DbType = "REAL", CanBeNull = false )]
		public double MaxRange_Perfect { get; set; }

		/// <summary>
		///		Great の最大ヒット距離[秒]。
		/// </summary>
		[Column( DbType = "REAL", CanBeNull = false )]
		public double MaxRange_Great { get; set; }

		/// <summary>
		///		Good の最大ヒット距離[秒]。
		/// </summary>
		[Column( DbType = "REAL", CanBeNull = false )]
		public double MaxRange_Good { get; set; }

		/// <summary>
		///		Ok の最大ヒット距離[秒]。
		/// </summary>
		[Column( DbType = "REAL", CanBeNull = false )]
		public double MaxRange_Ok { get; set; }
		//----------------
		#endregion

		/// <summary>
		///		曲ファイル検索フォルダのリスト。
		///		各フォルダ（相対or絶対パス；フォルダ変数なし）は、";" で区切られている。末尾が \ である必要はない。
		/// </summary>
		[Column( DbType = "NVARCHAR", CanBeNull = false )]
		public string SongFolders { get; set; }

		/// <summary>
		///		シンバルフリーモード。
		///		0: OFF, その他: ON
		/// </summary>
		[Column( DbType = "INT", CanBeNull = false )]
		public int CymbalFree { get; set; }

		
		// Column を追加したら、コンストラクタでの初期化コードも忘れず追加すること。


		///////////////////////////

		/// <summary>
		///		既定値で初期化。
		/// </summary>
		public User()
		{
			this.Id = "Anonymous";
			this.Name = "Anonymous";
			this.ScrollSpeed = 1.0;
			this.Fullscreen = 0;
			this.AutoPlay_LeftCymbal = 1;
			this.AutoPlay_HiHat = 1;
			this.AutoPlay_LeftPedal = 1;
			this.AutoPlay_Snare = 1;
			this.AutoPlay_Bass = 1;
			this.AutoPlay_HighTom = 1;
			this.AutoPlay_LowTom = 1;
			this.AutoPlay_FloorTom = 1;
			this.AutoPlay_RightCymbal = 1;
			this.MaxRange_Perfect = 0.034;
			this.MaxRange_Great = 0.067;
			this.MaxRange_Good = 0.084;
			this.MaxRange_Ok = 0.117;
			this.SongFolders = VariablePath.フォルダ変数の内容を返す( "Exe" ) ?? "";
			this.CymbalFree = 1;
		}

		// ICloneable 実装
		public User Clone()
		{
			return (User) this.MemberwiseClone();
		}
		object ICloneable.Clone()
		{
			return this.Clone();
		}

		///////////////////////////

		/// <summary>
		///		テーブルのカラム部分を列挙したSQL。
		/// </summary>
		public static readonly string ColumnList =
			@"( Id NVARCHAR NOT NULL PRIMARY KEY" +
			@", Name NVARCHAR NOT NULL" +
			@", ScrollSpeed REAL NOT NULL" +
			@", Fullscreen INTEGER NOT NULL" +
			@", AutoPlay_LeftCymbal INTEGER NOT NULL" +
			@", AutoPlay_HiHat INTEGER NOT NULL" +
			@", AutoPlay_LeftPedal INTEGER NOT NULL" +
			@", AutoPlay_Snare INTEGER NOT NULL" +
			@", AutoPlay_Bass INTEGER NOT NULL" +
			@", AutoPlay_HighTom INTEGER NOT NULL" +
			@", AutoPlay_LowTom INTEGER NOT NULL" +
			@", AutoPlay_FloorTom INTEGER NOT NULL" +
			@", AutoPlay_RightCymbal INTEGER NOT NULL" +
			@", MaxRange_Perfect REAL NOT NULL" +
			@", MaxRange_Great REAL NOT NULL" +
			@", MaxRange_Good REAL NOT NULL" +
			@", MaxRange_Ok REAL NOT NULL" +
			@", SongFolders NVARCHAR NOT NULL" +
			@", CymbalFree INTEGER NOT NULL" +
			@")";
	}
}
