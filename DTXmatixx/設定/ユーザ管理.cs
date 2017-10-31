using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using FDK;
using DTXmatixx.データベース.ユーザ;

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
			var user = (ユーザ設定) null;

			user = new ユーザ設定( "AutoPlayer" );
			if( null == user.ユーザID )	// DBにいない
			{
				user = ユーザ設定.作成する( new User() {
					Id = "AutoPlayer",
					Name = "AutoPlayer",
					// 他は規定値
				} );
			}
			if( null != user )
				this.ユーザリスト.Add( user );

			user = new ユーザ設定( "Guest" );
			if( null == user.ユーザID )	// DBにいない
			{
				user = ユーザ設定.作成する( new User() {
					Id = "Guest",
					Name = "Guest",
					AutoPlay_LeftCymbal = 0,
					AutoPlay_HiHat = 0,
					AutoPlay_LeftPedal = 0,
					AutoPlay_Snare = 0,
					AutoPlay_Bass = 0,
					AutoPlay_HighTom = 0,
					AutoPlay_LowTom = 0,
					AutoPlay_FloorTom = 0,
					AutoPlay_RightCymbal = 0,
					// 他は規定値
				} );
			}
			if( null != user )
				this.ユーザリスト.Add( user );
		}
		public void Dispose()
		{
			this.ユーザリスト = null;
		}
	}
}
