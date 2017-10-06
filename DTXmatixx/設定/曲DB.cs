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

		public static void 曲を追加または更新する( string 曲ファイルパス, ユーザ設定 ユーザ設定 )
		{
			string 調べる曲のパス = Folder.絶対パスに含まれるフォルダ変数を展開して返す( 曲ファイルパス );

			using( var songdb = new SongDB() )
			{
				var 同一パス検索クエリ =
					from song in songdb.Songs
					where ( song.Path == 調べる曲のパス )
					select song;

				if( 0 == 同一パス検索クエリ.Count() )
				{
					// (A) 同一パスを持つレコードがDBになかった

					var 調べる曲のハッシュ = _ファイルのハッシュを算出して返す( 調べる曲のパス );
					var 同一ハッシュ検索クエリ =
						from song in songdb.Songs
						where ( song.HashId == 調べる曲のハッシュ )
						select song;

					if( 0 == 同一ハッシュ検索クエリ.Count() )
					{
						#region " (A-a) 同一ハッシュを持つレコードがDBになかった → 新規追加 "
						//----------------
						using( var score = new SSTFormatCurrent.スコア( 曲ファイルパス ) )
						{
							var ノーツ数 = _ノーツ数を算出して返す( score, ユーザ設定 );

							songdb.Songs.InsertOnSubmit(
								new Song() {
									Id = null,
									HashId = _ファイルのハッシュを算出して返す( 調べる曲のパス ),
									Title = score.Header.曲名,
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
						foreach( var record in 同一ハッシュ検索クエリ )
						{
							using( var score = new SSTFormatCurrent.スコア( 調べる曲のパス ) )
							{
								record.Title = score.Header.曲名;
								record.Path = 調べる曲のパス;
								record.LastWriteTime = File.GetLastWriteTime( 調べる曲のパス ).ToString( "G" );

								var ノーツ数 = _ノーツ数を算出して返す( score, ユーザ設定 );
								record.LeftCymbalNotes = ノーツ数[ 表示レーン種別.LeftCrash ];
								record.HiHatNotes = ノーツ数[ 表示レーン種別.HiHat ];
								record.LeftPedalNotes = ノーツ数[ 表示レーン種別.Foot ];
								record.SnareNotes = ノーツ数[ 表示レーン種別.Snare ];
								record.BassNotes = ノーツ数[ 表示レーン種別.Bass ];
								record.HighTomNotes = ノーツ数[ 表示レーン種別.Tom1 ];
								record.LowTomNotes = ノーツ数[ 表示レーン種別.Tom2 ];
								record.FloorTomNotes = ノーツ数[ 表示レーン種別.Tom3 ];
								record.RightCymbalNotes = ノーツ数[ 表示レーン種別.RightCrash ];
							}
							songdb.DataContext.SubmitChanges();
						}

						Log.Info( $"パスが異なりハッシュが同一であるレコードが検出されたため、曲の情報を更新しました。{曲ファイルパス}" );
						//----------------
						#endregion
					}
				}
				else
				{
					// (B) 同一パスを持つレコードがDBにあった

					foreach( var record in 同一パス検索クエリ )
					{
						string レコードの最終更新日時 = record.LastWriteTime;
						string 調べる曲の最終更新日時 = File.GetLastWriteTime( 調べる曲のパス ).ToString( "G" );

						if( レコードの最終更新日時 != 調べる曲の最終更新日時 )
						{
							#region " (B-a) 最終更新日時が変更されている → 更新 "
							//----------------
							using( var score = new SSTFormatCurrent.スコア( 調べる曲のパス ) )
							{
								record.HashId = _ファイルのハッシュを算出して返す( 調べる曲のパス );
								record.Title = score.Header.曲名;
								record.LastWriteTime = 調べる曲の最終更新日時;

								var ノーツ数 = _ノーツ数を算出して返す( score, ユーザ設定 );
								record.LeftCymbalNotes = ノーツ数[ 表示レーン種別.LeftCrash ];
								record.HiHatNotes = ノーツ数[ 表示レーン種別.HiHat ];
								record.LeftPedalNotes = ノーツ数[ 表示レーン種別.Foot ];
								record.SnareNotes = ノーツ数[ 表示レーン種別.Snare ];
								record.BassNotes = ノーツ数[ 表示レーン種別.Bass ];
								record.HighTomNotes = ノーツ数[ 表示レーン種別.Tom1 ];
								record.LowTomNotes = ノーツ数[ 表示レーン種別.Tom2 ];
								record.FloorTomNotes = ノーツ数[ 表示レーン種別.Tom3 ];
								record.RightCymbalNotes = ノーツ数[ 表示レーン種別.RightCrash ];
							}
							songdb.DataContext.SubmitChanges();

							Log.Info( $"最終更新日時が変更されているため、曲の情報を更新しました。{曲ファイルパス}" );
							//----------------
							#endregion
						}
						else
						{
							// (B-b) それ以外 → 何もしない
						}

						break;	// 1つしか存在しないはずだが念のため。
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
				var query = from song in songdb.Songs
							where ( song.Path == filePath )
							select song;

				foreach( var song in query )
					return song;   // あっても1個しかないはずなので即返す。
			}

			return null;
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
	}
}
