using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using FDK;

namespace DTXmatixx.設定
{
	/// <summary>
	///		全ユーザの管理（ログオン、ログオフなど）を行う。
	/// </summary>
	class ユーザ管理 : IDisposable
	{
		public SelectableList<ユーザ設定> ユーザリスト
		{
			get;
			protected set;
		} = null;

		/// <summary>
		///		現在ログオンしているユーザ。
		///		ユーザリストの１要素を参照しているので解放しないこと。
		/// </summary>
		public ユーザ設定 ログオン中のユーザ
			=> ( 0 <= this.ユーザリスト.SelectedIndex ) ? this.ユーザリスト[ this.ユーザリスト.SelectedIndex ] : null;

		public ユーザ管理()
		{
			this.ユーザリスト = new SelectableList<ユーザ設定>();

			// 現在は、AutoPlayer と Guest しかいない。
			this.ユーザリスト.Add( new ユーザ設定( "AutoPlayer" ) );
			this.ユーザリスト.Add( new ユーザ設定( "Guest" ) );
		}
		public void Dispose()
		{
			this.ユーザリスト = null;
		}
	}
}
