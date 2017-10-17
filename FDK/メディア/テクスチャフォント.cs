using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SharpDX;

namespace FDK.メディア
{
	/// <summary>
	///		任意個の文字を格納した一枚のテクスチャ画像と、それぞれの文字領域の矩形リストから、
	///		文字列を連続するテクスチャ画像で表示する。
	/// </summary>
	public class テクスチャフォント : Activity
	{
		/// <summary>
		///		コンストラクタ。
		///		指定された画像ファイルと矩形リストファイルを使って、テクスチャフォントを生成する。
		/// </summary>
		public テクスチャフォント( string 文字盤の画像ファイルパス, string 文字矩形リストファイルパス )
			: this( 文字盤の画像ファイルパス, new 矩形リスト( 文字矩形リストファイルパス ) )
		{
		}

		/// <summary>
		///		コンストラクタ。
		///		指定された画像ファイルと矩形リストを使って、テクスチャフォントを生成する。
		/// </summary>
		public テクスチャフォント( string 文字盤の画像ファイルパス, 矩形リスト 文字矩形リスト )
		{
			this.子リスト.Add( this._文字盤 = new テクスチャ( 文字盤の画像ファイルパス ) );
			this._文字矩形リスト = 文字矩形リスト;
		}

		/// <param name="文字列全体のワールド変換行列">
		///		スケーリングは、等倍にした「あと」の拡大縮小率を指定すること。
		///	</param>
		public void 描画する( グラフィックデバイス gd, string 表示文字列, Matrix 文字列全体のワールド変換行列 )
		{
			if( 表示文字列.Nullまたは空である() )
				return;

			// 有効文字（矩形リストに登録されている文字）の矩形、文字数を抽出し、文字列全体のサイズを計算する。
			var 有効文字矩形s =
				from 文字 in 表示文字列
				where ( this._文字矩形リスト.文字列to矩形.ContainsKey( new string( new char[] { 文字 } ) ) )
				select this._文字矩形リスト.文字列to矩形[ new string( new char[] { 文字 } ) ];

			int 有効文字数 = 有効文字矩形s.Count();
			if( 0 == 有効文字数 )
				return;

			var 文字列全体のサイズ = Size2F.Empty;
			foreach( var 文字矩形 in 有効文字矩形s )
			{
				文字列全体のサイズ.Width += 文字矩形.Width;

				// 文字列全体の高さは、最大の文字高に一致。
				if( 文字列全体のサイズ.Height < 文字矩形.Height )
				{
					文字列全体のサイズ.Height = 文字矩形.Height;
				}
			}

			// 描画する。
			float 左端 = 0f;
			for( int i = 0; i < 有効文字数; i++ )
			{
				var 文字矩形 = 有効文字矩形s.ElementAt( i );

				float 中央X = 0f - ( 文字列全体のサイズ.Width / 2f ) + 左端 + ( 文字矩形.Width / 2f );
				var world = Matrix.Scaling( 文字列全体のサイズ.Width, 文字列全体のサイズ.Height, 1f )
					* Matrix.Translation( 中央X, 0f, 0f )
					* 文字列全体のワールド変換行列;

				this._文字盤.描画する( gd, world, 文字矩形 );

				左端 += 文字矩形.Width;
			}
		}


		private テクスチャ _文字盤 = null;

		private 矩形リスト _文字矩形リスト = null;
	}
}
