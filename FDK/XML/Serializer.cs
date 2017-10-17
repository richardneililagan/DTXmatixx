using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Serialization;

namespace FDK.XML
{
	/// <summary>
	///		XmlSerializer を使った XML 入出力機能を提供する。
	/// </summary>
	/// <remarks>
	///		C# のインスタンスを丸ごと XML に変換・復号できる……　が、制限がいろいろあるので注意。
	///		インスタンスの構造が複雑なら、FDK.XML.ReaderWriter 名前空間を使うほうが楽かも。
	/// </remarks>
	public class Serializer
	{
		public static void インスタンスをシリアライズしてファイルに保存する<T>( string strXMLファイル名, T target )
		{
			var ファイル名 = Folder.絶対パスに含まれるフォルダ変数を展開して返す( strXMLファイル名 );

			using( var sw = new StreamWriter(
				new FileStream( ファイル名, FileMode.Create, FileAccess.Write, FileShare.ReadWrite ),
				Encoding.UTF8 ) )
			{
				new XmlSerializer( typeof( T ) ).Serialize( sw, target );
			}
		}

		public static T ファイルをデシリアライズしてインスタンスを生成する<T>( string strXMLファイル名 )
		{
			var ファイル名 = Folder.絶対パスに含まれるフォルダ変数を展開して返す( strXMLファイル名 );

			using( var sr = new StreamReader(
				new FileStream( ファイル名, FileMode.Open, FileAccess.Read, FileShare.ReadWrite ),
				Encoding.UTF8 ) )
			{
				return ( T ) new XmlSerializer( typeof( T ) ).Deserialize( sr );
			}
		}
	}
}
