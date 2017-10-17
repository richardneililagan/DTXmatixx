using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SharpDX;
using SharpDX.Direct2D1;
using SharpDX.MediaFoundation;
using FDK.カウンタ;
using FDK.同期;

namespace FDK.メディア
{
	public class 動画 : Activity
	{
		public string 動画ファイルパス
		{
			get
			{
				return this._動画ファイルパス;
			}
			protected set
			{
				this._動画ファイルパス = value;
			}
		}

		public Size2F サイズ
		{
			get
			{
				return this._サイズ;
			}
			protected set
			{
				this._サイズ = value;
			}
		}

		public double 長さsec
		{
			get
			{
				return this._長さsec;
			}
			protected set
			{
				this._長さsec = value;
			}
		}

		public bool 加算合成
		{
			get
			{
				return this._加算合成;
			}
			set
			{
				this._加算合成 = value;
			}
		}

		/// <summary>
		///		0:透明 ～ 1:不透明
		/// </summary>
		public float 不透明度
		{
			get
			{
				return this._不透明度;
			}
			set
			{
				this._不透明度 = value;
			}
		}

		public bool 動画がエラーまたは再生を終了した
		{
			get
				=> ( this._動画の再生状態.現在の状態 == TriStateEvent.状態種別.無効 );
		}


		public 動画( string 動画ファイルパス )
		{
			this.動画ファイルパス = Folder.絶対パスに含まれるフォルダ変数を展開して返す( 動画ファイルパス );
			this._キューのサイズ = 3;	// 固定

			this._動画の再生状態 = new TriStateEvent( TriStateEvent.状態種別.OFF );
		}

		public void 再生を開始する( double 開始位置sec = 0.0, bool ループ再生する = false )
		{
			this._ループ再生する = ループ再生する;
			this._動画の再生状態.現在の状態 = TriStateEvent.状態種別.ON;

			// タスクを起動する。
			this._デコードタスク = Task.Factory.StartNew( this._デコードタスクエントリ, (object) 開始位置sec );
			this._デコードタスク起動完了.WaitOne();
		}

		public void 描画する( グラフィックデバイス gd, RectangleF 描画先矩形, float 不透明度0to1 = 1.0f )
		{
			var 変換行列2D =
				Matrix3x2.Scaling( 描画先矩形.Width / this.サイズ.Width, 描画先矩形.Height / this.サイズ.Height )  // スケーリング
				* Matrix3x2.Translation( 描画先矩形.Left, 描画先矩形.Top );  // 平行移動

			this.描画する( gd, 変換行列2D, 不透明度0to1 );
		}

		public void 描画する( グラフィックデバイス gd, Matrix3x2 変換行列, float 不透明度0to1 = 1.0f )
		{
			#region " 終了チェック "
			//----------------
			if( this._動画の再生状態.現在の状態 != TriStateEvent.状態種別.ON )
				return; // エラーまたは再生が修了済み

			// タスクが終わってても、キューにまだフレームが残っている場合がある。
			if( this._デコードタスク.IsCompleted && ( 0 == this._フレームキュー.Count ) )
			{
				this._動画の再生状態.現在の状態 = TriStateEvent.状態種別.無効;
				return;
			}
			//----------------
			#endregion

			this._次のフレームを確認する( out FrameQueueItem フレーム );

			if( null != フレーム )  // 次のフレームがある。
			{
				// (A) 次のフレームが前のフレームより過去 → ループしたので、タイマをリセットする。
				if( ( null != this._最後に表示したフレーム ) &&
					( フレーム.表示時刻sec < this._最後に表示したフレーム.表示時刻sec ) )
				{
					this._再生タイマ.リセットする( QPCTimer.秒をカウントに変換して返す( フレーム.表示時刻sec ) );
					this._次のフレームを表示する( gd, 変換行列, 不透明度0to1 );
				}

				// (B) 次のフレームの表示時刻に達した。
				else if( フレーム.表示時刻sec <= this._再生タイマ.現在のリアルタイムカウントsec )
				{
					this._次のフレームを表示する( gd, 変換行列, 不透明度0to1 );
				}

				// (C) 次のフレームの表示時刻にはまだ達していない。
				else
				{
					this.前のフレームを描画する( gd, 変換行列, 不透明度0to1 );
				}
			}

			// (D) デコードが追い付いてない、またはループせず再生が終わっている。
			else
			{
				// 何も表示しない → 真っ黒画像。デコードが追い付いてないなら点滅するだろう。
			}
		}

