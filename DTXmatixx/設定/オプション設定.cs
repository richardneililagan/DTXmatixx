using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using FDK;
using DTXmatixx.ステージ.演奏;

namespace DTXmatixx.設定
{
	[DataContract( Name = "Options", Namespace = "" )]
	[KnownType( typeof( AutoPlay種別 ) )]
	class オプション設定 : IExtensibleDataObject
	{
		/// <summary>
		///		AutoPlay 設定。
		///		true なら AutoPlay ON。 
		/// </summary>
		[DataMember]
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
		[DataMember]
		public Dictionary<判定種別, double> 最大ヒット距離sec { get; set; }
		private Dictionary<判定種別, double> _最大ヒット距離secの既定値 = null;

		/// <summary>
		///		演奏画面での譜面スクロール速度の倍率。1.0 で等倍。
		/// </summary>
		[DataMember]
		public double 譜面スクロール速度の倍率 { get; set; }

		/// <summary>
		///		初期の表示モード。
		///		true なら全画面モードで、false ならウィンドウモード。
		/// </summary>
		[DataMember]
		public bool 全画面モードである { get; set; }

		public ドラムとチップと入力の対応表 ドラムとチップと入力の対応表
		{
			get;
			protected set;
		} = null;


		/// <summary>
		///		コンストラクタ。
		/// </summary>
		public オプション設定()
		{
			using( Log.Block( FDKUtilities.現在のメソッド名 ) )
			{
				this._既定値で初期化する( new StreamingContext() );
			}
		}

		/// <summary>
		///		ファイルに保存する。
		/// </summary>
		/// <param name="ユーザフォルダパス">ユーザフォルダのパス。ユーザ名の部分は、ファイル名やパスで使える文字に調整済みであること。</param>
		public void 保存する( string ユーザフォルダパス )
		{
			using( Log.Block( FDKUtilities.現在のメソッド名 ) )
			{
				if( false == Directory.Exists( ユーザフォルダパス ) )
					Directory.CreateDirectory( ユーザフォルダパス ); // 失敗したら例外発生。

				var path = Path.Combine( ユーザフォルダパス, _Optionファイル名 );
				FDKUtilities.保存する( this, path );
			}
		}

		/// <summary>
		///		ファイルから復元する。
		/// </summary>
		/// <param name="ユーザフォルダパス">ユーザフォルダのパス。ユーザ名の部分は、ファイル名やパスで使える文字に調整済みであること。</param>
		public static オプション設定 復元する( string ユーザフォルダパス )
		{
			using( Log.Block( FDKUtilities.現在のメソッド名 ) )
			{
				var path = Path.Combine( ユーザフォルダパス, _Optionファイル名 );
				var options = FDKUtilities.復元または新規作成する<オプション設定>( path );

				options.ドラムとチップと入力の対応表 = new ドラムとチップと入力の対応表( new 表示レーンの左右() );	// 左右オプションは規定で固定。

				// ファイルに反映されていないメンバはいつまでも反映されないので、ここで一度、明示的に保存することにする。
				options.保存する( ユーザフォルダパス );

				return options;
			}
		}

		/// <summary>
		///		コンストラクタ時、または逆シリアル化時のメンバの既定値を設定する。
		/// </summary>
		/// <param name="sc">未使用。</param>
		/// <remarks>
		///		.NET 既定の初期値だと支障のある（逆シリアル化対象の）メンバがあれば、ここで初期化しておくこと。
		/// </remarks>
		[OnDeserializing]
		private void _既定値で初期化する( StreamingContext sc )
		{
			using( Log.Block( FDKUtilities.現在のメソッド名 ) )
			{
				this.AutoPlay = new Dictionary<AutoPlay種別, bool>();
				foreach( AutoPlay種別 autoPlayType in Enum.GetValues( typeof( AutoPlay種別 ) ) )
					this.AutoPlay[ autoPlayType ] = false;

				// ※メンバ初期化子で設定してはならない。（OnDeserializing 時にはコンストラクタが呼び出されない。）
				this._最大ヒット距離secの既定値 = new Dictionary<判定種別, double>() {
					{ 判定種別.PERFECT, 0.034 },
					{ 判定種別.GREAT, 0.067 },
					{ 判定種別.GOOD, 0.084 },
					{ 判定種別.OK, 0.117 },
					{ 判定種別.MISS, double.NaN },  // 使わない
				};
				this.最大ヒット距離sec = new Dictionary<判定種別, double>();
				foreach( 判定種別 judge in Enum.GetValues( typeof( 判定種別 ) ) )
					this.最大ヒット距離sec[ judge ] = this._最大ヒット距離secの既定値[ judge ];

				this.譜面スクロール速度の倍率 = 1.0;
			}
		}


		/// <summary>
		///		オプション設定を保存するファイルのパス。
		/// </summary>
		private static readonly string _Optionファイル名 = @"Options.json";        // パスなし

		#region " IExtensibleDataObject の実装 "
		//----------------
		private ExtensionDataObject _ExData;

		public virtual ExtensionDataObject ExtensionData
		{
			get
				=> this._ExData;

			set
				=> this._ExData = value;
		}
		//----------------
		#endregion
	}
}
