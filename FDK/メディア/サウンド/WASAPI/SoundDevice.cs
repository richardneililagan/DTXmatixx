using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using CSCore;
using CSCore.Codecs;
using CSCore.CoreAudioAPI;
using CSCore.SoundOut;
using CSCore.Win32;

namespace FDK.メディア.サウンド.WASAPI
{
	public class SoundDevice : IDisposable
	{
		public PlaybackState レンダリング状態
			=> this._レンダリング状態;

		public double 再生遅延sec
		{
			get;
			protected set;
		}

		/// <summary>
		///		デバイスのレンダリングフォーマット。
		/// </summary>
		public WaveFormat WaveFormat
			=> this._WaveFormat;

		/// <summary>
		///		レンダリングボリューム。
		///		0.0 (0%) ～ 1.0 (100%) 。
		/// </summary>
		public float 音量
		{
			get
				=> this.Mixer.Volume;

			set
			{
				if( ( 0.0f > value ) || ( 1.0f < value ) )
					throw new ArgumentOutOfRangeException( $"音量の値が、範囲(0～1)を超えています。[{value}]" );

				this.Mixer.Volume = value;
			}
		}

		/// <summary>
		///		ミキサー。
		/// </summary>
		internal Mixer Mixer
		{
			get;
			private set;
		} = null;


		/// <summary>
		///		デバイスを初期化する。
		/// </summary>
		/// <param name="共有モード">true なら共有モード、false なら排他モード。</param>
		public SoundDevice( AudioClientShareMode 共有モード, double バッファサイズsec = 0.010, WaveFormat 希望フォーマット = null )
		{
			using( Log.Block( FDKUtilities.現在のメソッド名 ) )
			{
				this._レンダリング状態 = PlaybackState.Stopped;
				this._共有モード = 共有モード;
				this.再生遅延sec = バッファサイズsec;

				lock( this._スレッド間同期 )
				{
					if( this._レンダリング状態 != PlaybackState.Stopped )
						throw new InvalidOperationException( "WASAPI のレンダリングを停止しないまま初期化することはできません。" );
					if( null != this._レンダリングスレッド )
						throw new Exception( "レンダリングスレッドがすでに起動しています。" );

					this._解放する();

					// MMDevice を取得する。
					this._MMDevice = MMDeviceEnumerator.DefaultAudioEndpoint(
						DataFlow.Render,    // 方向 ... 書き込み
						Role.Console );     // 用途 ... ゲーム、システム通知音、音声命令

					// AudioClient を取得する。
					this._AudioClient = AudioClient.FromMMDevice( this._MMDevice );

					// フォーマットを決定する。
					this._WaveFormat = this._適切なフォーマットを調べて返す( 希望フォーマット ) ??
						throw new NotSupportedException( "サポート可能な WaveFormat が見つかりませんでした。" );

					// AudioClient を初期化する。
					try
					{
						long 期間100ns = ( this._共有モード == AudioClientShareMode.Shared ) ?
							this._AudioClient.DefaultDevicePeriod :						// 共有モードの場合、遅延を既定値に設定する。
							FDKUtilities.変換_sec単位から100ns単位へ( this.再生遅延sec );	// 排他モードの場合、コンストラクタで指定された値。

						this._AudioClientを初期化する( 期間100ns );
					}
					catch( CoreAudioAPIException e )
					{
						// 排他モードかつイベント駆動 の場合、この例外が返されることがある。
						// この場合、バッファサイズを調整して再度初期化する。
						if( e.ErrorCode == AUDCLNT_E_BUFFER_SIZE_NOT_ALIGNED )
						{
							int サイズframe = this._AudioClient.GetBufferSize();   // アライメント済みサイズが取得できる。
							this.再生遅延sec = (double) サイズframe / this._WaveFormat.SampleRate;
							long 期間100ns = FDKUtilities.変換_sec単位から100ns単位へ( this.再生遅延sec );

							this._AudioClientを初期化する( 期間100ns );    // 再度初期化。
						}
						else
						{
							throw;  // それでも例外なら知らん。
						}
					}

					// イベント駆動に使うイベントを生成し、AudioClient へ登録する。
					this._レンダリングイベント = new EventWaitHandle( false, EventResetMode.AutoReset );
					this._AudioClient.SetEventHandle( this._レンダリングイベント.SafeWaitHandle.DangerousGetHandle() );

					// その他のインターフェースを取得する。
					this._AudioRenderClient = AudioRenderClient.FromAudioClient( this._AudioClient );
					this._AudioClock = AudioClock.FromAudioClient( this._AudioClient );

					// ミキサーを生成する。
					this.Mixer = new Mixer( this._WaveFormat );
				}

				this.レンダリングを開始する();

				// 完了。
				var format = ( this.WaveFormat is WaveFormatExtensible wfx ) ?
					$"{wfx.WaveFormatTag}[{AudioSubTypes.EncodingFromSubType( wfx.SubFormat )}], {wfx.SampleRate}Hz, {wfx.Channels}ch, {wfx.BitsPerSample}bits" :
					$"{this.WaveFormat.WaveFormatTag}, {this.WaveFormat.SampleRate}Hz, {this.WaveFormat.Channels}ch, {this.WaveFormat.BitsPerSample}bits";

				Log.Info( $"WASAPIデバイスを初期化しました。({this._共有モード}, {this.再生遅延sec * 1000.0}ms, {format})" );
			}
		}

