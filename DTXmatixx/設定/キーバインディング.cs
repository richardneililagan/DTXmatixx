using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using SharpDX.DirectInput;
using FDK;
using DTXmatixx.入力;

namespace DTXmatixx.設定
{
	[DataContract( Name = "KeyBindings", Namespace = "" )]
	class キーバインディング : IExtensibleDataObject
	{
		/// <summary>
		///		入力コードのマッピング用 Dictionary のキーとなる型。
		/// </summary>
		/// <remarks>
		///		入力は、デバイスID（入力デバイスの内部識別用ID; FDKのIInputEvent.DeviceIDと同じ）と、
		///		キー（キーコード、ノート番号などデバイスから得られる入力値）の組で定義される。
		/// </remarks>
		[DataContract( Name = "IDとキー", Namespace = "" )]
		public struct IdKey
		{
			[DataMember]
			public int deviceId;

			[DataMember]
			public int key;

			public IdKey( int deviceId, int key )
			{
				this.deviceId = deviceId;
				this.key = key;
			}
			public IdKey( FDK.入力.InputEvent ie )
			{
				this.deviceId = ie.DeviceID;
				this.key = ie.Key;
			}
		}

		/// <summary>
		///		MIDI番号(0～7)とMIDIデバイス名のマッピング用 Dictionary。
		/// </summary>
		[DataMember]
		public Dictionary<int, string> MIDIデバイス番号toデバイス名
		{
			get;
			protected set;
		}

		/// <summary>
		///		キーボードの入力（DirectInputのKey値）からドラム入力へのマッピング用 Dictionary 。
		/// </summary>
		[DataMember]
		public Dictionary<IdKey, ドラム入力種別> キーボードtoドラム
		{
			get;
			protected set;
		}

		/// <summary>
		///		MIDI入力の入力（MIDIノート番号）からドラム入力へのマッピング用 Dictionary 。
		/// </summary>
		[DataMember]
		public Dictionary<IdKey, ドラム入力種別> MIDItoドラム
		{
			get;
			protected set;
		}


		/// <summary>
		///		コンストラクタ。
		/// </summary>
		public キーバインディング()
		{
			this.OnDeserializing( new StreamingContext() );
		}


