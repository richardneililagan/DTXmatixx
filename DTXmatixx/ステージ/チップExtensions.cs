using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SSTFormat.v3;

namespace DTXmatixx.ステージ
{
	/// <summary>
	///		SSTFormat.v3.チップ種別/チップ の拡張メソッド
	/// </summary>
	static class チップExtensions
	{
		/// <summary>
		///		チップの排他発声グループID を表す。
		///		ただし、グループID 0 は排他発声にはしないことを意味する。
		/// </summary>
		/// <remarks>
		///		このグループIDを 0 以外に設定すると、チップの発声時に、そのチップ種別に対応する排他発声グループIDを参照して
		///		それと同じ値を持つチップ種別のサウンドがすべて再生停止する必要があるかを確認し、必要あれば停止してから発声されるようなる。
		///		なお、同一かどうかだけを見るので、グループIDに設定する数値は（0を除いて）どんな値でもいい。
		/// </remarks>
		/// <param name="チップ種別">調べるチップ種別。</param>
		/// <returns>チップ種別に対応する排他発声グループID。</returns>
		public static int 排他発声グループID( this チップ種別 チップ種別 )
		{
			switch( チップ種別 )
			{
				case チップ種別.HiHat_Close:
				case チップ種別.HiHat_Foot:
				case チップ種別.HiHat_HalfOpen:
				case チップ種別.HiHat_Open:
					return GID_HIHAT;

				case チップ種別.LeftCrash:
				case チップ種別.LeftCymbal_Mute:
					return GID_LEFT_CYMBAL;

				case チップ種別.RightCrash:
				case チップ種別.RightCymbal_Mute:
					return GID_RIGHT_CYMBAL;

				case チップ種別.China:
					// return ( user.オプション設定.表示レーンの左右.Chinaは左 ) ? GID_LEFT_CYMBAL : GID_RIGHT_CYMBAL;
					return GID_RIGHT_CYMBAL;	// China は右で固定

				case チップ種別.Splash:
					//return ( user.オプション設定.表示レーンの左右.Splashは左 ) ? GID_LEFT_CYMBAL : GID_RIGHT_CYMBAL;
					return GID_LEFT_CYMBAL;    // Splash は左で固定

				case チップ種別.Ride:
				case チップ種別.Ride_Cup:
					//return ( user.オプション設定.表示レーンの左右.Rideは左 ) ? GID_LEFT_CYMBAL : GID_RIGHT_CYMBAL;
					return GID_RIGHT_CYMBAL;    // Ride は右で固定
			}

			return 0;
		}
		public static int 排他発声グループID( this チップ チップ )
		{
			return チップ.チップ種別.排他発声グループID();
		}

		public static bool 直前のチップを消音する( this チップ種別 今回のチップの種別, チップ種別 直前のチップの種別 )
		{
			int 今回のチップのGID = 今回のチップの種別.排他発声グループID();
			int 直前のチップのGID = 直前のチップの種別.排他発声グループID();

			if( 直前のチップのGID != 今回のチップのGID )
				return false;

			// グループIDがシンバルである場合は、Mute が来た場合を除き、消音しない。

			if( 直前のチップのGID == GID_LEFT_CYMBAL )
			{
				return ( 今回のチップの種別 == チップ種別.LeftCymbal_Mute );
			}
			if( 直前のチップのGID == GID_RIGHT_CYMBAL )
			{
				return ( 今回のチップの種別 == チップ種別.RightCymbal_Mute );
			}

			return true;
		}

		private const int GID_HIHAT = 1;
		private const int GID_LEFT_CYMBAL = 2;
		private const int GID_RIGHT_CYMBAL = 3;
	}
}