		/// <summary>
		///		ミキサーの出力を開始する。
		///		以降、ミキサーに Sound を追加すれば自動的に再生され、再生が完了した Sound は自動的にミキサーから削除される。
		/// </summary>
		public void レンダリングを開始する()
		{
			var 現在の状態 = PlaybackState.Stopped;
			lock( this._スレッド間同期 )
				現在の状態 = this._レンダリング状態;

			switch( 現在の状態 )
			{
				case PlaybackState.Paused:
					this.レンダリングを再開する(); // Resume する。
					break;

				case PlaybackState.Stopped:
					using( var 起動完了通知 = new AutoResetEvent( false ) )
					{
						Debug.Assert( ( null == this._レンダリングスレッド ), "レンダリングスレッドがすでに起動しています。" );

						// レンダリングスレッドを起動する。
						this._レンダリングスレッド = new Thread( this._レンダリングスレッドエントリ ) {
							Name = "WASAPI Playback",
							Priority = ThreadPriority.AboveNormal, // 標準よりやや上
						};
						this._レンダリングスレッド.Start( 起動完了通知 );

						// スレッドからの起動完了通知を待つ。
						起動完了通知.WaitOne();
						Log.Info( "WASAPIのレンダリングスレッドを起動しました。" );
					}
					break;
			}
		}

		/// <summary>
		///		ミキサーの出力を停止する。
		///		ミキサーに登録されているすべての Sound の再生が停止する。
		/// </summary>
		public void レンダリングを停止する()
		{
			lock( this._スレッド間同期 )
			{
				if( ( this._レンダリング状態 != PlaybackState.Stopped ) && ( null != this._レンダリングスレッド ) )
				{
					// レンダリングスレッドに終了を通知し、その終了を待つ。
					this._レンダリング状態 = PlaybackState.Stopped;
				}
				else
				{
					Log.WARNING( "WASAPIのレンダリングを停止しようとしましたが、すでに停止しています。" );
					return;
				}
			}

			// lock を外してから Join しないとデッドロックするので注意。
			if( null != this._レンダリングスレッド )
			{
				this._レンダリングスレッド.Join();
				this._レンダリングスレッド = null;
				Log.Info( "WASAPIのレンダリングを停止しました。" );
			}
		}

		/// <summary>
		///		ミキサーの出力を一時停止する。
		///		ミキサーに登録されているすべての Sound の再生が一時停止する。
		/// </summary>
		public void レンダリングを一時停止する()
		{
			lock( this._スレッド間同期 )
			{
				switch( this._レンダリング状態 )
				{
					case PlaybackState.Playing:
						this._レンダリング状態 = PlaybackState.Paused;
						Log.Info( "WASAPIのレンダリングを一時停止しました。" );
						break;

					default:
						Log.WARNING( "WASAPIのレンダリングを一時停止しようとしましたが、すでに一時停止しています。" );
						break;
				}
			}
		}

		/// <summary>
		///		ミキサーの出力を再開する。
		///		一時停止状態にあるときのみ有効。
		/// </summary>
		public void レンダリングを再開する()
		{
			lock( this._スレッド間同期 )
			{
				switch( this._レンダリング状態 )
				{
					case PlaybackState.Paused:
						this._レンダリング状態 = PlaybackState.Playing;
						Log.Info( "WASAPIのレンダリングを再開しました。" );
						break;

					default:
						Log.WARNING( "WASAPIのレンダリングを再開しようとしましたが、すでに再開されています。" );
						break;
				}
			}
		}

