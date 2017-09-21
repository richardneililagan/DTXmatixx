using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SharpDX;
using FDK;
using FDK.メディア;

namespace DTXmatixx.ステージ.演奏
{
	/// <summary>
	///		チップの背景であり、レーン全体を示すフレーム画像。
	///		レーンフラッシュアニメも追加。
	/// </summary>
	class レーンフレーム : Activity
	{
		/// <summary>
		///		画面全体に対する、レーンフレームの表示位置と範囲。
		/// </summary>
		public static RectangleF 領域
		{
			get;
			protected set;
		} = new RectangleF( 445f, 0f, 775f, 938f );

		/// <summary>
		///		表示レーンの左端X位置を、レーンフレームの左端からの相対位置で示す。
		/// </summary>
		public static Dictionary<表示レーン種別, float> レーンto左端位置dpx
		{
			get;
			protected set;
		} = new Dictionary<表示レーン種別, float>() {
			{ 表示レーン種別.Unknown, +0f },
			{ 表示レーン種別.LeftCrash, +4f },
			{ 表示レーン種別.HiHat, +107f },
			{ 表示レーン種別.Foot, +181f },
			{ 表示レーン種別.Snare, +259f },
			{ 表示レーン種別.Bass, +352f },
			{ 表示レーン種別.Tom1, +447f },
			{ 表示レーン種別.Tom2, +522f },
			{ 表示レーン種別.Tom3, +598f },
			{ 表示レーン種別.RightCrash, +677f },
		};
		
		/// <summary>
		///		表示レーンの幅。
		/// </summary>
		public static Dictionary<表示レーン種別, float> レーンtoレーン幅dpx
		{
			get;
			protected set;
		} = new Dictionary<表示レーン種別, float>() {
			{ 表示レーン種別.Unknown, 0f },
			{ 表示レーン種別.LeftCrash, 96f },
			{ 表示レーン種別.HiHat, 70f },
			{ 表示レーン種別.Foot, 73f },
			{ 表示レーン種別.Snare, 84f },
			{ 表示レーン種別.Bass, 90f },
			{ 表示レーン種別.Tom1, 71f },
			{ 表示レーン種別.Tom2, 71f },
			{ 表示レーン種別.Tom3, 71f },
			{ 表示レーン種別.RightCrash, 96f },
		};

		protected override void On活性化( グラフィックデバイス gd )
		{
			using( Log.Block( FDKUtilities.現在のメソッド名 ) )
			{
			}
		}
		protected override void On非活性化( グラフィックデバイス gd )
		{
			using( Log.Block( FDKUtilities.現在のメソッド名 ) )
			{
			}
		}

		public void 進行描画する( グラフィックデバイス gd )
		{
		}
	}
}
