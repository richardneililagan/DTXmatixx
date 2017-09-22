using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace DTXmatixx.入力
{
	/// <summary>
	///		入力データの種別を、ドラム様式で定義したもの。
	///		入力デバイスの入力データは、すべてこのドラム入力種別へマッピングされる。
	/// </summary>
	enum ドラム入力種別
	{
		Unknown,
		LeftCrash,
		Ride,
		//Ride_Cup,			--> Ride として扱う。（打ち分けない。）
		China,
		Splash,
		HiHat_Open,
		//HiHat_HalfOpen,	--> HiHat_Open として扱う。（打ち分けない。）
		HiHat_Close,
		//HiHat_Foot,		--> ヒット判定しない。
		HiHat_Control,  //	--> 開度（入力信号である）
		Snare,
		Snare_OpenRim,
		Snare_ClosedRim,
		//Snare_Ghost,		--> ヒット判定しない。
		Bass,
		Tom1,
		Tom1_Rim,
		Tom2,
		Tom2_Rim,
		Tom3,
		Tom3_Rim,
		RightCrash,
		//LeftCymbal_Mute,	--> （YAMAHAでは）入力信号じゃない
		//RightCymbal_Mute,	--> （YAMAHAでは）入力信号じゃない
	}
}
