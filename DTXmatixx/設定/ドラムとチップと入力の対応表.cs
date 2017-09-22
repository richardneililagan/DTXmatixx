using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using FDK;
using DTXmatixx.設定;
using DTXmatixx.入力;
using DTXmatixx.ステージ.演奏;

using チップ種別 = SSTFormat.v2.チップ種別;
using レーン種別 = SSTFormat.v2.レーン種別;

namespace DTXmatixx.設定
{
	class ドラムとチップと入力の対応表
	{
		/// <summary>
		///		対応表の列定義。
		/// </summary>
		public class Column
		{
			public チップ種別 チップ種別 { get; set; }

			public レーン種別 レーン種別 { get; set; }

			public ドラム入力種別 ドラム入力種別 { get; set; }

			public 表示レーン種別 表示レーン種別 { get; set; }

			public AutoPlay種別 AutoPlay種別 { get; set; }

			public struct Columnヒット処理
			{
				public bool 再生 { get; set; }

				public bool 非表示 { get; set; }

				public bool 判定 { get; set; }
			}

			public struct ColumnAutoPlayON
			{
				public bool 自動ヒット { get; set; }

				public Columnヒット処理 自動ヒット時処理 { get; set; }

				public bool MISS判定 { get; set; }
			}
			public ColumnAutoPlayON AutoPlayON { get; set; }

			public struct ColumnAutoPlayOFF
			{
				public bool 自動ヒット { get; set; }

				public Columnヒット処理 自動ヒット時処理 { get; set; }

				public bool ユーザヒット { get; set; }

				public Columnヒット処理 ユーザヒット時処理 { get; set; }

				public bool MISS判定 { get; set; }
			}
			public ColumnAutoPlayOFF AutoPlayOFF { get; set; }

			public bool シンバルフリーの対象 { get; set; }
		}

		/// <summary>
		///		チップ種別をキーとする対応表。
		/// </summary>
		public Dictionary<チップ種別, Column> 対応表
		{
			get;
			protected set;
		}

		public Column this[ チップ種別 chipType ]
		{
			get
				=> this.対応表[ chipType ];
		}

