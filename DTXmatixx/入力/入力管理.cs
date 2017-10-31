using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using FDK;
using FDK.入力;
using DTXmatixx.設定;

namespace DTXmatixx.入力
{
	class 入力管理 : IDisposable
	{
		// 外部依存アクション
		public Func<キーバインディング> キーバインディングを取得する = null;
		public Action キーバインディングを保存する = null;

		public Keyboard Keyboard
		{
			get;
			protected set;
		} = null;
		public MidiIn MidiIn
		{
			get;
			protected set;
		} = null;

		/// <summary>
		///		全デバイスの入力イベントをドラム入力イベントに変換し、それを集めたリスト。
		/// </summary>
		/// <remarks>
		///		<see cref="すべての入力デバイスをポーリングする(bool)"/> の呼び出し時にクリアされ、
		///		その時点における、前回のポーリング以降の入力イベントで再構築される。
		///		ただし、キーとして登録されていない入力イベントは含まれない。
		/// </remarks>
		public List<ドラム入力イベント> ポーリング結果
		{
			get;
		} = new List<ドラム入力イベント>();


		/// <summary>
		///		コンストラクタ。個々の入力デバイスを生成する。
		/// </summary>
		/// <param name="hWindow">ウィンドウハンドル。キーボードのAcquireに使用する。</param>
		///	<param name="最大入力履歴数">１つのシーケンスの最大入力サイズ。</param>
		public 入力管理( IntPtr hWindow, int 最大入力履歴数 = 32 )
		{
			this._hWindow = hWindow;

			Trace.Assert( 0 < 最大入力履歴数 );
			this._最大入力履歴数 = 最大入力履歴数;
		}

		/// <summary>
		///		初期化する。
		///		コンストラクタの実行時点では外部依存アクションが設定されていないので、初期化にはこのメソッドを呼び出すこと。
		/// </summary>
		public void Initialize()
		{
			this.Keyboard = new Keyboard( this._hWindow );
			this.MidiIn = new MidiIn();
			this.ポーリング結果.Clear();
			this._入力履歴 = new List<ドラム入力イベント>( this._最大入力履歴数 );

			// MIDI入力デバイスの可変IDへの対応を行う。
			if( 0 < this.MidiIn.DeviceName.Count )
			{
				var デバイスリスト = new List<string>();
				var キーバインディング = this.キーバインディングを取得する();

				#region " (1) 先に列挙された実際のデバイスに合わせて、デバイスリストを作成する。"
				//----------------
				foreach( var 列挙されたデバイス名 in this.MidiIn.DeviceName )
					デバイスリスト.Add( 列挙されたデバイス名 );
				//----------------
				#endregion

				#region " (2) キーバインディングのデバイスリストとマージして、新しいデバイスリストを作成する。"
				//----------------
				foreach( var kvp in キーバインディング.MIDIデバイス番号toデバイス名 )
				{
					var キーバインディング側のデバイス名 = kvp.Value;

					if( デバイスリスト.Contains( キーバインディング側のデバイス名 ) )
					{
						// (A) 今回も存在しているデバイスなら、何もしない。
					}
					else
					{
						// (B) 今回は存在していないデバイスなら、末尾（＝未使用ID）に登録する。
						デバイスリスト.Add( キーバインディング側のデバイス名 );
					}
				}
				//----------------
				#endregion

				#region " (3) キーバインディングのデバイスから新しいデバイスへ、キーのIDを付け直す。"
				//----------------
				var 中間バッファ = new Dictionary<キーバインディング.IdKey, ドラム入力種別>();

				foreach( var kvp in キーバインディング.MIDItoドラム )
				{
					var デバイスID = kvp.Key.deviceId;

					if( キーバインディング.MIDIデバイス番号toデバイス名.ContainsKey( デバイスID ) )
					{
						var デバイス名 = キーバインディング.MIDIデバイス番号toデバイス名[ デバイスID ];
						デバイスID = デバイスリスト.IndexOf( デバイス名 );  // 必ず存在する。
					}

					中間バッファ.Add( new キーバインディング.IdKey( デバイスID, kvp.Key.key ), kvp.Value );    // デバイスID以外は変更なし。
				}

				キーバインディング.MIDItoドラム.Clear();

				for( int i = 0; i < 中間バッファ.Count; i++ )
				{
					var kvp = 中間バッファ.ElementAt( i );
					キーバインディング.MIDItoドラム.Add( new キーバインディング.IdKey( kvp.Key.deviceId, kvp.Key.key ), kvp.Value );
				}
				//----------------
				#endregion

				#region " (4) 新しいデバイスリストをキーバインディングに格納して、保存する。"
				//----------------
				キーバインディング.MIDIデバイス番号toデバイス名.Clear();

				for( int i = 0; i < デバイスリスト.Count; i++ )
					キーバインディング.MIDIデバイス番号toデバイス名.Add( i, デバイスリスト[ i ] );

				this.キーバインディングを保存する();
				//----------------
				#endregion
			}
			else
			{
				// 列挙されたMIDI入力デバイスがまったくないなら、キーバインディングは何もいじらない。
			}
		}

