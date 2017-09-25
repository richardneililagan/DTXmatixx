using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SharpDX;
using SharpDX.Direct2D1;
using FDK;
using FDK.メディア;

namespace DTXmatixx.ステージ.演奏
{
	/// <summary>
	///		チップの背景であり、レーン全体を示すフレーム画像。
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
		public static Dictionary<表示レーン種別, float> レーンtoチップの左端位置dpx
		{
			get;
			protected set;
		} = new Dictionary<表示レーン種別, float>() {
			{ 表示レーン種別.Unknown, +0f },
			{ 表示レーン種別.LeftCrash, +1f },
			{ 表示レーン種別.HiHat, +108f },
			{ 表示レーン種別.Foot, +183f },
			{ 表示レーン種別.Snare, +258f },
			{ 表示レーン種別.Bass, +352f },
			{ 表示レーン種別.Tom1, +446f },
			{ 表示レーン種別.Tom2, +521f },
			{ 表示レーン種別.Tom3, +595f },
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
				this._レーン色ブラシ = new SolidColorBrush( gd.D2DDeviceContext, new Color4( 0xff5d5d5d ) );
			}
		}
		protected override void On非活性化( グラフィックデバイス gd )
		{
			using( Log.Block( FDKUtilities.現在のメソッド名 ) )
			{
				FDKUtilities.解放する( ref this._レーン色ブラシ );
			}
		}

		public void 描画する( グラフィックデバイス gd )
		{
			gd.D2DBatchDraw( ( dc ) => {

				// レーンラインを描画する。
				for( int i = 0; i < _レーンライン.Length; i++ )
				{
					var rc = _レーンライン[ i ];
					rc.Left += レーンフレーム.領域.Left;
					rc.Right += レーンフレーム.領域.Left;
					dc.FillRectangle( rc, _レーン色ブラシ );
				}

			} );
		}

		/// <summary>
		///		レーンラインの領域。
		///		<see cref="レーンフレーム.領域"/>.Left からの相対値[dpx]。
		/// </summary>
		private static RectangleF[] _レーンライン = {
			new RectangleF( +96f, +0f, 4f, 938f ),
			new RectangleF( +104f, +0f, 4f, 938f ),
			new RectangleF( +179f, +0f, 4f, 938f ),
			new RectangleF( +256f, +0f, 4f, 938f ),
			new RectangleF( +340f, +0f, 4f, 938f ),
			new RectangleF( +347f, +0f, 4f, 938f ),
			new RectangleF( +443f, +0f, 4f, 938f ),
			new RectangleF( +518f, +0f, 4f, 938f ),
			new RectangleF( +592f, +0f, 4f, 938f ),
			new RectangleF( +665f, +0f, 4f, 938f ),
			new RectangleF( +673f, +0f, 4f, 938f ),
		};

		private SolidColorBrush _レーン色ブラシ = null;
	}
}