		public void 前のフレームを描画する( グラフィックデバイス gd, RectangleF 描画先矩形, float 不透明度0to1 = 1.0f )
		{
			var 変換行列2D =
				Matrix3x2.Scaling( 描画先矩形.Width / this.サイズ.Width, 描画先矩形.Height / this.サイズ.Height )  // スケーリング
				* Matrix3x2.Translation( 描画先矩形.Left, 描画先矩形.Top );  // 平行移動

			this.前のフレームを描画する( gd, 変換行列2D, 不透明度0to1 );
		}

		public void 前のフレームを描画する( グラフィックデバイス gd, Matrix3x2 変換行列, float 不透明度0to1 = 1.0f )
		{
			this._フレームを描画する( gd, 変換行列, this._最後に表示したフレーム, 不透明度0to1 );
		}

		private void _次のフレームを表示する( グラフィックデバイス gd, Matrix3x2 変換行列, float 不透明度0to1 = 1.0f )
		{
			this._次のフレームを取り出す( out FrameQueueItem frame );

			this._最後に表示したフレーム?.Dispose();
			this._最後に表示したフレーム = frame;

			this._フレームを描画する( gd, 変換行列, frame, 不透明度0to1 );
		}

		protected override void On活性化( グラフィックデバイス gd )
		{
			using( Log.Block( FDKUtilities.現在のメソッド名 ) )
			{
				this._デコードタスク = null;  // タスクが起動していないときは null であることを保証する。
				this._デコードタスク起動完了 = new AutoResetEvent( false );
				this._キューが空いた = new ManualResetEvent( true );
				this._デコードタスクを終了せよ = new AutoResetEvent( false );
				this._再生タイマ = new QPCTimer();

				this._デコードタスク用D2DDeviceContext参照 = gd.D2DDeviceContext;
				this._フレームキュー = new Queue<FrameQueueItem>();
				this._最後に表示したフレーム = null;

				// 動画ファイルから、SourceReaderEx, MediaType, WicBitmap を生成する。

				string 変数付きファイルパス = Folder.絶対パスをフォルダ変数付き絶対パスに変換して返す( 動画ファイルパス );   // Log出力用。

				#region " 動画ファイルパスの有効性を確認する。"
				//-----------------
				if( 動画ファイルパス.Nullまたは空である() )
				{
					Log.ERROR( $"動画ファイルパスが null または空文字列です。[{変数付きファイルパス}]" );
					return;
				}
				if( false == File.Exists( 動画ファイルパス ) )
				{
					Log.ERROR( $"動画ファイルが存在しません。[{変数付きファイルパス}]" );
					return;
				}
				//-----------------
				#endregion

				#region " SourceReaderEx を生成する。"
				//-----------------
				try
				{
					using( var 属性 = new MediaAttributes() )
					{
						// DXVAに対応しているGPUの場合には、それをデコードに利用するよう指定する。
						属性.Set( SourceReaderAttributeKeys.D3DManager, gd.DXGIDeviceManager );

						// 追加のビデオプロセッシングを有効にする。
						属性.Set( SourceReaderAttributeKeys.EnableAdvancedVideoProcessing, true );  // 真偽値が bool だったり

						// 追加のビデオプロセッシングを有効にしたら、こちらは無効に。
						属性.Set( SinkWriterAttributeKeys.ReadwriteDisableConverters, 0 );           // int だったり

						// 属性を使って、SourceReaderEx を生成。
						using( var sourceReader = new SourceReader( 動画ファイルパス, 属性 ) )    // パスは URI 扱い
						{
							this._SourceReaderEx = sourceReader.QueryInterface<SourceReaderEx>();
						}
					}
				}
				catch( SharpDXException e )
				{
					Log.ERROR( $"SourceReaderEx の作成に失敗しました。(0x{e.HResult:x8})[{変数付きファイルパス}]" );
					throw;
				}
				//-----------------
				#endregion

				#region " 最初のビデオストリームを選択し、その他のすべてのストリームを非選択にする。"
				//-----------------
				try
				{
					this._SourceReaderEx.SetStreamSelection( SourceReaderIndex.AllStreams, false );
					this._SourceReaderEx.SetStreamSelection( SourceReaderIndex.FirstVideoStream, true );
				}
				catch( SharpDXException e )
				{
					Log.ERROR( $"ストリームの選択に失敗しました。(0x{e.HResult:x8})[{変数付きファイルパス}]" );
					throw;
				}
				//-----------------
				#endregion

				#region " 部分 MediaType を作成し、SourceReaderEx に登録する。"
				//-----------------
				try
				{
					using( var mediaType = new MediaType() )
					{
						// フォーマットは ARGB32 で固定とする。（SourceReaderEx を使わない場合、H264 では ARGB32 が選べないので注意。）
						mediaType.Set( MediaTypeAttributeKeys.MajorType, MediaTypeGuids.Video );
						mediaType.Set( MediaTypeAttributeKeys.Subtype, VideoFormatGuids.Argb32 );

						// 部分メディアタイプを SourceReaderEx にセットする。SourceReaderEx は、必要なデコーダをロードするだろう。
						this._SourceReaderEx.SetCurrentMediaType( SourceReaderIndex.FirstVideoStream, mediaType );
					}
				}
				catch( SharpDXException e )
				{
					Log.ERROR( $"MediaType (Video, ARGB32) の設定または必要なデコーダの読み込みに失敗しました。(0x{e.HResult:x8})[{変数付きファイルパス}]" );
					throw;
				}
				//-----------------
				#endregion

				#region " ビデオストリームが選択されていることを再度保証する。"
				//-----------------
				try
				{
					this._SourceReaderEx.SetStreamSelection( SourceReaderIndex.FirstVideoStream, true );
				}
				catch( SharpDXException e )
				{
					Log.ERROR( $"最初のビデオストリームの選択に失敗しました（MediaType 設定後）。(0x{e.HResult:x8})[{変数付きファイルパス}]" );
					throw;
				}
				//-----------------
				#endregion

				#region " 完全 MediaType と動画の情報を取得する。"
				//-----------------
				try
				{
					this._MediaType = this._SourceReaderEx.GetCurrentMediaType( SourceReaderIndex.FirstVideoStream );
				}
				catch( SharpDXException e )
				{
					Log.ERROR( $"完全メディアタイプの取得に失敗しました。(0x{e.HResult:x8})[{変数付きファイルパス}]" );
					throw;
				}

				// フレームサイズを取得する。
				try
				{
					// 動画の途中でのサイズ変更には対応しない。
					long packedFrameSize = this._MediaType.Get( MediaTypeAttributeKeys.FrameSize );
					this.サイズ = new Size2F( ( packedFrameSize >> 32 ) & 0xFFFFFFFF, ( packedFrameSize ) & 0xFFFFFFFF );
				}
				catch( SharpDXException e )
				{
					Log.ERROR( $"フレームサイズの取得に失敗しました。(0x{e.HResult:x8})[{変数付きファイルパス}]" );
					throw;
				}

				// 動画の長さを取得する。
				try
				{
					this.長さsec = this._SourceReaderEx.GetPresentationAttribute(
						SourceReaderIndex.MediaSource,
						PresentationDescriptionAttributeKeys.Duration
						) / ( 1000.0 * 1000.0 * 10.0 );
				}
				catch( SharpDXException e )
				{
					Log.ERROR( $"動画の長さの取得に失敗しました。(0x{e.HResult:x8})[{変数付きファイルパス}]" );
					throw;
				}
				//-----------------
				#endregion

				this._ストックする();
			}
		}