		/// <summary>
		///		個々のデバイスを解放する。
		/// </summary>
		public void Dispose()
		{
			using( Log.Block( FDKUtilities.現在のメソッド名 ) )
			{
				this.キーバインディングを取得する = null;
				this.キーバインディングを保存する = null;

				this.MidiIn?.Dispose();
				this.MidiIn = null;

				this.Keyboard?.Dispose();
				this.Keyboard = null;
			}
		}

		/// <summary>
		///		すべての入力デバイスをポーリングし、<see cref="ポーリング結果"/>のクリア＆再構築と、<see cref="_入力履歴"/>の更新を行う。
		/// </summary>
		/// <param name="入力履歴を記録する">履歴に残す必要がないとき（演奏時の入力イベントなど）には false を指定する。</param>
		public void すべての入力デバイスをポーリングする( bool 入力履歴を記録する = true )
		{
			// 入力履歴が OFF から ON に変わった場合には、入力履歴を全クリアする。
			if( 入力履歴を記録する && this._入力履歴の記録を中断している )
			{
				this._入力履歴.Clear();
				this._前回の入力履歴の追加時刻sec = null;
			}
			this._入力履歴を記録中である = 入力履歴を記録する;

			// 個別にポーリングする。
			// hack: 追加の入力デバイスクラスを実装したら、ここにコードを追加すること。
			this.ポーリング結果.Clear();

			var キーバインディング = this.キーバインディングを取得する();

			// キーボード
			this._入力デバイスをポーリングする(
				this.Keyboard,
				キーバインディング.キーボードtoドラム,
				入力履歴を記録する );

			// MIDI入力
			this._入力デバイスをポーリングする(
				this.MidiIn,
				キーバインディング.MIDItoドラム,
				入力履歴を記録する );

			// タイムスタンプの小さい順にソートする。
			this.ポーリング結果.Sort( ( x, y ) => (int) ( x.InputEvent.TimeStamp - y.InputEvent.TimeStamp ) );
		}

		/// <summary>
		///		現在の<see cref="ポーリング結果"/>に、指定したドラム入力イベントが含まれているかを確認する。
		/// </summary>
		/// <param name="イベント">調べるドラム入力イベント。</param>
		/// <returns><see cref="ポーリング結果"/>に含まれていれば true。</returns>
		public bool ドラムが入力された( ドラム入力種別 drumType )
		{
			if( 0 == this.ポーリング結果.Count )   // 0 であることが大半だと思われるので、特別扱い。
			{
				return false;
			}
			else
			{
				return ( null != this.ポーリング結果.FirstOrDefault( ( ev ) => ( ev.Type == drumType ) ) );
			}
		}

		/// <summary>
		///		現在の<see cref="ポーリング結果"/>に、指定したドラム入力イベント集合のいずれか１つ以上が含まれているかを確認する。
		/// </summary>
		/// <param name="drumTypes">調べるドラム入力イベントの集合。</param>
		/// <returns><see cref="ポーリング結果"/>に、指定したドラム入力イベントのいずれか１つ以上が含まれていれば true。</returns>
		public bool ドラムのいずれか１つが入力された( IEnumerable<ドラム入力種別> drumTypes )
		{
			if( 0 == this.ポーリング結果.Count )   // 0 であることが大半だと思われるので、特別扱い。
			{
				return false;
			}
			else
			{
				return ( null != this.ポーリング結果.FirstOrDefault( ( ev ) => ( drumTypes.Contains( ev.Type ) ) ) );
			}
		}

		/// <summary>
		///		現在の<see cref="ポーリング結果"/>に、シンバルとみなせるドラム入力イベントが含まれているかを確認する。
		/// </summary>
		/// <returns><see cref="ポーリング結果"/>に含まれていれば true。</returns>
		public bool シンバルが入力された()
		{
			return this.ドラムのいずれか１つが入力された(
				new[] {
					ドラム入力種別.LeftCrash,
					ドラム入力種別.RightCrash,
					ドラム入力種別.China,
					ドラム入力種別.Ride,
					ドラム入力種別.Splash,
				} );
		}

