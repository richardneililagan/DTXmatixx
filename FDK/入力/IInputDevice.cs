using System;
using System.Collections.Generic;

namespace FDK.入力
{
	/// <summary>
	///		全入力デバイスが実装して提供しなければならないインターフェース。
	/// </summary>
	public interface IInputDevice
	{
		/// <summary>
		///		デバイスの種別。デバイスごとに固定値を返す。
		/// </summary>
		InputDeviceType 入力デバイス種別 { get; }

		/// <summary>
		///		入力イベントのリスト。
		///		ポーリング時に、前回のポーリング（またはコンストラクタ）以降に発生した入力イベントが格納される。
		/// </summary>
		List<InputEvent> 入力イベントリスト { get; }

		/// <summary>
		///		ポーリングを行い、前回のポーリングからのイベントを取得して内部に上書き保管する。
		/// </summary>
		void ポーリングする();

		/// <summary>
		///		最後にポーリングした結果に対して key の Push イベントを検索し、存在したなら true を返す。
		/// </summary>
		bool キーが押された( int deviceID, int key );
		
		/// <summary>
		///		最後にポーリングした結果に対して key の Push イベントを検索し、最初に見つけた入力イベントを返す。
		///		見つからなかった場合、ev には null が返される。
		/// </summary>
		bool キーが押された( int deviceID, int key, out InputEvent ev );
		
		/// <summary>
		///		累積されたポーリング結果に対して key が現在押されているか確認する。
		/// </summary>
		bool キーが押されている( int deviceID, int key );

		/// <summary>
		///		最後にポーリングした結果に対して key の Pull イベントを検索し、存在したなら true を返す。
		/// </summary>
		bool キーが離された( int deviceID, int key );
		
		/// <summary>
		///		最後にポーリングした結果に対して key の Pull イベントを検索し、最初に見つけた入力イベントを返す。
		///		見つからなかった場合、ev には null が返される。
		/// </summary>
		bool キーが離された( int deviceID, int key, out InputEvent ev );
		
		/// <summary>
		///		累積されたポーリング結果に対して key が現在離されているか確認する。
		/// </summary>
		bool キーが離されている( int deviceID, int key );
	}
}
