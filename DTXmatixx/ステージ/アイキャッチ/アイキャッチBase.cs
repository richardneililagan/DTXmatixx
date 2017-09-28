using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SharpDX.Animation;
using FDK;
using FDK.メディア;

namespace DTXmatixx.アイキャッチ
{
	public enum フェーズ
	{
		未定,
		クローズ,
		オープン,
		クローズ完了,
		オープン完了
	}
	class アイキャッチBase : Activity
	{
		public フェーズ 現在のフェーズ
		{
			get;
			protected set;
		} = フェーズ.未定;

		protected override void On活性化( グラフィックデバイス gd )
		{
			this.現在のフェーズ = フェーズ.未定;
		}

		public virtual void クローズする( グラフィックデバイス gd, float 速度倍率 = 1.0f )
		{
			//
			// ここに、ストーリーボードと変数の生成、トラジションの追加、ストーリーボードの開始コードを記述する。
			//
			this.現在のフェーズ = フェーズ.クローズ;
		}
		public virtual void オープンする( グラフィックデバイス gd, float 速度倍率 = 1.0f )
		{
			//
			// ここに、ストーリーボードと変数の生成、トラジションの追加、ストーリーボードの開始コードを記述する。
			//
			this.現在のフェーズ = フェーズ.オープン;
		}

		public virtual void 進行描画する( グラフィックデバイス gd )
		{
			switch( this.現在のフェーズ )
			{
				case フェーズ.未定:
					break;

				case フェーズ.クローズ:
				case フェーズ.クローズ完了:
					this.進行描画する( gd, StoryboardStatus.Scheduled );
					break;

				case フェーズ.オープン:
				case フェーズ.オープン完了:
					this.進行描画する( gd, StoryboardStatus.Ready );
					break;
			}
		}

		protected virtual void 進行描画する( グラフィックデバイス gd, StoryboardStatus 描画しないStatus )
		{
			bool すべて完了 = true;

			// ストーリーボードが動作しているなら、すべて完了 フラグを切る。
			//if( context.ストーリーボード.Status != StoryboardStatus.Ready )
			//	すべて完了 = false;

			// 描画するステータスなら描画する。
			//if( context.ストーリーボード.Status != 描画しないStatus )
			//{
			//   ...
			//}

			if( すべて完了 )
			{
				if( this.現在のフェーズ == フェーズ.クローズ )
				{
					this.現在のフェーズ = フェーズ.クローズ完了;
				}
				else if( this.現在のフェーズ == フェーズ.オープン )
				{
					this.現在のフェーズ = フェーズ.オープン完了;
				}
			}
		}
	}
}
