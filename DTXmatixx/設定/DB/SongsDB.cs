using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Linq;
using System.Data.Linq.Mapping;
using System.Data.SQLite;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using FDK;
using SSTFormatCurrent = SSTFormat.v3;
using DTXmatixx.ステージ.演奏;

namespace DTXmatixx.設定.DB
{
	/// <summary>
	///		曲の情報をキャッシュしておくデータベースを管理するためのクラス。
	///		<see cref="Song"/> テーブルを包含。
	/// </summary>
	class SongsDB : IDisposable
	{
		public SongsDB( string DBファイルパス )
		{
			this.初期化する( DBファイルパス );
		}

		public void Dispose()
		{
		}

		/// <summary>
		///		指定されたデータベースへ接続し、もし存在していないなら、テーブルとインデックスを作成する。
		/// </summary>
		/// <param name="DBファイルパス">データベースファイルの絶対パス。フォルダ変数使用可。</param>
		public void 初期化する( string DBファイルパス )
		{
			this._DBファイルパス = Folder.絶対パスに含まれるフォルダ変数を展開して返す( DBファイルパス );

			var DC接続文字列 = new SQLiteConnectionStringBuilder { DataSource = this._DBファイルパス };
			using( var DB接続 = new SQLiteConnection( DC接続文字列.ToString() ) )
			{
				DB接続.Open();

				using( var command = new SQLiteCommand( DB接続 ) )
				{
					// Songs テーブルを作成。
					command.CommandText = Song.CreateTable;
					command.ExecuteNonQuery();

					// Songs テーブルにインデックスを作成。
					command.CommandText = Song.CreateIndex;
					command.ExecuteNonQuery();
				}
			}
		}

		/// <summary>
		///		曲ファイルをデータベースに追加する。
		///		すでに存在している場合は、必要があればレコードを更新する。
		/// </summary>
		/// <param name="曲ファイルパス">曲ファイルへの絶対パス。フォルダ変数使用可。</param>
		public void 曲を追加または更新する( string 曲ファイルパス, ユーザ設定 options )
		{
			string 調べる曲のパス = Folder.絶対パスに含まれるフォルダ変数を展開して返す( 曲ファイルパス );

			var DC接続文字列 = new SQLiteConnectionStringBuilder { DataSource = this._DBファイルパス };
			using( var DB接続 = new SQLiteConnection( DC接続文字列.ToString() ) )
			{
				DB接続.Open();

				using( var context = new DataContext( DB接続 ) )
				{
					var table = context.GetTable<Song>();

					var 同一パス検索クエリ = from s in table where ( s.Path == 調べる曲のパス ) select s;

					if( 0 == 同一パス検索クエリ.Count() )
					{
						// (A) 同一パスを持つレコードがDBになかった

						var 調べる曲のハッシュ = this._ファイルのハッシュを算出して返す( 調べる曲のパス );
						var 同一ハッシュ検索クエリ = from s in table where ( s.HashId == 調べる曲のハッシュ ) select s;

						if( 0 == 同一ハッシュ検索クエリ.Count() )
						{
							#region " (A-a) 同一ハッシュを持つレコードがDBになかった → 新規追加 "
							//----------------
							using( var score = new SSTFormatCurrent.スコア( 曲ファイルパス ) )
							{
								var ノーツ数 = this._ノーツ数を算出して返す( score, options );

								table.InsertOnSubmit( new Song() {

									Id = null,
									HashId = this._ファイルのハッシュを算出して返す( 調べる曲のパス ),
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

									var ノーツ数 = this._ノーツ数を算出して返す( score, options );
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
									record.HashId = this._ファイルのハッシュを算出して返す( 調べる曲のパス );
									record.Title = score.Header.曲名;
									record.LastWriteTime = 調べる曲の最終更新日時;

									var ノーツ数 = this._ノーツ数を算出して返す( score, options );
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

								Log.Info( $"最終更新日時が変更されているため、曲の情報を更新しました。{曲ファイルパス}" );
								//----------------
								#endregion
							}
							else
							{
								// (B-b) それ以外 → 何もしない
							}
						}
					}

					context.SubmitChanges();
				}
			}
		}

		/// <summary>
		///		データベースから曲の情報を検索して返す。
		///		存在しなかったら null を返す。
		/// </summary>
		public Song 曲の情報を返す( string 曲ファイルパス )
		{
			string songFile = Folder.絶対パスに含まれるフォルダ変数を展開して返す( 曲ファイルパス );

			var DC接続文字列 = new SQLiteConnectionStringBuilder { DataSource = this._DBファイルパス };
			using( var DB接続 = new SQLiteConnection( DC接続文字列.ToString() ) )
			{
				DB接続.Open();

				using( var context = new DataContext( DB接続 ) )
				{
					var table = context.GetTable<Song>();
					var query = from s in table where ( s.Path == songFile ) select s;

					foreach( var s in query )
						return s;	// あっても1個しかないはずなので即返す。
				}
			}

			return null;
		}


		/// <summary>
		///		コンストラクタで渡された、データベースファイルの絶対パス。
		///		フォルダ変数は展開済み。
		/// </summary>
		private string _DBファイルパス;

		private Dictionary<表示レーン種別, int> _ノーツ数を算出して返す( SSTFormatCurrent.スコア score, ユーザ設定 options )
		{
			var ノーツ数 = new Dictionary<表示レーン種別, int>();

			foreach( 表示レーン種別 lane in Enum.GetValues( typeof( 表示レーン種別 ) ) )
				ノーツ数.Add( lane, 0 );

			foreach( var chip in score.チップリスト )
			{
				var チップの対応表 = options.ドラムとチップと入力の対応表[ chip.チップ種別 ];

				// AutoPlay ON のチップは、すべてがONである場合を除いて、カウントしない。
				if( options.AutoPlay[ チップの対応表.AutoPlay種別 ] )
				{
					if( !( options.AutoPlayがすべてONである ) )
						continue;
				}
				// AutoPlay OFF 時でもユーザヒットの対象にならないチップはカウントしない。
				if( !( チップの対応表.AutoPlayOFF.ユーザヒット ) )
					continue;

				ノーツ数[ チップの対応表.表示レーン種別 ]++;
			}

			return ノーツ数;
		}
		private string _ファイルのハッシュを算出して返す( string 曲ファイルパス )
		{
			var sha512 = new SHA512CryptoServiceProvider();
			byte[] hash = null;

			using( var fs = new FileStream( 曲ファイルパス, FileMode.Open ) )
				hash = sha512.ComputeHash( fs );

			var hashString = new StringBuilder();
			foreach( byte b in hash )
				hashString.Append( b.ToString( "X2" ) );

			return hashString.ToString();
		}
	}
}
