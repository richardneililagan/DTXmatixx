using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using FDK;
using SSTFormatCurrent = SSTFormat.v3;
using DTXmatixx.データベース.ユーザ;
using DTXmatixx.データベース.曲;
using DTXmatixx.ステージ.演奏;

namespace DTXmatixx.設定
{
	/// <summary>
	///		曲データベースを管理するstaticクラス。
	/// </summary>
	class 曲DB
	{
		/// <summary>
		///		インスタンス生成不可。
		/// </summary>
		protected 曲DB()
		{
		}

		/// <summary>
		///		指定した曲ファイルに対応するレコードがデータベースになければレコードを追加し、
		///		あればそのレコードを更新する。
		/// </summary>
		public static void 曲を追加または更新する( VariablePath 曲ファイルパス, ユーザ設定 ユーザ設定 )
		{
			try
			{
				using( var songdb = new SongDB() )
				{
					var 同一パス検索クエリ = songdb.Songs.Where( ( song ) => ( song.Path == 曲ファイルパス.変数なしパス ) );

					if( 0 == 同一パス検索クエリ.Count() )
					{
						// (A) 同一パスを持つレコードがDBになかった

						var 調べる曲のハッシュ = _ファイルのハッシュを算出して返す( 曲ファイルパス );

						var 同一ハッシュレコード = songdb.Songs.Where( ( song ) => ( song.HashId == 調べる曲のハッシュ ) ).SingleOrDefault();

						if( null == 同一ハッシュレコード )
						{
							#region " (A-a) 同一ハッシュを持つレコードがDBになかった → 新規追加 "
							//----------------
							var 拡張子名 = Path.GetExtension( 曲ファイルパス.変数なしパス );
							var score = (SSTFormatCurrent.スコア) null;

							#region " スコアを読み込む "
							//----------------
							if( ".sstf" == 拡張子名 )
							{
								score = new SSTFormatCurrent.スコア( 曲ファイルパス.変数なしパス );
							}
							else if( ".dtx" == 拡張子名 )
							{
								score = SSTFormatCurrent.DTXReader.ReadFromFile( 曲ファイルパス.変数なしパス );
							}
							else
							{
								throw new Exception( $"未対応のフォーマットファイルです。[{曲ファイルパス.変数付きパス}]" );
							}
							//----------------
							#endregion

							using( score )
							{
								// Songs レコード新規追加。
								var ノーツ数 = _ノーツ数を算出して返す( score, ユーザ設定 );
								var BPMs = _最小最大BPMを調べて返す( score );

								songdb.Songs.InsertOnSubmit(
									new Song() {
										HashId = _ファイルのハッシュを算出して返す( 曲ファイルパス ),
										Title = score.曲名,
										Path = 曲ファイルパス.変数なしパス,
										LastWriteTime = File.GetLastWriteTime( 曲ファイルパス.変数なしパス ).ToString( "G" ),
										Level = score.難易度,
										MinBPM = BPMs.最小BPM,
										MaxBPM = BPMs.最大BPM,
										TotalNotes_LeftCymbal = ノーツ数[ 表示レーン種別.LeftCrash ],
										TotalNotes_HiHat = ノーツ数[ 表示レーン種別.HiHat ],
										TotalNotes_LeftPedal = ノーツ数[ 表示レーン種別.Foot ],
										TotalNotes_Snare = ノーツ数[ 表示レーン種別.Snare ],
										TotalNotes_Bass = ノーツ数[ 表示レーン種別.Bass ],
										TotalNotes_HighTom = ノーツ数[ 表示レーン種別.Tom1 ],
										TotalNotes_LowTom = ノーツ数[ 表示レーン種別.Tom2 ],
										TotalNotes_FloorTom = ノーツ数[ 表示レーン種別.Tom3 ],
										TotalNotes_RightCymbal = ノーツ数[ 表示レーン種別.RightCrash ],
									} );
							}

							songdb.DataContext.SubmitChanges();

							Log.Info( $"DBに曲を追加しました。{曲ファイルパス.変数付きパス}" );
							//----------------
							#endregion
						}
						else
						{
							#region " (A-b) 同一ハッシュを持つレコードがDBにあった → 更新 "
							//----------------
							var 拡張子名 = Path.GetExtension( 曲ファイルパス.変数なしパス );
							var score = (SSTFormatCurrent.スコア) null;

							#region " スコアを読み込む "
							//----------------
							if( ".sstf" == 拡張子名 )
							{
								score = new SSTFormatCurrent.スコア( 曲ファイルパス.変数なしパス );
							}
							else if( ".dtx" == 拡張子名 )
							{
								score = SSTFormatCurrent.DTXReader.ReadFromFile( 曲ファイルパス.変数なしパス );
							}
							else
							{
								throw new Exception( $"未対応のフォーマットファイルです。[{曲ファイルパス.変数付きパス}]" );
							}
							//----------------
							#endregion

							using( score )
							{
								// Songs レコード更新。
								var ノーツ数 = _ノーツ数を算出して返す( score, ユーザ設定 );
								var BPMs = _最小最大BPMを調べて返す( score );
								var song = 同一ハッシュレコード;

								song.Title = score.曲名;
								song.Path = 曲ファイルパス.変数なしパス;
								song.LastWriteTime = File.GetLastWriteTime( 曲ファイルパス.変数なしパス ).ToString( "G" );
								song.Level = score.難易度;
								song.MinBPM = BPMs.最小BPM;
								song.MaxBPM = BPMs.最大BPM;
								song.TotalNotes_LeftCymbal = ノーツ数[ 表示レーン種別.LeftCrash ];
								song.TotalNotes_HiHat = ノーツ数[ 表示レーン種別.HiHat ];
								song.TotalNotes_LeftPedal = ノーツ数[ 表示レーン種別.Foot ];
								song.TotalNotes_Snare = ノーツ数[ 表示レーン種別.Snare ];
								song.TotalNotes_Bass = ノーツ数[ 表示レーン種別.Bass ];
								song.TotalNotes_HighTom = ノーツ数[ 表示レーン種別.Tom1 ];
								song.TotalNotes_LowTom = ノーツ数[ 表示レーン種別.Tom2 ];
								song.TotalNotes_FloorTom = ノーツ数[ 表示レーン種別.Tom3 ];
								song.TotalNotes_RightCymbal = ノーツ数[ 表示レーン種別.RightCrash ];
							}

							songdb.DataContext.SubmitChanges();

							Log.Info( $"パスが異なりハッシュが同一であるレコードが検出されたため、曲の情報を更新しました。{曲ファイルパス.変数付きパス}" );
							//----------------
							#endregion
						}
					}
					else
					{
						// (B) 同一パスを持つレコードがDBにあった

						var record = 同一パス検索クエリ.Single();

						string レコードの最終更新日時 = record.LastWriteTime;
						string 調べる曲の最終更新日時 = File.GetLastWriteTime( 曲ファイルパス.変数なしパス ).ToString( "G" );

						if( レコードの最終更新日時 != 調べる曲の最終更新日時 )
						{
							#region " (B-a) 最終更新日時が変更されている → 更新 "
							//----------------
							var 拡張子名 = Path.GetExtension( 曲ファイルパス.変数なしパス );
							var score = (SSTFormatCurrent.スコア) null;

							#region " スコアを読み込む "
							//----------------
							if( ".sstf" == 拡張子名 )
							{
								score = new SSTFormatCurrent.スコア( 曲ファイルパス.変数なしパス );
							}
							else if( ".dtx" == 拡張子名 )
							{
								score = SSTFormatCurrent.DTXReader.ReadFromFile( 曲ファイルパス.変数なしパス );
							}
							else
							{
								throw new Exception( $"未対応のフォーマットファイルです。[{曲ファイルパス.変数付きパス}]" );
							}
							//----------------
							#endregion

							using( score )
							{
								// Songsレコード更新。
								var ノーツ数 = _ノーツ数を算出して返す( score, ユーザ設定 );
								var BPMs = _最小最大BPMを調べて返す( score );

								record.HashId = _ファイルのハッシュを算出して返す( 曲ファイルパス );
								record.Title = score.曲名;
								record.LastWriteTime = 調べる曲の最終更新日時;
								record.Level = score.難易度;
								record.MinBPM = BPMs.最小BPM;
								record.MaxBPM = BPMs.最大BPM;
								record.TotalNotes_LeftCymbal = ノーツ数[ 表示レーン種別.LeftCrash ];
								record.TotalNotes_HiHat = ノーツ数[ 表示レーン種別.HiHat ];
								record.TotalNotes_LeftPedal = ノーツ数[ 表示レーン種別.Foot ];
								record.TotalNotes_Snare = ノーツ数[ 表示レーン種別.Snare ];
								record.TotalNotes_Bass = ノーツ数[ 表示レーン種別.Bass ];
								record.TotalNotes_HighTom = ノーツ数[ 表示レーン種別.Tom1 ];
								record.TotalNotes_LowTom = ノーツ数[ 表示レーン種別.Tom2 ];
								record.TotalNotes_FloorTom = ノーツ数[ 表示レーン種別.Tom3 ];
								record.TotalNotes_RightCymbal = ノーツ数[ 表示レーン種別.RightCrash ];
							}

							songdb.DataContext.SubmitChanges();

							Log.Info( $"最終更新日時が変更されているため、曲の情報を更新しました。{曲ファイルパス.変数付きパス}" );
							//----------------
							#endregion
						}
						else
						{
							#region " (B-b) それ以外 → 何もしない "
							//----------------
							//----------------
							#endregion
						}
					}
				}
			}
			catch
			{
				Log.ERROR( $"曲DBへの曲の追加に失敗しました。[{曲ファイルパス.変数付きパス}]" );
				throw;
			}
		}

