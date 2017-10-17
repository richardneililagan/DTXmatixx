using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace FDK
{
	/// <summary>
	///		任意の１つの要素を選択できる機能を持つ List。
	/// </summary>
	/// <typeparam name="T">要素の型。</typeparam>
	public class SelectableList<T> : List<T>
	{
		/// <summary>
		///		現在選択されている要素のインデックス番号（0～Count-1）。
		///		未選択またはリストが空なら、負数。
		/// </summary>
		public int SelectedIndex
		{
			get;
			protected set;
		} = -1;


		/// <summary>
		///		コンストラクタ。
		/// </summary>
		public SelectableList()
		{
		}

		/// <summary>
		///		要素を選択する。
		/// </summary>
		/// <param name="インデックス番号">選択する要素のインデックス番号（0～Count-1）。負数なら未選択状態にする。</param>
		/// <returns>選択または未選択状態にできたら true、できなかったら false。</returns>
		public bool SelectItem( int インデックス番号 )
		{
			if( ( 0 == this.Count ) ||				// リストが空だったり、
				( this.Count <= インデックス番号 ) )	// 指定されたインデックスが大きすぎた場合には
			{
				return false;						// false を返す。
			}

			this.SelectedIndex = インデックス番号;		// 0 または 負数は OK。

			return true;
		}

		/// <summary>
		///		要素を選択する。
		/// </summary>
		/// <remarks>
		///		要素が存在しない場合には、未選択状態になる。
		/// </remarks>
		/// <param name="要素">選択する要素。</param>
		/// <returns>選択または未選択状態にできたら true、できなかったら false。</returns>
		public bool SelectItem( T 要素 )
		{
			if( 0 == this.Count )
				return false;

			this.SelectedIndex = this.IndexOf( 要素 );      // 見つからなければ負数が返される --> 未選択状態になる。

			return true;
		}

		/// <summary>
		///		リストの末尾の要素を選択する。
		/// </summary>
		/// <returns>選択できたら true、できなかったら false。</returns>
		public bool SelectLast()
		{
			if( 0 == this.Count )	// リストが空なら
				return false;       // false を返す。

			this.SelectedIndex = this.Count - 1;

			return true;
		}

		/// <summary>
		///		現在選択されている要素のひとつ後ろの要素を選択する。
		/// </summary>
		/// <returns>選択できたら true、できなかったら false。</returns>
		public bool SelectNext()
		{
			if( ( 0 > this.SelectedIndex ) ||				// 未選択だったり
				( 0 == this.Count ) ||						// リストが空だったり
				( this.Count - 1 <= this.SelectedIndex ) )	// すでに末尾に位置してたりする場合は
			{
				return false;								// false を返す。
			}

			this.SelectedIndex++;

			return true;
		}

		/// <summary>
		///		現在選択されている要素のひとつ前の要素を選択する。
		/// </summary>
		/// <returns>選択できたら true、できなかったら false。</returns>
		public bool SelectPrev()
		{
			if( ( 0 > this.SelectedIndex ) ||               // 未選択だったり
				( 0 == this.Count ) ||                      // リストが空だったり
				( 0 == this.SelectedIndex ) )				// すでに先頭に位置してたりする場合は
			{
				return false;                               // false を返す。
			}

			this.SelectedIndex--;

			return true;
		}
	}
}
