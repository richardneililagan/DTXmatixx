using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SharpDX;
using SharpDX.Animation;
using SharpDX.Direct2D1;
using SharpDX.DirectInput;
using FDK;
using FDK.メディア;
using DTXmatixx.アイキャッチ;
using DTXmatixx.曲;

namespace DTXmatixx.ステージ.選曲
{
	class 選曲ステージ : ステージ
	{
		public enum フェーズ
		{
			フェードイン,
			表示,
			フェードアウト,
			確定,
			キャンセル,
		}
		public フェーズ 現在のフェーズ
		{
			get;
			protected set;
		}

		public 選曲ステージ()
		{
			this.子リスト.Add( this._舞台画像 = new 舞台画像( @"$(System)images\舞台_暗.jpg" ) );
			this.子リスト.Add( this._曲リスト = new 曲リスト() );
			this.子リスト.Add( this._ステージタイマー = new 画像( @"$(System)images\ステージタイマー.png" ) );
			this.子リスト.Add( this._青い枠 = new 青い枠() );
			this.子リスト.Add( this._選択曲枠ランナー = new 選択曲枠ランナー() );
		}

		protected override void On活性化( グラフィックデバイス gd )
		{
			using( Log.Block( FDKUtilities.現在のメソッド名 ) )
			{
				this._白 = new SolidColorBrush( gd.D2DDeviceContext, Color4.White );
				this._黒 = new SolidColorBrush( gd.D2DDeviceContext, Color4.Black );
				this._黒透過 = new SolidColorBrush( gd.D2DDeviceContext, new Color4( Color3.Black, 0.5f ) );
				this._灰透過 = new SolidColorBrush( gd.D2DDeviceContext, new Color4( 0x80535353 ) );
				this._ソートタブ上色 = new SolidColorBrush( gd.D2DDeviceContext, new Color4( 0xFF121212 ) );
				this._ソートタブ下色 = new SolidColorBrush( gd.D2DDeviceContext, new Color4( 0xFF1f1f1f ) );

				this._上に伸びる導線の長さdpx = null;
				this._左に伸びる導線の長さdpx = null;
				this._プレビュー枠の長さdpx = null;
				this._導線のストーリーボード = null;

				this.現在のフェーズ = フェーズ.フェードイン;
				this._初めての進行描画 = true;
			}
		}
		protected override void On非活性化( グラフィックデバイス gd )
		{
			using( Log.Block( FDKUtilities.現在のメソッド名 ) )
			{
				FDKUtilities.解放する( ref this._上に伸びる導線の長さdpx );
				FDKUtilities.解放する( ref this._左に伸びる導線の長さdpx );
				FDKUtilities.解放する( ref this._プレビュー枠の長さdpx );
				FDKUtilities.解放する( ref this._導線のストーリーボード );
				FDKUtilities.解放する( ref this._白 );
				FDKUtilities.解放する( ref this._黒 );
				FDKUtilities.解放する( ref this._黒透過 );
				FDKUtilities.解放する( ref this._灰透過 );
				FDKUtilities.解放する( ref this._ソートタブ上色 );
				FDKUtilities.解放する( ref this._ソートタブ下色 );
			}
		}

		public override void 進行描画する( グラフィックデバイス gd )
		{
			if( this._初めての進行描画 )
			{
				App.ステージ管理.現在のアイキャッチ.オープンする( gd );
				this._導線アニメをリセットする( gd );
				this._初めての進行描画 = false;
			}

			this._舞台画像.進行描画する( gd );
			this._曲リスト.進行描画する( gd );
			this._その他パネルを描画する( gd );
			this._プレビュー画像を描画する( gd, App.曲ツリー.フォーカスノード );
			this._選択曲を囲む枠を描画する( gd );
			this._選択曲枠ランナー.進行描画する( gd );
			this._導線を描画する( gd );

			App.Keyboard.ポーリングする();

			switch( this.現在のフェーズ )
			{
				case フェーズ.フェードイン:
					App.ステージ管理.現在のアイキャッチ.進行描画する( gd );
					if( App.ステージ管理.現在のアイキャッチ.現在のフェーズ == アイキャッチ.フェーズ.オープン完了 )
						this.現在のフェーズ = フェーズ.表示;
					break;

				case フェーズ.表示:
					if( App.Keyboard.キーが押された( 0, Key.Return ) )
					{
						App.ステージ管理.アイキャッチを選択しクローズする( gd, nameof( GO ) );
						this.現在のフェーズ = フェーズ.フェードアウト;
					}
					else if( App.Keyboard.キーが押された( 0, Key.Up ) )
					{
						//App.曲ツリー.前のノードをフォーカスする();	--> 曲リストへ委譲
						this._曲リスト.前のノードを選択する( gd );
						this._導線アニメをリセットする( gd );
					}
					else if( App.Keyboard.キーが押された( 0, Key.Down ) )
					{
						//App.曲ツリー.次のノードをフォーカスする();	--> 曲リストへ委譲
						this._曲リスト.次のノードを選択する( gd );
						this._導線アニメをリセットする( gd );
					}
					break;

				case フェーズ.フェードアウト:
					App.ステージ管理.現在のアイキャッチ.進行描画する( gd );
					if( App.ステージ管理.現在のアイキャッチ.現在のフェーズ == アイキャッチ.フェーズ.クローズ完了 )
						this.現在のフェーズ = フェーズ.確定;
					break;

				case フェーズ.確定:
				case フェーズ.キャンセル:
					break;
			}
		}

