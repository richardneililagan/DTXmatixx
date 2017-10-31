using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using FDK;

namespace DTXmatixx.曲
{
	class SetNode : Node
	{
		/// <summary>
		///		このset.defブロックに登録される、最大５つの曲ノード。
		/// </summary>
		public (string label, MusicNode music)[] MusicNodes = new(string, MusicNode)[ 5 ];

		public SetNode()
		{
		}
		public SetNode( SetDef.Block block, VariablePath 基点フォルダパス, Node 親ノード )
			: this()
		{
			this.タイトル = block.Title;
			this.親ノード = 親ノード;

			for( int i = 0; i < 5; i++ )
			{
				this.MusicNodes[ i ] = (null, null);

				if( block.File[ i ].Nullでも空でもない() )
				{
					try
					{
						this.MusicNodes[ i ] = (block.Label[ i ], new MusicNode( Path.Combine( 基点フォルダパス.変数なしパス, block.File[ i ] ).ToVariablePath(), 親ノード ));
						this.子リスト.Add( this.MusicNodes[ i ].music );
					}
					catch
					{
						Log.ERROR( "SetNode 内での MusicNode の生成に失敗しました。" );
					}
				}
			}
		}
	}
}