		/// <summary>
		///		現在のデバイス位置を返す[秒]。
		/// </summary>
		public double GetDevicePosition()
		{
			lock( this._スレッド間同期 )
			{
				this.GetClock( out long position, out long qpcPosition, out long frequency );

				return ( (double) position / frequency );
			}
		}

		/// <summary>
		///		終了する。
		/// </summary>
		public void Dispose()
		{
			this.レンダリングを停止する();
			this._解放する();
		}


		private volatile PlaybackState _レンダリング状態 = PlaybackState.Stopped;
		private AudioClientShareMode _共有モード;
		private WaveFormat _WaveFormat = null;
		private AudioClock _AudioClock = null;
		private AudioRenderClient _AudioRenderClient = null;
		private AudioClient _AudioClient = null;
		private MMDevice _MMDevice = null;

		private Thread _レンダリングスレッド = null;
		private EventWaitHandle _レンダリングイベント = null;
		private readonly object _スレッド間同期 = new object();

		private void _AudioClientを初期化する( long 期間100ns )
		{
			this._AudioClient.Initialize(
				this._共有モード,
				AudioClientStreamFlags.StreamFlagsEventCallback,    // イベント駆動で固定。
				期間100ns,
				期間100ns,      // イベント駆動の場合、Periodicity は BufferDuration と同じ値でなければならない。
				this._WaveFormat,
				Guid.Empty );
		}

		private void _解放する()
		{
			Debug.Assert( null == this._レンダリングスレッド, "レンダリングスレッドが稼働しています。先に終了してください。" );

			this.Mixer?.Dispose();
			this.Mixer = null;

			FDKUtilities.解放する( ref this._AudioClock );
			FDKUtilities.解放する( ref this._AudioRenderClient );

			if( ( null != this._AudioClient ) && ( this._AudioClient.BasePtr != IntPtr.Zero ) )
			{
				try
				{
					this._AudioClient.StopNative();
					this._AudioClient.Reset();
				}
				catch( CoreAudioAPIException e )
				{
					if( e.ErrorCode != AUDCLNT_E_NOT_INITIALIZED )
						throw;
				}
			}

			FDKUtilities.解放する( ref this._AudioClient );
			FDKUtilities.解放する( ref this._レンダリングイベント );
			FDKUtilities.解放する( ref this._MMDevice );
		}

