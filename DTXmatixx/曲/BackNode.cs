using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using FDK;

namespace DTXmatixx.曲
{
	/// <summary>
	///		曲ツリー階層において「戻る」を表すノード。
	/// </summary>
	class BackNode : Node
	{
		public BackNode()
		{
			this.タイトル = "戻る";
			this.親ノード = null;
		}

		public BackNode( Node 親ノード )
			: this()
		{
			this.親ノード = 親ノード;
		}
	}
}
