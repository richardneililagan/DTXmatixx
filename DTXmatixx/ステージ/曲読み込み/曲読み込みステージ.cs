using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using SharpDX;
using SharpDX.Direct2D1;
using SharpDX.DirectWrite;
using FDK;
using FDK.メディア;
using DTXmatixx.曲;
using SSTFormat.v3;

namespace DTXmatixx.ステージ.曲読み込み
{
	class 曲読み込みステージ : ステージ
	{
		public enum フェーズ
		{
			フェードイン,
			表示,
			完了,
			キャンセル,
		}
		public フェーズ 現在のフェーズ
		{
			get;
			protected set;
		}

		public 曲読み込みステージ()
		{
			this.子リスト.Add( this._舞台画像 = new 舞台画像() );
			this.子リスト.Add( this._注意文 = new 画像( @"$(System)images\ご注意ください.png" ) );
			this.子リスト.Add( this._曲名画像 = new 文字列画像() {
				フォント名 = "HGMaruGothicMPRO",
				フォントサイズpt = 70f,
				フォント幅 = FontWeight.Regular,
				フォントスタイル = FontStyle.Normal,
				描画効果 = 文字列画像.効果.縁取り,
				縁のサイズdpx = 10f,
				前景色 = Color4.Black,
				背景色 = Color4.White,
			} );
			this.子リスト.Add( this._プレビュー画像 = new プレビュー画像() );
			this.子リスト.Add( this._難易度 = new 難易度() );
		}

		protected override void On活性化( グラフィックデバイス gd )
		{
			using( Log.Block( FDKUtilities.現在のメソッド名 ) )
			{
				var 選択曲 = App.曲ツリー.フォーカスノード as MusicNode;
				Debug.Assert( null != 選択曲 );

				this._曲名画像.表示文字列 = 選択曲.タイトル;

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
				this._舞台画像.ぼかしと縮小を適用する( gd, 0.0 );
				App.ステージ管理.現在のアイキャッチ.オープンする( gd );
				this._初めての進行描画 = false;
			}

			this._舞台画像.進行描画する( gd );
			this._注意文.描画する( gd, 0f, 760f );
			this._プレビュー画像.描画する( gd );
			this._難易度.描画する( gd );
			this._曲名を描画する( gd );

			switch( this.現在のフェーズ )
			{
				case フェーズ.フェードイン:
					App.ステージ管理.現在のアイキャッチ.進行描画する( gd );
					if( App.ステージ管理.現在のアイキャッチ.現在のフェーズ == アイキャッチ.フェーズ.オープン完了 )
						this.現在のフェーズ = フェーズ.表示;
					break;

				case フェーズ.表示:
					this._スコアを読み込む();
					this.現在のフェーズ = フェーズ.完了;
					break;

				case フェーズ.完了:
				case フェーズ.キャンセル:
					break;
			}
		}

		private bool _初めての進行描画 = true;
		private 舞台画像 _舞台画像 = null;
		private 画像 _注意文 = null;
		private 文字列画像 _曲名画像 = null;
		private プレビュー画像 _プレビュー画像 = null;
		private 難易度 _難易度 = null;

		private void _曲名を描画する( グラフィックデバイス gd )
		{
			var 表示位置dpx = new Vector2( 782f, 409f );

			// 拡大率を計算して描画する。
			float 最大幅dpx = gd.設計画面サイズ.Width - 表示位置dpx.X;

			this._曲名画像.描画する(
				gd,
				表示位置dpx.X,
				表示位置dpx.Y,
				X方向拡大率: ( this._曲名画像.サイズ.Width <= 最大幅dpx ) ? 1f : 最大幅dpx / this._曲名画像.サイズ.Width );
		}
		private void _スコアを読み込む()
		{
			using( Log.Block( FDKUtilities.現在のメソッド名 ) )
			{
				var 選択ノード = App.曲ツリー.フォーカスノード;
				Debug.Assert( null != 選択ノード );

				var 選択曲 = 選択ノード as MusicNode;
				Debug.Assert( null != 選択曲 );

				string 選択曲ファイルパス = 選択曲.曲ファイルパス;
				Debug.Assert( 選択曲ファイルパス.Nullでも空でもない() );

				var path = Folder.絶対パスに含まれるフォルダ変数を展開して返す( 選択曲ファイルパス );
				var 拡張子名 = Path.GetExtension( path );

				if( ".sstf" == 拡張子名 )
				{
					App.演奏スコア = new スコア( path );
				}
				else if( ".dtx" == 拡張子名 )
				{
					App.演奏スコア = DTXReader.ReadFromFile( path );
				}
				else
				{
					throw new Exception( $"未対応のフォーマットファイルです。[{選択曲ファイルパス}]" );
				}

				// サウンドデバイス遅延を取得し、全チップの発声時刻へ反映する。
				float 再生時遅延ms = (float) ( App.サウンドデバイス.遅延sec * 1000.0 );
				foreach( var chip in App.演奏スコア.チップリスト )
					chip.発声時刻ms -= (long) 再生時遅延ms;

				Log.Info( $"曲ファイルを読み込みました。" );
				Log.Info( $"曲名: {App.演奏スコア.曲名}" );
			}
		}
	}
}
