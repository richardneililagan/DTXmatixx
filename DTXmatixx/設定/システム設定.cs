using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SSTFormat.v2;
using DTXmatixx.曲;
using DTXmatixx.ステージ.演奏;

namespace DTXmatixx.設定
{
	class システム設定
	{
		/// <summary>
		///		SSTFのチップ種別を、DTXMatixx の表示レーン種別に変換する。
		/// </summary>
		public Dictionary<チップ種別, 表示レーン種別> チップto表示レーン = new Dictionary<チップ種別, 表示レーン種別>() {
			{ チップ種別.Unknown, 表示レーン種別.Unknown },
			{ チップ種別.LeftCrash, 表示レーン種別.LeftCrash },
			{ チップ種別.Ride, 表示レーン種別.RightCrash },	// Ride は右固定
			{ チップ種別.Ride_Cup, 表示レーン種別.RightCrash },
			{ チップ種別.China, 表示レーン種別.RightCrash },	// China は右固定
			{ チップ種別.Splash, 表示レーン種別.LeftCrash },    // Splash は左固定
			{ チップ種別.HiHat_Open, 表示レーン種別.HiHat },
			{ チップ種別.HiHat_HalfOpen, 表示レーン種別.HiHat },
			{ チップ種別.HiHat_Close, 表示レーン種別.HiHat },
			{ チップ種別.HiHat_Foot, 表示レーン種別.Foot },
			{ チップ種別.Snare, 表示レーン種別.Snare },
			{ チップ種別.Snare_OpenRim, 表示レーン種別.Snare },
			{ チップ種別.Snare_ClosedRim, 表示レーン種別.Snare },
			{ チップ種別.Snare_Ghost, 表示レーン種別.Snare },
			{ チップ種別.Bass, 表示レーン種別.Bass },
			{ チップ種別.Tom1, 表示レーン種別.Tom1 },
			{ チップ種別.Tom1_Rim, 表示レーン種別.Tom1 },
			{ チップ種別.Tom2, 表示レーン種別.Tom2 },
			{ チップ種別.Tom2_Rim, 表示レーン種別.Tom2 },
			{ チップ種別.Tom3, 表示レーン種別.Tom3 },
			{ チップ種別.Tom3_Rim, 表示レーン種別.Tom3 },
			{ チップ種別.RightCrash, 表示レーン種別.RightCrash },
			{ チップ種別.BPM, 表示レーン種別.Unknown },
			{ チップ種別.小節線, 表示レーン種別.Unknown },
			{ チップ種別.拍線, 表示レーン種別.Unknown },
			{ チップ種別.背景動画, 表示レーン種別.Unknown },
			{ チップ種別.小節メモ, 表示レーン種別.Unknown },
			{ チップ種別.LeftCymbal_Mute, 表示レーン種別.Unknown },
			{ チップ種別.RightCymbal_Mute, 表示レーン種別.Unknown },
			{ チップ種別.小節の先頭, 表示レーン種別.Unknown },
		};
	}
}
