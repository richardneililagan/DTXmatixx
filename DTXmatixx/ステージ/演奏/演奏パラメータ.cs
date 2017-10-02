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
	class 演奏パラメータ : Activity
	{
		public int MaxComboヒット数
		{
			get;
			protected set;
		} = 0;
		public IReadOnlyDictionary<判定種別, int> 判定toヒット数
		{
			get
				=> this._判定toヒット数;
		}
		public IReadOnlyDictionary<判定種別, int> 判定toヒット割合
		{
			get
			{
				int ヒット数の合計 = 0;
				var ヒット割合_実数 = new Dictionary<判定種別, double>();  // 実値（0～100）
				var ヒット割合_整数 = new Dictionary<判定種別, int>(); // 実値を整数にしてさらに補正した値（0～100）
				var ヒット数リスト = new List<(判定種別 judge, int hits)>();
				var 切り捨てした = new Dictionary<判定種別, bool>();
				判定種別 判定;

				// ヒット数の合計を算出。
				for( int i = 0; i < this._判定toヒット数.Count; i++ )
				//foreach( var kvp in this._判定toヒット数 )	--> コレクションが変更されました例外が発生する...
				{
					var kvp = this._判定toヒット数.ElementAt( i );
					ヒット数の合計 += kvp.Value;
				}

				// 各判定のヒット割合（実数）を算出。
				for( int i = 0; i < this._判定toヒット数.Count; i++ )
				//foreach( var kvp in this._判定toヒット数 )	--> コレクションが変更されました例外が発生する...
				{
					var kvp = this._判定toヒット数.ElementAt( i );
					ヒット割合_実数.Add( kvp.Key, ( 100.0 * kvp.Value ) / ヒット数の合計 );
					切り捨てした.Add( kvp.Key, false );   // ついでに初期化。
				}

				// ヒット数の大きいもの順（降順）に、リストを作成。
				foreach( 判定種別 j in Enum.GetValues( typeof( 判定種別 ) ) )
					ヒット数リスト.Add( (j, this._判定toヒット数[ j ]) );

				ヒット数リスト.Sort( ( x, y ) => ( y.hits - x.hits ) );    // 降順

				// ヒット数が一番大きい判定は、ヒット割合の小数部を切り捨てる。
				判定 = ヒット数リスト[ 0 ].judge;
				ヒット割合_整数.Add( 判定, (int) Math.Floor( ヒット割合_実数[ 判定 ] ) );
				切り捨てした[ 判定 ] = true;

				// 以下、二番目以降についてヒット割合（整数）を算出する。
				int 整数割合の合計 = ヒット割合_整数[ 判定 ];

				for( int i = 1; i < ヒット数リスト.Count; i++ )
				{
					判定 = ヒット数リスト[ i ].judge;

					// まずは四捨五入する。
					ヒット割合_整数.Add( 判定, (int) Math.Round( ヒット割合_実数[ 判定 ], MidpointRounding.AwayFromZero ) );

					// 合計が100になり、かつ、まだ後続に非ゼロがいるなら、値を -1 する。
					// → まだ非ゼロの後続がいる場合は、ここで100になってはならない。逆に、後続がすべてゼロなら、ここで100にならなければならない。
					if( 100 <= ( 整数割合の合計 + ヒット割合_整数[ 判定 ] ) )
					{
						bool 後続にまだ非ゼロがいる = false;
						for( int n = ( i + 1 ); n < ヒット数リスト.Count; n++ )
						{
							if( ヒット数リスト[ n ].hits > 0 )
							{
								後続にまだ非ゼロがいる = true;
								break;
							}
						}
						if( 後続にまだ非ゼロがいる )
						{
							ヒット割合_整数[ 判定 ]--;
							切り捨てした[ 判定 ] = true;
						}
					}

					// 合計に加算して、次へ。
					整数割合の合計 += ヒット割合_整数[ 判定 ];
				}

				// 合計が100に足りない場合は、「四捨五入した値と実数値との差の絶対値」が一番大きい判定に +1 する。
				// ただし、「ヒット数が一番大きい判定」と、「切り捨てした判定」は除外する。
				if( 100 > 整数割合の合計 )
				{
					var 差の絶対値リスト = new List<(判定種別 judge, double 差の絶対値)>();

					for( int i = 1; i < ヒット数リスト.Count; i++ )    // i = 0 （ヒット数が一番大きい判定）は除く
					{
						判定 = ヒット数リスト[ i ].judge;
						差の絶対値リスト.Add( (判定, Math.Abs( ヒット割合_実数[ 判定 ] - ヒット割合_整数[ 判定 ] )) );
					}

					差の絶対値リスト.Sort( ( x, y ) => (int) ( y.差の絶対値 * 1000.0 - x.差の絶対値 * 1000.0 ) );     // 降順; 0.xxxx だと (int) で詰むが、1000倍したらだいたいOk

					// 余るときはたいてい 99 だと思うが、念のため、100になるまで降順に+1していく。
					for( int i = 0; i < 差の絶対値リスト.Count; i++ )
					{
						判定 = 差の絶対値リスト[ i ].judge;

						if( 切り捨てした[ 判定 ] )
							continue;   // 切り捨てした判定は除く

						ヒット割合_整数[ 判定 ]++;
						整数割合の合計++;

						if( 100 <= 整数割合の合計 )
							break;
					}
				}

				return ヒット割合_整数;
			}
		}

		public 演奏パラメータ( bool 単体テスト = false )
		{
			if( !( 単体テスト ) )
			{
				this.子リスト.Add( this._パラメータ文字 = new 画像フォント( @"$(System)images\パラメータ文字.png", @"$(System)images\パラメータ文字矩形.xml" ) );
				this.子リスト.Add( this._判定種別文字 = new 画像( @"$(System)images\パラメータ用判定種別文字.png" ) );
			}
		}
		protected override void On活性化( グラフィックデバイス gd )
		{
			using( Log.Block( FDKUtilities.現在のメソッド名 ) )
			{
				this._判定種別文字の矩形リスト = new 矩形リスト( @"$(System)images\パラメータ用判定種別文字矩形.xml" );

				this._判定toヒット数 = new Dictionary<判定種別, int>();
				foreach( 判定種別 judge in Enum.GetValues( typeof( 判定種別 ) ) )
					this._判定toヒット数.Add( judge, 0 );

				this.MaxComboヒット数 = 0;
			}
		}

		public void ヒット数を加算する( 判定種別 judge, int 加算値 = 1 )
		{
			this._判定toヒット数[ judge ] += 加算値;

			if( judge == 判定種別.OK || judge == 判定種別.MISS )
			{
				this.MaxComboヒット数 = 0;  // コンボ切れ
			}
			else
			{
				this.MaxComboヒット数++;
			}
		}
		public virtual void 描画する( DeviceContext dc, float x, float y )
		{
			dc.Transform = Matrix3x2.Identity;

			var 割合表 = this.判定toヒット割合;
			int 合計 = 0;

			this.パラメータを一行描画する( dc, x, y, 判定種別.PERFECT, this._判定toヒット数[ 判定種別.PERFECT ], 割合表[ 判定種別.PERFECT ] );
			合計 += this._判定toヒット数[ 判定種別.PERFECT ];
			y += _改行幅dpx;

			this.パラメータを一行描画する( dc, x, y, 判定種別.GREAT, this._判定toヒット数[ 判定種別.GREAT ], 割合表[ 判定種別.GREAT ] );
			合計 += this._判定toヒット数[ 判定種別.GREAT ];
			y += _改行幅dpx;

			this.パラメータを一行描画する( dc, x, y, 判定種別.GOOD, this._判定toヒット数[ 判定種別.GOOD ], 割合表[ 判定種別.GOOD ] );
			合計 += this._判定toヒット数[ 判定種別.GOOD ];
			y += _改行幅dpx;

			this.パラメータを一行描画する( dc, x, y, 判定種別.OK, this._判定toヒット数[ 判定種別.OK ], 割合表[ 判定種別.OK ] );
			合計 += this._判定toヒット数[ 判定種別.OK ];
			y += _改行幅dpx;

			this.パラメータを一行描画する( dc, x, y, 判定種別.MISS, this._判定toヒット数[ 判定種別.MISS ], 割合表[ 判定種別.MISS ] );
			合計 += this._判定toヒット数[ 判定種別.MISS ];
			y += _改行幅dpx;

			y += 3f;    // ちょっと間を開けて
			var 矩形 = (RectangleF) this._判定種別文字の矩形リスト[ "MaxCombo" ];
			this._判定種別文字.描画する( dc, x, y, 転送元矩形: 矩形 );
			x += 矩形.Width + 16f;
			this.数値を描画する( dc, x, y, this.MaxComboヒット数, 桁数: 4 );
			this.数値を描画する( dc, x + _dr, y, (int) Math.Floor( 100.0 * this.MaxComboヒット数 / 合計 ), 桁数: 3 );    // 切り捨てでいいやもう
			this._パラメータ文字.描画する( dc, x + _dp, y, "%" );
		}
		public void パラメータを一行描画する( DeviceContext dc, float x, float y, 判定種別 judge, int ヒット数, int ヒット割合, float 不透明度 = 1.0f )
		{
			var 矩形 = (RectangleF) this._判定種別文字の矩形リスト[ judge.ToString() ];
			this._判定種別文字.描画する( dc, x, y, 不透明度, 転送元矩形: 矩形 );
			x += 矩形.Width + 16f;

			this.数値を描画する( dc, x, y, ヒット数, 4, 不透明度 );
			this.数値を描画する( dc, x + _dr, y, ヒット割合, 3, 不透明度 );
			this._パラメータ文字.不透明度 = 不透明度;
			this._パラメータ文字.描画する( dc, x + _dp, y, "%" );
		}

		protected const float _dr = 78f;       // 割合(%)までのXオフセット[dpx]
		protected const float _dp = 131f;      // "%" 文字までのXオフセット[dpx]
		protected const float _改行幅dpx = 40f;

		protected Dictionary<判定種別, int> _判定toヒット数 = null;
		protected 画像フォント _パラメータ文字 = null;
		protected 画像 _判定種別文字 = null;
		protected 矩形リスト _判定種別文字の矩形リスト = null;

		protected void 数値を描画する( DeviceContext 描画先dc, float x, float y, int 描画する数値, int 桁数, float 不透明度 = 1.0f )
		{
			Debug.Assert( 1 <= 桁数 && 10 >= 桁数 );    // 最大10桁まで

			int 最大値 = (int) Math.Pow( 10, 桁数 ) - 1;     // 1桁なら9, 2桁なら99, 3桁なら999, ... でカンスト。
			int 判定数 = Math.Max( Math.Min( 描画する数値, 最大値 ), 0 );   // 丸める。
			var 判定数文字列 = 判定数.ToString().PadLeft( 桁数 ).Replace( ' ', 'o' );  // グレーの '0' は 'o' で描画できる（矩形リスト参照）。

			this._パラメータ文字.不透明度 = 不透明度;
			this._パラメータ文字.描画する( 描画先dc, x, y, 判定数文字列 );
		}
	}
}
