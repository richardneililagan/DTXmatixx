using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SharpDX;
using FDK;
using FDK.メディア;
using FDK.カウンタ;

namespace DTXmatixx.ステージ.演奏
{
	class フェーズパネル : Activity
	{
		/// <summary>
		///		現在の位置を 開始点:0～1:終了点 で示す。
		/// </summary>
		public float 現在位置
		{
			get
				=> this._現在位置;

			set
				=> this._現在位置 = Math.Min( Math.Max( 0.0f, value ), 1.0f );
		}

		public フェーズパネル()
		{
			this.子リスト.Add( this._演奏位置カーソル画像 = new 画像( @"$(System)images\演奏位置カーソル.png" ) );
		}

		protected override void On活性化( グラフィックデバイス gd )
		{
			using( Log.Block( FDKUtilities.現在のメソッド名 ) )
			{
				this._現在位置 = 0.0f;
				this._演奏位置カーソルの矩形リスト = new 矩形リスト( @"$(System)images\演奏位置カーソル矩形.xml" );
				this._初めての進行描画 = true;
			}
		}
		protected override void On非活性化( グラフィックデバイス gd )
		{
			using( Log.Block( FDKUtilities.現在のメソッド名 ) )
			{
			}
		}

		public void 進行描画する( グラフィックデバイス gd )
		{
			if( this._初めての進行描画 )
			{
				this._左右三角アニメ用カウンタ = new LoopCounter( 0, 100, 5 );
				this._初めての進行描画 = false;
			}

			var 中央位置dpx = new Vector2( 1308f, 876f - this._現在位置 * 767f );

			var バー矩形 = (RectangleF) this._演奏位置カーソルの矩形リスト[ "Bar" ];
			this._演奏位置カーソル画像.描画する(
				gd,
				中央位置dpx.X - バー矩形.Width / 2f,
				中央位置dpx.Y - バー矩形.Height / 2f,
				転送元矩形: バー矩形 );

			var 左三角矩形 = (RectangleF) this._演奏位置カーソルの矩形リスト[ "Left" ];
			this._演奏位置カーソル画像.描画する( 
				gd,
				中央位置dpx.X - 左三角矩形.Width / 2f - this._左右三角アニメ用カウンタ.現在値の割合 * 40f,
				中央位置dpx.Y - 左三角矩形.Height / 2f,
				転送元矩形: 左三角矩形 );

			var 右三角矩形 = (RectangleF) this._演奏位置カーソルの矩形リスト[ "Right" ];
			this._演奏位置カーソル画像.描画する(
				gd,
				中央位置dpx.X - 右三角矩形.Width / 2f + this._左右三角アニメ用カウンタ.現在値の割合 * 40f,
				中央位置dpx.Y - 右三角矩形.Height / 2f,
				転送元矩形: 右三角矩形 );

		}

		private bool _初めての進行描画 = true;
		private float _現在位置 = 0.0f;
		private 画像 _演奏位置カーソル画像 = null;
		private 矩形リスト _演奏位置カーソルの矩形リスト = null;
		private LoopCounter _左右三角アニメ用カウンタ = null;
	}
}
