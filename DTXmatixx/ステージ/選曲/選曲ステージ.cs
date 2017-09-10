using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SharpDX;
using SharpDX.DirectInput;
using FDK;
using FDK.メディア;
using DTXmatixx.画面遷移.ABC遷移;

namespace DTXmatixx.ステージ.選曲
{
	class 選曲ステージ : ステージ
	{
		public enum フェーズ
		{
			フェードイン,
			表示,
			確定,
			キャンセル,
		}
		public フェーズ 現在のフェーズ
		{
			get;
			protected set;
		}

		public 選曲ステージ()
		{
			this.子リスト.Add( this._舞台画像 = new 舞台画像() );
			this.子リスト.Add( this._フェードイン = new 回転幕() );
		}
		protected override void On活性化( グラフィックデバイス gd )
		{
			using( Log.Block( FDKUtilities.現在のメソッド名 ) )
			{
				this.現在のフェーズ = フェーズ.フェードイン;
				this._初めての進行描画 = true;
			}
		}
		protected override void On非活性化( グラフィックデバイス gd )
		{
			using( Log.Block( FDKUtilities.現在のメソッド名 ) )
			{
			}
		}
		public override void 進行描画する( グラフィックデバイス gd )
		{
			if( this._初めての進行描画 )
			{
				this._フェードイン.オープンする( gd );
				this._初めての進行描画 = false;
			}

			App.Keyboard.ポーリングする();

			switch( this.現在のフェーズ )
			{
				case フェーズ.フェードイン:
					this._フェードイン.進行描画する( gd );

					if( this._フェードイン.現在のフェーズ == 回転幕.フェーズ.オープン完了 )
					{
						this.現在のフェーズ = フェーズ.表示;
					}
					break;

				case フェーズ.表示:
					break;

				case フェーズ.確定:
				case フェーズ.キャンセル:
					break;
			}
		}

		private bool _初めての進行描画 = true;
		private 舞台画像 _舞台画像 = null;
		private 回転幕 _フェードイン = null;
	}
}