		protected override void On非活性化( グラフィックデバイス gd )
		{
			using( Log.Block( FDKUtilities.現在のメソッド名 ) )
			{
				#region " デコードタスクが起動していたら、終了する。"
				//----------------
				if( null != this._デコードタスク )
				{
					this._デコードタスクを終了せよ.Set();

					if( false == this._デコードタスク.Wait( 2000 ) )
						Log.WARNING( "デコードタスクの終了待ちがタイムアウトしました。" );

					this._デコードタスク = null;
				}
				//----------------
				#endregion

				FDKUtilities.解放する( ref this._最後に表示したフレーム );
				FDKUtilities.解放する( ref this._MediaType );
				FDKUtilities.解放する( ref this._SourceReaderEx );

				this._キューをクリアする();
				this._フレームキュー = null;
				this._デコードタスク用D2DDeviceContext参照 = null;

				this._キューが空いた.Close();
				this._デコードタスクを終了せよ.Close();
			}
		}


		private string _動画ファイルパス;

		private Size2F _サイズ;

		private double _長さsec = 0.0;

		private bool _加算合成 = false;

		private float _不透明度 = 1.0f;

		private int _キューのサイズ = 0;

		private class FrameQueueItem : IDisposable
		{
			public double 表示時刻sec = 0;
			public Sample Sample = null;	// 変換前。
			public Bitmap Bitmap = null;	// 変換後。Sample だけ Dispose したら Bitmap がえらいことになるので注意（ビデオメモリを共有しているため）。

