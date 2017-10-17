using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using SharpDX;
using FDK.メディア;

namespace FDK
{
	public class ApplicationForm : SharpDX.Windows.RenderForm, IDisposable
	{
		/// <summary>
		///		ウィンドウの表示モード（全画面 or ウィンドウ）を示す。
		///		true なら全画面モード、false ならウィンドウモードである。
		///		値を set することで、モードを変更することもできる。
		/// </summary>
		/// <remarks>
		///		正確には、「全画面(fullscreen)」ではなく「最大化(maximize)」。
		/// </remarks>
		public bool 全画面モード
		{
			get
				=> this.IsFullscreen;

			set
			{
				Trace.Assert( this._初期化完了 );

				if( value )
				{
					if( !( this.IsFullscreen ) )
					{
						Log.Info( "全画面モードに移行します。" );

						this._ウィンドウモードの情報のバックアップ.clientSize = this.ClientSize;
						this._ウィンドウモードの情報のバックアップ.formBorderStyle = this.FormBorderStyle;

						// (参考) http://www.atmarkit.co.jp/ait/articles/0408/27/news105.html
						this.WindowState = FormWindowState.Normal;
						this.FormBorderStyle = FormBorderStyle.None;
						this.WindowState = FormWindowState.Maximized;

						Cursor.Hide();
						this.IsFullscreen = true;
					}
					else
					{
						// すでに全画面モードなので何もしない。
					}
				}
				else
				{
					if( this.IsFullscreen )
					{
						Log.Info( "ウィンドウモードに移行します。" );

						this.WindowState = FormWindowState.Normal;
						this.ClientSize = this._ウィンドウモードの情報のバックアップ.clientSize;
						this.FormBorderStyle = this._ウィンドウモードの情報のバックアップ.formBorderStyle;

						Cursor.Show();
						this.IsFullscreen = false;
					}
					else
					{
						// すでにウィンドウモードなので何もしない。
					}
				}
			}
		}

		static protected グラフィックデバイス グラフィックデバイス
		{
			get;
			set;
		} = null;


		/// <summary>
		///		初期化処理。
		/// </summary>
		public ApplicationForm( SizeF 設計画面サイズ, SizeF 物理画面サイズ, bool 深度ステンシルを使う = true )
		{
			this.SetStyle( ControlStyles.ResizeRedraw, true );
			this.ClientSize = 物理画面サイズ.ToSize();
			this.MinimumSize = new Size( 640, 360 );
			this.Text = "FDK.ApplicationForm";

			ApplicationForm.グラフィックデバイス = new グラフィックデバイス( this.Handle, 設計画面サイズ, 物理画面サイズ, 深度ステンシルを使う );

			this.UserResized += this.OnUserResize;

			this._初期化完了 = true;
		}

		/// <summary>
		///		終了処理。
		/// </summary>
		public new void Dispose()
		{
			Debug.Assert( this._初期化完了 );

			ApplicationForm.グラフィックデバイス.Dispose();
			ApplicationForm.グラフィックデバイス = null;

			this._初期化完了 = false;

			base.Dispose();
		}

		/// <summary>
		///		メインループ。
		///		派生クラスでオーバーライドすること。
		/// </summary>
		public virtual void Run()
		{
			SharpDX.Windows.RenderLoop.Run( this, () => {

				var gd = ApplicationForm.グラフィックデバイス;

				gd.D3DDeviceを取得する( ( d3dDevice ) => {

					// アニメーションを進行する。
					gd.Animation.進行する();

					// 現在のUIツリーを描画する。
					gd.UIFramework.Render( gd );

					// 全面を黒で塗りつぶすだけのサンプル。
					gd.D2DDeviceContext.BeginDraw();
					gd.D2DDeviceContext.Clear( Color4.Black );
					gd.D2DDeviceContext.EndDraw();

				} );

				gd.SwapChain.Present( 1, SharpDX.DXGI.PresentFlags.None );

			} );
		}


		/// <summary>
		///		コンストラクタでの初期化が終わっていれば true。
		/// </summary>
		protected bool _初期化完了 = false;

		/// <summary>
		///		フォーム生成時のパラメータを編集して返す。
		/// </summary>
		protected override CreateParams CreateParams
		{
			get
			{
				// DWM によってトップウィンドウごとに割り当てられるリダイレクトサーフェスを持たない。（リダイレクトへの画像転送がなくなる分、少し速くなるらしい）
				const int WS_EX_NOREDIRECTIONBITMAP = 0x00200000;

				var cp = base.CreateParams;
				cp.ExStyle |= WS_EX_NOREDIRECTIONBITMAP;
				return cp;
			}
		}

		/// <summary>
		///		ウィンドウサイズが変更された。
		/// </summary>
		protected virtual void OnUserResize( object sender, EventArgs e )
		{
			using( Log.Block( FDKUtilities.現在のメソッド名 ) )
			{
				Log.Info( $"新しいサイズ: {this.ClientSize}" );

				// メインループ（RenderLoop）が始まる前にも数回呼び出されることがあるので、それをはじく。
				if( !( this._初期化完了 ) )
				{
					Log.Info( "アプリケーションの初期化がまだ完了していないので、リサイズ処理をスキップします。" );
					return;
				}

				//if( ApplicationForm.グラフィックデバイス.物理画面サイズ.Equals( this.ClientSize ) )
				//{
				//	Log.Info( "サイズが変更されていないので、リサイズ処理をスキップします。" );
				//	return;
				//}

				// スワップチェーンとその依存リソースを解放し、改めて作成しなおす。
				var gd = ApplicationForm.グラフィックデバイス;

				gd.D3DDeviceを取得する( ( d3dDevice ) => {

					this.スワップチェーンに依存するグラフィックリソースを解放する();

					gd.サイズを変更する( this.ClientSize, d3dDevice );

					this.スワップチェーンに依存するグラフィックリソースを作成する( d3dDevice );
				} );
			}
		}

		protected virtual void スワップチェーンに依存するグラフィックリソースを作成する( SharpDX.Direct3D11.Device d3dDevice )
		{
			// 派生クラスで実装すること。
		}
		protected virtual void スワップチェーンに依存するグラフィックリソースを解放する()
		{
			// 派生クラスで実装すること。
		}

		/// <summary>
		///		ウィンドウを全画面モードにする直前に取得し、
		///		再びウィンドウモードに戻して状態を復元する時に参照する。
		///		（<see cref="全画面モード"/> を参照。）
		/// </summary>
		private (Size clientSize, FormBorderStyle formBorderStyle) _ウィンドウモードの情報のバックアップ;
	}
}
