using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;

namespace DTXmatixx.設定
{
	[DataContract( Name = "LanePosition", Namespace = "" )]
	struct 表示レーンの左右
	{
		/// <summary>
		///		演奏画面で、Ride/Ride_Cupチップを左シンバルレーン上に表示するなら true、
		///		右シンバルレーン上に表示するなら false。
		/// </summary>
		[DataMember]
		public bool Rideは左 { get; set; }

		/// <summary>
		///		演奏画面で、Chinaチップを左シンバルレーン上に表示するなら true、
		///		右シンバルレーン上に表示するなら false。
		/// </summary>
		[DataMember]
		public bool Chinaは左 { get; set; }

		/// <summary>
		///		演奏画面で、Splashチップを左シンバルレーン上に表示するなら true、
		///		右シンバルレーン上に表示するなら false。
		/// </summary>
		[DataMember]
		public bool Splashは左 { get; set; }
	}
}
