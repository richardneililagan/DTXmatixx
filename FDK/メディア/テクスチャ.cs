using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SharpDX;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.Mathematics.Interop;
using FDK;
using FDK.同期;

namespace FDK.メディア
{
	/// <summary>
	///		Direct3D の D3DTexture を使って描画する画像。
	/// </summary>
	public class テクスチャ : Activity
	{
		/// <summary>
		///		0:透明～1:不透明
		/// </summary>
		public float 不透明度
		{
			get;
			set;
		} = 1f;

		public bool 加算合成する
		{
			get;
			set;
		} = false;

		public Size2F サイズ
		{
			get
				=> this._ShaderResourceViewSize;
		}


		public テクスチャ( string 画像ファイルパス, BindFlags bindFlags = BindFlags.ShaderResource )
		{
			this._bindFlags = bindFlags;

			// ↓ どちらかを選択的に指定すること。
			this._画像ファイルパス = 画像ファイルパス;	// 選択
			this.ユーザ指定サイズ = Size2.Empty;
		}

		public テクスチャ( Size2 サイズ, BindFlags bindFlags = BindFlags.ShaderResource )
		{
			this._bindFlags = bindFlags;

			// ↓ どちらかを選択的に指定すること。
			this._画像ファイルパス = null;
			this.ユーザ指定サイズ = サイズ;	// 選択
		}

		protected override void On活性化( グラフィックデバイス gd )
		{
			Debug.Assert( null != gd.D3DDevice, "D3DDevice が取得されていません。" );

			#region " 定数バッファを生成する。"
			//----------------
			var cBufferDesc = new BufferDescription() {
				Usage = ResourceUsage.Dynamic,				// 動的使用法
				BindFlags = BindFlags.ConstantBuffer,		// 定数バッファ
				CpuAccessFlags = CpuAccessFlags.Write,		// CPUから書き込む
				OptionFlags = ResourceOptionFlags.None,
				SizeInBytes = SharpDX.Utilities.SizeOf<ST定数バッファの転送元データ>(),   // バッファサイズ
				StructureByteStride = 0,
			};
			this._ConstantBuffer = new SharpDX.Direct3D11.Buffer( gd.D3DDevice, cBufferDesc );
			//----------------
			#endregion

			#region " テクスチャとシェーダーリソースビューを生成する。"
			//----------------
			if( this._画像ファイルパス.Nullでも空でもない() )
			{
				// (A) 画像ファイルから生成する場合。
				var 戻り値 = FDKUtilities.CreateShaderResourceViewFromFile(
					gd.D3DDevice,
					this._bindFlags,
					Folder.絶対パスに含まれるフォルダ変数を展開して返す( this._画像ファイルパス ) );
				this._ShaderResourceView = 戻り値.srv;
				this._ShaderResourceViewSize = 戻り値.viewSize;
				this.Texture = 戻り値.texture;
			}
			else if( this.ユーザ指定サイズ != Size2.Empty )
			{
				// (B) サイズを指定して生成する場合。
				var 戻り値 = FDKUtilities.CreateShaderResourceView(
					gd.D3DDevice,
					this._bindFlags,
					this.ユーザ指定サイズ );
				this._ShaderResourceView = 戻り値.srv;
				this.Texture = 戻り値.texture;

				this._ShaderResourceViewSize = new Size2F( this.ユーザ指定サイズ.Width, this.ユーザ指定サイズ.Height );
			}
			else
			{
				throw new InvalidOperationException();
			}
			//----------------
			#endregion
		}

		protected override void On非活性化( グラフィックデバイス gd )
		{
			FDKUtilities.解放する( ref this._ShaderResourceView );
			FDKUtilities.解放する( ref this.Texture );
			FDKUtilities.解放する( ref this._ConstantBuffer );
		}

