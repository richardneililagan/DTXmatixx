using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using SharpDX;
using SharpDX.Windows;
using FDK;
using FDK.入力;
using FDK.メディア;
using FDK.同期;
using DTXmatixx.ステージ;
using DTXmatixx.曲;
using DTXmatixx.設定;
using SSTFormat.v3;

namespace DTXmatixx
{
	class App : ApplicationForm, IDisposable
	{
		public static ステージ管理 ステージ管理
		{
			get;
			protected set;
		} = null;
		public static 曲ツリー 曲ツリー
		{
			get;
			protected set;
		} = null;
		public static Keyboard Keyboard
		{
			get;
			protected set;
		} = null;
		public static スコア 演奏スコア
		{
			get;
			set;
		} = null;
		public static FDK.メディア.サウンド.WASAPI.Device サウンドデバイス
		{
			get;
			protected set;
		} = null;
		public static FDK.メディア.サウンド.WASAPI.SoundTimer サウンドタイマ
		{
			get;
			protected set;
		} = null;
		public static システム設定 システム設定
		{
			get;
			protected set;
		} = null;
		public static オプション設定 オプション設定
		{
			get;
			protected set;
		} = null;
		public static ドラムサウンド ドラムサウンド
		{
			get;
			protected set;
		} = null;

		public App()
			: base( 設計画面サイズ: new SizeF( 1920f, 1080f ), 物理画面サイズ: new SizeF( 1920f, 1080f ), 深度ステンシルを使う: false )
		{
			this.Text = $"{Application.ProductName} {Application.ProductVersion}";

			var exePath = Path.GetDirectoryName( Assembly.GetExecutingAssembly().Location );
			Folder.フォルダ変数を追加または更新する( "Exe", $@"{exePath}\" );
			Folder.フォルダ変数を追加または更新する( "System", Path.Combine( exePath, @"System\" ) );
			Folder.フォルダ変数を追加または更新する( "AppData", Path.Combine( Environment.GetFolderPath( Environment.SpecialFolder.ApplicationData, Environment.SpecialFolderOption.Create ), @"DTXMatixx\" ) );

			if( !( Directory.Exists( Folder.フォルダ変数の内容を返す( "AppData" ) ) ) )
				Directory.CreateDirectory( Folder.フォルダ変数の内容を返す( "AppData" ) );	// なければ作成。

			App.Keyboard = new Keyboard( this.Handle );
			App.ステージ管理 = new ステージ管理();
			App.曲ツリー = new 曲ツリー();
			App.サウンドデバイス = new FDK.メディア.サウンド.WASAPI.Device( CSCore.CoreAudioAPI.AudioClientShareMode.Shared );
			App.サウンドタイマ = new FDK.メディア.サウンド.WASAPI.SoundTimer( App.サウンドデバイス );
			App.システム設定 = new システム設定();
			App.オプション設定 = オプション設定.復元する( Folder.フォルダ変数の内容を返す( "AppData" ) );
			App.ドラムサウンド = new ドラムサウンド();

			this._活性化する();

			base.全画面モード = App.オプション設定.全画面モードである;

			// 最初のステージへ遷移する。
			App.ステージ管理.ステージを遷移する( App.グラフィックデバイス, App.ステージ管理.最初のステージ名 );
		}
		public new void Dispose()
		{
			using( Log.Block( FDKUtilities.現在のメソッド名 ) )
			{
				this._非活性化する();

				App.ドラムサウンド?.Dispose();
				App.ドラムサウンド = null;

				App.オプション設定.保存する( Folder.フォルダ変数の内容を返す( "AppData" ) );

				App.サウンドタイマ?.Dispose();
				App.サウンドタイマ = null;

				App.サウンドデバイス?.Dispose();
				App.サウンドデバイス = null;

				App.曲ツリー.Dispose();
				App.曲ツリー = null;

				App.ステージ管理.Dispose( App.グラフィックデバイス );
				App.ステージ管理 = null;

				App.Keyboard.Dispose();
				App.Keyboard = null;

				base.Dispose();
			}
		}
		public override void Run()
		{
			RenderLoop.Run( this, () => {

				switch( this._AppStatus )
				{
					case AppStatus.開始:
						
						// 高速進行タスク起動。
						this._高速進行ステータス = new TriStateEvent( TriStateEvent.状態種別.OFF );
						Task.Factory.StartNew( this._高速進行タスクエントリ );
						
						// 描画タスク起動。
						this._AppStatus = AppStatus.実行中;

						break;

					case AppStatus.実行中:
						this._進行と描画を行う();
						break;

					case AppStatus.終了:
						Thread.Sleep( 500 );    // 終了待機中。
						break;
				}

			} );
		}
		protected override void OnClosing( CancelEventArgs e )
		{
			using( Log.Block( FDKUtilities.現在のメソッド名 ) )
			{
				lock( this._高速進行と描画の同期 )
				{
					// 通常は進行タスクから終了するが、Alt+F4でここに来た場合はそれが行われてないので、行う。
					if( this._AppStatus != AppStatus.終了 )
					{
						this._アプリを終了する();
					}

					base.OnClosing( e );
				}
			}
		}
		protected override void OnKeyDown( KeyEventArgs e )
		{
			if( e.KeyCode == Keys.F11 )
			{
				this.全画面モード = !( this.全画面モード );
				App.オプション設定.全画面モードである = this.全画面モード;
			}
		}

		// ※ Form イベントの override メソッドは描画スレッドで実行されるため、処理中に進行タスクが呼び出されると困る場合には、進行タスクとの lock を忘れないこと。
		private readonly object _高速進行と描画の同期 = new object();

		private enum AppStatus { 開始, 実行中, 終了 };
		private AppStatus _AppStatus = AppStatus.開始;

		/// <summary>
		///		進行タスクの状態。
		///		OFF:タスク起動前、ON:タスク実行中、OFF:タスク終了済み
		/// </summary>
		private TriStateEvent _高速進行ステータス;

		/// <summary>
		///		グローバルリソースのうち、グラフィックリソースを持つものについて、活性化がまだなら活性化する。
		/// </summary>
		private void _活性化する()
		{
			var gd = App.グラフィックデバイス;

			using( Log.Block( FDKUtilities.現在のメソッド名 ) )
			{
				App.ステージ管理.活性化する( gd );
				App.曲ツリー.活性化する( gd );
			}
		}

		/// <summary>
		///		グローバルリソースのうち、グラフィックリソースを持つものについて、活性化中なら非活性化する。
		/// </summary>
		private void _非活性化する()
		{
			var gd = App.グラフィックデバイス;

			using( Log.Block( FDKUtilities.現在のメソッド名 ) )
			{
				App.ステージ管理.非活性化する( gd );
				App.曲ツリー.非活性化する( gd );
			}
		}

		/// <summary>
		///		高速進行ループの処理内容。
		/// </summary>
		private void _高速進行タスクエントリ()
		{
			Log.現在のスレッドに名前をつける( "高速進行" );
			Log.Header( "高速進行タスクを開始します。" );

			this._高速進行ステータス.現在の状態 = TriStateEvent.状態種別.ON;

			while( true )
			{
				lock( this._高速進行と描画の同期 )
				{
					if( this._高速進行ステータス.現在の状態 != TriStateEvent.状態種別.ON )    // lock してる間に状態が変わることがあるので注意。
						break;

					//App.入力管理.すべての入力デバイスをポーリングする();
					// --> 入力ポーリングの挙動はステージごとに異なるので、それぞれのステージ内で行う。

					App.ステージ管理.現在のステージ.高速進行する();
				}

				Thread.Sleep( 1 );  // ウェイト。
			}

			this._高速進行ステータス.現在の状態 = TriStateEvent.状態種別.無効;

			Log.Header( "高速進行タスクを終了しました。" );
		}

		/// <summary>
		///		進行描画ループの処理内容。
		/// </summary>
		private void _進行と描画を行う()
		{
			var gd = App.グラフィックデバイス;
			bool vsync = true;

			if( this._AppStatus != AppStatus.実行中 )  // 上記lock中に終了されている場合があればそれをはじく。
				return;

			gd.D3DDeviceを取得する( ( d3dDevice ) => {

				#region " D3Dレンダリングの前処理を行う。"
				//----------------
				// 既定のD3Dレンダーターゲットビューを黒でクリアする。
				d3dDevice.ImmediateContext.ClearRenderTargetView( gd.D3DRenderTargetView, Color4.Black );

				// 深度バッファを 1.0f でクリアする。
				d3dDevice.ImmediateContext.ClearDepthStencilView(
						gd.D3DDepthStencilView,
						SharpDX.Direct3D11.DepthStencilClearFlags.Depth,
						depth: 1.0f,
						stencil: 0 );
				//----------------
				#endregion

				// アニメーション全体を一括進行。
				gd.Animation.進行する();

				// 現在のステージを進行＆描画。
				App.ステージ管理.現在のステージ.進行描画する( gd );

				// UIFramework を描画。
				gd.UIFramework.Render( gd );

				// ステージの進行描画の結果（フェーズの状態など）を受けての後処理。
				switch( App.ステージ管理.現在のステージ )
				{
					case ステージ.曲ツリー構築.曲ツリー構築ステージ stage:
						#region " 確定 → タイトルステージへ "
						//----------------
						if( stage.現在のフェーズ == ステージ.曲ツリー構築.曲ツリー構築ステージ.フェーズ.確定 )
						{
							//App.ステージ管理.ステージを遷移する( gd, nameof( ステージ.タイトル.タイトルステージ ) );
							App.ステージ管理.ステージを遷移する( gd, nameof( ステージ.選曲.選曲ステージ ) );
						}
						//----------------
						#endregion
						break;

					case ステージ.タイトル.タイトルステージ stage:
						#region " キャンセル → アプリを終了する。"
						//----------------
						if( stage.現在のフェーズ == ステージ.タイトル.タイトルステージ.フェーズ.キャンセル )
						{
							App.ステージ管理.ステージを遷移する( gd, null );
							this._アプリを終了する();
						}
						//----------------
						#endregion
						#region " 確定 → 認証ステージへ "
						//----------------
						if( stage.現在のフェーズ == ステージ.タイトル.タイトルステージ.フェーズ.確定 )
						{
							App.ステージ管理.ステージを遷移する( gd, nameof( ステージ.認証.認証ステージ ) );
						}
						//----------------
						#endregion
						break;

					case ステージ.認証.認証ステージ stage:
						#region " キャンセル → アプリを終了する。"
						//----------------
						if( stage.現在のフェーズ == ステージ.認証.認証ステージ.フェーズ.キャンセル )
						{
							App.ステージ管理.ステージを遷移する( gd, null );
							this._アプリを終了する();
						}
						//----------------
						#endregion
						#region " 確定 → 選曲ステージへ "
						//----------------
						if( stage.現在のフェーズ == ステージ.認証.認証ステージ.フェーズ.確定 )
						{
							App.ステージ管理.ステージを遷移する( gd, nameof( ステージ.選曲.選曲ステージ ) );
						}
						//----------------
						#endregion
						break;

					case ステージ.選曲.選曲ステージ stage:
						#region " キャンセル → アプリを終了する。"
						//----------------
						if( stage.現在のフェーズ == ステージ.選曲.選曲ステージ.フェーズ.キャンセル )
						{
							App.ステージ管理.ステージを遷移する( gd, null );
							this._アプリを終了する();
						}
						//----------------
						#endregion
						#region " 確定 → 曲読み込みステージへ "
						//----------------
						if( stage.現在のフェーズ == ステージ.選曲.選曲ステージ.フェーズ.確定 )
						{
							App.ステージ管理.ステージを遷移する( gd, nameof( ステージ.曲読み込み.曲読み込みステージ ) );
						}
						//----------------
						#endregion
						break;

					case ステージ.曲読み込み.曲読み込みステージ stage:
						#region " 確定 → 演奏ステージへ "
						//----------------
						if( stage.現在のフェーズ == ステージ.曲読み込み.曲読み込みステージ.フェーズ.完了 )
						{
							App.ステージ管理.ステージを遷移する( gd, nameof( ステージ.演奏.演奏ステージ ) );

							// 曲読み込みステージ画面をキャプチャする（演奏ステージのクロスフェードで使う）
							var 演奏ステージ = App.ステージ管理.ステージリスト[ nameof( ステージ.演奏.演奏ステージ ) ] as ステージ.演奏.演奏ステージ;
							演奏ステージ.キャプチャ画面 = 画面キャプチャ.取得する( gd );
						}
						//----------------
						#endregion
						break;

					case ステージ.演奏.演奏ステージ stage:
						#region " キャンセル → 選曲ステージへ "
						//----------------
						if( stage.現在のフェーズ == ステージ.演奏.演奏ステージ.フェーズ.キャンセル )
						{
							App.ステージ管理.ステージを遷移する( gd, nameof( ステージ.選曲.選曲ステージ ) );
						}
						//----------------
						#endregion
						if( stage.現在のフェーズ == ステージ.演奏.演奏ステージ.フェーズ.クリア時フェードアウト )
						{
							// 仮実装
							App.ステージ管理.ステージを遷移する( gd, nameof( ステージ.選曲.選曲ステージ ) );
						}
						break;
				}

				// コマンドフラッシュ。
				if( vsync )
					d3dDevice.ImmediateContext.Flush();
			} );

			// スワップチェーン表示。
			gd.SwapChain.Present( ( vsync ) ? 1 : 0, SharpDX.DXGI.PresentFlags.None );
		}

		/// <summary>
		///		進行タスクを終了し、ウィンドウを閉じ、アプリを終了する。
		/// </summary>
		private void _アプリを終了する()
		{
			using( Log.Block( FDKUtilities.現在のメソッド名 ) )
			{
				if( this._AppStatus != AppStatus.終了 )
				{
					this._高速進行ステータス.現在の状態 = TriStateEvent.状態種別.OFF;

					// _AppStatus を変更してから、GUI スレッドで非同期実行を指示する。
					this._AppStatus = AppStatus.終了;
					this.BeginInvoke( new Action( () => { this.Close(); } ) );
				}
			}
		}
	}
}