		/// <summary>
		///		希望したフォーマットをもとに、適切なフォーマットを調べて返す。
		/// </summary>
		/// <param name="waveFormat">希望するフォーマット</param>
		/// <param name="audioClient">AudioClient インスタンス。Initialize 前でも可。</param>
		/// <returns>適切なフォーマット。見つからなかったら null。</returns>
		private WaveFormat _適切なフォーマットを調べて返す( WaveFormat waveFormat )
		{
			Trace.Assert( null != this._AudioClient );

			var 最も近いフォーマット = (WaveFormat) null;
			var 最終的に決定されたフォーマット = (WaveFormat) null;

			if( ( null != waveFormat ) && this._AudioClient.IsFormatSupported( this._共有モード, waveFormat, out 最も近いフォーマット ) )
			{
				// (A) そのまま使える。
				最終的に決定されたフォーマット = waveFormat;
			}
			else if( null != 最も近いフォーマット )
			{
				// (B) AudioClient が推奨フォーマットを返してきたので、それを採択する。
				最終的に決定されたフォーマット = 最も近いフォーマット;
			}
			else
			{
				// (C) AudioClient からの提案がなかったので、共有モードのフォーマットを採択してみる。

				var 共有モードのフォーマット = this._AudioClient.GetMixFormat();

				if( ( null != 共有モードのフォーマット ) && this._AudioClient.IsFormatSupported( this._共有モード, 共有モードのフォーマット ) )
				{
					最終的に決定されたフォーマット = 共有モードのフォーマット;
				}
				else
				{
					// (D) 共有モードのフォーマットも NG である場合は、以下から探す。

					bool found = this._AudioClient.IsFormatSupported( AudioClientShareMode.Exclusive,
						new WaveFormat( 48000, 24, 2, AudioEncoding.Pcm ),
						out WaveFormat closest );

					最終的に決定されたフォーマット = new[] {
						new WaveFormat( 48000, 32, 2, AudioEncoding.IeeeFloat ),
						new WaveFormat( 44100, 32, 2, AudioEncoding.IeeeFloat ),
						/*
						 * 24bit PCM には対応しない。
						 * 
						 * https://msdn.microsoft.com/ja-jp/library/cc371566.aspx
						 * > wFormatTag が WAVE_FORMAT_PCM の場合、wBitsPerSample は 8 または 16 でなければならない。
						 * > wFormatTag が WAVE_FORMAT_EXTENSIBLE の場合、この値は、任意の 8 の倍数を指定できる。
						 * 
						 * また、Realtek HD Audio の場合、IAudioClient.IsSupportedFormat() は 24bit PCM でも true を返してくるが、
						 * 単純に 1sample = 3byte で書き込んでも正常に再生できない。
						 * おそらく 32bit で包む必要があると思われるが、その方法は不明。
						 */
						//new WaveFormat( 48000, 24, 2, AudioEncoding.Pcm ),
						//new WaveFormat( 44100, 24, 2, AudioEncoding.Pcm ),
						new WaveFormat( 48000, 16, 2, AudioEncoding.Pcm ),
						new WaveFormat( 44100, 16, 2, AudioEncoding.Pcm ),
						new WaveFormat( 48000,  8, 2, AudioEncoding.Pcm ),
						new WaveFormat( 44100,  8, 2, AudioEncoding.Pcm ),
						new WaveFormat( 48000, 32, 1, AudioEncoding.IeeeFloat ),
						new WaveFormat( 44100, 32, 1, AudioEncoding.IeeeFloat ),
						//new WaveFormat( 48000, 24, 1, AudioEncoding.Pcm ),
						//new WaveFormat( 44100, 24, 1, AudioEncoding.Pcm ),
						new WaveFormat( 48000, 16, 1, AudioEncoding.Pcm ),
						new WaveFormat( 44100, 16, 1, AudioEncoding.Pcm ),
						new WaveFormat( 48000,  8, 1, AudioEncoding.Pcm ),
						new WaveFormat( 44100,  8, 1, AudioEncoding.Pcm ),
					}
					.FirstOrDefault( ( format ) => ( this._AudioClient.IsFormatSupported( this._共有モード, format ) ) );

					// (E) それでも見つからなかったら null のまま。
				}
			}

			return 最終的に決定されたフォーマット;
		}
		
