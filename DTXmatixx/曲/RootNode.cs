using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace DTXmatixx.曲
{
	/// <summary>
	///		曲ツリー階層においてルートを表すノード。
	/// </summary>
	/// <remarks>
	///		ルートノードは曲ツリーの最初の階層のためのプレイスホルダであり、その親ノードは null である。
	///		逆に、最初の階層の全ノードの親ノードは、すべてこのルートノードを示す。
	/// </remarks>
	class RootNode : Node
	{
		public RootNode()
		{
			this.タイトル = "(Root)";
			this.親ノード = null;
		}
	}
}
