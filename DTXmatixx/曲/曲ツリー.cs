using System;	
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using FDK;
using FDK.メディア;

namespace DTXmatixx.曲
{
	/// <summary>
	///		選曲画面で使用される、曲ツリーを管理する。
	///		曲ツリーは、<see cref="ユーザ"/>ごとに１つずつ持つことができる。
	/// </summary>
	class 曲ツリー : Activity, IDisposable
	{
		// オンメモリ

		/// <summary>
		///		曲ツリーのルートを表すノード。
		///		フォーカスリストやフォーカスノードも、このツリーの中に実態がある。
		/// </summary>
		public RootNode ルートノード
		{
			get;
		} = new RootNode();

		/// <summary>
		///		現在選択されているノード。
		/// </summary>
		/// <remarks>
		///		未選択またはフォーカスリストが空の場合は null 。
		///		<see cref="フォーカスリスト"/>の<see cref="SelectableList{T}.SelectedIndex"/>で変更できる。
		///	</remarks>
		public Node フォーカスノード
		{
			get
				=> ( null == this.フォーカスリスト ) ? null : // フォーカスリストが未設定なら null。
				   ( 0 > this.フォーカスリスト.SelectedIndex ) ? null : // フォーカスリストが空なら null。
				   this.フォーカスリスト[ this.フォーカスリスト.SelectedIndex ];
		}

		/// <summary>
		///		フォーカスノードが存在するノードリスト。
		///		変更するには、変更先のリスト内の任意のノードを選択すること。
		/// </summary>
		public SelectableList<Node> フォーカスリスト
		{
			get;
			protected set;
		} = null;

		// 構築・破棄

		public 曲ツリー()
		{
		}
		protected override void On活性化( グラフィックデバイス gd )
		{
			Debug.Assert( this.活性化していない );

			// フォーカスリストを活性化する。
			if( null != this.フォーカスリスト )
			{
				foreach( var node in this.フォーカスリスト )
					node.活性化する( gd );
			}
		}
		protected override void On非活性化( グラフィックデバイス gd )
		{
			Debug.Assert( this.活性化している );

			// フォーカスリストを非活性化する。
			if( null != this.フォーカスリスト )
			{
				foreach( var node in this.フォーカスリスト )
					node.非活性化する( gd );
			}
		}
		public void Dispose()
		{
			this.すべてのノードを削除する();
		}

		/// <remarks>
		///		追加されたノードは、ここでは活性化されない。
		/// </remarks>
		public void 曲を検索して親ノードに追加する( Node 親ノード, string フォルダパス )
		{
			フォルダパス = Folder.絶対パスに含まれるフォルダ変数を展開して返す( フォルダパス );
			var ログ用フォルダパス = Folder.絶対パスをフォルダ変数付き絶対パスに変換して返す( フォルダパス );

			// フォルダが存在しないなら何もしない。
			if( !( Directory.Exists( フォルダパス ) ) )
			{
				Log.WARNING( $"指定されたフォルダが存在しません。無視します。[{ログ用フォルダパス}]" );
				return;
			}

			Log.Info( $"曲検索: {ログ用フォルダパス}" );
			var dirInfo = new DirectoryInfo( フォルダパス );

			// (1) このフォルダにあるすべてのsstfファイルから、曲ノードを作成する。
			foreach( var fileInfo in dirInfo.GetFiles( "*.sstf", SearchOption.TopDirectoryOnly ) )
			{
				親ノード.子ノードリスト.Add( new MusicNode( fileInfo.FullName, 親ノード ) );
			}

			// (2) このフォルダのすべてのサブフォルダについて再帰処理。
			foreach( var subDirInfo in dirInfo.GetDirectories() )
			{
				var boxファイルパス = Path.Combine( subDirInfo.FullName, @"box.def" );
				if( File.Exists( boxファイルパス ) )
				{
					// box.def を含むフォルダの場合、BOXノードを作成する。
					var boxNode = new BoxNode( boxファイルパス, 親ノード );
					親ノード.子ノードリスト.Add( boxNode );

					// BOXノードを親として、サブフォルダへ再帰。
					this.曲を検索して親ノードに追加する( boxNode, subDirInfo.FullName );
				}
				else
				{
					// サブフォルダへ再帰。（通常フォルダ）
					this.曲を検索して親ノードに追加する( 親ノード, subDirInfo.FullName );
				}
			}
		}
		public void すべてのノードを削除する()
		{
			Debug.Assert( this.活性化していない );	// 活性化状態のノードが存在していないこと。

			this.フォーカスリスト = null;
			this.ルートノード.子ノードリスト.Clear();
		}

		// フォーカス

		/// <summary>
		///		指定されたノードをフォーカスする。
		///		<see cref="フォーカスリスト"/>もそのノードのあるリストへ変更される。
		/// </summary>
		/// <remarks>
		///		現在活性化中である場合、フォーカスリストも活性化状態にする。
		/// </remarks>
		public void フォーカスする( グラフィックデバイス gd, Node ノード )
		{
			//Debug.Assert( this.活性化している );	--> どちらの状態で呼び出してもよい。

			using( Log.Block( FDKUtilities.現在のメソッド名 ) )
			{
				var 親ノード = ノード?.親ノード ?? this.ルートノード;
				Trace.Assert( null != 親ノード?.子ノードリスト );

				var 旧フォーカスリスト = this.フォーカスリスト;	// 初回は null 。

				this.フォーカスリスト = 親ノード.子ノードリスト;	// 常に非null。（先のAssertで保証されている。）

				if( null != ノード )
				{
					// (A) ノードの指定がある（非null）なら、それを選択する。
					this.フォーカスリスト.SelectItem( ノード );
				}
				else
				{
					// (B) ノードの指定がない（null）なら、フォーカスノードは現状のまま維持する。
				}

				if( 旧フォーカスリスト != this.フォーカスリスト )
				{
					Log.Info( "フォーカスリストが変更されました。" );

					if( this.活性化している )
					{
						if( null != 旧フォーカスリスト )	// 初回は null 。
						{
							foreach( var node in 旧フォーカスリスト )
								node.非活性化する( gd );
						}

						foreach( var node in this.フォーカスリスト )
							node.活性化する( gd );
					}
				}
			}
		}

		/// <remarks>
		///		末尾なら先頭に戻る。
		/// </remarks>
		public void 次のノードをフォーカスする()
		{
			var index = this.フォーカスリスト.SelectedIndex;

			if( 0 > index )
				return;	// 現在フォーカスされているノードがない。

			index = ( index + 1 ) % this.フォーカスリスト.Count;

			this.フォーカスリスト.SelectItem( index );
		}

		/// <remarks>
		///		先頭なら末尾に戻る。
		/// </remarks>
		public void 前のノードをフォーカスする()
		{
			var index = this.フォーカスリスト.SelectedIndex;

			if( 0 > index )
				return; // 現在フォーカスされているノードがない。

			index = ( index - 1 + this.フォーカスリスト.Count ) % this.フォーカスリスト.Count;

			this.フォーカスリスト.SelectItem( index );
		}
	}
}