		/// <summary>
		///		WASAPIイベント駆動スレッドのエントリ。
		/// </summary>
		/// <param name="起動完了通知">
		///		無事に起動できたら、これを Set して（スレッドの生成元に）知らせる。
		///	</param>
		private void _レンダリングスレッドエントリ( object 起動完了通知 )
		{
			var 例外 = (Exception) null;
			var 元のMMCSS特性 = IntPtr.Zero;

			try
			{
				#region " 初期化。"
				//----------------
				int バッファサイズframe = this._AudioClient.BufferSize;
				var バッファ = new float[ バッファサイズframe * this.WaveFormat.Channels ];    // 前提１・this._レンダリング先（ミキサー）の出力は 32bit-float で固定。

				// このスレッドの MMCSS 型を登録する。
				string mmcssType;
				switch( this.再生遅延sec )
				{
					case double 遅延 when( 0.0105 > 遅延 ):
						mmcssType = "Pro Audio";
						break;

					case double 遅延 when( 0.0150 > 遅延 ):
						mmcssType = "Games";
						break;

					default:
						mmcssType = "Audio";
						break;
				}
				元のMMCSS特性 = SoundDevice.AvSetMmThreadCharacteristics( mmcssType, out int taskIndex );

				// AudioClient を開始する。
				this._AudioClient.Start();
				lock( this._スレッド間同期 )
					this._レンダリング状態 = PlaybackState.Playing;

				// 起動完了を通知する。
				( 起動完了通知 as EventWaitHandle )?.Set();
				起動完了通知 = null;
				//----------------
				#endregion

				#region " メインループ。"
				//----------------
				var イベントs = new WaitHandle[] { this._レンダリングイベント };

				while( true )
				{
					// 終了？
					var 現在の状態 = PlaybackState.Playing;
					lock( this._スレッド間同期 )
						現在の状態 = this._レンダリング状態;

					if( 現在の状態 == PlaybackState.Stopped )
						break;	// 終わる。

					// イベントs[] のいずれかのイベントが発火する（かタイムアウトする）まで待つ。
					int イベント番号 = WaitHandle.WaitAny(
						waitHandles: イベントs,
						millisecondsTimeout: (int) ( 3000.0 * this.再生遅延sec ), // 適正値は レイテンシ×3 [ms] (MSDNより)
						exitContext: false );

					// タイムアウトした＝まだどのイベントもきてない。
					if( イベント番号 == WaitHandle.WaitTimeout )
						continue;

					lock( this._スレッド間同期 )
					{
						if( this.レンダリング状態 != PlaybackState.Playing )
							continue;

						int 未再生数frame = ( this._共有モード == AudioClientShareMode.Exclusive ) ? 0 : this._AudioClient.GetCurrentPadding();
						int 空きframe = バッファサイズframe - 未再生数frame;
						if( 5 >= 空きframe )
							continue;   // あまりに空きが小さいなら、何もせずスキップする。

						// レンダリング先からデータを取得して AudioRenderClient へ出力する。

						int 読み込むサイズsample = 空きframe * this.WaveFormat.Channels; // 前提・レンダリング先.WaveFormat と this.WaveFormat は同一。
						読み込むサイズsample -= ( 読み込むサイズsample % ( this.WaveFormat.BlockAlign / this.WaveFormat.BytesPerSample ) );  // BlockAlign 境界にそろえる。
						if( 0 >= 読み込むサイズsample )
							continue;   // サンプルなし。スキップ。

						// ミキサーからの出力（32bit-float）をバッファに取得する。
						int 読み込んだサイズsample = this.Mixer.Read( バッファ, 0, 読み込むサイズsample );

						// バッファのデータをレンダリングフォーマットに変換しつつ、AudioRenderClient へ出力する。
						IntPtr bufferPtr = this._AudioRenderClient.GetBuffer( 空きframe );
						try
						{
							var encoding = AudioSubTypes.EncodingFromSubType( WaveFormatExtensible.SubTypeFromWaveFormat( this.WaveFormat ) );

							switch( encoding )
							{
								case AudioEncoding.IeeeFloat:
									#region " FLOAT32 → FLOAT32 "
									//----------------
									Marshal.Copy( バッファ, 0, bufferPtr, 読み込んだサイズsample );
									//----------------
									#endregion
									break;

								case AudioEncoding.Pcm:
									switch( this.WaveFormat.BitsPerSample )
									{
										case 24:
											#region " FLOAT32 → PCM24 "
											//----------------
											{
												// ※ 以下のコードでは、まだ、まともに再生できない。おそらくザーッという大きいノイズだらけの音になる。
												unsafe
												{
													byte* ptr = (byte*) bufferPtr.ToPointer();  // AudioRenderClient のバッファは GC 対象外なのでピン止め不要。

													for( int i = 0; i < 読み込んだサイズsample; i++ )
													{
														float data = バッファ[ i ];
														if( -1.0f > data ) data = -1.0f;
														if( +1.0f < data ) data = +1.0f;

														uint sample32 = (uint) ( data * 8388608f - 1f );    // 24bit PCM の値域は -8388608～+8388607
														byte* psample32 = (byte*) &sample32;
														*ptr++ = *psample32++;
														*ptr++ = *psample32++;
														*ptr++ = *psample32++;
													}
												}
											}
											//----------------
											#endregion
											break;

										case 16:
											#region " FLOAT32 → PCM16 "
											//----------------
											unsafe
											{
												byte* ptr = (byte*) bufferPtr.ToPointer();  // AudioRenderClient のバッファは GC 対象外なのでピン止め不要。

												for( int i = 0; i < 読み込んだサイズsample; i++ )
												{
													float data = バッファ[ i ];
													if( -1.0f > data ) data = -1.0f;
													if( +1.0f < data ) data = +1.0f;

													short sample16 = (short) ( data * short.MaxValue );
													byte* psample16 = (byte*) &sample16;
													*ptr++ = *psample16++;
													*ptr++ = *psample16++;
												}
											}
											//----------------
											#endregion
											break;

										case 8:
											#region " FLOAT32 → PCM8 "
											//----------------
											unsafe
											{
												byte* ptr = (byte*) bufferPtr.ToPointer();  // AudioRenderClient のバッファは GC 対象外なのでピン止め不要。

												for( int i = 0; i < 読み込んだサイズsample; i++ )
												{
													float data = バッファ[ i ];
													if( -1.0f > data ) data = -1.0f;
													if( +1.0f < data ) data = +1.0f;

													byte value = (byte) ( ( data + 1 ) * 128f );
													*ptr++ = unchecked(value);
												}
											}
											//----------------
											#endregion
											break;
									}
									break;
							}
						}
						finally
						{
							int 出力したフレーム数 = 読み込んだサイズsample / this.WaveFormat.Channels;

							this._AudioRenderClient.ReleaseBuffer(
								出力したフレーム数,
								( 0 < 出力したフレーム数 ) ? AudioClientBufferFlags.None : AudioClientBufferFlags.Silent );
						}

						// レンダリング先からの出力がなくなったらおしまい。
						if( 0 == 読み込んだサイズsample )
							this._レンダリング状態 = PlaybackState.Stopped;
					}
				}
				//----------------
				#endregion

				#region " 終了。"
				//----------------
				// AudioClient を停止する。
				this._AudioClient.Stop();
				this._AudioClient.Reset();

				// ハードウェアの再生が終わるくらいまで、少し待つ。
				Thread.Sleep( (int) ( this.再生遅延sec * 1000 / 2 ) );

				// このスレッドの MMCSS 特性を元に戻す。
				SoundDevice.AvRevertMmThreadCharacteristics( 元のMMCSS特性 );
				元のMMCSS特性 = IntPtr.Zero;
				//----------------
				#endregion
			}
			catch( Exception e )
			{
				Log.ERROR( $"例外が発生しました。レンダリングスレッドを中断します。[{e.Message}]" );
				例外 = e;
			}
			finally
			{
				#region " 完了。"
				//----------------
				if( 元のMMCSS特性 != IntPtr.Zero )
					SoundDevice.AvRevertMmThreadCharacteristics( 元のMMCSS特性 );

				// 失敗時を想定して。
				( 起動完了通知 as EventWaitHandle )?.Set();
				//----------------
				#endregion
			}
		}

