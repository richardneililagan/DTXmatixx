using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SharpDX;
using FDK;
using FDK.メディア;
using FDK.カウンタ;
using DTXmatixx.設定;

namespace DTXmatixx.ステージ.認証
{
	class ユーザリスト : Activity
	{
		/// <summary>
		///		現在選択中のユーザ。
		///		0 ～ App.ユーザ管理.ユーザリスト.Count-1。
		/// </summary>
		public int 選択中のユーザ
		{
			get;
			protected set;
		} = 0;

		public ユーザリスト()
		{
			this.子リスト.Add( this._ユーザパネル = new 画像( @"$(System)images\認証画面_パネル.png" ) );
			this.子リスト.Add( this._ユーザパネル光彩付き = new 画像( @"$(System)images\認証画面_パネル光彩あり.png" ) );
			this.子リスト.Add( this._ユーザ肩書きパネル = new 画像( @"$(System)images\認証画面_肩書きパネル.png" ) );
			this.子リスト.Add( this._ユーザ名 = new 文字列画像() {
				表示文字列 = "",
				フォントサイズpt = 46f,
				描画効果 = 文字列画像.効果.縁取り,
				縁のサイズdpx = 6f,
				前景色 = Color4.Black,
				背景色 = Color4.White,
			} );
		}

		protected override void On活性化( グラフィックデバイス gd )
		{
			using( Log.Block( FDKUtilities.現在のメソッド名 ) )
			{
				this._光彩アニメカウンタ = new LoopCounter( 0, 200, 5 );
			}
		}
		protected override void On非活性化( グラフィックデバイス gd )
		{
			using( Log.Block( FDKUtilities.現在のメソッド名 ) )
			{
			}
		}

		public void 前のユーザを選択する()
			=> this.選択中のユーザ = ( this.選択中のユーザ - 1 + App.ユーザ管理.ユーザリスト.Count ) % App.ユーザ管理.ユーザリスト.Count;
		public void 次のユーザを選択する()
			=> this.選択中のユーザ = ( this.選択中のユーザ + 1 ) % App.ユーザ管理.ユーザリスト.Count;

		public void 進行描画する( グラフィックデバイス gd )
		{
			var 描画位置 = new Vector2( 569f, 188f );
			float リストの改行幅 = 160f;

			// 選択中のパネルの光彩アニメーションの進行。
			float 不透明度 = 0f;
			var 割合 = this._光彩アニメカウンタ.現在値の割合;
			if( 0.5f > 割合 )
			{
				不透明度 = ( 割合 * 2.0f );		// 0→1
			}
			else
			{
				不透明度 = 1.0f - ( 割合 - 0.5f ) * 2.0f;		// 1→0
			}

			// ユーザリストを描画する。
			
			// hack: 現状は最大５人までとする。
			int 表示人数 = Math.Min( 5, App.ユーザ管理.ユーザリスト.Count );

			for( int i = 0; i < 表示人数; i++ )
			{
				if( i == this.選択中のユーザ )
					this._ユーザパネル光彩付き.描画する( gd, 描画位置.X, 描画位置.Y + リストの改行幅 * i, 不透明度0to1: 不透明度 );

				this._ユーザパネル.描画する( gd, 描画位置.X, 描画位置.Y + リストの改行幅 * i );

				this._ユーザ名.表示文字列 = App.ユーザ管理.ユーザリスト[ i ].ユーザ名;
				this._ユーザ名.描画する( gd, 描画位置.X + 32f, 描画位置.Y + 40f + リストの改行幅 * i );

				this._ユーザ肩書きパネル.描画する( gd, 描画位置.X, 描画位置.Y + リストの改行幅 * i );
			}
		}

		private 画像 _ユーザパネル = null;
		private 画像 _ユーザパネル光彩付き = null;
		private 画像 _ユーザ肩書きパネル = null;
		private LoopCounter _光彩アニメカウンタ = null;
		private 文字列画像 _ユーザ名 = null;
	}
}
