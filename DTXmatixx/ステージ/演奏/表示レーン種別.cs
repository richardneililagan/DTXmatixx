using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace DTXmatixx.ステージ.演奏
{
	/// <summary>
	///		チップや判定文字列の表示先となるレーンの種別。
	/// </summary>
	enum 表示レーン種別
	{
		Unknown,
		LeftCrash,
		HiHat,
		Foot,	// 左ペダル
		Snare,
		Bass,
		Tom1,
		Tom2,
		Tom3,
		RightCrash,
	}
}
