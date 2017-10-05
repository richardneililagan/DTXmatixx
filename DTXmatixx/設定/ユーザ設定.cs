using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using FDK;
using DTXmatixx.設定.DB;
using DTXmatixx.ステージ.演奏;

namespace DTXmatixx.設定
{
	/// <summary>
	///		<see cref="User"/> クラスを使いやすくするためのクラス。
	/// </summary>
	class ユーザ設定
	{
		public string ID
		{
			get;
			protected set;
		} = null;
		public string ユーザ名
		{
			get;
			set;
		} = null;

		/// <summary>
		///		AutoPlay 設定。
		///		true なら AutoPlay ON。 
		/// </summary>
		public Dictionary<AutoPlay種別, bool> AutoPlay { get; set; }

		public bool AutoPlayがすべてONである
		{
			get
			{
				bool すべてON = true;

				foreach( var kvp in this.AutoPlay )
					すべてON &= kvp.Value;

				return すべてON;
			}
		}

		/// <summary>
		///		チップがヒット判定バーから（上または下に）どれだけ離れていると Perfect ～ Ok 判定になるのかの定義。秒単位。
		/// </summary>
		public Dictionary<判定種別, double> 最大ヒット距離sec { get; set; }

		/// <summary>
		///		演奏画面での譜面スクロール速度の倍率。1.0 で等倍。
		/// </summary>
		public double 譜面スクロール速度の倍率 { get; set; }

		/// <summary>
		///		初期の表示モード。
		///		true なら全画面モードで、false ならウィンドウモード。
		/// </summary>
		public bool 全画面モードである { get; set; }

		public ドラムとチップと入力の対応表 ドラムとチップと入力の対応表
		{
			get;
			protected set;
		} = null;

		/// <summary>
		///		<see cref="User"/> インスタンスから生成する。
		/// </summary>
		public ユーザ設定( User user )
		{
			this.ID = user.Id;
			this.ユーザ名 = user.Name;
			this.AutoPlay = new Dictionary<AutoPlay種別, bool>() {
				{ AutoPlay種別.LeftCrash, ( 0 != user.AutoPlayLeftCymbal ) },
				{ AutoPlay種別.HiHat, ( 0 != user.AutoPlayHiHat ) },
				{ AutoPlay種別.Foot, ( 0 != user.AutoPlayLeftPedal ) },
				{ AutoPlay種別.Snare, ( 0 != user.AutoPlaySnare ) },
				{ AutoPlay種別.Bass, ( 0 != user.AutoPlayBass ) },
				{ AutoPlay種別.Tom1, ( 0 != user.AutoPlayHighTom ) },
				{ AutoPlay種別.Tom2, ( 0 != user.AutoPlayLowTom ) },
				{ AutoPlay種別.Tom3, ( 0 != user.AutoPlayFloorTom ) },
				{ AutoPlay種別.RightCrash, ( 0 != user.AutoPlayRightCymbal ) },
			};
			this.最大ヒット距離sec = new Dictionary<判定種別, double>() {
				{ 判定種別.PERFECT, user.MaxRangePerfect },
				{ 判定種別.GREAT, user.MaxRangeGreat },
				{ 判定種別.GOOD, user.MaxRangeGood },
				{ 判定種別.OK, user.MaxRangeOk },
			};
			this.譜面スクロール速度の倍率 = Math.Max( user.ScrollSpeed, 0.0 );
			this.全画面モードである = ( 0 != user.Fullscreen );
			this.ドラムとチップと入力の対応表 = new ドラムとチップと入力の対応表(
				new 表示レーンの左右() {
					Chinaは左 = false,    // 使わないので固定。
					Rideは左 = false,
					Splashは左 = true,
				} );
		}
	}
}