		/// <summary>
		///		指定したユーザID＆曲ファイルハッシュに対応するレコードがデータベースになければレコードを追加し、
		///		あればそのレコードを（最高記録であれば）更新する。
		/// </summary>
		public static void 成績を追加または更新する( 成績 今回の成績, string ユーザID, string 曲ファイルハッシュ )
		{
			using( var userdb = new UserDB() )
			{
				var record = userdb.Records.Where( ( r ) => ( r.UserId == ユーザID && r.SongHashId == 曲ファイルハッシュ ) ).SingleOrDefault();
				if( null == record )
				{
					// (A) レコードが存在しないので、追加する。
					userdb.Records.InsertOnSubmit( new Record() {
						UserId = ユーザID,
						SongHashId = 曲ファイルハッシュ,
						Score = 今回の成績.Score,
						// todo: CountMap を成績クラスに保存する。
						CountMap = "",
						Skill = 今回の成績.Skill,
						Achievement = 今回の成績.Achievement,
					} );
				}
				else
				{
					// (B) レコードがすでに存在するので、更新する。（記録更新したレコードのみ）

					if( record.Score < 今回の成績.Score )
						record.Score = 今回の成績.Score;

					// todo: CountMap を成績クラスに保存する。

					if( record.Skill < 今回の成績.Skill )
						record.Skill = 今回の成績.Skill;

					if( record.Achievement < 今回の成績.Achievement )
						record.Achievement = 今回の成績.Achievement;
				}

				userdb.DataContext.SubmitChanges();
			}
		}


