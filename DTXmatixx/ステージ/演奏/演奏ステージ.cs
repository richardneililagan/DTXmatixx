using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SharpDX;
using SharpDX.Direct2D1;
using SharpDX.DirectInput;
using FDK;
using FDK.メディア;
using FDK.カウンタ;
using DTXmatixx.曲;

namespace DTXmatixx.ステージ.演奏
{
	class 演奏ステージ : ステージ
	{
		public enum フェーズ
		{
			フェードイン,
			表示,
			キャンセル,
		}
		public フェーズ 現在のフェーズ
		{
			get;
			protected set;
		}

		public Bitmap キャプチャ画面
		{
			get;
			set;
		} = null;

		public 演奏ステージ()
		{
			this.子リスト.Add( this._背景画像 = new 画像( @"$(System)images\演奏画面.jpg" ) );
			this.子リスト.Add( this._曲名パネル = new 曲名パネル() );
			this.子リスト.Add( this._ステータスパネル = new ステータスパネル() );
		}

		protected override void On活性化( グラフィックデバイス gd )
		{
			using( Log.Block( FDKUtilities.現在のメソッド名 ) )
			{
				this.キャプチャ画面 = null;

				this.現在のフェーズ = フェーズ.フェードイン;
				this._初めての進行描画 = true;
			}
		}
		protected override void On非活性化( グラフィックデバイス gd )
		{
			using( Log.Block( FDKUtilities.現在のメソッド名 ) )
			{
				this.キャプチャ画面?.Dispose();
				this.キャプチャ画面 = null;
			}
		}

		public override void 進行描画する( グラフィックデバイス gd )
		{
			// 進行描画

			if( this._初めての進行描画 )
			{
				this._フェードインカウンタ = new Counter( 0, 100, 10 );
				this._初めての進行描画 = false;
			}

			this._背景画像.描画する( gd, 0f, 0f );
			this._曲名パネル.描画する( gd );
			this._ステータスパネル.描画する( gd );

			switch( this.現在のフェーズ )
			{
				case フェーズ.フェードイン:
					this._キャプチャ画面を描画する( gd, ( 1.0f - this._フェードインカウンタ.現在値の割合 ) );
					if( this._フェードインカウンタ.終了値に達した )
						this.現在のフェーズ = フェーズ.表示;
					break;

				case フェーズ.表示:
					break;

				case フェーズ.キャンセル:
					break;
			}


			// 入力

			App.Keyboard.ポーリングする();

			if( App.Keyboard.キーが押された( 0, Key.Escape ) )
			{
				this.現在のフェーズ = フェーズ.キャンセル;
			}
		}

		private bool _初めての進行描画 = true;
		private 画像 _背景画像 = null;
		private 曲名パネル _曲名パネル = null;
		private ステータスパネル _ステータスパネル = null;
		/// <summary>
		///		読み込み画面: 0 ～ 1: 演奏画面
		/// </summary>
		private Counter _フェードインカウンタ = null;

		private void _キャプチャ画面を描画する( グラフィックデバイス gd, float 不透明度 = 1.0f )
		{
			Debug.Assert( null != this.キャプチャ画面, "キャプチャ画面が設定されていません。" );

			gd.D2DBatchDraw( ( dc ) => {
				dc.DrawBitmap(
					this.キャプチャ画面,
					new RectangleF( 0f, 0f, gd.設計画面サイズ.Width, gd.設計画面サイズ.Height ),
					不透明度,
					BitmapInterpolationMode.Linear );
			} );
		}
	}
}
