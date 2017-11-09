using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using FDK;
using DTXmatixx.データベース.曲;
using FDK.メディア;
using SharpDX;

namespace DTXmatixx.曲
{
	class SetNode : Node
	{
		/// <summary>
		///		このset.defブロックに登録される、最大５つの曲ノード。
		/// </summary>
		public MusicNode[] MusicNodes = new MusicNode[ 5 ];

		/// <summary>
		///		ノードを表す画像の SetNode 用オーバーライド。
		/// </summary>
		/// <remarks>
		///		このプロパティで返す値には、現在フォーカス中の<see cref="SetNode.MusicNodes"/>のノード画像が優先的に使用される。
		///		<see cref="SetNode.MusicNodes"/>のノード画像が無効（または null）なら、このプロパティで返す値には、set.def と同じ場所にあるthumb画像（または null）が使用される。
		/// </remarks>
		public override テクスチャ ノード画像
		{
			get
			{
				// (1) 現在選択されている MusicNode のノード画像が有効ならそれを返す。
				var 現在の難易度のMusicNode = this.MusicNodes[ App.曲ツリー.フォーカス難易度 ];
				if( null != 現在の難易度のMusicNode?.ノード画像 )
				{
					return 現在の難易度のMusicNode.ノード画像;
				}
				// (2) MusicNode のノード画像が無効なら、SetNode の持つノード画像を返す。
				else
				{
					return this._ノード画像;
				}
			}
			protected set
			{
				this._ノード画像 = value;
			}
		}

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

			// 基点フォルダパス（set.def ファイルと同じ場所）に画像ファイルがあるなら、それをノード画像として採用する。
			var サムネイル画像ファイルパス =
				( from ファイル名 in Directory.GetFiles( 基点フォルダパス.変数なしパス )
				  where _対応するサムネイル画像名.Any( thumbファイル名 => ( Path.GetFileName( ファイル名 ).ToLower() == thumbファイル名 ) )
				  select ファイル名 ).FirstOrDefault();

			if( null != サムネイル画像ファイルパス )
			{
				this.子リスト.Add( this._ノード画像 = new テクスチャ( サムネイル画像ファイルパス ) );
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

		private テクスチャ _ノード画像 = null;
		private readonly string[] _対応するサムネイル画像名 = { "thumb.png", "thumb.bmp", "thumb.jpg", "thumb.jpeg" };
	}
}
