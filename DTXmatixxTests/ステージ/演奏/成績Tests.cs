using Microsoft.VisualStudio.TestTools.UnitTesting;
using DTXmatixx.ステージ.演奏;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace DTXmatixx.ステージ.演奏.Tests
{
	[TestClass()]
	public class 成績Tests
	{
		[TestMethod()]
		public void 判定toヒット数割合Test()
		{
			#region " 実サンプル1 "
			//----------------
			{
				var result = new 成績();
				result.ヒット数を加算する( 判定種別.PERFECT, 48 );
				result.ヒット数を加算する( 判定種別.GREAT, 1 );
				result.ヒット数を加算する( 判定種別.GOOD, 0 );
				result.ヒット数を加算する( 判定種別.OK, 0 );
				result.ヒット数を加算する( 判定種別.MISS, 0 );

				var dic = result.判定toヒット割合;
				Assert.AreEqual( 97, dic[ 判定種別.PERFECT ] );
				Assert.AreEqual( 3, dic[ 判定種別.GREAT ] );
				Assert.AreEqual( 0, dic[ 判定種別.GOOD ] );
				Assert.AreEqual( 0, dic[ 判定種別.OK ] );
				Assert.AreEqual( 0, dic[ 判定種別.MISS ] );
			}
			//----------------
			#endregion
			#region " 実サンプル2 "
			//----------------
			{
				var result = new 成績();
				result.ヒット数を加算する( 判定種別.PERFECT, 49 );
				result.ヒット数を加算する( 判定種別.GREAT, 1 );
				result.ヒット数を加算する( 判定種別.GOOD, 0 );
				result.ヒット数を加算する( 判定種別.OK, 0 );
				result.ヒット数を加算する( 判定種別.MISS, 0 );

				var dic = result.判定toヒット割合;
				Assert.AreEqual( 98, dic[ 判定種別.PERFECT ] );
				Assert.AreEqual( 2, dic[ 判定種別.GREAT ] );
				Assert.AreEqual( 0, dic[ 判定種別.GOOD ] );
				Assert.AreEqual( 0, dic[ 判定種別.OK ] );
				Assert.AreEqual( 0, dic[ 判定種別.MISS ] );
			}
			//----------------
			#endregion
			#region " 実サンプル3 "
			//----------------
			{
				var result = new 成績();
				result.ヒット数を加算する( 判定種別.PERFECT, 90 );
				result.ヒット数を加算する( 判定種別.GREAT, 1 );
				result.ヒット数を加算する( 判定種別.GOOD, 0 );
				result.ヒット数を加算する( 判定種別.OK, 0 );
				result.ヒット数を加算する( 判定種別.MISS, 0 );

				var dic = result.判定toヒット割合;
				Assert.AreEqual( 98, dic[ 判定種別.PERFECT ] );
				Assert.AreEqual( 2, dic[ 判定種別.GREAT ] );
				Assert.AreEqual( 0, dic[ 判定種別.GOOD ] );
				Assert.AreEqual( 0, dic[ 判定種別.OK ] );
				Assert.AreEqual( 0, dic[ 判定種別.MISS ] );
			}
			//----------------
			#endregion
			#region " 実サンプル4 "
			//----------------
			{
				var result = new 成績();
				result.ヒット数を加算する( 判定種別.PERFECT, 90 );
				result.ヒット数を加算する( 判定種別.GREAT, 2 );
				result.ヒット数を加算する( 判定種別.GOOD, 0 );
				result.ヒット数を加算する( 判定種別.OK, 0 );
				result.ヒット数を加算する( 判定種別.MISS, 0 );

				var dic = result.判定toヒット割合;
				Assert.AreEqual( 97, dic[ 判定種別.PERFECT ] );
				Assert.AreEqual( 3, dic[ 判定種別.GREAT ] );
				Assert.AreEqual( 0, dic[ 判定種別.GOOD ] );
				Assert.AreEqual( 0, dic[ 判定種別.OK ] );
				Assert.AreEqual( 0, dic[ 判定種別.MISS ] );
			}
			//----------------
			#endregion
			#region " 実サンプル5 "
			//----------------
			{
				var result = new 成績();
				result.ヒット数を加算する( 判定種別.PERFECT, 148 );
				result.ヒット数を加算する( 判定種別.GREAT, 2 );
				result.ヒット数を加算する( 判定種別.GOOD, 0 );
				result.ヒット数を加算する( 判定種別.OK, 0 );
				result.ヒット数を加算する( 判定種別.MISS, 1 );

				var dic = result.判定toヒット割合;
				Assert.AreEqual( 98, dic[ 判定種別.PERFECT ] );
				Assert.AreEqual( 1, dic[ 判定種別.GREAT ] );
				Assert.AreEqual( 0, dic[ 判定種別.GOOD ] );
				Assert.AreEqual( 0, dic[ 判定種別.OK ] );
				Assert.AreEqual( 1, dic[ 判定種別.MISS ] );
			}
			//----------------
			#endregion
			#region " 実サンプル6 "
			//----------------
			{
				var result = new 成績();
				result.ヒット数を加算する( 判定種別.PERFECT, 883 );
				result.ヒット数を加算する( 判定種別.GREAT, 19 );
				result.ヒット数を加算する( 判定種別.GOOD, 2 );
				result.ヒット数を加算する( 判定種別.OK, 2 );
				result.ヒット数を加算する( 判定種別.MISS, 1 );

				var dic = result.判定toヒット割合;
				Assert.AreEqual( 97, dic[ 判定種別.PERFECT ] );
				Assert.AreEqual( 2, dic[ 判定種別.GREAT ] );
				Assert.AreEqual( 1, dic[ 判定種別.GOOD ] );
				Assert.AreEqual( 0, dic[ 判定種別.OK ] );
				Assert.AreEqual( 0, dic[ 判定種別.MISS ] );
			}
			//----------------
			#endregion
			#region " 実サンプル7 "
			//----------------
			{
				var result = new 成績();
				result.ヒット数を加算する( 判定種別.PERFECT, 1397 );
				result.ヒット数を加算する( 判定種別.GREAT, 36 );
				result.ヒット数を加算する( 判定種別.GOOD, 1 );
				result.ヒット数を加算する( 判定種別.OK, 1 );
				result.ヒット数を加算する( 判定種別.MISS, 2 );

				var dic = result.判定toヒット割合;
				Assert.AreEqual( 97, dic[ 判定種別.PERFECT ] );
				Assert.AreEqual( 2, dic[ 判定種別.GREAT ] );
				Assert.AreEqual( 0, dic[ 判定種別.GOOD ] );
				Assert.AreEqual( 0, dic[ 判定種別.OK ] );
				Assert.AreEqual( 1, dic[ 判定種別.MISS ] );
			}
			//----------------
			#endregion
			#region " 実サンプル8 "
			//----------------
			{
				var result = new 成績();
				result.ヒット数を加算する( 判定種別.PERFECT, 1397 );
				result.ヒット数を加算する( 判定種別.GREAT, 36 );
				result.ヒット数を加算する( 判定種別.GOOD, 1 );
				result.ヒット数を加算する( 判定種別.OK, 1 );
				result.ヒット数を加算する( 判定種別.MISS, 2 );

				var dic = result.判定toヒット割合;
				Assert.AreEqual( 97, dic[ 判定種別.PERFECT ] );
				Assert.AreEqual( 2, dic[ 判定種別.GREAT ] );
				Assert.AreEqual( 0, dic[ 判定種別.GOOD ] );
				Assert.AreEqual( 0, dic[ 判定種別.OK ] );
				Assert.AreEqual( 1, dic[ 判定種別.MISS ] );
			}
			//----------------
			#endregion
			#region " 実サンプル9 "
			//----------------
			{
				var result = new 成績();
				result.ヒット数を加算する( 判定種別.PERFECT, 1055 );
				result.ヒット数を加算する( 判定種別.GREAT, 41 );
				result.ヒット数を加算する( 判定種別.GOOD, 3 );
				result.ヒット数を加算する( 判定種別.OK, 0 );
				result.ヒット数を加算する( 判定種別.MISS, 3 );

				var dic = result.判定toヒット割合;
				Assert.AreEqual( 95, dic[ 判定種別.PERFECT ] );
				Assert.AreEqual( 4, dic[ 判定種別.GREAT ] );
				Assert.AreEqual( 1, dic[ 判定種別.GOOD ] );
				Assert.AreEqual( 0, dic[ 判定種別.OK ] );
				Assert.AreEqual( 0, dic[ 判定種別.MISS ] );
			}
			//----------------
			#endregion
			#region " 実サンプル10 "
			//----------------
			{
				var result = new 成績();
				result.ヒット数を加算する( 判定種別.PERFECT, 403 );
				result.ヒット数を加算する( 判定種別.GREAT, 10 );
				result.ヒット数を加算する( 判定種別.GOOD, 1 );
				result.ヒット数を加算する( 判定種別.OK, 3 );
				result.ヒット数を加算する( 判定種別.MISS, 5 );

				var dic = result.判定toヒット割合;
				Assert.AreEqual( 95, dic[ 判定種別.PERFECT ] );
				Assert.AreEqual( 2, dic[ 判定種別.GREAT ] );
				Assert.AreEqual( 1, dic[ 判定種別.GOOD ] );
				Assert.AreEqual( 1, dic[ 判定種別.OK ] );
				Assert.AreEqual( 1, dic[ 判定種別.MISS ] );
			}
			//----------------
			#endregion
		}
	}
}