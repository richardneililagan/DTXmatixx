using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using SharpDX;

namespace FDK.メディア
{
	public class グラフィックデバイス : IDisposable
	{
		#region " グラフィックリソースプロパティ(1) スワップチェーンに依存しないもの "
		//----------------
		public SharpDX.DXGI.SwapChain1 SwapChain
			=> this._SwapChain;

		public SharpDX.DirectWrite.Factory DWriteFactory
			=> this._DWriteFactory;

		public SharpDX.Direct2D1.Factory2 D2DFactory
			=> this._D2DFactory;

		public SharpDX.Direct2D1.Device1 D2DDevice
			=> this._D2DDevice;

		public SharpDX.Direct2D1.DeviceContext1 D2DDeviceContext
			=> this._D2DDeviceContext;

		public SharpDX.DirectComposition.DesktopDevice DCompDevice
			=> this._DCompDevice;

		public SharpDX.DirectComposition.Target DCompTarget
			=> this._DCompTarget;

		public SharpDX.DirectComposition.Visual2 DCompVisualForSwapChain
			=> this._DCompVisualForSwapChain;

		public SharpDX.WIC.ImagingFactory2 WicImagingFactory
			=> this._WicImagingFactory;

		public SharpDX.MediaFoundation.DXGIDeviceManager DXGIDeviceManager
			=> this._DXGIDeviceManager;

		public SharpDX.Direct3D11.DeviceDebug D3DDeviceDebug
			=> this._D3DDeviceDebug;

		public FDK.カウンタ.アニメーション管理 Animation
			=> this._Animation;

		public FDK.UI.Framework UIFramework
			=> this._UIFramework;

		/// <summary>
		///		IMFDXGIDeviceManager で取得している場合には、Direct3DDevice を返す。
		///		取得していない場合には、null を返す。
		/// </summary>
		/// <remarks>
		///		取得方法には <see cref="D3DDeviceを取得する(Action{SharpDX.Direct3D11.Device})"/> を使用する。
		///	</remarks>
		public SharpDX.Direct3D11.Device D3DDevice { get; protected set; }
		//----------------
		#endregion
		#region " グラフィックリソースプロパティ(2) スワップチェーンに依存するもの "
		//----------------
		public SharpDX.Direct2D1.Bitmap1 D2DRenderBitmap
			=> this._D2DRenderBitmap;

		public SharpDX.Direct3D11.RenderTargetView D3DRenderTargetView
			=> this._D3DRenderTargetView;

		public SharpDX.Direct3D11.Texture2D D3DDepthStencil
			=> this._D3DDepthStencil;

		public SharpDX.Direct3D11.DepthStencilView D3DDepthStencilView
			=> this._D3DDepthStencilView;

		public SharpDX.Direct3D11.DepthStencilState D3DDepthStencilState
			=> this._D3DDepthStencilState;

		public SharpDX.Mathematics.Interop.RawViewportF[] D3DViewPort
			=> this._D3DViewPort;
		//----------------
		#endregion

		#region " 物理画面、設計画面とその変換に関するプロパティ "
		//----------------
		public SizeF 設計画面サイズ
		{
			get;
			protected set;
		} = SizeF.Empty;
		public SizeF 物理画面サイズ
		{
			get;
			protected set;
		} = SizeF.Empty;
		// ↑ int より float での利用が多いので、Size ではなく SizeF とする。（int 同士ということを忘れて、割り算しておかしくなるケースも多発したので。）

		public float 拡大率DPXtoPX横
		{
			get
				=> ( this.物理画面サイズ.Width / this.設計画面サイズ.Width );
		}
		public float 拡大率DPXtoPX縦
		{
			get
				=> ( this.物理画面サイズ.Height / this.設計画面サイズ.Height );
		}
		public float 拡大率PXtoDPX横
		{
			get
				=> ( this.設計画面サイズ.Width / this.物理画面サイズ.Width );
		}
		public float 拡大率PXtoDPX縦
		{
			get
				=> ( this.設計画面サイズ.Height / this.物理画面サイズ.Height );
		}
		public Matrix3x2 拡大行列DPXtoPX
		{
			get
				=> Matrix3x2.Scaling( this.拡大率DPXtoPX横, this.拡大率DPXtoPX縦 );
		}
		public Matrix3x2 拡大行列PXtoDPX
		{
			get
				=> Matrix3x2.Scaling( this.拡大率PXtoDPX横, this.拡大率PXtoDPX縦 );
		}
		//----------------
		#endregion