		/// <summary>
		///		テクスチャを描画する。
		///	</summary>
		/// <param name="ワールド行列変換">テクスチャは1×1のモデルサイズで表現されており、それにこのワールド行列を適用する。</param>
		/// <param name="転送元矩形">テクスチャ座標(値域0～1)で指定する。</param>
		public void 描画する( グラフィックデバイス gd, Matrix ワールド行列変換, RectangleF? 転送元矩形 = null )
		{
			Debug.Assert( null != gd.D3DDevice, "D3DDevice が取得されていません。" );

			#region " 定数バッファを更新する。"
			//----------------
			{
				// ワールド変換行列
				ワールド行列変換.Transpose();    // 転置
				this._定数バッファの転送元データ.World = ワールド行列変換;

				// ビュー変換行列
				this._定数バッファの転送元データ.View = gd.ビュー変換行列;  // 転置済み

				// 射影変換行列
				this._定数バッファの転送元データ.Projection = gd.射影変換行列; // 転置済み

				// 描画元矩形（x,y,zは0～1で指定する（UV座標））
				if( null == 転送元矩形 )
					転送元矩形 = new RectangleF( 0f, 0f, this.サイズ.Width, this.サイズ.Height );
				this._定数バッファの転送元データ.TexLeft = 転送元矩形.Value.Left / this.サイズ.Width;
				this._定数バッファの転送元データ.TexTop = 転送元矩形.Value.Top / this.サイズ.Height;
				this._定数バッファの転送元データ.TexRight = 転送元矩形.Value.Right / this.サイズ.Width;
				this._定数バッファの転送元データ.TexBottom = 転送元矩形.Value.Bottom / this.サイズ.Height;

				// アルファ
				this._定数バッファの転送元データ.TexAlpha = this.不透明度;
				this._定数バッファの転送元データ.dummy1 = 0f;
				this._定数バッファの転送元データ.dummy2 = 0f;
				this._定数バッファの転送元データ.dummy3 = 0f;

				// 定数バッファへ書き込む。
				var dataBox = gd.D3DDevice.ImmediateContext.MapSubresource(
					resourceRef: this._ConstantBuffer,
					subresource: 0,
					mapType: MapMode.WriteDiscard,
					mapFlags: MapFlags.None );
				SharpDX.Utilities.Write( dataBox.DataPointer, ref this._定数バッファの転送元データ );
				gd.D3DDevice.ImmediateContext.UnmapSubresource( this._ConstantBuffer, 0 );
			}
			//----------------
			#endregion

			#region " 3Dパイプラインを設定する。"
			//----------------
			{
				// 入力アセンブラ
				gd.D3DDevice.ImmediateContext.InputAssembler.InputLayout = null;
				gd.D3DDevice.ImmediateContext.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleStrip;

				// 頂点シェーダ
				gd.D3DDevice.ImmediateContext.VertexShader.Set( テクスチャ._VertexShader );
				gd.D3DDevice.ImmediateContext.VertexShader.SetConstantBuffers( 0, this._ConstantBuffer );

				// ジオメトリシェーダ
				gd.D3DDevice.ImmediateContext.GeometryShader.Set( null );

				// ラスタライザ
				gd.D3DDevice.ImmediateContext.Rasterizer.SetViewports( gd.D3DViewPort );
				gd.D3DDevice.ImmediateContext.Rasterizer.State = テクスチャ._RasterizerState;

				// ピクセルシェーダ
				gd.D3DDevice.ImmediateContext.PixelShader.Set( テクスチャ._PixelShader );
				gd.D3DDevice.ImmediateContext.PixelShader.SetConstantBuffers( 0, this._ConstantBuffer );
				gd.D3DDevice.ImmediateContext.PixelShader.SetShaderResources( 0, 1, this._ShaderResourceView );
				gd.D3DDevice.ImmediateContext.PixelShader.SetSamplers( 0, 1, テクスチャ._SamplerState );

				// 出力マージャ
				gd.D3DDevice.ImmediateContext.OutputMerger.SetTargets( gd.D3DDepthStencilView, gd.D3DRenderTargetView );
				gd.D3DDevice.ImmediateContext.OutputMerger.SetBlendState(
					( this.加算合成する ) ? テクスチャ._BlendState加算合成 : テクスチャ._BlendState通常合成,
					new Color4( 0f, 0f, 0f, 0f ),
					-1 );
				gd.D3DDevice.ImmediateContext.OutputMerger.SetDepthStencilState( gd.D3DDepthStencilState, 0 );
			}
			//----------------
			#endregion

			// 頂点バッファとインデックスバッファを使わずに 4 つの頂点を描画する。
			gd.D3DDevice.ImmediateContext.Draw( vertexCount: 4, startVertexLocation: 0 );
		}


