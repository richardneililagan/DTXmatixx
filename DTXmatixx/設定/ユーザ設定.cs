using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Data.Linq.Mapping;
using System.Data.SQLite;
using System.Diagnostics;
using System.Linq;
using FDK;
using DTXmatixx.ステージ.演奏;

namespace DTXmatixx.設定
{
	class ユーザ設定 : DB.User
	{
		public bool 全画面モードである
		{
			get
				=> ( 0 != this.Fullscreen );
			set
				=> this.Fullscreen = value ? 1 : 0;
		}

		/// <summary>
		///		AutoPlay 設定。
		///		true なら AutoPlay ON。 
		/// </summary>
		public HookedDictionary<AutoPlay種別,bool> AutoPlay
		{
			get;
			protected set;
		} = null;

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

		public ドラムとチップと入力の対応表 ドラムとチップと入力の対応表
		{
			get;
			protected set;
		} = null;


		public ユーザ設定()
			: base()
		{
			this.AutoPlay = new HookedDictionary<AutoPlay種別, bool>() {

				get時アクション = null,

				// Dictionary が変更されたらDB用の個別プロパティも変更する。
				set時アクション = ( type, flag ) => {
					switch( type )
					{
						case AutoPlay種別.LeftCrash:
							this.AutoPlayLeftCymbal = flag ? 1 : 0;
							break;

						case AutoPlay種別.HiHat:
							this.AutoPlayHiHat = flag ? 1 : 0;
							break;

						case AutoPlay種別.Foot:
							this.AutoPlayLeftPedal = flag ? 1 : 0;
							break;

						case AutoPlay種別.Snare:
							this.AutoPlaySnare = flag ? 1 : 0;
							break;

						case AutoPlay種別.Bass:
							this.AutoPlayBass = flag ? 1 : 0;
							break;

						case AutoPlay種別.Tom1:
							this.AutoPlayHighTom = flag ? 1 : 0;
							break;

						case AutoPlay種別.Tom2:
							this.AutoPlayLowTom = flag ? 1 : 0;
							break;

						case AutoPlay種別.Tom3:
							this.AutoPlayFloorTom = flag ? 1 : 0;
							break;

						case AutoPlay種別.RightCrash:
							this.AutoPlayRightCymbal = flag ? 1 : 0;
							break;
					}
				}
			};

			this.ドラムとチップと入力の対応表 = new ドラムとチップと入力の対応表(
				new 表示レーンの左右() {    // 使わないので固定。
					Chinaは左 = false,
					Rideは左 = false,
					Splashは左 = true,
				} );
		}

		/// <summary>
		///		User 情報を使って初期化する。
		/// </summary>
		public ユーザ設定( DB.User user )
			: this()
		{
			this.CopyFrom( user );
		}

		/// <summary>
		///		ユーザ名で User テーブルを検索し、得られた User 情報を使って初期化する。
		/// </summary>
		public ユーザ設定( string ユーザ名 )
			: this()
		{
			using( var userdb = new DB.UserDB() )
			{
				var user = userdb.Users.Where(
					( u ) => ( u.Name == "AutoPlayer" )
					).SingleOrDefault();

				if( null != user )
				{
					this.CopyFrom( user );
				}
			}
		}


		private void CopyFrom( DB.User user )
		{
			this.Id = user.Id;
			this.Name = user.Name;

			this.AutoPlayLeftCymbal = user.AutoPlayLeftCymbal;
			this.AutoPlayHiHat = user.AutoPlayHiHat;
			this.AutoPlayLeftPedal = user.AutoPlayLeftPedal;
			this.AutoPlaySnare = user.AutoPlaySnare;
			this.AutoPlayBass = user.AutoPlayBass;
			this.AutoPlayHighTom = user.AutoPlayHighTom;
			this.AutoPlayLowTom = user.AutoPlayLowTom;
			this.AutoPlayFloorTom = user.AutoPlayFloorTom;
			this.AutoPlayRightCymbal = user.AutoPlayRightCymbal;

			this.AutoPlay = new HookedDictionary<AutoPlay種別, bool>() {
				{ AutoPlay種別.Unknown, true },
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

			this.MaxRangePerfect = user.MaxRangePerfect;
			this.MaxRangeGreat = user.MaxRangeGreat;
			this.MaxRangeGood = user.MaxRangeGood;
			this.MaxRangeOk = user.MaxRangeOk;

			this.最大ヒット距離sec = new Dictionary<判定種別, double>() {
				{ 判定種別.PERFECT, user.MaxRangePerfect },
				{ 判定種別.GREAT, user.MaxRangeGreat },
				{ 判定種別.GOOD, user.MaxRangeGood },
				{ 判定種別.OK, user.MaxRangeOk },
			};

			this.ScrollSpeed = Math.Max( user.ScrollSpeed, 0.0 );
			this.Fullscreen = user.Fullscreen;
		}
	}
}
