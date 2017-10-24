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
		///		現在選択されているノードが対応している、現在の難易度アンカに一番近い難易度（0:BASIC～4:ULTIMATE）の MusicNode を返す。
		/// </summary>
		/// <remarks>
		///		難易度アンカはどのノードを選択しても不変である。
		///		<see cref="フォーカスノード"/>が<see cref="SetNode"/>型である場合は、それが保有する難易度（最大５つ）の中で、
		///		現在の難易度アンカに一番近い難易度の <see cref="MusicNode"/> が返される。
		///		それ以外の場合は常に null が返される。
		/// </remarks>
		public MusicNode フォーカス曲ノード
		{
			get
			{
				if( this.フォーカスノード is MusicNode musicnode )
				{
					return musicnode;
				}
				if( this.フォーカスノード is SetNode setnode )
				{
					return setnode.MusicNodes[ this._現在の難易度アンカに最も近い難易度レベルを返す( setnode ) ].music;
				}
				else
				{
					return null;
				}
			}
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

			//this._難易度アンカ = 3;		-> 初期化せず、前回の値を継承する。
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

			// (1) このフォルダに set.def があるなら、その内容でSetノード（任意個）を作成する。

			var setDefPath = Path.Combine( フォルダパス, @"set.def" );

			if( File.Exists( setDefPath ) )
			{
				var setDef = SetDef.復元する( setDefPath );

				foreach( var block in setDef.Blocks )
				{
					var setNode = new SetNode( block, フォルダパス, 親ノード );
					親ノード.子ノードリスト.Add( setNode );
				}
			}
			else
			{
				// (2) set.def がなかった場合、このフォルダにあるすべてのsstf/dtxファイルを検索して、曲ノードを作成する。

				var fileInfos = dirInfo.GetFiles( "*.*", SearchOption.TopDirectoryOnly )
					.Where( ( fileInfo ) => new string[] { ".sstf", ".dtx" }.Any( 拡張子名 => ( Path.GetExtension( fileInfo.Name ).ToLower() == 拡張子名 ) ) );

				foreach( var fileInfo in fileInfos )
				{
					try
					{
						var music = new MusicNode( fileInfo.FullName, 親ノード );
						親ノード.子ノードリスト.Add( music );
					}
					catch
					{
						Log.ERROR( "MusicNode の生成に失敗しました。" );
					}
				}
			}

			// (3) このフォルダのすべてのサブフォルダについて再帰処理。（set.defがあってもなくても）

			foreach( var subDirInfo in dirInfo.GetDirectories() )
			{
				var boxDefPath = Path.Combine( subDirInfo.FullName, @"box.def" );

				if( File.Exists( boxDefPath ) )
				{
					// (3-a) box.def を含むフォルダの場合、BOXノードを作成する。
					var boxNode = new BoxNode( boxDefPath, 親ノード );
					親ノード.子ノードリスト.Add( boxNode );

					// BOXノードを親として、サブフォルダへ再帰。
					this.曲を検索して親ノードに追加する( boxNode, subDirInfo.FullName );
				}
				else
				{
					// (3-b) サブフォルダへ再帰。（通常フォルダ）
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


		private int _難易度アンカ = 3;

		private int _現在の難易度アンカに最も近い難易度レベルを返す( SetNode setnode )
		{
			if( null == setnode )
				return this._難易度アンカ;

			if( null != setnode.MusicNodes[ this._難易度アンカ ].label )
				return this._難易度アンカ;    // 難易度ぴったりの曲があった

			// 現在のアンカレベルから、難易度上向きに検索開始。

			int 最も近いレベル = this._難易度アンカ;
			for( int i = 0; i < 5; i++ )
			{
				if( null != setnode.MusicNodes[ 最も近いレベル ].label )
					break;  // 曲があった。

				// 曲がなかったので次の難易度レベルへGo。（5以上になったら0に戻る。）
				最も近いレベル = ( 最も近いレベル + 1 ) % 5;
			}

			// 見つかった曲がアンカより下のレベルだった場合……
			// アンカから下向きに検索すれば、もっとアンカに近い曲があるんじゃね？

			if( 最も近いレベル < this._難易度アンカ )
			{
				// 現在のアンカレベルから、難易度下向きに検索開始。

				最も近いレベル = this._難易度アンカ;
				for( int i = 0; i < 5; i++ )
				{
					if( null != setnode.MusicNodes[ 最も近いレベル ].label )
						break;  // 曲があった。

					// 曲がなかったので次の難易度レベルへGo。（0未満になったら4に戻る。）
					最も近いレベル = ( ( 最も近いレベル - 1 ) + 5 ) % 5;
				}
			}

			return 最も近いレベル;
		}
	}
}
