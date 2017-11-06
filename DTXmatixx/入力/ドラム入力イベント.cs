using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using FDK.入力;

namespace DTXmatixx.入力
{
	/// <summary>
	///		単一の入力イベントを表す。
	/// </summary>
	/// <remarks>
	///		FDK の InputEvent に、それからマッピングされた ドラム入力種別 を付与したもの。
	///		ただし、継承や拡張ではなく包含。
	/// </remarks>
	class ドラム入力イベント
	{
		// Key
		public InputEvent InputEvent
		{
			get;
			protected set;
		} = null;

		// Value
		public ドラム入力種別 Type
		{
			get;
			protected set;
		} = ドラム入力種別.Unknown;


		public ドラム入力イベント( InputEvent 入力イベント, ドラム入力種別 ドラム入力種別 )
		{
			this.Type = ドラム入力種別;
			this.InputEvent = 入力イベント;
		}
	}
}
