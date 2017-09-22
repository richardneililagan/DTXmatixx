using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using FDK;

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
