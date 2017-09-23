using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using SSTFormat.v3;

namespace DTXmatixx.曲
{
	/// <summary>
	///		BOX定義。
	/// </summary>
	class BoxDef
	{
		public string TITLE
		{
			get;
			set;
		} = null;

		public string ARTIST
		{
			get;
			set;
		} = null;


		public BoxDef()
		{
		}

		public void 保存する( string Box定義ファイルパス )
		{
			// todo: 現在の内容を box.def に保存する。
		}

		public static BoxDef 復元する( string Box定義ファイルパス )
		{
			var boxDef = new BoxDef();

			using( var sr = new StreamReader( Box定義ファイルパス ) )
			{
				string 行;
				while( ( 行 = sr.ReadLine() ) != null )
				{
					try
					{
						string パラメータ = "";

						#region " TITLE コマンド "
						//---------------------
						if( スコア.コマンドのパラメータ文字列部分を返す( 行, @"TITLE", out パラメータ ) )
						{
							boxDef.TITLE = パラメータ;
							continue;
						}
						//---------------------
						#endregion

						#region " ARTIST コマンド "
						//---------------------
						if( スコア.コマンドのパラメータ文字列部分を返す( 行, @"ARTIST", out パラメータ ) )
						{
							boxDef.ARTIST = パラメータ;
							continue;
						}
						//---------------------
						#endregion
					}
					catch
					{
						// 例外は無視。
					}
				}
			}

			return boxDef;
		}
	}
}
