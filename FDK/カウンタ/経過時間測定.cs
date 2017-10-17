using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FDK.カウンタ
{
	public class 経過時間測定
	{
		public ConcurrentDictionary<string, float> スタック
		{
			get;
			protected set;
		} = new ConcurrentDictionary<string, float>();

		public 経過時間測定()
		{
			this.リセット();
		}

		public void リセット()
		{
			lock( this._lock )
			{
				this.スタック.Clear();

				this._Timer.リセットする();
				this.経過ポイント( "開始" );
			}
		}

		public void 経過ポイント( string ポイント名 )
		{
			lock( this._lock )
			{
				this.スタック.TryAdd( ポイント名, (float) ( this._Timer.現在のリアルタイムカウントsec ) );
			}
		}

		public void 表示()
		{
			lock( this._lock )
			{
				var sortedDic = this.スタック.OrderBy( ( kvp ) => ( kvp.Value ) );
				for( int i = 0; i < sortedDic.Count(); i++ )
				{
					var kvp = sortedDic.ElementAt( i );
					Debug.Write( $"{kvp.Key}:{1000 * kvp.Value:0.00000}ms " );
				}
				Debug.WriteLine( "" );
			}
		}

		private QPCTimer _Timer = new QPCTimer();
		private readonly object _lock = new object();

	}
}