		/// <summary>
		///		コンストラクタまたは逆シリアル化前（復元前）に呼び出される。
		///		ここでは主に、メンバを規定値で初期化する。
		/// </summary>
		/// <param name="sc">未使用。</param>
		[OnDeserializing]
		private void OnDeserializing( StreamingContext sc )
		{
			this.MIDIデバイス番号toデバイス名 = new Dictionary<int, string>();

			this.キーボードtoドラム = new Dictionary<IdKey, ドラム入力種別>() {
				{ new IdKey( 0, (int) Key.Q ), ドラム入力種別.LeftCrash },
				{ new IdKey( 0, (int) Key.Return ), ドラム入力種別.LeftCrash },
				{ new IdKey( 0, (int) Key.A ), ドラム入力種別.HiHat_Open },
				{ new IdKey( 0, (int) Key.Z ), ドラム入力種別.HiHat_Close },
				{ new IdKey( 0, (int) Key.X ), ドラム入力種別.Snare },
				{ new IdKey( 0, (int) Key.C ), ドラム入力種別.Bass },
				{ new IdKey( 0, (int) Key.Space ), ドラム入力種別.Bass },
				{ new IdKey( 0, (int) Key.V ), ドラム入力種別.Tom1 },
				{ new IdKey( 0, (int) Key.B ), ドラム入力種別.Tom2 },
				{ new IdKey( 0, (int) Key.N ), ドラム入力種別.Tom3 },
				{ new IdKey( 0, (int) Key.M ), ドラム入力種別.RightCrash },
				{ new IdKey( 0, (int) Key.K ), ドラム入力種別.Ride },
			};

			this.MIDItoドラム = new Dictionary<IdKey, ドラム入力種別>() {
				// うちの環境(2017.6.11)
				{ new IdKey( 0, 36 ), ドラム入力種別.Bass },
				{ new IdKey( 0, 30 ), ドラム入力種別.RightCrash },
				{ new IdKey( 0, 29 ), ドラム入力種別.RightCrash },
				{ new IdKey( 1, 51 ), ドラム入力種別.RightCrash },
				{ new IdKey( 1, 52 ), ドラム入力種別.RightCrash },
				{ new IdKey( 1, 57 ), ドラム入力種別.RightCrash },
				{ new IdKey( 0, 52 ), ドラム入力種別.RightCrash },
				{ new IdKey( 0, 43 ), ドラム入力種別.Tom3 },
				{ new IdKey( 0, 58 ), ドラム入力種別.Tom3 },
				{ new IdKey( 0, 42 ), ドラム入力種別.HiHat_Close },
				{ new IdKey( 0, 22 ), ドラム入力種別.HiHat_Close },
				{ new IdKey( 0, 26 ), ドラム入力種別.HiHat_Open },
				{ new IdKey( 0, 46 ), ドラム入力種別.HiHat_Open },
				{ new IdKey( 0, 255 ), ドラム入力種別.HiHat_Control },	// FDK の MidiIn クラスは、FootControl を ノート 255 として扱う。
				{ new IdKey( 0, 48 ), ドラム入力種別.Tom1 },
				{ new IdKey( 0, 50 ), ドラム入力種別.Tom1 },
				{ new IdKey( 0, 49 ), ドラム入力種別.LeftCrash },
				{ new IdKey( 0, 55 ), ドラム入力種別.LeftCrash },
				{ new IdKey( 1, 48 ), ドラム入力種別.LeftCrash },
				{ new IdKey( 1, 49 ), ドラム入力種別.LeftCrash },
				{ new IdKey( 1, 59 ), ドラム入力種別.LeftCrash },
				{ new IdKey( 0, 45 ), ドラム入力種別.Tom2 },
				{ new IdKey( 0, 47 ), ドラム入力種別.Tom2 },
				{ new IdKey( 0, 51 ), ドラム入力種別.Ride },
				{ new IdKey( 0, 59 ), ドラム入力種別.Ride },
				{ new IdKey( 0, 38 ), ドラム入力種別.Snare },
				{ new IdKey( 0, 40 ), ドラム入力種別.Snare },
				{ new IdKey( 0, 37 ), ドラム入力種別.Snare },

				// とりあえず、DTXMania からベタ移植。
				//{ new IdKey( 0, 42 ), ドラム入力種別.HiHat_Close },
				//{ new IdKey( 0, 93 ), ドラム入力種別.HiHat_Close },
				//{ new IdKey( 0, 46 ), ドラム入力種別.HiHat_Open },
				//{ new IdKey( 0, 92 ), ドラム入力種別.HiHat_Open },
				//{ new IdKey( 0, 255 ), ドラム入力種別.HiHat_Control },	// FDK の MidiIn クラスは、FootControl を ノート 255 として扱う。
				//{ new IdKey( 0, 25 ), ドラム入力種別.Snare },
				//{ new IdKey( 0, 26 ), ドラム入力種別.Snare },
				//{ new IdKey( 0, 27 ), ドラム入力種別.Snare },
				//{ new IdKey( 0, 28 ), ドラム入力種別.Snare },
				//{ new IdKey( 0, 29 ), ドラム入力種別.Snare },
				//{ new IdKey( 0, 31 ), ドラム入力種別.Snare },
				//{ new IdKey( 0, 32 ), ドラム入力種別.Snare },
				//{ new IdKey( 0, 34 ), ドラム入力種別.Snare },
				//{ new IdKey( 0, 37 ), ドラム入力種別.Snare },
				//{ new IdKey( 0, 38 ), ドラム入力種別.Snare },
				//{ new IdKey( 0, 40 ), ドラム入力種別.Snare },
				//{ new IdKey( 0, 113 ), ドラム入力種別.Snare },
				//{ new IdKey( 0, 33 ), ドラム入力種別.Bass },
				//{ new IdKey( 0, 35 ), ドラム入力種別.Bass },
				//{ new IdKey( 0, 36 ), ドラム入力種別.Bass },
				//{ new IdKey( 0, 112 ), ドラム入力種別.Bass },
				//{ new IdKey( 0, 48 ), ドラム入力種別.Tom1 },
				//{ new IdKey( 0, 50 ), ドラム入力種別.Tom1 },
				//{ new IdKey( 0, 47 ), ドラム入力種別.Tom2 },
				//{ new IdKey( 0, 41 ), ドラム入力種別.Tom3 },
				//{ new IdKey( 0, 43 ), ドラム入力種別.Tom3 },
				//{ new IdKey( 0, 45 ), ドラム入力種別.Tom3 },
				//{ new IdKey( 0, 49 ), ドラム入力種別.RightCrash },
				//{ new IdKey( 0, 52 ), ドラム入力種別.RightCrash },
				//{ new IdKey( 0, 55 ), ドラム入力種別.RightCrash },
				//{ new IdKey( 0, 57 ), ドラム入力種別.RightCrash },
				//{ new IdKey( 0, 91 ), ドラム入力種別.RightCrash },
				//{ new IdKey( 0, 51 ), ドラム入力種別.Ride },
				//{ new IdKey( 0, 53 ), ドラム入力種別.Ride },
				//{ new IdKey( 0, 59 ), ドラム入力種別.Ride },
				//{ new IdKey( 0, 89 ), ドラム入力種別.Ride },
			};
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
