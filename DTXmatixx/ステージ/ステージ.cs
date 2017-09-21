using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using FDK;
using FDK.メディア;

namespace DTXmatixx.ステージ
{
	/// <summary>
	///		全ステージに共通する機能。
	/// </summary>
	class ステージ : Activity
	{
		/// <summary>
		///		ステージの進行処理のうち、高速に行うべき処理を行う。
		/// </summary>
		/// <remarks>
		///		高速処理が不要な進行（描画用のアニメなど）は、進行描画メソッド側で行うこと。
		/// </remarks>
		public virtual void 高速進行する()
		{
		}

		/// <summary>
		///		ステージの通常速度での進行と描画を行う。
		/// </summary>
		public virtual void 進行描画する( グラフィックデバイス gd )
		{
		}
	}
}
