using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SharpDX;
using SharpDX.Direct2D1;

namespace FDK.メディア
{
	/// <summary>
	///		任意個の文字を格納した一枚の画像と、それぞれの文字領域の矩形リストから、文字列を連続するD2D画像で表示する。
	/// </summary>
	public class 画像フォント : Activity
	{
		/// <summary>
		///		それぞれの文字矩形の幅に加算する補正値。
		/// </summary>
		public float 文字幅補正dpx { get; set; }

		/// <summary>
		///		透明: 0 ～ 1 :不透明
		/// </summary>
		public float 不透明度 { get; set; }

		public 画像フォント( string 文字盤の画像ファイルパス, string 文字矩形リストファイルパス, float 文字幅補正dpx = 0f, float 不透明度 = 1f )
			: this( 文字盤の画像ファイルパス, new 矩形リスト( 文字矩形リストファイルパス ), 文字幅補正dpx, 不透明度 )
		{
		}
		public 画像フォント( string 文字盤の画像ファイルパス, 矩形リスト 文字矩形リスト, float 文字幅補正dpx = 0f, float 不透明度 = 1f )
		{
			this.子リスト.Add( this._文字盤 = new 画像( 文字盤の画像ファイルパス ) );
			this._文字矩形リスト = 文字矩形リスト;
			this.文字幅補正dpx = 文字幅補正dpx;
			this.不透明度 = 不透明度;
		}

		/// <param name="基点のX位置">左揃えなら左端位置、右揃えなら右端位置のX座標。</param>
		/// <param name="右揃え">trueなら右揃え、falseなら左揃え。</param>
		public void 描画する( グラフィックデバイス gd, float 基点のX位置, float 上位置, string 表示文字列, bool 右揃え = false )
		{
			if( 表示文字列.Nullまたは空である() )
				return;

			var 文字列全体のサイズ = SharpDX.Size2F.Empty;

			#region " 有効文字（矩形リストに登録されている文字）の矩形、文字数を抽出し、文字列全体のサイズを計算する。"
			//----------------
			var 有効文字矩形リスト =
				from 文字 in 表示文字列
				where ( this._文字矩形リスト.文字列to矩形.ContainsKey( new string( new char[] { 文字 } ) ) )
				select this._文字矩形リスト.文字列to矩形[ new string( new char[] { 文字 } ) ];

			int 有効文字数 = 有効文字矩形リスト.Count();
			if( 0 == 有効文字数 )
				return;

			foreach( var 文字矩形 in 有効文字矩形リスト )
			{
				文字列全体のサイズ.Width += ( 文字矩形.Width + this.文字幅補正dpx );

				if( 文字列全体のサイズ.Height < 文字矩形.Height )
					文字列全体のサイズ.Height = 文字矩形.Height;  // 文字列全体の高さは、最大の文字高に一致。
			}
			//----------------
			#endregion

			// 描画する。
			if( 右揃え )
			{
				基点のX位置 -= 文字列全体のサイズ.Width;
			}
			for( int i = 0; i < 有効文字数; i++ )
			{
				var 文字矩形 = 有効文字矩形リスト.ElementAt( i );

				this._文字盤.描画する( gd, 基点のX位置, 上位置 + ( 文字列全体のサイズ.Height - 文字矩形.Height ), 転送元矩形: 文字矩形, 不透明度0to1: this.不透明度 );

				基点のX位置 += ( 文字矩形.Width + this.文字幅補正dpx );
			}
		}
		public void 描画する( DeviceContext dc, float 基点のX位置, float 上位置, string 表示文字列, bool 右揃え = false )
		{
			if( 表示文字列.Nullまたは空である() )
				return;

			var 文字列全体のサイズ = SharpDX.Size2F.Empty;

			#region " 有効文字（矩形リストに登録されている文字）の矩形、文字数を抽出し、文字列全体のサイズを計算する。"
			//----------------
			var 有効文字矩形リスト =
				from 文字 in 表示文字列
				where ( this._文字矩形リスト.文字列to矩形.ContainsKey( new string( new char[] { 文字 } ) ) )
				select this._文字矩形リスト.文字列to矩形[ new string( new char[] { 文字 } ) ];

			int 有効文字数 = 有効文字矩形リスト.Count();
			if( 0 == 有効文字数 )
				return;

			foreach( var 文字矩形 in 有効文字矩形リスト )
			{
				文字列全体のサイズ.Width += ( 文字矩形.Width + this.文字幅補正dpx );

				if( 文字列全体のサイズ.Height < 文字矩形.Height )
					文字列全体のサイズ.Height = 文字矩形.Height;  // 文字列全体の高さは、最大の文字高に一致。
			}
			//----------------
			#endregion

			// 描画する。
			if( 右揃え )
			{
				基点のX位置 -= 文字列全体のサイズ.Width;
			}
			for( int i = 0; i < 有効文字数; i++ )
			{
				var 文字矩形 = 有効文字矩形リスト.ElementAt( i );

				dc.DrawBitmap(
					this._文字盤.Bitmap,
					new RectangleF( 基点のX位置, 上位置 + ( 文字列全体のサイズ.Height - 文字矩形.Height ), 文字矩形.Width, 文字矩形.Height ),
					this.不透明度,
					InterpolationMode.Linear,
					文字矩形,
					null );

				基点のX位置 += ( 文字矩形.Width + this.文字幅補正dpx );
			}
		}

		private 画像 _文字盤 = null;

		private 矩形リスト _文字矩形リスト = null;
	}
}