			public void Dispose()
			{
				FDKUtilities.解放する( ref this.Bitmap );
				FDKUtilities.解放する( ref this.Sample );
			}
		}

		private Queue<FrameQueueItem> _フレームキュー = null;

		private FrameQueueItem _最後に表示したフレーム = null;

		private bool _ループ再生する = false;

		private Task _デコードタスク = null;

		private AutoResetEvent _デコードタスク起動完了 = null;

		private ManualResetEvent _キューが空いた = null;

		private AutoResetEvent _デコードタスクを終了せよ = null;

		private SourceReaderEx _SourceReaderEx = null;

		private MediaType _MediaType = null;

		private DeviceContext1 _デコードタスク用D2DDeviceContext参照 = null; // D2Dはスレッドセーフであること。

		private QPCTimer _再生タイマ = null;

		/// <summary>
		///		OFF:未再生、ON:再生中、無効:エラーまたは再生が終了
		/// </summary>
		private TriStateEvent _動画の再生状態 = null;


		private void _キューをクリアする()
		{
			lock( this._フレームキュー )
			{
				foreach( var frame in this._フレームキュー )
					frame.Dispose();

				this._フレームキュー.Clear();
				this._キューが空いた?.Set();
			}
		}

		private void _次のフレームを確認する( out FrameQueueItem フレーム )
		{
			lock( this._フレームキュー )
			{
				if( ( 0 == this._フレームキュー.Count ) ||
					( null == ( フレーム = this._フレームキュー.Peek() ) ) )    // キューから取り出さない
				{
					フレーム = null;    // キューが空だったか、Peek が一歩遅かった？（ないはずだが
				}
			}
		}

		private void _次のフレームを取り出す( out FrameQueueItem フレーム )
		{
			lock( this._フレームキュー )
			{
				フレーム = null;

				if( 0 < this._フレームキュー.Count )
				{
					フレーム = this._フレームキュー.Dequeue();
				}

				this._キューが空いた.Set();
			}
		}

