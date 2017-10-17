using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using FDK.メディア;

namespace FDK
{
	public class Activity
	{
		/// <summary>
		///		子リストに Activity を登録すると、活性化と非活性化が親と連動するようになる。
		/// </summary>
		/// <remarks>
		///		子リストには静的・動的の２種類があり、それぞれ以下のように使い分ける。
		/// 
		///		(A) メンバとして定義する静的な子の場合：
		///		　・子Activity の生成と子リストへの追加は、親Activity のコンストラクタで行う。
		///		　・子リストからの削除は不要。
		///	　
		///		(B) 活性化時に生成する動的な子の場合：
		///		　・子Activity の生成と子リストへの追加は、親Activity の On活性化() で行う。
		///		　・子リストからの削除は、親Activity の On非活性化() で行う。
		/// </remarks>
		public List<Activity> 子リスト
		{
			get;
		} = new List<Activity>();

		public bool 活性化している
		{
			get;
			private set;    // 派生クラスからも設定は禁止。
		} = false;
		public bool 活性化していない
		{
			get
				=> !( this.活性化している );

			// 派生クラスからも設定は禁止。
			private set
				=> this.活性化している = !( value );
		}

		/// <summary>
		///		この Activity を初期化し、進行や描画を行える状態にする。
		///		これにはデバイス依存リソースの作成も含まれる。
		/// </summary>
		public void 活性化する( グラフィックデバイス gd )
		{
			Debug.Assert( this.活性化していない );

			// (1) 自分を活性化する。
			this.On活性化( gd );
			this.活性化している = true;

			// (2) すべての子Activityを活性化する。
			foreach( var child in this.子リスト )
				child.活性化する( gd );
		}

		/// <summary>
		///		この Activity を終了し、進行や描画を行わない状態に戻す。
		///		これにはデバイス依存リソースの解放も含まれる。
		/// </summary>
		public void 非活性化する( グラフィックデバイス gd )
		{
			Debug.Assert( this.活性化している );

			// (1) すべての子Activityを非活性化する。
			foreach( var child in this.子リスト )
				child.非活性化する( gd );

			// (2) 自分を非活性化する。
			this.On非活性化( gd );
			this.活性化していない = true;
		}


		// 以下、派生クラスでオーバーライドするもの。

		protected virtual void On活性化( グラフィックデバイス gd ) { }

		protected virtual void On非活性化( グラフィックデバイス gd ) { }
	}
}