		/// <summary>
		///		現在のデバイス位置を取得する。
		/// </summary>
		private void GetClock( out long Pu64Position, out long QPCPosition, out long Pu64Frequency )
		{
			//lock( this._スレッド間同期 )	なくてもいいっぽい。
			{
				this._AudioClock.GetFrequencyNative( out Pu64Frequency );

				int hr = 0;
				long pos = 0;
				long qpcPos = 0;

				for( int リトライ回数 = 0; リトライ回数 < 10; リトライ回数++ )    // 最大10回までリトライ。
				{
					hr = this._AudioClock.GetPositionNative( out pos, out qpcPos );

					// ※IAudioClock::GetPosition() は、S_FALSE を返すことがある。
					// 　これは、WASAPI排他モードにおいて、GetPosition 時に優先度の高いイベントが発生しており
					// 　規定時間内にデバイス位置を取得できなかった場合に返される。(MSDNより)

					if( ( (int) HResult.S_OK ) == hr )
					{
						break;      // OK
					}
					else if( ( (int) HResult.S_FALSE ) == hr )
					{
						continue;   // リトライ
					}
					else
					{
						throw new Win32ComException( hr, "IAudioClock", "GetPosition" );
					}
				}

				Pu64Position = pos;
				QPCPosition = qpcPos;
			}
		}

		#region " Win32 "
		//----------------
		private const int AUDCLNT_E_BUFFER_SIZE_NOT_ALIGNED = unchecked((int) 0x88890019);
		private const int AUDCLNT_E_INVALID_DEVICE_PERIOD = unchecked((int) 0x88890020);
		private const int AUDCLNT_E_NOT_INITIALIZED = unchecked((int) 0x88890001);

		[DllImport( "Avrt.dll", CharSet = CharSet.Unicode )]
		private static extern IntPtr AvSetMmThreadCharacteristics( [MarshalAs( UnmanagedType.LPWStr )] string proAudio, out int taskIndex );

		[DllImport( "Avrt.dll" )]
		private static extern bool AvRevertMmThreadCharacteristics( IntPtr avrtHandle );
		//----------------
		#endregion
	}
}