		protected Size2 ユーザ指定サイズ;
		
		protected Texture2D Texture = null;


		private string _画像ファイルパス = null;

		private SharpDX.Direct3D11.Buffer _ConstantBuffer = null;

		private ShaderResourceView _ShaderResourceView = null;

		private Size2F _ShaderResourceViewSize;

		private BindFlags _bindFlags;

		private struct ST定数バッファの転送元データ
		{
			public Matrix World;      // ワールド変換行列
			public Matrix View;       // ビュー変換行列
			public Matrix Projection; // 透視変換行列

			public float TexLeft;   // 描画元矩形の左u座標(0～1)
			public float TexTop;    // 描画元矩形の上v座標(0～1)
			public float TexRight;  // 描画元矩形の右u座標(0～1)
			public float TexBottom; // 描画元矩形の下v座標(0～1)

			public float TexAlpha;  // テクスチャに乗じるアルファ値(0～1)
			public float dummy1;    // float4境界に合わせるためのダミー
			public float dummy2;    // float4境界に合わせるためのダミー
			public float dummy3;    // float4境界に合わせるためのダミー
		};
		private ST定数バッファの転送元データ _定数バッファの転送元データ;


		// 全インスタンス共通項目(static) 

		public static void 全インスタンスで共有するリソースを作成する( グラフィックデバイス gd )
		{
			Debug.Assert( null != gd.D3DDevice, "D3DDevice が取得されていません。" );

			var シェーダコンパイルのオプション =
				ShaderFlags.Debug |
				ShaderFlags.SkipOptimization |
				ShaderFlags.EnableStrictness |
				ShaderFlags.PackMatrixColumnMajor;

			#region " 頂点シェーダを生成する。"
			//----------------
			{
				// シェーダコードをコンパイルする。
				using( var code = ShaderBytecode.Compile(
					Properties.Resources.テクスチャ用シェーダコード,
					"VS", "vs_4_0", シェーダコンパイルのオプション ) )
				{
					// 頂点シェーダを生成する。
					テクスチャ._VertexShader = new VertexShader( gd.D3DDevice, code );
				}
			}
			//----------------
			#endregion

			#region " ピクセルシェーダを生成する。"
			//----------------
			{
				// シェーダコードをコンパイルする。
				using( var code = ShaderBytecode.Compile(
					Properties.Resources.テクスチャ用シェーダコード,
					"PS", "ps_4_0", シェーダコンパイルのオプション ) )
				{
					// ピクセルシェーダを作成する。
					テクスチャ._PixelShader = new PixelShader( gd.D3DDevice, code );
				}
			}
			//----------------
			#endregion

			#region " ブレンドステート通常版を生成する。"
			//----------------
			{
				var BlendStateNorm = new BlendStateDescription() {
					AlphaToCoverageEnable = false,  // アルファマスクで透過する（するならZバッファ必須）
					IndependentBlendEnable = false, // 個別設定。false なら BendStateDescription.RenderTarget[0] だけが有効で、[1～7] は無視される。
				};
				BlendStateNorm.RenderTarget[ 0 ].IsBlendEnabled = true; // true ならブレンディングが有効。
				BlendStateNorm.RenderTarget[ 0 ].RenderTargetWriteMask = ColorWriteMaskFlags.All;        // RGBA の書き込みマスク。

				// アルファ値のブレンディング設定 ... 特になし
				BlendStateNorm.RenderTarget[ 0 ].SourceAlphaBlend = BlendOption.One;
				BlendStateNorm.RenderTarget[ 0 ].DestinationAlphaBlend = BlendOption.Zero;
				BlendStateNorm.RenderTarget[ 0 ].AlphaBlendOperation = BlendOperation.Add;

				// 色値のブレンディング設定 ... アルファ強度に応じた透明合成（テクスチャのアルファ値は、テクスチャのアルファ×ピクセルシェーダでの全体アルファとする（HLSL参照））
				BlendStateNorm.RenderTarget[ 0 ].SourceBlend = BlendOption.SourceAlpha;
				BlendStateNorm.RenderTarget[ 0 ].DestinationBlend = BlendOption.InverseSourceAlpha;
				BlendStateNorm.RenderTarget[ 0 ].BlendOperation = BlendOperation.Add;

				// ブレンドステートを作成する。
				テクスチャ._BlendState通常合成 = new BlendState( gd.D3DDevice, BlendStateNorm );
			}
			//----------------
			#endregion

			#region " ブレンドステート加算合成版を生成する。"
			//----------------
			{
				var BlendStateAdd = new BlendStateDescription() {
					AlphaToCoverageEnable = false,  // アルファマスクで透過する（するならZバッファ必須）
					IndependentBlendEnable = false, // 個別設定。false なら BendStateDescription.RenderTarget[0] だけが有効で、[1～7] は無視される。
				};
				BlendStateAdd.RenderTarget[ 0 ].IsBlendEnabled = true; // true ならブレンディングが有効。
				BlendStateAdd.RenderTarget[ 0 ].RenderTargetWriteMask = ColorWriteMaskFlags.All;        // RGBA の書き込みマスク。

				// アルファ値のブレンディング設定 ... 特になし
				BlendStateAdd.RenderTarget[ 0 ].SourceAlphaBlend = BlendOption.One;
				BlendStateAdd.RenderTarget[ 0 ].DestinationAlphaBlend = BlendOption.Zero;
				BlendStateAdd.RenderTarget[ 0 ].AlphaBlendOperation = BlendOperation.Add;

				// 色値のブレンディング設定 ... 加算合成
				BlendStateAdd.RenderTarget[ 0 ].SourceBlend = BlendOption.SourceAlpha;
				BlendStateAdd.RenderTarget[ 0 ].DestinationBlend = BlendOption.One;
				BlendStateAdd.RenderTarget[ 0 ].BlendOperation = BlendOperation.Add;

				// ブレンドステートを作成する。
				テクスチャ._BlendState加算合成 = new BlendState( gd.D3DDevice, BlendStateAdd );
			}
			//----------------
			#endregion

			#region " ラスタライザステートを生成する。"
			//----------------
			{
				var RSDesc = new RasterizerStateDescription() {
					FillMode = FillMode.Solid,   // 普通に描画する
					CullMode = CullMode.None,    // 両面を描画する
					IsFrontCounterClockwise = false,    // 時計回りが表面
					DepthBias = 0,
					DepthBiasClamp = 0,
					SlopeScaledDepthBias = 0,
					IsDepthClipEnabled = true,
					IsScissorEnabled = false,
					IsMultisampleEnabled = false,
					IsAntialiasedLineEnabled = false,
				};

				テクスチャ._RasterizerState = new RasterizerState( gd.D3DDevice, RSDesc );
			}
			//----------------
			#endregion

			#region " サンプラーステートを生成する。"
			//----------------
			{
				var descSampler = new SamplerStateDescription() {
					Filter = Filter.Anisotropic,
					AddressU = TextureAddressMode.Border,
					AddressV = TextureAddressMode.Border,
					AddressW = TextureAddressMode.Border,
					MipLodBias = 0.0f,
					MaximumAnisotropy = 2,
					ComparisonFunction = Comparison.Never,
					BorderColor = new RawColor4( 0f, 0f, 0f, 0f ),
					MinimumLod = float.MinValue,
					MaximumLod = float.MaxValue,
				};

				テクスチャ._SamplerState = new SamplerState( gd.D3DDevice, descSampler );
			}
			//----------------
			#endregion
		}

		public static void 全インスタンスで共有するリソースを解放する()
		{
			FDKUtilities.解放する( ref テクスチャ._SamplerState );
			FDKUtilities.解放する( ref テクスチャ._RasterizerState );
			FDKUtilities.解放する( ref テクスチャ._BlendState加算合成 );
			FDKUtilities.解放する( ref テクスチャ._BlendState通常合成 );
			FDKUtilities.解放する( ref テクスチャ._PixelShader );
			FDKUtilities.解放する( ref テクスチャ._VertexShader );
		}


		private static VertexShader _VertexShader = null;

		private static PixelShader _PixelShader = null;

		private static BlendState _BlendState通常合成 = null;

		private static BlendState _BlendState加算合成 = null;

		private static RasterizerState _RasterizerState = null;

		private static SamplerState _SamplerState = null;
	}
}
