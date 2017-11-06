using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using FDK;
using FDK.メディア;
using SharpDX;

namespace DTXmatixx.曲
{
	/// <summary>
	///		BOXを表すノード。
	/// </summary>
	/// <remarks>
	///		このインスタンスの<see cref="SST.曲.Node.子ノードリスト"/>メンバに、このBOXに含まれるノードが含まれる。
	/// </remarks>
	class BoxNode : Node
	{
		public BoxNode()
		{
		}

		/// <summary>
		///		box.def ファイルからBOXノードを生成する。
		/// </summary>
		public BoxNode( VariablePath BOX定義ファイルパス, Node 親ノード )
			:this()
		{
			var box = BoxDef.復元する( BOX定義ファイルパス );

			this.タイトル = box.TITLE;
			this.親ノード = 親ノード;
		}

		/// <summary>
		///		タイトル名からBOXノードを生成する。
		/// </summary>
		/// <remarks>
		///		「DTXFiles.」で始まるBOXフォルダの場合はこちらで初期化。
		/// </remarks>
		public BoxNode( string BOX名, Node 親ノード )
			: this()
		{
			this.タイトル = BOX名;
			this.親ノード = 親ノード;
		}
	}
}