		/// <summary>
		///		コンストラクタ。対応表を生成する。
		/// </summary>
		public ドラムとチップと入力の対応表( 表示レーンの左右 表示レーンの左右 )
		{
			using( Log.Block( FDKUtilities.現在のメソッド名 ) )
			{
				this.対応表 = new Dictionary<チップ種別, Column>() {
					#region " チップ種別.Unknown "
					//----------------
					[ チップ種別.Unknown ] = new Column() {
						チップ種別 = チップ種別.Unknown,
						レーン種別 = レーン種別.Unknown,
						ドラム入力種別 = ドラム入力種別.Unknown,
						表示レーン種別 = 表示レーン種別.Unknown,
						AutoPlay種別 = AutoPlay種別.Unknown,
						AutoPlayON = new Column.ColumnAutoPlayON() {
							自動ヒット = false,
							自動ヒット時処理 = new Column.Columnヒット処理() {
								再生 = false,
								非表示 = false,
								判定 = false,
							},
							MISS判定 = false,
						},
						AutoPlayOFF = new Column.ColumnAutoPlayOFF() {
							自動ヒット = false,
							自動ヒット時処理 = new Column.Columnヒット処理() {
								再生 = false,
								非表示 = false,
								判定 = false,
							},
							ユーザヒット = false,
							ユーザヒット時処理 = new Column.Columnヒット処理() {
								再生 = false,
								非表示 = false,
								判定 = false,
							},
							MISS判定 = false,
						},
						シンバルフリーの対象 = false,
					},
					//----------------
					#endregion
					#region " チップ種別.LeftCrash "
					//----------------
					[ チップ種別.LeftCrash ] = new Column() {
						チップ種別 = チップ種別.LeftCrash,
						レーン種別 = レーン種別.LeftCrash,
						ドラム入力種別 = ドラム入力種別.LeftCrash,
						表示レーン種別 = 表示レーン種別.LeftCrash,
						AutoPlay種別 = AutoPlay種別.LeftCrash,
						AutoPlayON = new Column.ColumnAutoPlayON() {
							自動ヒット = true,
							自動ヒット時処理 = new Column.Columnヒット処理() {
								再生 = true,
								非表示 = true,
								判定 = true,
							},
							MISS判定 = true,
						},
						AutoPlayOFF = new Column.ColumnAutoPlayOFF() {
							自動ヒット = false,
							自動ヒット時処理 = new Column.Columnヒット処理() {
								再生 = false,
								非表示 = false,
								判定 = false,
							},
							ユーザヒット = true,
							ユーザヒット時処理 = new Column.Columnヒット処理() {
								再生 = false,
								非表示 = true,
								判定 = true,
							},
							MISS判定 = true,
						},
						シンバルフリーの対象 = true,
					},
					//----------------
					#endregion
					#region " チップ種別.Ride "
					//----------------
					[ チップ種別.Ride ] = new Column() {
						チップ種別 = チップ種別.Ride,
						レーン種別 = レーン種別.Ride,
						ドラム入力種別 = ドラム入力種別.Ride,
						表示レーン種別 = ( 表示レーンの左右.Rideは左 ) ? 表示レーン種別.LeftCrash : 表示レーン種別.RightCrash,
						AutoPlay種別 = AutoPlay種別.RightCrash,
						AutoPlayON = new Column.ColumnAutoPlayON() {
							自動ヒット = true,
							自動ヒット時処理 = new Column.Columnヒット処理() {
								再生 = true,
								非表示 = true,
								判定 = true,
							},
							MISS判定 = true,
						},
						AutoPlayOFF = new Column.ColumnAutoPlayOFF() {
							自動ヒット = false,
							自動ヒット時処理 = new Column.Columnヒット処理() {
								再生 = false,
								非表示 = false,
								判定 = false,
							},
							ユーザヒット = true,
							ユーザヒット時処理 = new Column.Columnヒット処理() {
								再生 = false,
								非表示 = true,
								判定 = true,
							},
							MISS判定 = true,
						},
						シンバルフリーの対象 = true,
					},
					//----------------
					#endregion
					#region " チップ種別.Ride_Cup "
					//----------------
					[ チップ種別.Ride_Cup ] = new Column() {
						チップ種別 = チップ種別.Ride_Cup,
						レーン種別 = レーン種別.Ride,
						ドラム入力種別 = ドラム入力種別.Ride, // Rideと同義。
						表示レーン種別 = ( 表示レーンの左右.Rideは左 ) ? 表示レーン種別.LeftCrash : 表示レーン種別.RightCrash,
						AutoPlay種別 = AutoPlay種別.RightCrash,
						AutoPlayON = new Column.ColumnAutoPlayON() {
							自動ヒット = true,
							自動ヒット時処理 = new Column.Columnヒット処理() {
								再生 = true,
								非表示 = true,
								判定 = true,
							},
							MISS判定 = true,
						},
						AutoPlayOFF = new Column.ColumnAutoPlayOFF() {
							自動ヒット = false,
							自動ヒット時処理 = new Column.Columnヒット処理() {
								再生 = false,
								非表示 = false,
								判定 = false,
							},
							ユーザヒット = true,
							ユーザヒット時処理 = new Column.Columnヒット処理() {
								再生 = false,
								非表示 = true,
								判定 = true,
							},
							MISS判定 = true,
						},
						シンバルフリーの対象 = true,
					},
					//----------------
					#endregion
					#region " チップ種別.China "
					//----------------
					[ チップ種別.China ] = new Column() {
						チップ種別 = チップ種別.China,
						レーン種別 = レーン種別.China,
						ドラム入力種別 = ドラム入力種別.China,
						表示レーン種別 = ( 表示レーンの左右.Chinaは左 ) ? 表示レーン種別.LeftCrash : 表示レーン種別.RightCrash,
						AutoPlay種別 = AutoPlay種別.RightCrash,
						AutoPlayON = new Column.ColumnAutoPlayON() {
							自動ヒット = true,
							自動ヒット時処理 = new Column.Columnヒット処理() {
								再生 = true,
								非表示 = true,
								判定 = true,
							},
							MISS判定 = true,
						},
						AutoPlayOFF = new Column.ColumnAutoPlayOFF() {
							自動ヒット = false,
							自動ヒット時処理 = new Column.Columnヒット処理() {
								再生 = false,
								非表示 = false,
								判定 = false,
							},
							ユーザヒット = true,
							ユーザヒット時処理 = new Column.Columnヒット処理() {
								再生 = false,
								非表示 = true,
								判定 = true,
							},
							MISS判定 = true,
						},
						シンバルフリーの対象 = true,
					},
					//----------------
					#endregion
					#region " チップ種別.Splash "
					//----------------
					[ チップ種別.Splash ] = new Column() {
						チップ種別 = チップ種別.Splash,
						レーン種別 = レーン種別.Splash,
						ドラム入力種別 = ドラム入力種別.Splash,
						表示レーン種別 = ( 表示レーンの左右.Splashは左 ) ? 表示レーン種別.LeftCrash : 表示レーン種別.RightCrash,
						AutoPlay種別 = AutoPlay種別.LeftCrash,
						AutoPlayON = new Column.ColumnAutoPlayON() {
							自動ヒット = true,
							自動ヒット時処理 = new Column.Columnヒット処理() {
								再生 = true,
								非表示 = true,
								判定 = true,
							},
							MISS判定 = true,
						},
						AutoPlayOFF = new Column.ColumnAutoPlayOFF() {
							自動ヒット = false,
							自動ヒット時処理 = new Column.Columnヒット処理() {
								再生 = false,
								非表示 = false,
								判定 = false,
							},
							ユーザヒット = true,
							ユーザヒット時処理 = new Column.Columnヒット処理() {
								再生 = false,
								非表示 = true,
								判定 = true,
							},
							MISS判定 = true,
						},
						シンバルフリーの対象 = true,
					},
					//----------------
					#endregion
					#region " チップ種別.HiHat_Open "
					//----------------
					[ チップ種別.HiHat_Open ] = new Column() {
						チップ種別 = チップ種別.HiHat_Open,
						レーン種別 = レーン種別.HiHat,
						ドラム入力種別 = ドラム入力種別.HiHat_Open,
						表示レーン種別 = 表示レーン種別.HiHat,
						AutoPlay種別 = AutoPlay種別.HiHat,
						AutoPlayON = new Column.ColumnAutoPlayON() {
							自動ヒット = true,
							自動ヒット時処理 = new Column.Columnヒット処理() {
								再生 = true,
								非表示 = true,
								判定 = true,
							},
							MISS判定 = true,
						},
						AutoPlayOFF = new Column.ColumnAutoPlayOFF() {
							自動ヒット = false,
							自動ヒット時処理 = new Column.Columnヒット処理() {
								再生 = false,
								非表示 = false,
								判定 = false,
							},
							ユーザヒット = true,
							ユーザヒット時処理 = new Column.Columnヒット処理() {
								再生 = false,
								非表示 = true,
								判定 = true,
							},
							MISS判定 = true,
						},
						シンバルフリーの対象 = true,
					},
					//----------------
					#endregion
					#region " チップ種別.HiHat_HalfOpen "
					//----------------
					[ チップ種別.HiHat_HalfOpen ] = new Column() {
						チップ種別 = チップ種別.HiHat_HalfOpen,
						レーン種別 = レーン種別.HiHat,
						ドラム入力種別 = ドラム入力種別.HiHat_Open,   // Openと同義。
						表示レーン種別 = 表示レーン種別.HiHat,
						AutoPlay種別 = AutoPlay種別.HiHat,
						AutoPlayON = new Column.ColumnAutoPlayON() {
							自動ヒット = true,
							自動ヒット時処理 = new Column.Columnヒット処理() {
								再生 = true,
								非表示 = true,
								判定 = true,
							},
							MISS判定 = true,
						},
						AutoPlayOFF = new Column.ColumnAutoPlayOFF() {
							自動ヒット = false,
							自動ヒット時処理 = new Column.Columnヒット処理() {
								再生 = false,
								非表示 = false,
								判定 = false,
							},
							ユーザヒット = true,
							ユーザヒット時処理 = new Column.Columnヒット処理() {
								再生 = false,
								非表示 = true,
								判定 = true,
							},
							MISS判定 = true,
						},
						シンバルフリーの対象 = true,
					},
					//----------------
					#endregion
					#region " チップ種別.HiHat_Close "
					//----------------
					[ チップ種別.HiHat_Close ] = new Column() {
						チップ種別 = チップ種別.HiHat_Close,
						レーン種別 = レーン種別.HiHat,
						ドラム入力種別 = ドラム入力種別.HiHat_Close,
						表示レーン種別 = 表示レーン種別.HiHat,
						AutoPlay種別 = AutoPlay種別.HiHat,
						AutoPlayON = new Column.ColumnAutoPlayON() {
							自動ヒット = true,
							自動ヒット時処理 = new Column.Columnヒット処理() {
								再生 = true,
								非表示 = true,
								判定 = true,
							},
							MISS判定 = true,
						},
						AutoPlayOFF = new Column.ColumnAutoPlayOFF() {
							自動ヒット = false,
							自動ヒット時処理 = new Column.Columnヒット処理() {
								再生 = false,
								非表示 = false,
								判定 = false,
							},
							ユーザヒット = true,
							ユーザヒット時処理 = new Column.Columnヒット処理() {
								再生 = false,
								非表示 = true,
								判定 = true,
							},
							MISS判定 = true,
						},
						シンバルフリーの対象 = true,
					},
					//----------------
					#endregion
					#region " チップ種別.HiHat_Foot "
					//----------------
					[ チップ種別.HiHat_Foot ] = new Column() {
						チップ種別 = チップ種別.HiHat_Foot,
						レーン種別 = レーン種別.Foot,
						ドラム入力種別 = ドラム入力種別.Unknown,  // 使用しない。
						表示レーン種別 = 表示レーン種別.Foot,
						AutoPlay種別 = AutoPlay種別.Foot,
						AutoPlayON = new Column.ColumnAutoPlayON() {
							自動ヒット = true,
							自動ヒット時処理 = new Column.Columnヒット処理() {
								再生 = true,
								非表示 = true,
								判定 = false,
							},
							MISS判定 = false,
						},
						AutoPlayOFF = new Column.ColumnAutoPlayOFF() {
							自動ヒット = true,
							自動ヒット時処理 = new Column.Columnヒット処理() {
								再生 = false,
								非表示 = true,
								判定 = false,
							},
							ユーザヒット = true,
							ユーザヒット時処理 = new Column.Columnヒット処理() {
								再生 = false,
								非表示 = true,
								判定 = false,
							},
							MISS判定 = false,
						},
						シンバルフリーの対象 = false,
					},
					//----------------
					#endregion
					#region " チップ種別.Snare "
					//----------------
					[ チップ種別.Snare ] = new Column() {
						チップ種別 = チップ種別.Snare,
						レーン種別 = レーン種別.Snare,
						ドラム入力種別 = ドラム入力種別.Snare,
						表示レーン種別 = 表示レーン種別.Snare,
						AutoPlay種別 = AutoPlay種別.Snare,
						AutoPlayON = new Column.ColumnAutoPlayON() {
							自動ヒット = true,
							自動ヒット時処理 = new Column.Columnヒット処理() {
								再生 = true,
								非表示 = true,
								判定 = true,
							},
							MISS判定 = true,
						},
						AutoPlayOFF = new Column.ColumnAutoPlayOFF() {
							自動ヒット = false,
							自動ヒット時処理 = new Column.Columnヒット処理() {
								再生 = false,
								非表示 = false,
								判定 = false,
							},
							ユーザヒット = true,
							ユーザヒット時処理 = new Column.Columnヒット処理() {
								再生 = false,
								非表示 = true,
								判定 = true,
							},
							MISS判定 = true,
						},
						シンバルフリーの対象 = false,
					},
					//----------------
					#endregion
					#region " チップ種別.Snare_OpenRim "
					//----------------
					[ チップ種別.Snare_OpenRim ] = new Column() {
						チップ種別 = チップ種別.Snare_OpenRim,
						レーン種別 = レーン種別.Snare,
						ドラム入力種別 = ドラム入力種別.Snare_OpenRim,
						表示レーン種別 = 表示レーン種別.Snare,
						AutoPlay種別 = AutoPlay種別.Snare,
						AutoPlayON = new Column.ColumnAutoPlayON() {
							自動ヒット = true,
							自動ヒット時処理 = new Column.Columnヒット処理() {
								再生 = true,
								非表示 = true,
								判定 = true,
							},
							MISS判定 = true,
						},
						AutoPlayOFF = new Column.ColumnAutoPlayOFF() {
							自動ヒット = false,
							自動ヒット時処理 = new Column.Columnヒット処理() {
								再生 = false,
								非表示 = false,
								判定 = false,
							},
							ユーザヒット = true,
							ユーザヒット時処理 = new Column.Columnヒット処理() {
								再生 = false,
								非表示 = true,
								判定 = true,
							},
							MISS判定 = true,
						},
						シンバルフリーの対象 = false,
					},
					//----------------
					#endregion
					#region " チップ種別.Snare_ClosedRim "
					//----------------
					[ チップ種別.Snare_ClosedRim ] = new Column() {
						チップ種別 = チップ種別.Snare_ClosedRim,
						レーン種別 = レーン種別.Snare,
						ドラム入力種別 = ドラム入力種別.Snare_ClosedRim,
						表示レーン種別 = 表示レーン種別.Snare,
						AutoPlay種別 = AutoPlay種別.Snare,
						AutoPlayON = new Column.ColumnAutoPlayON() {
							自動ヒット = true,
							自動ヒット時処理 = new Column.Columnヒット処理() {
								再生 = true,
								非表示 = true,
								判定 = true,
							},
							MISS判定 = true,
						},
						AutoPlayOFF = new Column.ColumnAutoPlayOFF() {
							自動ヒット = false,
							自動ヒット時処理 = new Column.Columnヒット処理() {
								再生 = false,
								非表示 = false,
								判定 = false,
							},
							ユーザヒット = true,
							ユーザヒット時処理 = new Column.Columnヒット処理() {
								再生 = false,
								非表示 = true,
								判定 = true,
							},
							MISS判定 = true,
						},
						シンバルフリーの対象 = false,
					},
					//----------------
					#endregion
					#region " チップ種別.Snare_Ghost "
					//----------------
					[ チップ種別.Snare_Ghost ] = new Column() {
						チップ種別 = チップ種別.Snare_Ghost,
						レーン種別 = レーン種別.Snare,
						ドラム入力種別 = ドラム入力種別.Unknown,  // 使用しない。
						表示レーン種別 = 表示レーン種別.Snare,
						AutoPlay種別 = AutoPlay種別.Snare,
						AutoPlayON = new Column.ColumnAutoPlayON() {
							自動ヒット = true,
							自動ヒット時処理 = new Column.Columnヒット処理() {
								再生 = true,
								非表示 = true,
								判定 = false,
							},
							MISS判定 = false,
						},
						AutoPlayOFF = new Column.ColumnAutoPlayOFF() {
							自動ヒット = true,
							自動ヒット時処理 = new Column.Columnヒット処理() {
								再生 = false,
								非表示 = true,
								判定 = false,
							},
							ユーザヒット = true,
							ユーザヒット時処理 = new Column.Columnヒット処理() {
								再生 = false,
								非表示 = true,
								判定 = false,
							},
							MISS判定 = false,
						},
						シンバルフリーの対象 = false,
					},
					//----------------
					#endregion
					#region " チップ種別.Bass "
					//----------------
					[ チップ種別.Bass ] = new Column() {
						チップ種別 = チップ種別.Bass,
						レーン種別 = レーン種別.Bass,
						ドラム入力種別 = ドラム入力種別.Bass,
						表示レーン種別 = 表示レーン種別.Bass,
						AutoPlay種別 = AutoPlay種別.Bass,
						AutoPlayON = new Column.ColumnAutoPlayON() {
							自動ヒット = true,
							自動ヒット時処理 = new Column.Columnヒット処理() {
								再生 = true,
								非表示 = true,
								判定 = true,
							},
							MISS判定 = true,
						},
						AutoPlayOFF = new Column.ColumnAutoPlayOFF() {
							自動ヒット = false,
							自動ヒット時処理 = new Column.Columnヒット処理() {
								再生 = false,
								非表示 = false,
								判定 = false,
							},
							ユーザヒット = true,
							ユーザヒット時処理 = new Column.Columnヒット処理() {
								再生 = false,
								非表示 = true,
								判定 = true,
							},
							MISS判定 = true,
						},
						シンバルフリーの対象 = false,
					},
					//----------------
					#endregion
					#region " チップ種別.Tom1 "
					//----------------
					[ チップ種別.Tom1 ] = new Column() {
						チップ種別 = チップ種別.Tom1,
						レーン種別 = レーン種別.Tom1,
						ドラム入力種別 = ドラム入力種別.Tom1,
						表示レーン種別 = 表示レーン種別.Tom1,
						AutoPlay種別 = AutoPlay種別.Tom1,
						AutoPlayON = new Column.ColumnAutoPlayON() {
							自動ヒット = true,
							自動ヒット時処理 = new Column.Columnヒット処理() {
								再生 = true,
								非表示 = true,
								判定 = true,
							},
							MISS判定 = true,
						},
						AutoPlayOFF = new Column.ColumnAutoPlayOFF() {
							自動ヒット = false,
							自動ヒット時処理 = new Column.Columnヒット処理() {
								再生 = false,
								非表示 = false,
								判定 = false,
							},
							ユーザヒット = true,
							ユーザヒット時処理 = new Column.Columnヒット処理() {
								再生 = false,
								非表示 = true,
								判定 = true,
							},
							MISS判定 = true,
						},
						シンバルフリーの対象 = false,
					},
					//----------------
					#endregion
					#region " チップ種別.Tom1_Rim "
					//----------------
					[ チップ種別.Tom1_Rim ] = new Column() {
						チップ種別 = チップ種別.Tom1_Rim,
						レーン種別 = レーン種別.Tom1,
						ドラム入力種別 = ドラム入力種別.Tom1_Rim,
						表示レーン種別 = 表示レーン種別.Tom1,
						AutoPlay種別 = AutoPlay種別.Tom1,
						AutoPlayON = new Column.ColumnAutoPlayON() {
							自動ヒット = true,
							自動ヒット時処理 = new Column.Columnヒット処理() {
								再生 = true,
								非表示 = true,
								判定 = true,
							},
							MISS判定 = true,
						},
						AutoPlayOFF = new Column.ColumnAutoPlayOFF() {
							自動ヒット = false,
							自動ヒット時処理 = new Column.Columnヒット処理() {
								再生 = false,
								非表示 = false,
								判定 = false,
							},
							ユーザヒット = true,
							ユーザヒット時処理 = new Column.Columnヒット処理() {
								再生 = false,
								非表示 = true,
								判定 = true,
							},
							MISS判定 = true,
						},
						シンバルフリーの対象 = false,
					},
					//----------------
					#endregion
					#region " チップ種別.Tom2 "
					//----------------
					[ チップ種別.Tom2 ] = new Column() {
						チップ種別 = チップ種別.Tom2,
						レーン種別 = レーン種別.Tom2,
						ドラム入力種別 = ドラム入力種別.Tom2,
						表示レーン種別 = 表示レーン種別.Tom2,
						AutoPlay種別 = AutoPlay種別.Tom2,
						AutoPlayON = new Column.ColumnAutoPlayON() {
							自動ヒット = true,
							自動ヒット時処理 = new Column.Columnヒット処理() {
								再生 = true,
								非表示 = true,
								判定 = true,
							},
							MISS判定 = true,
						},
						AutoPlayOFF = new Column.ColumnAutoPlayOFF() {
							自動ヒット = false,
							自動ヒット時処理 = new Column.Columnヒット処理() {
								再生 = false,
								非表示 = false,
								判定 = false,
							},
							ユーザヒット = true,
							ユーザヒット時処理 = new Column.Columnヒット処理() {
								再生 = false,
								非表示 = true,
								判定 = true,
							},
							MISS判定 = true,
						},
						シンバルフリーの対象 = false,
					},
					//----------------
					#endregion
					#region " チップ種別.Tom2_Rim "
					//----------------
					[ チップ種別.Tom2_Rim ] = new Column() {
						チップ種別 = チップ種別.Tom2_Rim,
						レーン種別 = レーン種別.Tom2,
						ドラム入力種別 = ドラム入力種別.Tom2_Rim,
						表示レーン種別 = 表示レーン種別.Tom2,
						AutoPlay種別 = AutoPlay種別.Tom2,
						AutoPlayON = new Column.ColumnAutoPlayON() {
							自動ヒット = true,
							自動ヒット時処理 = new Column.Columnヒット処理() {
								再生 = true,
								非表示 = true,
								判定 = true,
							},
							MISS判定 = true,
						},
						AutoPlayOFF = new Column.ColumnAutoPlayOFF() {
							自動ヒット = false,
							自動ヒット時処理 = new Column.Columnヒット処理() {
								再生 = false,
								非表示 = false,
								判定 = false,
							},
							ユーザヒット = true,
							ユーザヒット時処理 = new Column.Columnヒット処理() {
								再生 = false,
								非表示 = true,
								判定 = true,
							},
							MISS判定 = true,
						},
						シンバルフリーの対象 = false,
					},
					//----------------
					#endregion
					#region " チップ種別.Tom3 "
					//----------------
					[ チップ種別.Tom3 ] = new Column() {
						チップ種別 = チップ種別.Tom3,
						レーン種別 = レーン種別.Tom3,
						ドラム入力種別 = ドラム入力種別.Tom3,
						表示レーン種別 = 表示レーン種別.Tom3,
						AutoPlay種別 = AutoPlay種別.Tom3,
						AutoPlayON = new Column.ColumnAutoPlayON() {
							自動ヒット = true,
							自動ヒット時処理 = new Column.Columnヒット処理() {
								再生 = true,
								非表示 = true,
								判定 = true,
							},
							MISS判定 = true,
						},
						AutoPlayOFF = new Column.ColumnAutoPlayOFF() {
							自動ヒット = false,
							自動ヒット時処理 = new Column.Columnヒット処理() {
								再生 = false,
								非表示 = false,
								判定 = false,
							},
							ユーザヒット = true,
							ユーザヒット時処理 = new Column.Columnヒット処理() {
								再生 = false,
								非表示 = true,
								判定 = true,
							},
							MISS判定 = true,
						},
						シンバルフリーの対象 = false,
					},
					//----------------
					#endregion
					#region " チップ種別.Tom3_Rim "
					//----------------
					[ チップ種別.Tom3_Rim ] = new Column() {
						チップ種別 = チップ種別.Tom3_Rim,
						レーン種別 = レーン種別.Tom3,
						ドラム入力種別 = ドラム入力種別.Tom3_Rim,
						表示レーン種別 = 表示レーン種別.Tom3,
						AutoPlay種別 = AutoPlay種別.Tom3,
						AutoPlayON = new Column.ColumnAutoPlayON() {
							自動ヒット = true,
							自動ヒット時処理 = new Column.Columnヒット処理() {
								再生 = true,
								非表示 = true,
								判定 = true,
							},
							MISS判定 = true,
						},
						AutoPlayOFF = new Column.ColumnAutoPlayOFF() {
							自動ヒット = false,
							自動ヒット時処理 = new Column.Columnヒット処理() {
								再生 = false,
								非表示 = false,
								判定 = false,
							},
							ユーザヒット = true,
							ユーザヒット時処理 = new Column.Columnヒット処理() {
								再生 = false,
								非表示 = true,
								判定 = true,
							},
							MISS判定 = true,
						},
						シンバルフリーの対象 = false,
					},
					//----------------
					#endregion
					#region " チップ種別.RightCrash "
					//----------------
					[ チップ種別.RightCrash ] = new Column() {
						チップ種別 = チップ種別.RightCrash,
						レーン種別 = レーン種別.RightCrash,
						ドラム入力種別 = ドラム入力種別.RightCrash,
						表示レーン種別 = 表示レーン種別.RightCrash,
						AutoPlay種別 = AutoPlay種別.RightCrash,
						AutoPlayON = new Column.ColumnAutoPlayON() {
							自動ヒット = true,
							自動ヒット時処理 = new Column.Columnヒット処理() {
								再生 = true,
								非表示 = true,
								判定 = true,
							},
							MISS判定 = true,
						},
						AutoPlayOFF = new Column.ColumnAutoPlayOFF() {
							自動ヒット = false,
							自動ヒット時処理 = new Column.Columnヒット処理() {
								再生 = false,
								非表示 = false,
								判定 = false,
							},
							ユーザヒット = true,
							ユーザヒット時処理 = new Column.Columnヒット処理() {
								再生 = false,
								非表示 = true,
								判定 = true,
							},
							MISS判定 = true,
						},
						シンバルフリーの対象 = true,
					},
					//----------------
					#endregion
					#region " チップ種別.BPM "
					//----------------
					[ チップ種別.BPM ] = new Column() {
						チップ種別 = チップ種別.BPM,
						レーン種別 = レーン種別.BPM,
						ドラム入力種別 = ドラム入力種別.Unknown,
						表示レーン種別 = 表示レーン種別.Unknown,
						AutoPlay種別 = AutoPlay種別.Unknown,
						AutoPlayON = new Column.ColumnAutoPlayON() {
							自動ヒット = false,
							自動ヒット時処理 = new Column.Columnヒット処理() {
								再生 = false,
								非表示 = false,
								判定 = false,
							},
							MISS判定 = false,
						},
						AutoPlayOFF = new Column.ColumnAutoPlayOFF() {
							自動ヒット = false,
							自動ヒット時処理 = new Column.Columnヒット処理() {
								再生 = false,
								非表示 = false,
								判定 = false,
							},
							ユーザヒット = false,
							ユーザヒット時処理 = new Column.Columnヒット処理() {
								再生 = false,
								非表示 = false,
								判定 = false,
							},
							MISS判定 = false,
						},
						シンバルフリーの対象 = false,
					},
					//----------------
					#endregion
					#region " チップ種別.小節線 "
					//----------------
					[ チップ種別.小節線 ] = new Column() {
						チップ種別 = チップ種別.小節線,
						レーン種別 = レーン種別.Unknown,
						ドラム入力種別 = ドラム入力種別.Unknown,
						表示レーン種別 = 表示レーン種別.Unknown,
						AutoPlay種別 = AutoPlay種別.Unknown,
						AutoPlayON = new Column.ColumnAutoPlayON() {
							自動ヒット = false,
							自動ヒット時処理 = new Column.Columnヒット処理() {
								再生 = false,
								非表示 = false,
								判定 = false,
							},
							MISS判定 = false,
						},
						AutoPlayOFF = new Column.ColumnAutoPlayOFF() {
							自動ヒット = false,
							自動ヒット時処理 = new Column.Columnヒット処理() {
								再生 = false,
								非表示 = false,
								判定 = false,
							},
							ユーザヒット = false,
							ユーザヒット時処理 = new Column.Columnヒット処理() {
								再生 = false,
								非表示 = false,
								判定 = false,
							},
							MISS判定 = false,
						},
						シンバルフリーの対象 = false,
					},
					//----------------
					#endregion
					#region " チップ種別.拍線 "
					//----------------
					[ チップ種別.拍線 ] = new Column() {
						チップ種別 = チップ種別.拍線,
						レーン種別 = レーン種別.Unknown,
						ドラム入力種別 = ドラム入力種別.Unknown,
						表示レーン種別 = 表示レーン種別.Unknown,
						AutoPlay種別 = AutoPlay種別.Unknown,
						AutoPlayON = new Column.ColumnAutoPlayON() {
							自動ヒット = false,
							自動ヒット時処理 = new Column.Columnヒット処理() {
								再生 = false,
								非表示 = false,
								判定 = false,
							},
							MISS判定 = false,
						},
						AutoPlayOFF = new Column.ColumnAutoPlayOFF() {
							自動ヒット = false,
							自動ヒット時処理 = new Column.Columnヒット処理() {
								再生 = false,
								非表示 = false,
								判定 = false,
							},
							ユーザヒット = false,
							ユーザヒット時処理 = new Column.Columnヒット処理() {
								再生 = false,
								非表示 = false,
								判定 = false,
							},
							MISS判定 = false,
						},
						シンバルフリーの対象 = false,
					},
					//----------------
					#endregion
					#region " チップ種別.背景動画 "
					//----------------
					[ チップ種別.背景動画 ] = new Column() {
						チップ種別 = チップ種別.背景動画,
						レーン種別 = レーン種別.Song,
						ドラム入力種別 = ドラム入力種別.Unknown,
						表示レーン種別 = 表示レーン種別.Unknown,
						AutoPlay種別 = AutoPlay種別.Unknown,
						AutoPlayON = new Column.ColumnAutoPlayON() {
							自動ヒット = true,
							自動ヒット時処理 = new Column.Columnヒット処理() {
								再生 = true,
								非表示 = true,
								判定 = false,
							},
							MISS判定 = false,
						},
						AutoPlayOFF = new Column.ColumnAutoPlayOFF() {
							自動ヒット = true,
							自動ヒット時処理 = new Column.Columnヒット処理() {
								再生 = true,
								非表示 = true,
								判定 = false,
							},
							ユーザヒット = false,
							ユーザヒット時処理 = new Column.Columnヒット処理() {
								再生 = false,
								非表示 = false,
								判定 = false,
							},
							MISS判定 = false,
						},
						シンバルフリーの対象 = false,
					},
					//----------------
					#endregion
					#region " チップ種別.小節メモ"
					//----------------
					[ チップ種別.小節メモ ] = new Column() {
						チップ種別 = チップ種別.小節メモ,
						レーン種別 = レーン種別.Unknown,
						ドラム入力種別 = ドラム入力種別.Unknown,
						表示レーン種別 = 表示レーン種別.Unknown,
						AutoPlay種別 = AutoPlay種別.Unknown,
						AutoPlayON = new Column.ColumnAutoPlayON() {
							自動ヒット = false,
							自動ヒット時処理 = new Column.Columnヒット処理() {
								再生 = false,
								非表示 = false,
								判定 = false,
							},
							MISS判定 = false,
						},
						AutoPlayOFF = new Column.ColumnAutoPlayOFF() {
							自動ヒット = false,
							自動ヒット時処理 = new Column.Columnヒット処理() {
								再生 = false,
								非表示 = false,
								判定 = false,
							},
							ユーザヒット = false,
							ユーザヒット時処理 = new Column.Columnヒット処理() {
								再生 = false,
								非表示 = false,
								判定 = false,
							},
							MISS判定 = false,
						},
						シンバルフリーの対象 = false,
					},
					//----------------
					#endregion
					#region " チップ種別.LeftCymbal_Mute "
					//----------------
					[ チップ種別.LeftCymbal_Mute ] = new Column() {
						チップ種別 = チップ種別.LeftCymbal_Mute,
						レーン種別 = レーン種別.LeftCrash,
						ドラム入力種別 = ドラム入力種別.Unknown,
						表示レーン種別 = 表示レーン種別.LeftCrash,
						AutoPlay種別 = AutoPlay種別.LeftCrash,
						AutoPlayON = new Column.ColumnAutoPlayON() {
							自動ヒット = true,
							自動ヒット時処理 = new Column.Columnヒット処理() {
								再生 = false,
								非表示 = true,
								判定 = false,
							},
							MISS判定 = false,
						},
						AutoPlayOFF = new Column.ColumnAutoPlayOFF() {
							自動ヒット = true,
							自動ヒット時処理 = new Column.Columnヒット処理() {
								再生 = false,
								非表示 = true,
								判定 = false,
							},
							ユーザヒット = false,
							ユーザヒット時処理 = new Column.Columnヒット処理() {
								再生 = false,
								非表示 = false,
								判定 = false,
							},
							MISS判定 = false,
						},
						シンバルフリーの対象 = false,
					},
					//----------------
					#endregion
					#region " チップ種別.RightCymbal_Mute "
					//----------------
					[ チップ種別.RightCymbal_Mute ] = new Column() {
						チップ種別 = チップ種別.RightCymbal_Mute,
						レーン種別 = レーン種別.RightCrash,
						ドラム入力種別 = ドラム入力種別.Unknown,
						表示レーン種別 = 表示レーン種別.RightCrash,
						AutoPlay種別 = AutoPlay種別.RightCrash,
						AutoPlayON = new Column.ColumnAutoPlayON() {
							自動ヒット = true,
							自動ヒット時処理 = new Column.Columnヒット処理() {
								再生 = false,
								非表示 = true,
								判定 = false,
							},
							MISS判定 = false,
						},
						AutoPlayOFF = new Column.ColumnAutoPlayOFF() {
							自動ヒット = true,
							自動ヒット時処理 = new Column.Columnヒット処理() {
								再生 = false,
								非表示 = true,
								判定 = false,
							},
							ユーザヒット = false,
							ユーザヒット時処理 = new Column.Columnヒット処理() {
								再生 = false,
								非表示 = false,
								判定 = false,
							},
							MISS判定 = false,
						},
						シンバルフリーの対象 = false,
					},
					//----------------
					#endregion
					#region " チップ種別.小節の先頭 "
					//----------------
					[ チップ種別.小節の先頭 ] = new Column() {
						チップ種別 = チップ種別.Unknown,
						レーン種別 = レーン種別.Unknown,
						ドラム入力種別 = ドラム入力種別.Unknown,
						表示レーン種別 = 表示レーン種別.Unknown,
						AutoPlay種別 = AutoPlay種別.Unknown,
						AutoPlayON = new Column.ColumnAutoPlayON() {
							自動ヒット = false,
							自動ヒット時処理 = new Column.Columnヒット処理() {
								再生 = false,
								非表示 = false,
								判定 = false,
							},
							MISS判定 = false,
						},
						AutoPlayOFF = new Column.ColumnAutoPlayOFF() {
							自動ヒット = false,
							自動ヒット時処理 = new Column.Columnヒット処理() {
								再生 = false,
								非表示 = false,
								判定 = false,
							},
							ユーザヒット = false,
							ユーザヒット時処理 = new Column.Columnヒット処理() {
								再生 = false,
								非表示 = false,
								判定 = false,
							},
							MISS判定 = false,
						},
						シンバルフリーの対象 = false,
					},
					//----------------
					#endregion
				};
			}
		}
	}
}