		#region " 3D変換用プロパティ "
		//----------------
		public float 視野角deg
		{
			get;
			set;
		} = 45f;
		public Matrix ビュー変換行列
		{
			get
			{
				var カメラの位置 = new Vector3( 0f, 0f, ( -2f * this._dz( this.設計画面サイズ.Height, this.視野角deg ) ) );
				var カメラの注視点 = new Vector3( 0f, 0f, 0f );
				var カメラの上方向 = new Vector3( 0f, 1f, 0f );

				var mat = Matrix.LookAtLH( カメラの位置, カメラの注視点, カメラの上方向 );

				mat.Transpose();  // 転置

				return mat;
			}
		}
		public Matrix 射影変換行列
		{
			get
			{
				float dz = this._dz( this.設計画面サイズ.Height, this.視野角deg );

				var mat = Matrix.PerspectiveFovLH(
					MathUtil.DegreesToRadians( 視野角deg ),
					設計画面サイズ.Width / 設計画面サイズ.Height,  // アスペクト比
					-dz,  // 前方投影面までの距離
					dz ); // 後方投影面までの距離

				mat.Transpose();  // 転置

				return mat;
			}
		}
		//----------------
		#endregion

		/// <summary>
		///		現在時刻から、DirectComposition Engine による次のフレーム表示時刻までの間隔[秒]を返す。
		/// </summary>
		/// <remarks>
		///		この時刻の仕様と使い方については、以下を参照。
		///		Architecture and components - MSDN
		///		https://msdn.microsoft.com/en-us/library/windows/desktop/hh437350.aspx
		/// </remarks>
		public double 次のDComp表示までの残り時間sec
		{
			get
			{
				var fs = this.DCompDevice.FrameStatistics;
				return ( fs.NextEstimatedFrameTime - fs.CurrentTime ) / fs.TimeFrequency;
			}
		}


		/// <summary>
		///		初期化処理。
		/// </summary>
		public グラフィックデバイス( IntPtr hWindow, SizeF 設計画面サイズ, SizeF 物理画面サイズ, bool 深度ステンシルを使う = true )
		{
			this._hWindow = hWindow;
			this.設計画面サイズ = 設計画面サイズ;
			this.物理画面サイズ = 物理画面サイズ;
			this._深度ステンシルを使う = 深度ステンシルを使う;

			SharpDX.MediaFoundation.MediaManager.Startup();
			
			this._スワップチェーンに依存しないグラフィックリソースを作成する();
			this.D3DDeviceを取得する( ( d3dDevice ) => {
				this._スワップチェーンを作成する( d3dDevice );
				this._スワップチェーンに依存するグラフィックリソースを作成する( d3dDevice );
			} );
		}

		/// <summary>
		///		終了処理。
		/// </summary>
		public void Dispose()
		{
			this._スワップチェーンに依存するグラフィックリソースを解放する();
			this._スワップチェーンを解放する();
			this._スワップチェーンに依存しないグラフィックリソースを解放する();

			SharpDX.MediaFoundation.MediaManager.Shutdown();
		}

		/// <summary>
		///		既定のD2Dデバイスコンテキストに対して描画処理を実行する。
		/// </summary>
		/// <remarks>
		///		描画処理は、BeginDraw() と EndDraw() の間で行われることを保証する。
		///		描画処理中に例外が発生しても EndDraw() の呼び出しは保証する。
		/// </remarks>
		/// <param name="描画処理">BeginDraw() と EndDraw() の間で行う処理。</param>
		public void D2DBatchDraw( Action<SharpDX.Direct2D1.DeviceContext1> 描画処理 )
		{
			try
			{
				this._D2DDeviceContext.Transform = this.拡大行列DPXtoPX;   // 設計画面単位 → 物理画面単位
				this._D2DDeviceContext.PrimitiveBlend = SharpDX.Direct2D1.PrimitiveBlend.SourceOver;

				this._D2DDeviceContext.BeginDraw();

				描画処理( this._D2DDeviceContext );
			}
			finally
			{
				this._D2DDeviceContext.EndDraw();
			}
		}