		private bool _初めての進行描画 = true;
		private 舞台画像 _舞台画像 = null;
		private 曲リスト _曲リスト = null;
		private 青い枠 _青い枠 = null;
		private 選択曲枠ランナー _選択曲枠ランナー = null;

		private SolidColorBrush _白 = null;
		private SolidColorBrush _黒 = null;
		private SolidColorBrush _ソートタブ上色 = null;
		private SolidColorBrush _ソートタブ下色 = null;
		private SolidColorBrush _黒透過 = null;
		private SolidColorBrush _灰透過 = null;
		private 画像 _ステージタイマー = null;
		private readonly Vector3 _プレビュー画像表示位置dpx = new Vector3( 471f, 61f, 0f );
		private readonly Vector3 _プレビュー画像表示サイズdpx = new Vector3( 444f, 444f, 0f );

		private void _その他パネルを描画する( グラフィックデバイス gd )
		{
			gd.D2DBatchDraw( ( dc ) => {

				// 曲リストソートタブ
				dc.FillRectangle( new RectangleF( 927f, 50f, 993f, 138f ), this._ソートタブ上色 );
				dc.FillRectangle( new RectangleF( 927f, 142f, 993f, 46f ), this._ソートタブ下色 );

				// インフォメーションバー
				dc.FillRectangle( new RectangleF( 0f, 0f, 1920f, 50f ), this._黒 );
				dc.DrawLine( new Vector2( 0f, 50f ), new Vector2( 1920f, 50f ), this._白, strokeWidth: 1f );

				// ボトムバー
				dc.FillRectangle( new RectangleF( 0f, 1080f - 43f, 1920f, 1080f ), this._黒 );

				// プレビュー領域
				dc.FillRectangle( new RectangleF( 0f, 52f, 927f, 476f ), this._黒透過 );
				dc.DrawRectangle( new RectangleF( 0f, 52f, 927f, 476f ), this._灰透過, strokeWidth: 1f );
				dc.DrawLine( new Vector2( 1f, 442f ), new Vector2( 925f, 442f ), this._灰透過, strokeWidth: 1f );

			} );

			this._ステージタイマー.描画する( gd, 1689f, 37f );
		}
		private void _プレビュー画像を描画する( グラフィックデバイス gd, Node ノード )
		{
			var 画像 = ノード?.ノード画像 ?? Node.既定のノード画像;

			// テクスチャは画面中央が (0,0,0) で、Xは右がプラス方向, Yは上がプラス方向, Zは奥がプラス方向+。

			var 画面左上dpx = new Vector3(
				-gd.設計画面サイズ.Width / 2f,
				+gd.設計画面サイズ.Height / 2f,
				0f );

			var 変換行列 =
				Matrix.Scaling( this._プレビュー画像表示サイズdpx ) *
				Matrix.Translation(
					画面左上dpx.X + this._プレビュー画像表示位置dpx.X + this._プレビュー画像表示サイズdpx.X / 2f,
					画面左上dpx.Y - this._プレビュー画像表示位置dpx.Y - this._プレビュー画像表示サイズdpx.Y / 2f,
					0f );

			画像.描画する( gd, 変換行列 );
		}
		private void _選択曲を囲む枠を描画する( グラフィックデバイス gd )
		{
			var 矩形 = new RectangleF( 1015f, 485f, 905f, 113f );

			this._青い枠.描画する( gd, new Vector2( 矩形.Left - this._青枠のマージンdpx, 矩形.Top ), 幅dpx: 矩形.Width + this._青枠のマージンdpx * 2f );
			this._青い枠.描画する( gd, new Vector2( 矩形.Left - this._青枠のマージンdpx, 矩形.Bottom ), 幅dpx: 矩形.Width + this._青枠のマージンdpx * 2f );
			this._青い枠.描画する( gd, new Vector2( 矩形.Left, 矩形.Top - this._青枠のマージンdpx ), 高さdpx: 矩形.Height + this._青枠のマージンdpx * 2f );
		}

		private Variable _上に伸びる導線の長さdpx = null;
		private Variable _左に伸びる導線の長さdpx = null;
		private Variable _プレビュー枠の長さdpx = null;
		private Storyboard _導線のストーリーボード = null;
		private readonly float _青枠のマージンdpx = 8f;