		/// <summary>
		///		現在の履歴において、指定したシーケンスが成立しているかを確認する。
		/// </summary>
		/// <param name="シーケンス">	確認したいシーケンス。</param>
		/// <returns>シーケンスが成立しているなら true。</returns>
		/// <remarks>
		///		指定したシーケンスが現在の履歴の一部に見られれば、成立しているとみなす。
		///		履歴内に複数存在している場合は、一番 古 い シーケンスが対象となる。
		///		成立した場合、そのシーケンスと、それより古い履歴はすべて削除される。
		/// </remarks>
		public bool シーケンスが入力された( IEnumerable<ドラム入力イベント> シーケンス )
		{
			int シーケンスのストローク数 = シーケンス.Count();       // ストローク ＝ ドラム入力イベント（シーケンスの構成単位）

			if( 0 == シーケンスのストローク数 )
				return false;   // 空シーケンスは常に不成立。

			if( this._入力履歴.Count < シーケンスのストローク数 )
				return false;   // 履歴数が足りない。

			int 履歴の検索開始位置 = this._入力履歴.IndexOf( シーケンス.ElementAt( 0 ) );

			if( -1 == 履歴の検索開始位置 )
				return false;   // 最初のストロークが見つからない。

			if( ( this._入力履歴.Count - 履歴の検索開始位置 ) < シーケンスのストローク数 )
				return false;   // 履歴数が足りない。

			// 検索開始位置から末尾へ、すべてのストロークが一致するか確認する。
			for( int i = 1; i < シーケンスのストローク数; i++ )
			{
				if( this._入力履歴[ 履歴の検索開始位置 + i ] != シーケンス.ElementAt( i ) )
					return false;   // 一致しなかった。
			}

			// 見つけたシーケンスならびにそれより古い履歴を削除する。
			this._入力履歴.RemoveRange( 0, ( 履歴の検索開始位置 + シーケンスのストローク数 ) );

			return true;
		}


		/// <summary>
		///		これまでにポーリングで取得された入力の履歴。
		/// </summary>
		/// <remarks>
		///		キーバインディングに従ってマッピングされた後の、ドラム入力イベントを対象とする。
		///		リストのサイズには制限があり（<see cref="_最大入力履歴数"/>）、それを超える場合は、ポーリング時に古いイベントから削除されていく。
		/// </remarks>
		private List<ドラム入力イベント> _入力履歴 = null;

		private int _最大入力履歴数 = 32;
		private IntPtr _hWindow;

		private bool _入力履歴を記録中である
		{
			get;
			set;
		} = true;
		private bool _入力履歴の記録を中断している
		{
			get
				=> !this._入力履歴を記録中である;
			set
				=> this._入力履歴を記録中である = !( value );
		}

		/// <summary>
		///		null なら、入力履歴に追加された入力がまだないことを示す。
		/// </summary>
		private double? _前回の入力履歴の追加時刻sec = null;

		/// <summary>
		///		単一の IInputDevice をポーリングし、対応表に従ってドラム入力へ変換して、ポーリング結果 に追加登録する。
		/// </summary>
		/// <param name="入力デバイス">ポーリングを行う入力デバイス。</param>
		/// <param name="デバイスtoドラム対応表">ドラム入力イベントへ変換するためのマッピング。</param>
		/// <param name="入力履歴を記録する">ドラム入力イベントを入力履歴に登録するなら true。</param>
		private void _入力デバイスをポーリングする( IInputDevice 入力デバイス, Dictionary<キーバインディング.IdKey, ドラム入力種別> デバイスtoドラム対応表, bool 入力履歴を記録する )
		{
			入力デバイス.ポーリングする();

			// ポーリングされた入力イベントのうち、キーバインディングに登録されているイベントだけを ポーリング結果 に追加する。
			foreach( var ev in 入力デバイス.入力イベントリスト )
			{
				// キーバインディングを使って、入力イベント ev をドラム入力 evKey にマッピングする。
				var evKey = new キーバインディング.IdKey( ev );

				if( false == デバイスtoドラム対応表.ContainsKey( evKey ) )
					continue;   // 使われないならスキップ。

				var ドラム入力 = new ドラム入力イベント( ev, デバイスtoドラム対応表[ evKey ] );

				// ドラム入力を、ポーリング結果に追加登録する。
				this.ポーリング結果.Add( ドラム入力 );

				// ドラム入力を入力履歴に追加登録する。
				if( 入力履歴を記録する &&
					ev.押された &&
					ドラム入力.Type != ドラム入力種別.HiHat_Control )   // HHC は入力履歴の対象外とする。
				{
					const double _連続入力だとみなす最大の間隔sec = 0.5;

					double 入力時刻sec = FDK.カウンタ.QPCTimer.生カウント相対値を秒へ変換して返す( ev.TimeStamp );   // 相対か絶対か、どっちかに統一さえされていればいい。

					// 容量がいっぱいなら、古い履歴から削除する。
					if( this._入力履歴.Count >= this._最大入力履歴数 )
						this._入力履歴.RemoveRange( 0, ( this._入力履歴.Count - this._最大入力履歴数 + 1 ) );

					// 前回の追加登録時刻からの経過時間がオーバーしているなら、履歴をすべて破棄する。
					if( null != this._前回の入力履歴の追加時刻sec )
					{
						var 前回の登録からの経過時間sec = 入力時刻sec - this._前回の入力履歴の追加時刻sec;

						if( _連続入力だとみなす最大の間隔sec < 前回の登録からの経過時間sec )
							this._入力履歴.Clear();
					}

					// 今回の入力を履歴に登録する。
					this._入力履歴.Add( ドラム入力 );
					this._前回の入力履歴の追加時刻sec = 入力時刻sec;
				}
			}
		}
	}
}