		/// <summary>
		///		レンダーターゲットに対して描画処理を実行する。
		/// </summary>
		/// <remarks>
		///		描画処理は、レンダーターゲットの BeginDraw() と EndDraw() の間で行われることを保証する。
		///		描画処理中に例外が発生しても EndDraw() の呼び出しは保証する。
		/// </remarks>
		/// <param name="target">レンダリングターゲット。</param>
		/// <param name="描画処理">BeginDraw() と EndDraw() の間で行う処理。</param>
		public void D2DBatchDraw( SharpDX.Direct2D1.RenderTarget target, Action<SharpDX.Direct2D1.RenderTarget> 描画処理 )
		{
			try
			{
				target.Transform = this.拡大行列DPXtoPX;   // 設計画面単位 → 物理画面単位

				target.BeginDraw();
				描画処理( target );
			}
			finally
			{
				target.EndDraw();
			}
		}

		/// <summary>
		///		D3DDevice をロックし、それを引数として任意の処理を実行したのち、D3DDevice をアンロックするまでの一連のバッチ処理。
		/// </summary>
		public void D3DDeviceを取得する( Action<SharpDX.Direct3D11.Device> action )
		{
			var deviceHandle = this._DXGIDeviceManager.OpenDeviceHandle();

			try
			{
				var dev = this._DXGIDeviceManager.LockDevice( deviceHandle, typeof( SharpDX.Direct3D11.Device ).GUID, true );   // Lockできるまでブロックする。

				try
				{
					using( var d3dDevice = SharpDX.Direct3D11.Device.FromPointer<SharpDX.Direct3D11.Device>( dev ) )
					{
						this.D3DDevice = d3dDevice;		// 取得している間のみ、このメンバが有効。
						action( d3dDevice );
					}
				}
				finally
				{
					this.D3DDevice = null;				// 利用が終わったので、このメンバは無効。
					this._DXGIDeviceManager.UnlockDevice( deviceHandle );
				}
			}
			finally
			{
				this._DXGIDeviceManager.CloseDeviceHandle( deviceHandle );
			}

			Debug.Assert( null == this.D3DDevice );	// このメンバがもう使えないことを確認。
		}

		/// <summary>
		///		バックバッファ（スワップチェーン）のサイズを変更する。
		/// </summary>
		/// <param name="newSize"></param>
		public void サイズを変更する( Size newSize, SharpDX.Direct3D11.Device d3dDevice )
		{
			Debug.Assert( null != d3dDevice );

			// (1) 依存リソースを解放。
			this._スワップチェーンに依存するグラフィックリソースを解放する();

			// (2) バックバッファのサイズを変更。
			this._SwapChain.ResizeBuffers(
				0,									// 現在のバッファ数を維持
				newSize.Width,                      // 新しいサイズ
				newSize.Height,                     //
				SharpDX.DXGI.Format.Unknown,        // 現在のフォーマットを維持
				SharpDX.DXGI.SwapChainFlags.None );

			this.物理画面サイズ = newSize; // 更新

			// (3) 依存リソースを作成。
			this._スワップチェーンに依存するグラフィックリソースを作成する( d3dDevice );
		}

		/// <summary>
		///		バックバッファに対応するウィンドウのハンドル。
		///		コンストラクタで指定する。
		/// </summary>
		private IntPtr _hWindow;

		private bool _深度ステンシルを使う = true;