		private void _フレームを描画する( グラフィックデバイス gd, Matrix3x2 変換行列2D, FrameQueueItem フレーム, float 不透明度 )
		{
			if( null == フレーム )
				return;

			gd.D2DBatchDraw( ( dc ) => {

				dc.Transform = ( 変換行列2D ) * dc.Transform;
				dc.PrimitiveBlend = ( this.加算合成 ) ? PrimitiveBlend.Add : PrimitiveBlend.SourceOver;

				dc.DrawBitmap( フレーム.Bitmap, 不透明度, InterpolationMode.NearestNeighbor );

			} );
		}


		// 以下、デコードタスク用

		private void _デコードタスクエントリ( object obj再生開始位置sec )
		{
			Log.現在のスレッドに名前をつける( "動画デコード" );
			Log.Info( "デコードタスクを起動しました。" );

			var 再生開始位置sec = (double) obj再生開始位置sec;

			if( 0.0 < 再生開始位置sec )
			{
				this._再生位置までストリームを進める( 再生開始位置sec );
			}

			const int EVID_キューが空いた = 0;
			const int EVID_デコードタスクを終了せよ = 1;
			var events = new WaitHandle[ 2 ];
			events[ EVID_キューが空いた ] = this._キューが空いた;
			events[ EVID_デコードタスクを終了せよ ] = this._デコードタスクを終了せよ;

			this._デコードタスク起動完了.Set();

			// デコードループ。
			this._再生タイマ.リセットする( QPCTimer.秒をカウントに変換して返す( 再生開始位置sec ) );
			while( WaitHandle.WaitAny( events ) == EVID_キューが空いた )
			{
				// キューが空いてるので、サンプルを１つデコードする。
				if( this._サンプルをひとつデコードしてフレームをキューへ格納する() )
				{
					// キューがいっぱいになったら、空くまで待つ。
					lock( this._フレームキュー )
					{
						if( _キューのサイズ == this._フレームキュー.Count )
							this._キューが空いた.Reset();   // 次の while で空くのを待つ。
					}
				}
				else
				{
					break;  // エラーあるいはストリーム終了 → デコードタスクを終了する。
				}
			}

			//this._デコードタスク = null;	--> メインスレッド側でスレッド終了時にチェックしてるので、ここでnullにしてはダメ。

			Log.Info( "デコードタスクを終了しました。" );
		}

		/// <returns>
		///		格納できたかスキップした場合は true、エラーあるいはストリーム終了なら false。
		///	</returns>
		private bool _サンプルをひとつデコードしてフレームをキューへ格納する()
		{
			var sample = (Sample) null;
			try
			{
				long サンプルの表示時刻100ns = 0;

				// ソースリーダーから次のサンプルをひとつデコードする。
				sample = this._SourceReaderEx.ReadSample(
					SourceReaderIndex.FirstVideoStream,
					SourceReaderControlFlags.None,
					out int 実ストリーム番号,
					out var ストリームフラグ,
					out サンプルの表示時刻100ns );

				if( ストリームフラグ.HasFlag( SourceReaderFlags.Endofstream ) )  // BOX化コストとか気にしない
				{
					#region " ストリーム終了 "
					//----------------
					if( this._ループ再生する )
					{
						Log.Info( "動画をループ再生します。" );
						this._SourceReaderEx.SetCurrentPosition( 0 );
						return this._サンプルをひとつデコードしてフレームをキューへ格納する();
					}
					else
					{
						Log.Info( "動画の再生を終了します。" );
						return false;
					}
					//----------------
					#endregion
				}
				else if( ストリームフラグ.HasFlag( SourceReaderFlags.Error ) )
				{
					#region " エラー。"
					//----------------
					throw new SharpDXException( Result.Fail );
					//----------------
					#endregion
				}

				//if( サンプルの表示時刻100ns < this.再生タイマ.現在のリアルタイムカウント100ns単位 )
				//	return true;    // もう表示時刻は通り過ぎてるのでスキップする。---> この実装だとループのし始めには常に true になってしまうので却下。

				lock( this._フレームキュー )
				{
					this._フレームキュー.Enqueue( new FrameQueueItem() {
						Sample = sample,
						Bitmap = this._サンプルからビットマップを取得する( sample ),
						表示時刻sec = サンプルの表示時刻100ns / 10_000_000.0,
					} );
				}
				sample = null;  // キューに格納したので、finally で Dispose しない。
			}
			catch( Exception e )
			{
				Log.Info( $"エラーが発生したので、動画の再生を終了します。[{e.Message}]" );
				return false;
			}
			finally
			{
				sample?.Dispose();	// 失敗した場合のため。
			}
			return true;
		}