		private static Dictionary<表示レーン種別, int> _ノーツ数を算出して返す( SSTFormatCurrent.スコア score, ユーザ設定 ユーザ設定 )
		{
			var ノーツ数 = new Dictionary<表示レーン種別, int>();

			foreach( 表示レーン種別 lane in Enum.GetValues( typeof( 表示レーン種別 ) ) )
				ノーツ数.Add( lane, 0 );

			foreach( var chip in score.チップリスト )
			{
				var チップの対応表 = ユーザ設定.ドラムとチップと入力の対応表[ chip.チップ種別 ];

				// AutoPlay ON のチップは、すべてがONである場合を除いて、カウントしない。
				if( ユーザ設定.AutoPlay[ チップの対応表.AutoPlay種別 ] )
				{
					if( !( ユーザ設定.AutoPlayがすべてONである ) )
						continue;
				}
				// AutoPlay OFF 時でもユーザヒットの対象にならないチップはカウントしない。
				if( !( チップの対応表.AutoPlayOFF.ユーザヒット ) )
					continue;

				ノーツ数[ チップの対応表.表示レーン種別 ]++;
			}

			return ノーツ数;
		}

		private static string _ファイルのハッシュを算出して返す( VariablePath 曲ファイルパス )
		{
			var sha512 = new SHA512CryptoServiceProvider();
			byte[] hash = null;

			using( var fs = new FileStream( 曲ファイルパス.変数なしパス, FileMode.Open ) )
				hash = sha512.ComputeHash( fs );

			var hashString = new StringBuilder();
			foreach( byte b in hash )
				hashString.Append( b.ToString( "X2" ) );

			return hashString.ToString();
		}

		private static (double 最小BPM, double 最大BPM) _最小最大BPMを調べて返す( SSTFormatCurrent.スコア score )
		{
			var result = (最小BPM: double.MaxValue, 最大BPM: double.MinValue);

			var BPMchips = score.チップリスト.Where( ( c ) => ( c.チップ種別 == SSTFormatCurrent.チップ種別.BPM ) );
			foreach( var chip in BPMchips )
			{
				result.最小BPM = Math.Min( result.最小BPM, chip.BPM );
				result.最大BPM = Math.Max( result.最大BPM, chip.BPM );
			}

			if( result.最小BPM == double.MaxValue || result.最大BPM == double.MinValue )	// BPMチップがひとつもなかった
			{
				double 初期BPM = SSTFormatCurrent.スコア.初期BPM;
				result = (初期BPM, 初期BPM);
			}

			return result;
		}
	}
}