		#region " グラフィックリソース(1) スワップチェーンに依存しないもの "
		//----------------
		//private SharpDX.Direct3D11.Device _D3DDevice = null;	--> MediaFoundation と共有するので、必要時ごとに IMFDXGIDeviceManager から取得し、使ったら解放するライフサイクルとなる。
		//private SharpDX.DXGI.Device _DXGIDevice = null;		--> D3DDevice と同一のインスタンスなので、D3DDevice と同じライフサイクルが必要。
		private SharpDX.DXGI.SwapChain1 _SwapChain = null;
		private SharpDX.DirectWrite.Factory _DWriteFactory = null;
		private SharpDX.Direct2D1.Factory2 _D2DFactory = null;
		private SharpDX.Direct2D1.Device1 _D2DDevice = null;
		private SharpDX.Direct2D1.DeviceContext1 _D2DDeviceContext = null;
		private SharpDX.DirectComposition.DesktopDevice _DCompDevice = null;
		private SharpDX.DirectComposition.Target _DCompTarget = null;
		private SharpDX.DirectComposition.Visual2 _DCompVisualForSwapChain = null;
		private SharpDX.WIC.ImagingFactory2 _WicImagingFactory = null;
		private SharpDX.MediaFoundation.DXGIDeviceManager _DXGIDeviceManager = null;
		private SharpDX.Direct3D11.DeviceDebug _D3DDeviceDebug = null;
		private SharpDX.Mathematics.Interop.RawViewportF[] _D3DViewPort = new SharpDX.Mathematics.Interop.RawViewportF[ 1 ];
		private FDK.カウンタ.アニメーション管理 _Animation = null;
		private FDK.UI.Framework _UIFramework = null;
		//----------------
		#endregion
		#region " グラフィックリソース(2) スワップチェーンに依存するもの "
		//----------------
		private SharpDX.Direct3D11.RenderTargetView _D3DRenderTargetView = null;
		private SharpDX.Direct3D11.Texture2D _D3DDepthStencil = null;
		private SharpDX.Direct3D11.DepthStencilView _D3DDepthStencilView = null;
		private SharpDX.Direct3D11.DepthStencilState _D3DDepthStencilState = null;
		private SharpDX.Direct2D1.Bitmap1 _D2DRenderBitmap = null;
		//----------------
		#endregion


