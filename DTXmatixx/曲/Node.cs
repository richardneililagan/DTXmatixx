using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SharpDX;
using SharpDX.DirectWrite;
using FDK;
using FDK.メディア;

namespace DTXmatixx.曲
{
	/// <summary>
	///		曲ノードの基本クラス。
	/// </summary>
	/// <remarks>
	///		曲ツリーを構成するすべてのノードは、このクラスを継承する。
	/// </remarks>
	abstract class Node : FDK.Activity
	{
		// プロパティ

		/// <summary>
		///		ノードのタイトル。
		///		曲名、BOX名など。
		/// </summary>
		public string タイトル
		{
			get;
			set;
		} = "(no title)";

		/// <summary>
		///		ノードのサブタイトル。
		///		制作者名など。
		/// </summary>
		public string サブタイトル
		{
			get;
			set;
		} = "";


		// 曲ツリー関連

		/// <summary>
		///		曲ツリー階層において、親となるノード。
		/// </summary>
		public Node 親ノード
		{
			get;
			set;
		} = null;

		/// <summary>
		///		曲ツリー階層において、このノードが持つ子ノードのリスト。
		///		子ノードを持たない場合は空リスト。
		///		null 不可。
		/// </summary>
		public SelectableList<Node> 子ノードリスト
		{
			get;
		} = new SelectableList<Node>();

		/// <summary>
		///		このノードの１つ前に位置する兄弟ノードを示す。
		/// </summary>
		/// <remarks>
		///		このノードが先頭である（このノードの親ノードの子ノードリストの先頭である）場合は、末尾に位置する兄弟ノードを示す。
		/// </remarks>
		public Node 前のノード
		{
			get
			{
				var index = this.親ノード.子ノードリスト.IndexOf( this );
				Trace.Assert( ( 0 <= index ), "[バグあり] 自分が、自分の親の子ノードリストに存在していません。" );

				index = index - 1;

				if( 0 > index )
					index = this.親ノード.子ノードリスト.Count - 1;    // 先頭なら、末尾へ。

				return this.親ノード.子ノードリスト[ index ];
			}
		}

		/// <summary>
		///		このノードの１つ後に位置する兄弟ノードを示す。
		/// </summary>
		/// <remarks>
		///		このノードが末尾である（このノードの親ノードの子ノードリストの末尾である）場合は、先頭に位置する兄弟ノードを示す。
		/// </remarks>
		public Node 次のノード
		{
			get
			{
				var index = this.親ノード.子ノードリスト.IndexOf( this );
				Trace.Assert( ( 0 <= index ), "[バグあり] 自分が、自分の親の子ノードリストに存在していません。" );

				index = index + 1;

				if( this.親ノード.子ノードリスト.Count <= index )
					index = 0;      // 末尾なら、先頭へ。

				return this.親ノード.子ノードリスト[ index ];
			}
		}


		// ノード画像関連

		/// <summary>
		///		ノードの全体サイズ（設計単位）。
		///		すべてのノードで同一、固定値。
		/// </summary>
		public static Size2F 全体サイズ
		{
			get
				=> new Size2F( 314f, 220f );
		}

		/// <summary>
		///		ノードを表す画像。
		///		派生クラスで、適切な画像を割り当てること。
		///		null にすると、既定のノード画像が使用される。
		/// </summary>
		public テクスチャ ノード画像
		{
			get;
			protected set;
		} = null;

		/// <summary>
		///		ノードを表す画像の既定画像。static。
		/// </summary>
		/// <remarks>
		///		<see cref="ノード画像"/>が null の再に、代わりに表示される。
		///		static であり、全ノードで１つのインスタンスを共有する。
		/// </remarks>
		public static テクスチャ 既定のノード画像
		{
			get;
			protected set;
		} = null;


		public Node()
		{
			//this.子リスト.Add( this._ノード画像 );	--> 派生クラスのコンストラクタで追加することができる。
			this.子リスト.Add( this._曲名テクスチャ = new 曲名() );
		}

		protected override void On活性化( グラフィックデバイス gd )
		{
			// 全インスタンスで共有する static メンバが未生成なら生成する。
			if( null == Node.既定のノード画像 )
			{
				Node.既定のノード画像 = new テクスチャ( @"$(System)images\既定のプレビュー画像.png" );
				Node.既定のノード画像.活性化する( gd );
			}
		}
		protected override void On非活性化( グラフィックデバイス gd )
		{
			// 全インスタンスで共有する static メンバが生成な済みなら解放する。
			if( null != Node.既定のノード画像 )
			{
				Node.既定のノード画像.非活性化する( gd );
				Node.既定のノード画像 = null;
			}
		}

		public virtual void 進行描画する( グラフィックデバイス gd, Matrix ワールド変換行列, bool キャプション表示 = true )
		{
			// (1) ノード画像を描画する。
			if( null != this.ノード画像 )
			{
				this.ノード画像.描画する( gd, ワールド変換行列 );
			}
			else
			{
				Node.既定のノード画像.描画する( gd, ワールド変換行列 );
			}

			// (2) キャプションを描画する。
			if( キャプション表示 )
			{
				ワールド変換行列 *= Matrix.Translation( 0f, 0f, 1f );    // ノード画像よりZ方向手前にほんのり移動
				this._曲名テクスチャ.タイトル = this.タイトル;
				this._曲名テクスチャ.サブタイトル = this.サブタイトル;
				this._曲名テクスチャ.描画する( gd, ワールド変換行列, new RectangleF( 0f, 138f, Node.全体サイズ.Width, Node.全体サイズ.Height - 138f + 27f ) );
			}
		}

		protected 曲名 _曲名テクスチャ = null;
	}
}
