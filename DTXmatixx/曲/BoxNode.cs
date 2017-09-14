﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

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

		public BoxNode( string BOX定義ファイルパス, Node 親ノード )
		{
			var box = BoxDef.復元する( BOX定義ファイルパス );

			this.タイトル = box.TITLE;
			this.親ノード = 親ノード;
		}
	}
}
