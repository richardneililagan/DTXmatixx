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
	class カウントマップライン : Activity
	{
		/// <summary>
		///		カウント値の配列。
		///		インデックスが小さいほど曲の前方に位置する。
		///		カウント値の値域は 0～12。今のところは 0 で非表示、1 で水色、2～12で黄色表示。
		/// </summary>
		public int[] カウントマップ
		{
			get;
			protected set;
		} = null;

		/// <summary>
		///		全曲の最大カウントを 768 とするので、カウントマップリストの要素数は、768÷12 = 64 個が最大となる。
		/// </summary>
		public const int カウントマップの最大要素数 = 768 / 12;

		public カウントマップライン()
		{
		}

		/// <summary>
		///		初期化。
		/// </summary>
		public void 過去最大のカウントマップを登録する( int[] カウントマップ )
		{
			Debug.Assert( カウントマップの最大要素数 == カウントマップ.Length, "カウントマップの要素数が不正です。" );

			this._過去最大のカウントマップ = new int[ カウントマップ.Length ];
			カウントマップ.CopyTo( this._過去最大のカウントマップ, 0 );	// コピー
		}

		/// <summary>
		///		指定された位置における現在の成績から、対応するカウント値を算出し、反映する。
		/// </summary>
		/// <param name="現在位置">現在の位置を 開始点:0～1:終了点 で示す。</param>
		/// <param name="判定toヒット数">現在の位置における成績。</param>
		public void カウント値を設定する( float 現在位置, IReadOnlyDictionary<判定種別, int> 判定toヒット数 )
		{
			// 成績の増加分を得る。
			var 増加値 = new Dictionary<判定種別, int>();
			foreach( 判定種別 judge in Enum.GetValues( typeof( 判定種別 ) ) )
				増加値.Add( judge, 判定toヒット数[ judge ] - this._最後にカウント値を設定したときの成績[ judge ] );

			// todo: カウント値を算出する。
			int カウント値 = ( 増加値[ 判定種別.MISS ] > 0 ) ? 1 : 12;      // 暫定式。

			// 最後にカウント値を設定した位置 から 現在位置 までの期間に対応するすべてのカウント値に反映する。
			int 開始 = (int) ( this._最後にカウント値を設定した位置 * カウントマップの最大要素数 );
			int 終了 = (int) ( 現在位置 * カウントマップの最大要素数 );
			if( 1.0f > 現在位置 )
			{
				for( int i = 開始; i <= 終了; i++ )
					this.カウントマップ[ i ] = カウント値;
			}

			// 位置と成績を保存。
			this._最後にカウント値を設定した位置 = 現在位置;
			foreach( 判定種別 judge in Enum.GetValues( typeof( 判定種別 ) ) )
				this._最後にカウント値を設定したときの成績[ judge ] = 判定toヒット数[ judge ];
		}

		protected override void On活性化( グラフィックデバイス gd )
		{
			using( Log.Block( FDKUtilities.現在のメソッド名 ) )
			{
				this.カウントマップ = new int[ カウントマップの最大要素数 ];
				for( int i = 0; i < this.カウントマップ.Length; i++ )
					this.カウントマップ[ i ] = 0;

				this._最後にカウント値を設定した位置 = 0f;
				this._最後にカウント値を設定したときの成績 = new Dictionary<判定種別, int>();
				foreach( 判定種別 judge in Enum.GetValues( typeof( 判定種別 ) ) )
					this._最後にカウント値を設定したときの成績.Add( judge, 0 );
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

			gd.D2DBatchDraw( ( dc ) => {

				using( var 水色ブラシ = new SolidColorBrush( dc, new Color4( 0xffdd8e69 ) ) )
				using( var 黄色ブラシ = new SolidColorBrush( dc, new Color4( 0xff17fffe ) ) )
				{
					var 今回のライン全体の矩形 = new RectangleF( 1357f, 108f, 10f, 768f );
					var 過去最高のライン全体の矩形 = new RectangleF( 1371f, 108f, 6f, 768f );
					const float 単位幅 = 12f;

					// (1) 今回のカウントマップラインを描画する。
					for( int i = 0; i < this.カウントマップ.Length; i++ )
					{
						if( 0 == this.カウントマップ[ i ] )
							continue;

						dc.FillRectangle(
							new RectangleF( 今回のライン全体の矩形.Left, 今回のライン全体の矩形.Bottom - 単位幅 * ( i + 1 ), 今回のライン全体の矩形.Width, 単位幅 ),
							( 2 <= this.カウントマップ[ i ] ) ? 黄色ブラシ : 水色ブラシ );
					}

					// (2) 過去の最高カウントマップラインを描画する。
					if( null != this._過去最大のカウントマップ )
					{
						for( int i = 0; i < this._過去最大のカウントマップ.Length; i++ )
						{
							if( 0 == this._過去最大のカウントマップ[ i ] )
								continue;

							dc.FillRectangle(
								new RectangleF( 過去最高のライン全体の矩形.Left, 過去最高のライン全体の矩形.Bottom - 単位幅 * ( i + 1 ), 過去最高のライン全体の矩形.Width, 単位幅 ),
								( 2 <= this._過去最大のカウントマップ[ i ] ) ? 黄色ブラシ : 水色ブラシ );
						}
					}
				}

			} );
		}

		private int[] _過去最大のカウントマップ = null;
		private float _最後にカウント値を設定した位置 = 0f;
		private Dictionary<判定種別, int> _最後にカウント値を設定したときの成績 = null;
	}
}
