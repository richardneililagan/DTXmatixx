using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using SSTFormat.v3;
using DTXmatixx.曲;
using DTXmatixx.ステージ.演奏;
using FDK;

namespace DTXmatixx.設定
{
	/// <summary>
	///		システム設定。
	///		全ユーザで共有される項目。
	/// </summary>
	/// <remarks>
	///		ユーザ別の項目は<see cref="ユーザ設定"/>で管理すること。
	/// </remarks>
	[DataContract( Name = "Configuration", Namespace = "" )]
	[KnownType( typeof( キーバインディング ) )]
	class システム設定 : IExtensibleDataObject
	{
		/// <remarks>
		///		キーバインディングは全ユーザで共通。
		/// </remarks>
		[DataMember( Name = "KeyBindings" )]
		public キーバインディング キーバインディング
		{
			get;
			protected set;
		} = null;


		public システム設定()
		{
			this.OnDeserializing( new StreamingContext() );
		}

		public static システム設定 復元する()
		{
			return FDKUtilities.復元または新規作成する<システム設定>( _ファイルパス, UseSimpleDictionaryFormat: false );
		}
		public void 保存する()
		{
			FDKUtilities.保存する( this, _ファイルパス, UseSimpleDictionaryFormat: false );
		}

		private static readonly string _ファイルパス = @"$(AppData)Configuration.json";


		/// <summary>
		///		コンストラクタまたは逆シリアル化前（復元前）に呼び出される。
		///		ここでは主に、メンバを規定値で初期化する。
		/// </summary>
		/// <param name="sc">未使用。</param>
		[OnDeserializing]
		private void OnDeserializing( StreamingContext sc )
		{
			this.キーバインディング = new キーバインディング();
		}

		/// <summary>
		///		逆シリアル化後（復元後）に呼び出される。
		///		DataMemver を使って他の 非DataMember を初期化する、などの処理を行う。
		/// </summary>
		/// <param name="sc">未使用。</param>
		[OnDeserialized]
		private void OnDeserialized( StreamingContext sc )
		{
		}

		/// <summary>
		///		シリアル化前に呼び出される。
		///		非DataMemer を使って保存用の DataMember を初期化する、などの処理を行う。
		/// </summary>
		/// <param name="sc">未使用。</param>
		[OnSerializing]
		private void OnSerializing( StreamingContext sc )
		{
		}

		/// <summary>
		///		シリアル化後に呼び出される。
		///		ログの出力などの処理を行う。
		/// </summary>
		/// <param name="sc">未使用。</param>
		[OnSerialized]
		private void OnSerialized( StreamingContext sc )
		{
		}

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