		private void _スワップチェーンに依存しないグラフィックリソースを作成する()
		{
			this._DXGIDeviceManager = new SharpDX.MediaFoundation.DXGIDeviceManager();
#if DEBUG
			this._D2DFactory = new SharpDX.Direct2D1.Factory2( SharpDX.Direct2D1.FactoryType.MultiThreaded, SharpDX.Direct2D1.DebugLevel.Information );
#else
			this._D2DFactory = new SharpDX.Direct2D1.Factory2( SharpDX.Direct2D1.FactoryType.MultiThreaded, SharpDX.Direct2D1.DebugLevel.None );
#endif
			this._DWriteFactory = new SharpDX.DirectWrite.Factory( SharpDX.DirectWrite.FactoryType.Shared );

			this._WicImagingFactory = new SharpDX.WIC.ImagingFactory2();

			using( var d3dDevice = new SharpDX.Direct3D11.Device(
				SharpDX.Direct3D.DriverType.Hardware,
#if DEBUG
				// D3D11 Debugメッセージは、プロジェクトプロパティで「ネイティブコードのデバッグを有効にする」を ON にしないと表示されない。
				// なお、デバッグレイヤーを有効にしてアプリケーションを実行すると、速度が大幅に低下する。
				SharpDX.Direct3D11.DeviceCreationFlags.Debug |
#endif
				SharpDX.Direct3D11.DeviceCreationFlags.BgraSupport,
				new SharpDX.Direct3D.FeatureLevel[] {
					SharpDX.Direct3D.FeatureLevel.Level_11_1,
					SharpDX.Direct3D.FeatureLevel.Level_11_0,
				} ) )
			using( var dxgiDevice = d3dDevice.QueryInterface<SharpDX.DXGI.Device1>() )
			{
				#region " D3DDevice が ID3D11VideoDevice を実装してないならエラー。（Windows8以降のPCで実装されている。）"
				//----------------
				using( var videoDevice = d3dDevice.QueryInterfaceOrNull<SharpDX.Direct3D11.VideoDevice>() )
				{
					if( null == videoDevice )
						throw new Exception( "Direct3D11デバイスが、ID3D11VideoDevice をサポートしていません。" );
				}
				//----------------
				#endregion

				#region " マルチスレッドモードを ON に設定する。DXVAを使う場合は必須。"
				//----------------
				using( var multithread = d3dDevice.QueryInterfaceOrNull<SharpDX.Direct3D.DeviceMultithread>() )
				{
					if( null == multithread )
						throw new Exception( "Direct3D11デバイスが、ID3D10Multithread をサポートしていません。" );

					multithread.SetMultithreadProtected( true );
				}
				//----------------
				#endregion

				// DXGIDevice のレイテンシ設定。
				dxgiDevice.MaximumFrameLatency = 1;

				// Debug フラグが立ってないなら null 。
				this._D3DDeviceDebug = d3dDevice.QueryInterfaceOrNull<SharpDX.Direct3D11.DeviceDebug>();

				// D2Dデバイスを作成する。
				this._D2DDevice = new SharpDX.Direct2D1.Device1( this._D2DFactory, dxgiDevice );

				// 既定のD2Dデバイスコンテキストを作成する。
				this._D2DDeviceContext = new SharpDX.Direct2D1.DeviceContext1( this._D2DDevice, SharpDX.Direct2D1.DeviceContextOptions.EnableMultithreadedOptimizations ) {
					TextAntialiasMode = SharpDX.Direct2D1.TextAntialiasMode.Grayscale,  // Grayscale がすべての Windows ストアアプリで推奨される。らしい。
				};

				// DirectCompositionデバイスを作成する。
				this._DCompDevice = new SharpDX.DirectComposition.DesktopDevice( this._D2DDevice );

				// スワップチェーン用のVisualを作成する。
				this._DCompVisualForSwapChain = new SharpDX.DirectComposition.Visual2( this._DCompDevice );

				// コンポジションターゲットを作成し、Visualツリーのルートにスワップチェーン用Visualを設定する。
				this._DCompTarget = SharpDX.DirectComposition.Target.FromHwnd( this._DCompDevice, this._hWindow, topmost: true );
				this._DCompTarget.Root = this._DCompVisualForSwapChain;

				// DXGIデバイスマネージャに D3Dデバイスを登録する。
				this._DXGIDeviceManager.ResetDevice( d3dDevice );
			}

			this.D3DDeviceを取得する( ( d3dDevice ) => {
				テクスチャ.全インスタンスで共有するリソースを作成する( this );
			} );

			this._Animation = new カウンタ.アニメーション管理();

			this._UIFramework = new UI.Framework();
		}
		private void _スワップチェーンに依存しないグラフィックリソースを解放する()
		{
			FDKUtilities.解放する( ref this._UIFramework );
			FDKUtilities.解放する( ref this._Animation );

			テクスチャ.全インスタンスで共有するリソースを解放する();

			this._DCompTarget.Root = null;

			FDKUtilities.解放する( ref this._DCompTarget );
			FDKUtilities.解放する( ref this._DCompVisualForSwapChain );
			FDKUtilities.解放する( ref this._DCompDevice );
			FDKUtilities.解放する( ref this._D2DDeviceContext );
			FDKUtilities.解放する( ref this._D2DDevice );
			FDKUtilities.解放する( ref this._WicImagingFactory );
			FDKUtilities.解放する( ref this._DWriteFactory );
			FDKUtilities.解放する( ref this._D2DFactory );
			FDKUtilities.解放する( ref this._DXGIDeviceManager );
#if DEBUG
			this._D3DDeviceDebug?.ReportLiveDeviceObjects( SharpDX.Direct3D11.ReportingLevel.Detail );
#endif
			FDKUtilities.解放する( ref this._D3DDeviceDebug );
		}