		private void _導線アニメをリセットする( グラフィックデバイス gd )
		{
			this._選択曲枠ランナー.リセットする();

			this._上に伸びる導線の長さdpx?.Dispose();
			this._上に伸びる導線の長さdpx = new Variable( gd.Animation.Manager, initialValue: 0.0 );

			this._左に伸びる導線の長さdpx?.Dispose();
			this._左に伸びる導線の長さdpx = new Variable( gd.Animation.Manager, initialValue: 0.0 );

			this._プレビュー枠の長さdpx?.Dispose();
			this._プレビュー枠の長さdpx = new Variable( gd.Animation.Manager, initialValue: 0.0 );

			this._導線のストーリーボード?.Abandon();
			this._導線のストーリーボード?.Dispose();
			this._導線のストーリーボード = new Storyboard( gd.Animation.Manager );

			double 期間 = 0.3;
			using( var 上に伸びる = gd.Animation.TrasitionLibrary.Constant( 期間 ) )
			using( var 左に伸びる = gd.Animation.TrasitionLibrary.Constant( 期間 ) )
			using( var 枠が広がる = gd.Animation.TrasitionLibrary.Constant( 期間 ) )
			{
				this._導線のストーリーボード.AddTransition( this._上に伸びる導線の長さdpx, 上に伸びる );
				this._導線のストーリーボード.AddTransition( this._左に伸びる導線の長さdpx, 左に伸びる );
				this._導線のストーリーボード.AddTransition( this._プレビュー枠の長さdpx, 枠が広がる );
			}

			期間 = 0.07;
			using( var 上に伸びる = gd.Animation.TrasitionLibrary.Linear( 期間, finalValue: 209.0 ) )
			using( var 左に伸びる = gd.Animation.TrasitionLibrary.Constant( 期間 ) )
			using( var 枠が広がる = gd.Animation.TrasitionLibrary.Constant( 期間 ) )
			{
				this._導線のストーリーボード.AddTransition( this._上に伸びる導線の長さdpx, 上に伸びる );
				this._導線のストーリーボード.AddTransition( this._左に伸びる導線の長さdpx, 左に伸びる );
				this._導線のストーリーボード.AddTransition( this._プレビュー枠の長さdpx, 枠が広がる );
			}

			期間 = 0.06;
			using( var 上に伸びる = gd.Animation.TrasitionLibrary.Constant( 期間 ) )
			using( var 左に伸びる = gd.Animation.TrasitionLibrary.Linear( 期間, finalValue: 129.0 ) )
			using( var 枠が広がる = gd.Animation.TrasitionLibrary.Constant( 期間 ) )
			{
				this._導線のストーリーボード.AddTransition( this._上に伸びる導線の長さdpx, 上に伸びる );
				this._導線のストーリーボード.AddTransition( this._左に伸びる導線の長さdpx, 左に伸びる );
				this._導線のストーリーボード.AddTransition( this._プレビュー枠の長さdpx, 枠が広がる );
			}

			期間 = 0.07;
			using( var 維持 = gd.Animation.TrasitionLibrary.Constant( 期間 ) )
			using( var 上に伸びる = gd.Animation.TrasitionLibrary.Constant( 期間 ) )
			using( var 左に伸びる = gd.Animation.TrasitionLibrary.Constant( 期間 ) )
			using( var 枠が広がる = gd.Animation.TrasitionLibrary.Linear( 期間, finalValue: 444.0 + this._青枠のマージンdpx * 2f ) )
			{
				this._導線のストーリーボード.AddTransition( this._上に伸びる導線の長さdpx, 上に伸びる );
				this._導線のストーリーボード.AddTransition( this._左に伸びる導線の長さdpx, 左に伸びる );
				this._導線のストーリーボード.AddTransition( this._プレビュー枠の長さdpx, 枠が広がる );
			}

			this._導線のストーリーボード.Schedule( gd.Animation.Timer.Time );
		}
		private void _導線を描画する( グラフィックデバイス gd )
		{
			var h = (float) this._上に伸びる導線の長さdpx.Value;
			this._青い枠.描画する( gd, new Vector2( 1044f, 485f - h ), 高さdpx: h );

			var w = (float) this._左に伸びる導線の長さdpx.Value;
			this._青い枠.描画する( gd, new Vector2( 1046f - w, 278f ), 幅dpx: w );

			var z = (float) this._プレビュー枠の長さdpx.Value;   // マージン×2 込み
			var 上 = this._プレビュー画像表示位置dpx.Y;
			var 下 = this._プレビュー画像表示位置dpx.Y + this._プレビュー画像表示サイズdpx.Y;
			var 左 = this._プレビュー画像表示位置dpx.X;
			var 右 = this._プレビュー画像表示位置dpx.X + this._プレビュー画像表示サイズdpx.X;
			this._青い枠.描画する( gd, new Vector2( 右 + this._青枠のマージンdpx - z, 上 ), 幅dpx: z ); // 上辺
			this._青い枠.描画する( gd, new Vector2( 右 + this._青枠のマージンdpx - z, 下 ), 幅dpx: z ); // 下辺
			this._青い枠.描画する( gd, new Vector2( 左, 下 + this._青枠のマージンdpx - z ), 高さdpx: z ); // 左辺
			this._青い枠.描画する( gd, new Vector2( 右, 下 + this._青枠のマージンdpx - z ), 高さdpx: z ); // 右辺
		}
	}
}
