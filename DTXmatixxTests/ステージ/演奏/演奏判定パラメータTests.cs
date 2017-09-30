using Microsoft.VisualStudio.TestTools.UnitTesting;
using DTXmatixx.ステージ.演奏;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace DTXmatixx.ステージ.演奏.Tests
{
	[TestClass()]
	public class 演奏判定パラメータTests
	{
		[TestMethod()]
		public void ヒット割合を取得するTest()
		{
			#region " 実サンプル1 "
			//----------------
			{
				var param = new 演奏パラメータ( 単体テスト: true );
				param.活性化する( null );
				param.ヒット数を加算する( 判定種別.PERFECT, 48 );
				param.ヒット数を加算する( 判定種別.GREAT, 1 );
				param.ヒット数を加算する( 判定種別.GOOD, 0 );
				param.ヒット数を加算する( 判定種別.OK, 0 );
				param.ヒット数を加算する( 判定種別.MISS, 0 );

				var dic = param.判定toヒット割合;
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
				var param = new 演奏パラメータ( 単体テスト: true );
				param.活性化する( null );
				param.ヒット数を加算する( 判定種別.PERFECT, 49 );
				param.ヒット数を加算する( 判定種別.GREAT, 1 );
				param.ヒット数を加算する( 判定種別.GOOD, 0 );
				param.ヒット数を加算する( 判定種別.OK, 0 );
				param.ヒット数を加算する( 判定種別.MISS, 0 );

				var dic = param.判定toヒット割合;
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
				var param = new 演奏パラメータ( 単体テスト: true );
				param.活性化する( null );
				param.ヒット数を加算する( 判定種別.PERFECT, 90 );
				param.ヒット数を加算する( 判定種別.GREAT, 1 );
				param.ヒット数を加算する( 判定種別.GOOD, 0 );
				param.ヒット数を加算する( 判定種別.OK, 0 );
				param.ヒット数を加算する( 判定種別.MISS, 0 );

				var dic = param.判定toヒット割合;
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
				var param = new 演奏パラメータ( 単体テスト: true );
				param.活性化する( null );
				param.ヒット数を加算する( 判定種別.PERFECT, 90 );
				param.ヒット数を加算する( 判定種別.GREAT, 2 );
				param.ヒット数を加算する( 判定種別.GOOD, 0 );
				param.ヒット数を加算する( 判定種別.OK, 0 );
				param.ヒット数を加算する( 判定種別.MISS, 0 );

				var dic = param.判定toヒット割合;
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
				var param = new 演奏パラメータ( 単体テスト: true );
				param.活性化する( null );
				param.ヒット数を加算する( 判定種別.PERFECT, 148 );
				param.ヒット数を加算する( 判定種別.GREAT, 2 );
				param.ヒット数を加算する( 判定種別.GOOD, 0 );
				param.ヒット数を加算する( 判定種別.OK, 0 );
				param.ヒット数を加算する( 判定種別.MISS, 1 );

				var dic = param.判定toヒット割合;
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
				var param = new 演奏パラメータ( 単体テスト: true );
				param.活性化する( null );
				param.ヒット数を加算する( 判定種別.PERFECT, 883 );
				param.ヒット数を加算する( 判定種別.GREAT, 19 );
				param.ヒット数を加算する( 判定種別.GOOD, 2 );
				param.ヒット数を加算する( 判定種別.OK, 2 );
				param.ヒット数を加算する( 判定種別.MISS, 1 );

				var dic = param.判定toヒット割合;
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
				var param = new 演奏パラメータ( 単体テスト: true );
				param.活性化する( null );
				param.ヒット数を加算する( 判定種別.PERFECT, 1397 );
				param.ヒット数を加算する( 判定種別.GREAT, 36 );
				param.ヒット数を加算する( 判定種別.GOOD, 1 );
				param.ヒット数を加算する( 判定種別.OK, 1 );
				param.ヒット数を加算する( 判定種別.MISS, 2 );

				var dic = param.判定toヒット割合;
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
				var param = new 演奏パラメータ( 単体テスト: true );
				param.活性化する( null );
				param.ヒット数を加算する( 判定種別.PERFECT, 1397 );
				param.ヒット数を加算する( 判定種別.GREAT, 36 );
				param.ヒット数を加算する( 判定種別.GOOD, 1 );
				param.ヒット数を加算する( 判定種別.OK, 1 );
				param.ヒット数を加算する( 判定種別.MISS, 2 );

				var dic = param.判定toヒット割合;
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
				var param = new 演奏パラメータ( 単体テスト: true );
				param.活性化する( null );
				param.ヒット数を加算する( 判定種別.PERFECT, 1055 );
				param.ヒット数を加算する( 判定種別.GREAT, 41 );
				param.ヒット数を加算する( 判定種別.GOOD, 3 );
				param.ヒット数を加算する( 判定種別.OK, 0 );
				param.ヒット数を加算する( 判定種別.MISS, 3 );

				var dic = param.判定toヒット割合;
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
				var param = new 演奏パラメータ( 単体テスト: true );
				param.活性化する( null );
				param.ヒット数を加算する( 判定種別.PERFECT, 403 );
				param.ヒット数を加算する( 判定種別.GREAT, 10 );
				param.ヒット数を加算する( 判定種別.GOOD, 1 );
				param.ヒット数を加算する( 判定種別.OK, 3 );
				param.ヒット数を加算する( 判定種別.MISS, 5 );

				var dic = param.判定toヒット割合;
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