		private void _スワップチェーンを作成する( SharpDX.Direct3D11.Device d3dDevice )
		{
			Debug.Assert( null != d3dDevice );

			var swapChainDesc = new SharpDX.DXGI.SwapChainDescription1() {
				BufferCount = 2,
				Width = (int) this.物理画面サイズ.Width,
				Height = (int) this.物理画面サイズ.Height,
				Format = SharpDX.DXGI.Format.B8G8R8A8_UNorm,    // D2D をサポートするなら B8G8R8A8 を使う必要がある。
				AlphaMode = SharpDX.DXGI.AlphaMode.Ignore,      // Premultiplied にすると、ウィンドウの背景（デスクトップ画像）と加算合成される（意味ない）
				Stereo = false,
				SampleDescription = new SharpDX.DXGI.SampleDescription( 1, 0 ), // マルチサンプリングは使わない。
				SwapEffect = SharpDX.DXGI.SwapEffect.FlipSequential,    // SwapChainForComposition での必須条件。
				Scaling = SharpDX.DXGI.Scaling.Stretch,                 // SwapChainForComposition での必須条件。
				Usage = SharpDX.DXGI.Usage.RenderTargetOutput,
				Flags = SharpDX.DXGI.SwapChainFlags.None,

				// https://msdn.microsoft.com/en-us/library/windows/desktop/bb174579.aspx
				// > You cannot call SetFullscreenState on a swap chain that you created with IDXGIFactory2::CreateSwapChainForComposition.
				//Flags = SharpDX.DXGI.SwapChainFlags.AllowModeSwitch,
			};

			using( var dxgiDevice = d3dDevice.QueryInterface<SharpDX.DXGI.Device1>() )
			using( var dxgiAdapter = dxgiDevice.Adapter )
			using( var dxgiFactory = dxgiAdapter.GetParent<SharpDX.DXGI.Factory2>() )
			{
				this._SwapChain = new SharpDX.DXGI.SwapChain1( dxgiFactory, d3dDevice, ref swapChainDesc );   // CreateSwapChainForComposition

				// 標準機能である PrintScreen と Alt+Enter は使わない。
				dxgiFactory.MakeWindowAssociation(
					this._hWindow,
					SharpDX.DXGI.WindowAssociationFlags.IgnoreAll
					//SharpDX.DXGI.WindowAssociationFlags.IgnorePrintScreen
					//| SharpDX.DXGI.WindowAssociationFlags.IgnoreAltEnter
					);
			}

			// DirectComposition 関連
			this._DCompVisualForSwapChain.Content = this._SwapChain;
			this._DCompDevice.Commit();
		}
		private void _スワップチェーンを解放する()
		{
			//this._SwapChain.SetFullscreenState( false, null );
			// --> このクラスでは「全画面」を使わない（代わりに「最大化」を使う）ので不要。

			FDKUtilities.解放する( ref this._SwapChain );
		}

