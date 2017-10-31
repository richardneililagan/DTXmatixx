using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using FDK;
using FDK.メディア;

namespace DTXmatixx.ステージ
{
	class ステージ管理 : Activity, IDisposable
	{
		public string 最初のステージ名
			=> this.ステージリスト.ElementAt( 0 ).Value.GetType().Name;

		public ステージ 現在のステージ
			=> this._現在のステージ;

		public アイキャッチ.アイキャッチ 現在のアイキャッチ
			=> this._現在のアイキャッチ;

		/// <summary>
		///		全ステージのリスト。
		///		新しいステージができたら、ここに追加すること。
		/// </summary>
		public Dictionary<string, ステージ> ステージリスト = new Dictionary<string, ステージ>() {
			{ nameof( 曲ツリー構築.曲ツリー構築ステージ ), new 曲ツリー構築.曲ツリー構築ステージ() },
			{ nameof( タイトル.タイトルステージ ), new タイトル.タイトルステージ() },
			{ nameof( 認証.認証ステージ ), new 認証.認証ステージ() },
			{ nameof( 選曲.選曲ステージ ), new 選曲.選曲ステージ() },
			{ nameof( 曲読み込み.曲読み込みステージ ), new 曲読み込み.曲読み込みステージ() },
			{ nameof( 演奏.演奏ステージ ), new 演奏.演奏ステージ() },
			{ nameof( 結果.結果ステージ ), new 結果.結果ステージ() },
		};

		public ステージ管理()
		{
			// 各ステージの外部依存アクションを接続。
			var 結果ステージ = (結果.結果ステージ) this.ステージリスト[ nameof( 結果.結果ステージ ) ];
			var 演奏ステージ = (演奏.演奏ステージ) this.ステージリスト[ nameof( 演奏.演奏ステージ ) ];
			結果ステージ.結果を取得する = () => ( 演奏ステージ.成績 );
		}
		public void Dispose()
		{
			throw new InvalidOperationException( "このメソッドは使用できません。別のオーバーロードメソッドを使用してください。" );
		}
		public void Dispose( グラフィックデバイス gd )
		{
			Debug.Assert( null != gd );

			// 現在活性化しているステージがあれば、すべて非活性化する。
			foreach( var kvp in this.ステージリスト )
			{
				if( kvp.Value.活性化している )
				{
					kvp.Value.非活性化する( gd );
				}
			}
			// 現在活性化しているアイキャッチがあれば、すべて非活性化する。
			foreach( var kvp in this._アイキャッチリスト )
			{
				if( kvp.Value.活性化している )
				{
					kvp.Value.非活性化する( gd );
				}
			}

		}
		protected override void On活性化( グラフィックデバイス gd )
		{
			using( Log.Block( FDKUtilities.現在のメソッド名 ) )
			{
				// 全ステージの初期化
				if( this.現在のステージ?.活性化していない ?? false )
					this.現在のステージ?.活性化する( gd );

				// 全アイキャッチの初期化
				foreach( var kvp in this._アイキャッチリスト )
					kvp.Value.活性化する( gd );

				// 現在のアイキャッチを設定。
				this._現在のアイキャッチ = this._アイキャッチリスト.ElementAt( 0 ).Value;
			}
		}
		protected override void On非活性化( グラフィックデバイス gd )
		{
			using( Log.Block( FDKUtilities.現在のメソッド名 ) )
			{
				if( this.現在のステージ?.活性化している ?? false )
					this.現在のステージ?.非活性化する( gd );

				foreach( var kvp in this._アイキャッチリスト )
					kvp.Value.非活性化する( gd );

				this._現在のアイキャッチ = null;
			}
		}
		
		/// <summary>
		///		現在のステージを非活性化し、指定されたステージに遷移して、活性化する。
		/// </summary>
		/// <param name="遷移先ステージ名">Nullまたは空文字列なら、非活性化のみ行う。</param>
		public void ステージを遷移する( グラフィックデバイス gd, string 遷移先ステージ名 )
		{
			Log.Header( $"{遷移先ステージ名} へ遷移します。" );

			using( Log.Block( FDKUtilities.現在のメソッド名 ) )
			{
				if( null != this._現在のステージ &&
					this._現在のステージ.活性化している )
				{
					this._現在のステージ.非活性化する( gd );
				}

				if( 遷移先ステージ名.Nullでも空でもない() )
				{
					this._現在のステージ = this.ステージリスト[ 遷移先ステージ名 ];
					this._現在のステージ.活性化する( gd );

					//App.入力管理.すべての入力デバイスをポーリングする();
				}
				else
				{
					Log.Header( "ステージの遷移を終了します。" );
					this._現在のステージ = null;
				}
			}
		}
		public void アイキャッチを選択しクローズする( グラフィックデバイス gd, string 名前 )
		{
			this._現在のアイキャッチ = this._アイキャッチリスト[ 名前 ];
			this._現在のアイキャッチ.クローズする( gd );
		}

		/// <summary>
		///		現在実行中のステージ。<see cref="ステージリスト"/> の中のひとつを参照している（ので、うかつにDisposeとかしたりしないこと）。
		/// </summary>
		private ステージ _現在のステージ;

		/// <summary>
		///		現在選択中のアイキャッチ。アイキャッチリストの中のひとつを参照している（ので、うかつにDisposeとかしたりしないこと）。
		/// </summary>
		private アイキャッチ.アイキャッチ _現在のアイキャッチ = null;

		private Dictionary<string, アイキャッチ.アイキャッチ> _アイキャッチリスト = new Dictionary<string, アイキャッチ.アイキャッチ>() {
			{ nameof( アイキャッチ.シャッター ), new アイキャッチ.シャッター() },
			{ nameof( アイキャッチ.回転幕 ), new アイキャッチ.回転幕() },
			{ nameof( アイキャッチ.GO ), new アイキャッチ.GO() },
			{ nameof( アイキャッチ.半回転黒フェード ), new アイキャッチ.半回転黒フェード() },
		};
	}
}
