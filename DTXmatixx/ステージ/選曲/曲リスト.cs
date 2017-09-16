using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SharpDX;
using SharpDX.Animation;
using SharpDX.Direct2D1;
using SharpDX.DirectWrite;
using FDK;
using FDK.メディア;
using DTXmatixx.曲;

namespace DTXmatixx.ステージ.選曲
{
	/// <summary>
	///		曲のリスト表示、選択、スクロール。
	/// </summary>
	/// <remarks>
	///		画面に表示される曲は8行だが、スクロールを勘案して上下に１行ずつ追加し、計10行として扱う。
	/// </remarks>
	class 曲リスト : Activity
	{
		public 曲リスト()
		{
		}
		protected override void On活性化( グラフィックデバイス gd )
		{
			using( Log.Block( FDKUtilities.現在のメソッド名 ) )
			{
				#region " フォーカスノードを初期化する。"
				//----------------
				{
					var tree = App.曲ツリー;

					if( null == tree.フォーカスノード )
					{
						// (A) 未選択なら、ルートノードの先頭ノードをフォーカスする。
						if( 0 < tree.ルートノード.子ノードリスト.Count )
						{
							tree.フォーカスする( gd, tree.ルートノード.子ノードリスト[ 0 ] );
						}
						else
						{
							// ルートノードに子がないないなら null のまま。
						}
					}
					else
					{
						// (B) なんらかのノードを選択中なら、それを継続して使用する（フォーカスノードをリセットしない）。
					}
				}
				//----------------
				#endregion

				this._曲リストのスクロール割合 = new Variable( gd.Animation.Manager, initialValue: 0.0 );
				this._曲名フォーマット = new TextFormat( gd.DWriteFactory, "メイリオ", FontWeight.UltraBlack, FontStyle.Normal, 40.0f );
				this._曲名の色 = new SolidColorBrush( gd.D2DDeviceContext, Color4.White );
			}
		}
		protected override void On非活性化( グラフィックデバイス gd )
		{
			using( Log.Block( FDKUtilities.現在のメソッド名 ) )
			{
				FDKUtilities.解放する( ref this._曲リストのスクロール割合 );
				FDKUtilities.解放する( ref this._曲名の色 );
			}
		}
		public void 進行描画する( グラフィックデバイス gd )
		{
			// 進行


			// 描画

			// 現在のフォーカスノードを取得。
			var 描画するノード = App.曲ツリー.フォーカスノード;
			if( null == 描画するノード )
			{
				// todo: ノードが１つもない旨を表示する。
				return;
			}

			// １行目に対応するノードを検索。
			描画するノード = 描画するノード.前のノード;	// フォーカスノードの４つ前。
			描画するノード = 描画するノード.前のノード;
			描画するノード = 描画するノード.前のノード;
			描画するノード = 描画するノード.前のノード;

			// 10行描画。
			for( int i = 0; i < 10; i++ )
			{
				this._ノードを描画する( gd, i, 描画するノード );
				描画するノード = 描画するノード.次のノード;
			}
		}

		/// <param name="行番号">
		///		一番上:0 ～ 9:一番下。
		///		「静止時の」可視範囲は 1～8。
		///		4 がフォーカスノード。
		///	</param>
		private void _ノードを描画する( グラフィックデバイス gd, int 行番号, Node ノード )
		{
			Debug.Assert( 0 <= 行番号 && 9 >= 行番号 );
			Debug.Assert( null != ノード );
			Debug.Assert( ( ノード as RootNode ) is null );

			switch( ノード )
			{
				case MusicNode node:
					this._曲ノードを描画する( gd, 行番号, node );
					break;

				case BoxNode node:
					this._Boxノードを描画する( gd, 行番号, node );
					break;

				case BackNode node:
					this._戻るノードを描画する( gd, 行番号, node );
					break;
			}
		}

		private void _曲ノードを描画する( グラフィックデバイス gd, int 行番号, MusicNode ノード )
		{
			var ノード画像 = ノード.ノード画像 ?? Node.既定のノード画像;

			Debug.Assert( null != ノード画像 );
			Debug.Assert( ( 0.0f != ノード画像.サイズ.Width ) && ( 0.0f != ノード画像.サイズ.Height ) );    // 面積がゼロでないこと。

			// テクスチャは画面中央が (0,0,0) で、Xは右がプラス方向, Yは上がプラス方向, Zは奥がプラス方向+。

			var 画面左上dpx = new Vector3(
				-gd.設計画面サイズ.Width / 2f,
				+gd.設計画面サイズ.Height / 2f,
				0f );

			var ノード左上dpx = new Vector3(
				画面左上dpx.X + this._曲リストの基準左上隅座標dpx.X,
				画面左上dpx.Y - this._曲リストの基準左上隅座標dpx.Y - ( 行番号 * _ノードの高さdpx ),
				0f );

			#region " サムネイル画像 "
			//----------------
			{
				var ノード内サムネイルオフセットdpx = new Vector3( 58f, 4f, 0f );

				var サムネイル表示中央dpx = new Vector3(
					( ノード左上dpx.X ) + ( this._サムネイル表示サイズdpx.X / 2f ) + ノード内サムネイルオフセットdpx.X,
					( ノード左上dpx.Y ) - ( this._サムネイル表示サイズdpx.Y / 2f ) - ノード内サムネイルオフセットdpx.Y,
					0f );

				var 変換行列 =
					Matrix.Scaling( this._サムネイル表示サイズdpx ) *
					Matrix.Translation( サムネイル表示中央dpx );

				ノード.描画する( gd, 変換行列, キャプション表示: false );  // SST形式のキャプションは非表示
			}
			//----------------
			#endregion
			#region " タイトル文字列 "
			//----------------
			gd.D2DBatchDraw( ( dc ) => {

				dc.DrawText(
					ノード.タイトル,
					this._曲名フォーマット,
					new RectangleF( this._曲リストの基準左上隅座標dpx.X + 170f, this._曲リストの基準左上隅座標dpx.Y + ( 行番号 * _ノードの高さdpx ), 855f - 170f, _ノードの高さdpx ),
					this._曲名の色 );

			} );
			//----------------
			#endregion
			#region " サブ文字列（フォーカスノードのみ）"
			//----------------
			//----------------
			#endregion
		}
		private void _Boxノードを描画する( グラフィックデバイス gd, int 行番号, BoxNode ノード )
		{
			Debug.WriteLine( "Boxノードの表示には未対応です。" );
		}
		private void _戻るノードを描画する( グラフィックデバイス gd, int 行番号, BackNode ノード )
		{
			Debug.WriteLine( "戻るノードの表示には未対応です。" );
		}

		/// <summary>
		///		曲リスト（10行分！）の合計表示領域の左上隅の座標。
		///		基準というのは、曲リストがスクロールしていないとき、という意味。
		/// </summary>
		private readonly Vector3 _曲リストの基準左上隅座標dpx = new Vector3( 1065f, 145f - _ノードの高さdpx, 0f );

		private readonly Vector3 _サムネイル表示サイズdpx = new Vector3( 100f, 100f, 0f );

		private const float _ノードの高さdpx = ( 913f / 8f );

		/// <summary>
		///		0.0 で基準（静止）位置、-1.0 で上に１行、+1.0 で下に１行ずれている状態。
		/// </summary>
		private Variable _曲リストのスクロール割合 = null;

		private TextFormat _曲名フォーマット = null;
		private SolidColorBrush _曲名の色 = null;
	}
}
