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
using FDK.カウンタ;
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

				this._カーソル位置 = 4;
				this._曲リスト全体のY軸移動オフセット = 0;

				this._選択ノードの表示オフセットdpx = null;
				this._選択ノードの表示オフセットのストーリーボード = null;

				this._ノードto曲名画像 = new Dictionary<Node, 文字列画像>();

				this._初めての進行描画 = true;
			}
		}
		protected override void On非活性化( グラフィックデバイス gd )
		{
			using( Log.Block( FDKUtilities.現在のメソッド名 ) )
			{
				foreach( var kvp in this._ノードto曲名画像 )
					kvp.Value.非活性化する( gd );
				this._ノードto曲名画像.Clear();

				this._選択ノードの表示オフセットのストーリーボード?.Abandon();
				FDKUtilities.解放する( ref this._選択ノードの表示オフセットdpx );
				FDKUtilities.解放する( ref this._選択ノードの表示オフセットのストーリーボード );
			}
		}

		public void 進行描画する( グラフィックデバイス gd )
		{
			// 進行

			if( this._初めての進行描画 )
			{
				this._スクロール用カウンタ = new 定間隔進行();     // 生成と同時にカウント開始。
				this._選択ノードのオフセットアニメをリセットする( gd );
				this._初めての進行描画 = false;
			}

			#region " 曲リストの縦方向スクロール進行残があれば進行する。"
			//----------------
			this._スクロール用カウンタ.経過時間の分だけ進行する( 1, () => {

				int オフセットの加減算速度 = 1;

				#region " カーソルが中央から遠いほど速くなるよう、オフセットの加減算速度（絶対値）を計算する。"
				//------------------
				int 距離 = Math.Abs( 4 - this._カーソル位置 );

				if( 2 > 距離 )
					オフセットの加減算速度 = 1;
				else
					オフセットの加減算速度 = 2;
				//------------------
				#endregion

				// オフセット と カーソル位置 を更新する。
				if( ( 4 > this._カーソル位置 ) ||
				  ( ( 4 == this._カーソル位置 ) && ( 0 > this._曲リスト全体のY軸移動オフセット ) ) )
				{
					#region " (A) パネルは、上から下へ、移動する。"
					//-----------------
					this._曲リスト全体のY軸移動オフセット += オフセットの加減算速度;

					// １行分移動した
					if( 100 <= this._曲リスト全体のY軸移動オフセット )
					{
						this._曲リスト全体のY軸移動オフセット -= 100;  // 0 付近に戻る
						this._カーソル位置++;
					}
					//-----------------
					#endregion
				}
				else if( ( 4 < this._カーソル位置 ) ||
					   ( ( 4 == this._カーソル位置 ) && ( 0 < this._曲リスト全体のY軸移動オフセット ) ) )
				{
					#region " (B) パネルは、下から上へ、移動する。"
					//-----------------
					this._曲リスト全体のY軸移動オフセット -= オフセットの加減算速度;

					// １行分移動した
					if( -100 >= this._曲リスト全体のY軸移動オフセット )
					{
						this._曲リスト全体のY軸移動オフセット += 100;  // 0 付近に戻る
						this._カーソル位置--;
					}
					//-----------------
					#endregion
				}

			} );
			//----------------
			#endregion


			// 描画

			// 現在のフォーカスノードを取得。
			var 描画するノード = App.曲ツリー.フォーカスノード;
			if( null == 描画するノード )
			{
				// todo: ノードが１つもない旨を表示する。
				return;
			}

			// 表示する最上行のノードまで戻る。
			for( int i = 0; i < this._カーソル位置; i++ )
				描画するノード = 描画するノード.前のノード;

			// 10行描画。
			for( int i = 0; i < 10; i++ )
			{
				this._ノードを描画する( gd, i, 描画するノード );
				描画するノード = 描画するノード.次のノード;
			}
		}

		public void 前のノードを選択する( グラフィックデバイス gd )
		{
			this._カーソル位置--;		// 下限なし
			App.曲ツリー.前のノードをフォーカスする();
			this._選択ノードのオフセットアニメをリセットする( gd );
		}
		public void 次のノードを選択する( グラフィックデバイス gd )
		{
			this._カーソル位置++;		// 上限なし
			App.曲ツリー.次のノードをフォーカスする();
			this._選択ノードのオフセットアニメをリセットする( gd );
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
			bool 選択ノードである = ( 4 == 行番号 );

			Debug.Assert( null != ノード画像 );
			Debug.Assert( ( 0.0f != ノード画像.サイズ.Width ) && ( 0.0f != ノード画像.サイズ.Height ) );    // 面積がゼロでないこと。

			// テクスチャは画面中央が (0,0,0) で、Xは右がプラス方向, Yは上がプラス方向, Zは奥がプラス方向+。

			var 画面左上dpx = new Vector3(	// 3D視点で見る画面左上の座標。
				-gd.設計画面サイズ.Width / 2f,
				+gd.設計画面サイズ.Height / 2f,
				0f );

			var 実数行番号 = 行番号 + ( this._曲リスト全体のY軸移動オフセット / 100f );

			var ノード左上dpx = new Vector3(
				this._曲リストの基準左上隅座標dpx.X + ( ( 選択ノードである ) ? (float) this._選択ノードの表示オフセットdpx.Value : 0f ),
				this._曲リストの基準左上隅座標dpx.Y + ( 実数行番号 * _ノードの高さdpx ),
				0f );

			#region " サムネイル画像 "
			//----------------
			{
				var ノード内サムネイルオフセットdpx = new Vector3( 58f, 4f, 0f );

				var サムネイル表示中央dpx = new Vector3(
					画面左上dpx.X + ノード左上dpx.X + ( this._サムネイル表示サイズdpx.X / 2f ) + ノード内サムネイルオフセットdpx.X,
					画面左上dpx.Y - ノード左上dpx.Y - ( this._サムネイル表示サイズdpx.Y / 2f ) - ノード内サムネイルオフセットdpx.Y,
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
			{
				// 曲名画像が未生成なら生成する。
				if( !( this._ノードto曲名画像.ContainsKey( ノード ) ) )
				{
					var 曲名画像 = new 文字列画像() {
						表示文字列 = ノード.タイトル,
						フォント名 = "メイリオ",
						フォント幅 = FontWeight.Regular,
						フォントスタイル = FontStyle.Normal,
						フォントサイズpt = 40f,
						描画効果 = 文字列画像.効果.縁取り,
						縁のサイズdpx = 6f,
						前景色 = Color4.White,
						背景色 = Color4.Black,
					};
					曲名画像.活性化する( gd );

					this._ノードto曲名画像.Add( ノード, 曲名画像 );
				}

				// 曲名画像を描画する。
				this._ノードto曲名画像[ ノード ].描画する( gd, ノード左上dpx.X + 170f, ノード左上dpx.Y + 30f );
			}
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

		private bool _初めての進行描画 = true;

		/// <summary>
		///		曲リスト（10行分！）の合計表示領域の左上隅の座標。
		///		基準というのは、曲リストがスクロールしていないとき、という意味。
		/// </summary>
		private readonly Vector3 _曲リストの基準左上隅座標dpx = new Vector3( 1065f, 145f - _ノードの高さdpx, 0f );
		private readonly Vector3 _サムネイル表示サイズdpx = new Vector3( 100f, 100f, 0f );
		private const float _ノードの高さdpx = ( 913f / 8f );

		private Dictionary<Node, 文字列画像> _ノードto曲名画像 = new Dictionary<Node, 文字列画像>();

		/// <summary>
		///		静止時は 4 。曲リストがスクロールしているときは、4より大きい整数（下から上にスクロール中）か、
		///		または 4 より小さい整数（上から下にスクロール中）になる。
		/// </summary>
		private int _カーソル位置 = 4;
		private 定間隔進行 _スクロール用カウンタ = null;
		/// <summary>
		///		-100～100。曲リスト全体の表示位置を、負数は 上 へ、正数は 下 へずらす 。（正負と上下の対応に注意。）
		/// </summary>
		private int _曲リスト全体のY軸移動オフセット = 0;

		/// <summary>
		///		選択中の曲ノードエリアを左にずらす度合い。
		///		-50f ～ 0f [dpx] 。
		/// </summary>
		private Variable _選択ノードの表示オフセットdpx = null;
		private Storyboard _選択ノードの表示オフセットのストーリーボード = null;

		private void _選択ノードのオフセットアニメをリセットする( グラフィックデバイス gd )
		{
			this._選択ノードの表示オフセットdpx?.Dispose();
			this._選択ノードの表示オフセットdpx = new Variable( gd.Animation.Manager, initialValue: 0.0 );

			this._選択ノードの表示オフセットのストーリーボード?.Abandon();
			this._選択ノードの表示オフセットのストーリーボード?.Dispose();
			this._選択ノードの表示オフセットのストーリーボード = new Storyboard( gd.Animation.Manager );

			using( var 維持 = gd.Animation.TrasitionLibrary.Constant( 0.15 ) )
			using( var 左へ移動 = gd.Animation.TrasitionLibrary.Linear( 0.07, finalValue: -50f ) )
			{
				this._選択ノードの表示オフセットのストーリーボード.AddTransition( this._選択ノードの表示オフセットdpx, 維持 );
				this._選択ノードの表示オフセットのストーリーボード.AddTransition( this._選択ノードの表示オフセットdpx, 左へ移動 );
			}

			this._選択ノードの表示オフセットのストーリーボード.Schedule( gd.Animation.Timer.Time );
		}
	}
}