		private void _スワップチェーンに依存するグラフィックリソースを作成する( SharpDX.Direct3D11.Device d3dDevice )
		{
			Debug.Assert( null != d3dDevice );

			using( var backbufferTexture2D = this._SwapChain.GetBackBuffer<SharpDX.Direct3D11.Texture2D>( 0 ) )	// D3D 用
			using( var backbufferSurface = this._SwapChain.GetBackBuffer<SharpDX.DXGI.Surface>( 0 ) )			// D2D 用
			{
				// ※正確には、「スワップチェーン」というより、「スワップチェーンが持つバックバッファ」に依存するリソースだ。

				// D3D 関連

				#region " バックバッファに対するD3Dレンダーターゲットビューを作成する。"
				//----------------
				this._D3DRenderTargetView = new SharpDX.Direct3D11.RenderTargetView( d3dDevice, backbufferTexture2D );
				//----------------
				#endregion

				#region " バックバッファに対する深度ステンシル、深度ステンシルビュー、深度ステンシルステートを作成する。"
				//----------------
				var depthStencilDesc = new SharpDX.Direct3D11.Texture2DDescription() {
					Width = backbufferTexture2D.Description.Width,
					Height = backbufferTexture2D.Description.Height,
					MipLevels = 1,
					ArraySize = 1,
					Format = SharpDX.DXGI.Format.D32_Float, // Depthのみのフォーマット
					SampleDescription = backbufferTexture2D.Description.SampleDescription,
					Usage = SharpDX.Direct3D11.ResourceUsage.Default,
					BindFlags = SharpDX.Direct3D11.BindFlags.DepthStencil,
					CpuAccessFlags = SharpDX.Direct3D11.CpuAccessFlags.None,    // CPUからはアクセスしない
					OptionFlags = SharpDX.Direct3D11.ResourceOptionFlags.None,
				};
				this._D3DDepthStencil = new SharpDX.Direct3D11.Texture2D( d3dDevice, depthStencilDesc );

				var depthStencilViewDesc = new SharpDX.Direct3D11.DepthStencilViewDescription() {
					Format = depthStencilDesc.Format,
					Dimension = SharpDX.Direct3D11.DepthStencilViewDimension.Texture2D,
					Flags = SharpDX.Direct3D11.DepthStencilViewFlags.None,
					Texture2D = new SharpDX.Direct3D11.DepthStencilViewDescription.Texture2DResource() {
						MipSlice = 0,
					},
				};
				this._D3DDepthStencilView = new SharpDX.Direct3D11.DepthStencilView( d3dDevice, this._D3DDepthStencil, depthStencilViewDesc );

				var depthSencilStateDesc = new SharpDX.Direct3D11.DepthStencilStateDescription() {
					IsDepthEnabled = this._深度ステンシルを使う,                  // 深度テストあり？
					DepthWriteMask = SharpDX.Direct3D11.DepthWriteMask.All,     // 書き込む
					DepthComparison = SharpDX.Direct3D11.Comparison.Less,       // 手前の物体を描画
					IsStencilEnabled = false,                                   // ステンシルテストなし。
					StencilReadMask = 0,                                        // ステンシル読み込みマスク。
					StencilWriteMask = 0,                                       // ステンシル書き込みマスク。
																				// 面が表を向いている場合のステンシル・テストの設定
					FrontFace = new SharpDX.Direct3D11.DepthStencilOperationDescription() {
						FailOperation = SharpDX.Direct3D11.StencilOperation.Keep,       // 維持
						DepthFailOperation = SharpDX.Direct3D11.StencilOperation.Keep,  // 維持
						PassOperation = SharpDX.Direct3D11.StencilOperation.Keep,       // 維持
						Comparison = SharpDX.Direct3D11.Comparison.Never,               // 常に失敗
					},
					// 面が裏を向いている場合のステンシル・テストの設定
					BackFace = new SharpDX.Direct3D11.DepthStencilOperationDescription() {
						FailOperation = SharpDX.Direct3D11.StencilOperation.Keep,       // 維持
						DepthFailOperation = SharpDX.Direct3D11.StencilOperation.Keep,  // 維持
						PassOperation = SharpDX.Direct3D11.StencilOperation.Keep,       // 維持
						Comparison = SharpDX.Direct3D11.Comparison.Always,              // 常に成功
					},
				};
				this._D3DDepthStencilState = new SharpDX.Direct3D11.DepthStencilState( d3dDevice, depthSencilStateDesc );
				//----------------
				#endregion

				#region " バックバッファに対するビューポートを作成する。"
				//----------------
				this._D3DViewPort[ 0 ] = new SharpDX.Mathematics.Interop.RawViewportF() {
					X = 0.0f,
					Y = 0.0f,
					Width = (float) backbufferTexture2D.Description.Width,
					Height = (float) backbufferTexture2D.Description.Height,
					MinDepth = 0.0f,
					MaxDepth = 1.0f,
				};
				//----------------
				#endregion

				// D2D 関連

				#region " バックバッファとメモリを共有する、既定のD2Dレンダーターゲットビットマップを作成する。"
				//----------------
				this._D2DRenderBitmap = new SharpDX.Direct2D1.Bitmap1(  // このビットマップは、
					this._D2DDeviceContext,
					backbufferSurface,                                  // このDXGIサーフェスとメモリを共有する。
					new SharpDX.Direct2D1.BitmapProperties1() {
						PixelFormat = new SharpDX.Direct2D1.PixelFormat( backbufferSurface.Description.Format, SharpDX.Direct2D1.AlphaMode.Premultiplied ),
						BitmapOptions = SharpDX.Direct2D1.BitmapOptions.Target | SharpDX.Direct2D1.BitmapOptions.CannotDraw,
					} );

				this._D2DDeviceContext.Target = this._D2DRenderBitmap;
				//----------------
				#endregion
			}
		}
		private void _スワップチェーンに依存するグラフィックリソースを解放する()
		{
			if( null != this.D3DDevice )
			{
				this.D3DDevice.ImmediateContext.ClearState();
				this.D3DDevice.ImmediateContext.OutputMerger.ResetTargets();
			}

			this._DCompVisualForSwapChain.Content = null;
			this._D2DDeviceContext.Target = null;

			FDKUtilities.解放する( ref this._D2DRenderBitmap );
			FDKUtilities.解放する( ref this._D3DDepthStencilState );
			FDKUtilities.解放する( ref this._D3DDepthStencilView );
			FDKUtilities.解放する( ref this._D3DDepthStencil );
			FDKUtilities.解放する( ref this._D3DRenderTargetView );
		}

		/// <summary>
		///		ビューが Z=0 の位置に置かれるとき、ビューの高さと視野角から、カメラの Z 位置を計算して返す。
		/// </summary>
		private float _dz( float 高さ, float 視野角deg )
		{
			return (float) ( 高さ / ( 4.0 * Math.Tan( MathUtil.DegreesToRadians( 視野角deg / 2.0f ) ) ) );
		}
	}
}
