using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Data.Linq.Mapping;

namespace DTXmatixx.曲
{
	[Table( Name = "Songs" )]
	public class Songs
	{
		/// <summary>
		///		一意な ID。
		/// </summary>
		[Column( Name = "id", DbType = "INT", CanBeNull = false, IsPrimaryKey = true )] // Linq で自動増加させたい場合は、IsDbGenerate を指定してはならない。
		public Int32? Id { get; set; } = null;

		/// <summary>
		///		曲譜面ファイルへの絶対パス。
		///		これも一意であるものとする。
		/// </summary>
		[Column( Name = "path", DbType = "NVARCHAR", CanBeNull = false, UpdateCheck = UpdateCheck.Never )]
		public String Path { get; set; }
	}
}
