using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using FDK;
using DTXmatixx.データベース.曲;
using FDK.メディア;

namespace DTXmatixx.曲
{
	class SetNode : Node
	{
		/// <summary>
		///		このset.defブロックに登録される、最大５つの曲ノード。
		/// </summary>
		public MusicNode[] MusicNodes = new MusicNode[ 5 ];

		public SetNode()
		{
		}
		public SetNode( SetDef.Block block, VariablePath 基点フォルダパス, Node 親ノード )
			: this()
		{
			this.タイトル = block.Title;
			this.親ノード = 親ノード;

			using( var songdb = new SongDB() )
			{
				for( int i = 0; i < 5; i++ )
				{
					this.MusicNodes[ i ] = null;

					if( block.File[ i ].Nullでも空でもない() )
					{
						try
						{
							this.MusicNodes[ i ] = new MusicNode( Path.Combine( 基点フォルダパス.変数なしパス, block.File[ i ] ).ToVariablePath(), this );
							this.難易度[ i ].label = block.Label[ i ];
							this.子ノードリスト.Add( this.MusicNodes[ i ] );

							var song = songdb.Songs.Where( ( r ) => ( r.Path == this.MusicNodes[ i ].曲ファイルパス.変数なしパス ) ).SingleOrDefault();
							this.難易度[ i ].level = ( null != song ) ? (float) song.Level : 0.00f;
						}
						catch
						{
							Log.ERROR( "SetNode 内での MusicNode の生成に失敗しました。" );
						}
					}
				}
			}
		}

		protected override void On活性化( グラフィックデバイス gd )
		{
			foreach( var node in this.MusicNodes )
			{
				if( null != node )
					node.活性化する( gd );
			}

			base.On活性化( gd );
		}
		protected override void On非活性化( グラフィックデバイス gd )
		{
			foreach( var node in this.MusicNodes )
			{
				if( null != node )
					node.非活性化する( gd );
			}

			base.On非活性化( gd );
		}
	}
}