		private void _再生位置までストリームを進める( double 再生位置sec )
		{
			#region " ストリームがシーク不可なら何もしない。"
			//----------------
			var flags = this._SourceReaderEx.GetPresentationAttribute(
				SourceReaderIndex.MediaSource,
				SourceReaderAttributeKeys.MediaSourceCharacteristics );

			if( ( flags & (int) MediaSourceCharacteristics.CanSeek ) == 0 )
			{
				Log.WARNING( "この動画はシークできないようです。" );
				return;
			}
			//----------------
			#endregion

			// ストリームの再生位置を移動する。

			this._キューをクリアする();

			long 再生位置100ns = (long) ( 再生位置sec * 10_000_000 );
			this._SourceReaderEx.SetCurrentPosition( 再生位置100ns );

			// キーフレームから再生位置100nsまで ReadSample する。

			long サンプルの表示時刻100ns = 0;

			while( サンプルの表示時刻100ns < 再生位置100ns )
			{
				// サンプルを取得。
				var sample = this._SourceReaderEx.ReadSample(
					SourceReaderIndex.FirstVideoStream,
					SourceReaderControlFlags.None,
					out int 実ストリーム番号,
					out var ストリームフラグ,
					out サンプルの表示時刻100ns );

				// 即解放。
				sample?.Dispose();

				if( ストリームフラグ.HasFlag( SourceReaderFlags.Endofstream ) )
				{
					// ストリーム終了。
					return;
				}
				else if( ストリームフラグ.HasFlag( SourceReaderFlags.Error ) )
				{
					// エラー発生。
					Log.ERROR( $"動画の再生位置を移動中に、エラーが発生しました。" );
					return;
				}
			}

			this._ストックする();

			Log.Info( $"動画の再生位置を {再生位置sec}sec へ移動しました。" );
		}

		private void _ストックする()
		{
			for( int i = 0; i < this._キューのサイズ; i++ )
			{
				this._サンプルをひとつデコードしてフレームをキューへ格納する();
			}

			this._キューが空いた.Reset();  // 埋まった
		}

		private unsafe Bitmap _サンプルからビットマップを取得する( Sample Sample )
		{
			var d2dBitmap = (Bitmap) null;

			using( var mediaBuffer = Sample.ConvertToContiguousBuffer() )
			using( var dxgiBuffer = mediaBuffer.QueryInterface<DXGIBuffer>() )
			{
				dxgiBuffer.GetResource( typeof( SharpDX.DXGI.Surface ).GUID, out IntPtr vDxgiSurface );
				using( var dxgiSurface = new SharpDX.DXGI.Surface( vDxgiSurface ) )
				{
					d2dBitmap = new Bitmap( this._デコードタスク用D2DDeviceContext参照, dxgiSurface, new BitmapProperties( new PixelFormat( SharpDX.DXGI.Format.B8G8R8A8_UNorm, AlphaMode.Ignore ) ) );
				}
			}

			return d2dBitmap;
		}
	}
}