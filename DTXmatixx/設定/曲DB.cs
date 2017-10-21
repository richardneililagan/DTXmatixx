using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using FDK;
using SSTFormatCurrent = SSTFormat.v3;
using DTXmatixx.設定.DB;
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
		public static void 曲を追加または更新する( string 曲ファイルパス, ユーザ設定 ユーザ設定 )
		{
			string 調べる曲のパス = Folder.絶対パスに含まれるフォルダ変数を展開して返す( 曲ファイルパス );

			using( var songdb = new SongDB() )
			{
				var 同一パス検索クエリ = songdb.Songs.Where( 
					( song ) => ( song.Path == 調べる曲のパス ) );

				if( 0 == 同一パス検索クエリ.Count() )
				{
					// (A) 同一パスを持つレコードがDBになかった

					var 調べる曲のハッシュ = _ファイルのハッシュを算出して返す( 調べる曲のパス );

					var 同一ハッシュ検索クエリ = songdb.Songs.Where(
						( song ) => ( song.HashId == 調べる曲のハッシュ ) );

					if( 0 == 同一ハッシュ検索クエリ.Count() )
					{
						#region " (A-a) 同一ハッシュを持つレコードがDBになかった → 新規追加 "
						//----------------
						var 拡張子名 = Path.GetExtension( 調べる曲のパス );
						var score = (SSTFormatCurrent.スコア) null;

						if( ".sstf" == 拡張子名 )
						{
							score = new SSTFormatCurrent.スコア( 調べる曲のパス );
						}
						else if( ".dtx" == 拡張子名 )
						{
							score = SSTFormatCurrent.DTXReader.ReadFromFile( 調べる曲のパス );
						}
						else
						{
							throw new Exception( $"未対応のフォーマットファイルです。[{曲ファイルパス}]" );
						}
						using( score )
						{
							var ノーツ数 = _ノーツ数を算出して返す( score, ユーザ設定 );
							var BPMs = _最小最大BPMを調べて返す( score );

							songdb.Songs.InsertOnSubmit(
								new Song() {
									Id = null,
									HashId = _ファイルのハッシュを算出して返す( 調べる曲のパス ),
									Title = score.曲名,
									Path = 調べる曲のパス,
									LastWriteTime = File.GetLastWriteTime( 調べる曲のパス ).ToString( "G" ),
									LeftCymbalNotes = ノーツ数[ 表示レーン種別.LeftCrash ],
									HiHatNotes = ノーツ数[ 表示レーン種別.HiHat ],
									LeftPedalNotes = ノーツ数[ 表示レーン種別.Foot ],
									SnareNotes = ノーツ数[ 表示レーン種別.Snare ],
									BassNotes = ノーツ数[ 表示レーン種別.Bass ],
									HighTomNotes = ノーツ数[ 表示レーン種別.Tom1 ],
									LowTomNotes = ノーツ数[ 表示レーン種別.Tom2 ],
									FloorTomNotes = ノーツ数[ 表示レーン種別.Tom3 ],
									RightCymbalNotes = ノーツ数[ 表示レーン種別.RightCrash ],
									Level = score.難易度,
									MinBPM = BPMs.最小BPM,
									MaxBPM = BPMs.最大BPM,
								} );
							songdb.DataContext.SubmitChanges();
						}

						Log.Info( $"DBに曲を追加しました。{曲ファイルパス}" );
						//----------------
						#endregion
					}
					else
					{
						#region " (A-b) 同一ハッシュを持つレコードがDBにあった → 更新 "
						//----------------
						var record = 同一ハッシュ検索クエリ.Single();
						using( var score = new SSTFormatCurrent.スコア( 調べる曲のパス ) )
						{
							var ノーツ数 = _ノーツ数を算出して返す( score, ユーザ設定 );
							var BPMs = _最小最大BPMを調べて返す( score );

							record.Title = score.曲名;
							record.Path = 調べる曲のパス;
							record.LastWriteTime = File.GetLastWriteTime( 調べる曲のパス ).ToString( "G" );
							record.LeftCymbalNotes = ノーツ数[ 表示レーン種別.LeftCrash ];
							record.HiHatNotes = ノーツ数[ 表示レーン種別.HiHat ];
							record.LeftPedalNotes = ノーツ数[ 表示レーン種別.Foot ];
							record.SnareNotes = ノーツ数[ 表示レーン種別.Snare ];
							record.BassNotes = ノーツ数[ 表示レーン種別.Bass ];
							record.HighTomNotes = ノーツ数[ 表示レーン種別.Tom1 ];
							record.LowTomNotes = ノーツ数[ 表示レーン種別.Tom2 ];
							record.FloorTomNotes = ノーツ数[ 表示レーン種別.Tom3 ];
							record.RightCymbalNotes = ノーツ数[ 表示レーン種別.RightCrash ];
							record.Level = score.難易度;
							record.MinBPM = BPMs.最小BPM;
							record.MaxBPM = BPMs.最大BPM;
						}
						songdb.DataContext.SubmitChanges();

						Log.Info( $"パスが異なりハッシュが同一であるレコードが検出されたため、曲の情報を更新しました。{曲ファイルパス}" );
						//----------------
						#endregion
					}
				}
				else
				{
					// (B) 同一パスを持つレコードがDBにあった

					var record = 同一パス検索クエリ.Single();

					string レコードの最終更新日時 = record.LastWriteTime;
					string 調べる曲の最終更新日時 = File.GetLastWriteTime( 調べる曲のパス ).ToString( "G" );

					if( レコードの最終更新日時 != 調べる曲の最終更新日時 )
					{
						#region " (B-a) 最終更新日時が変更されている → 更新 "
						//----------------
						using( var score = new SSTFormatCurrent.スコア( 調べる曲のパス ) )
						{
							var ノーツ数 = _ノーツ数を算出して返す( score, ユーザ設定 );
							var BPMs = _最小最大BPMを調べて返す( score );

							record.HashId = _ファイルのハッシュを算出して返す( 調べる曲のパス );
							record.Title = score.曲名;
							record.LastWriteTime = 調べる曲の最終更新日時;
							record.LeftCymbalNotes = ノーツ数[ 表示レーン種別.LeftCrash ];
							record.HiHatNotes = ノーツ数[ 表示レーン種別.HiHat ];
							record.LeftPedalNotes = ノーツ数[ 表示レーン種別.Foot ];
							record.SnareNotes = ノーツ数[ 表示レーン種別.Snare ];
							record.BassNotes = ノーツ数[ 表示レーン種別.Bass ];
							record.HighTomNotes = ノーツ数[ 表示レーン種別.Tom1 ];
							record.LowTomNotes = ノーツ数[ 表示レーン種別.Tom2 ];
							record.FloorTomNotes = ノーツ数[ 表示レーン種別.Tom3 ];
							record.RightCymbalNotes = ノーツ数[ 表示レーン種別.RightCrash ];
							record.Level = score.難易度;
							record.MinBPM = BPMs.最小BPM;
							record.MaxBPM = BPMs.最大BPM;
						}
						songdb.DataContext.SubmitChanges();

						Log.Info( $"最終更新日時が変更されているため、曲の情報を更新しました。{曲ファイルパス}" );
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

		/// <summary>
		///		指定されたパスに対応する曲を曲データベースから取得して返す。
		///		見つからなければ null 。
		/// </summary>
		public static Song 曲を取得する( string 曲ファイルパス )
		{
			string filePath = Folder.絶対パスに含まれるフォルダ変数を展開して返す( 曲ファイルパス );

			using( var songdb = new SongDB() )
			{
				return songdb.Songs.Where(
					( song ) => ( song.Path == filePath )
					).SingleOrDefault();
			}
		}

		/// <summary>
		///		指定されたユーザIDと曲（パスで指定）に対応する成績をデータベースから取得して返す。
		///		見つからなければ null。
		/// </summary>
		public static Record ユーザと曲ファイルのパスから成績を取得する( string ユーザID, string 曲ファイルパス )
		{
			var song = 曲を取得する( 曲ファイルパス );
			return ( null != song ) ? ユーザと曲ファイルのハッシュから成績を取得する( ユーザID, song.HashId ) : null;
		}
		
		/// <summary>
		///		指定されたユーザIDと曲（ハッシュで指定）に対応する成績をデータベースから取得して返す。
		///		見つからなければ null。
		/// </summary>
		public static Record ユーザと曲ファイルのハッシュから成績を取得する( string ユーザID, string 曲ファイルハッシュ )
		{
			using( var userdb = new UserDB() )
			{
				return userdb.Records.Where( 
					( record ) => ( record.UserId == ユーザID && record.SongHashId == 曲ファイルハッシュ )
					).SingleOrDefault();
			}
		}

		/// <summary>
		///		指定したユーザID＆曲ファイルハッシュに対応するレコードがデータベースになければレコードを追加し、
		///		あればそのレコードを更新する。
		/// </summary>
		public static void 成績を追加または更新する( 成績 record, string ユーザID, string 曲ファイルハッシュ )
		{
			using( var userdb = new UserDB() )
			{
				var query = userdb.Records.Where(
					( r ) => ( r.UserId == ユーザID && r.SongHashId == 曲ファイルハッシュ )
					).SingleOrDefault();

				if( null != query )
				{
					// (A) レコードがすでに存在するなら、更新する。
					query.Score = record.Score;
					// todo: CountMap を成績クラスに保存する。
					//query.CountMap = record.CountMap;
					query.Skill = record.Skill;
				}
				else
				{
					// (B) レコードが存在しないなら、追加する。
					userdb.Records.InsertOnSubmit( new Record() {
						Id = null,
						UserId = ユーザID,
						SongHashId = 曲ファイルハッシュ,
						Score = record.Score,
						// todo: CountMap を成績クラスに保存する。
						CountMap = "",
						Skill = record.Skill,
					} );
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
		private static string _ファイルのハッシュを算出して返す( string 曲ファイルパス )
		{
			var filePath = Folder.絶対パスに含まれるフォルダ変数を展開して返す( 曲ファイルパス );

			var sha512 = new SHA512CryptoServiceProvider();
			byte[] hash = null;

			using( var fs = new FileStream( filePath, FileMode.Open ) )
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
