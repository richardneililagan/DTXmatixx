using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SharpDX;
using SharpDX.DirectInput;
using FDK;
using FDK.メディア;
using FDK.カウンタ;
using DTXmatixx.アイキャッチ;

namespace DTXmatixx.ステージ.認証
{
	/// <summary>
	///		ユーザ選択画面。
	/// </summary>
	class 認証ステージ : ステージ
	{
		public enum フェーズ
		{
			フェードイン,
			ユーザ選択,
			フェードアウト,
			ユーザ切り替え,
			確定,
			キャンセル,
		}
		public フェーズ 現在のフェーズ
		{
			get;
			protected set;
		}

		public 認証ステージ()
		{
			this.子リスト.Add( this._舞台画像 = new 舞台画像() );
			this.子リスト.Add( this._ウィンドウ画像 = new 画像( @"$(System)images\認証画面_ユーザ選択ウィンドウ.png" ) );
			this.子リスト.Add( this._プレイヤーを選択してください = new 文字列画像() { 表示文字列 = "プレイヤーを選択してください。", フォントサイズpt = 30f, 描画効果 = 文字列画像.効果.ドロップシャドウ } );
			this.子リスト.Add( this._ユーザリスト = new ユーザリスト() );
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
				App.ステージ管理.現在のアイキャッチ.オープンする( gd );
				this._初めての進行描画 = false;
			}

			var 描画領域 = new RectangleF( 566f, 60f, 784f, 943f );

			App.入力管理.すべての入力デバイスをポーリングする();

			switch( this.現在のフェーズ )
			{
				case フェーズ.フェードイン:
				case フェーズ.ユーザ選択:
					#region " *** "
					//----------------
					this._舞台画像.進行描画する( gd, 黒幕付き: true );
					this._ウィンドウ画像.描画する( gd, 描画領域.X, 描画領域.Y );
					this._プレイヤーを選択してください.描画する( gd, 描画領域.X + 28f, 描画領域.Y + 45f );
					this._ユーザリスト.進行描画する( gd );

					// 以下、フェーズ別に処理分岐。
					if( this.現在のフェーズ == フェーズ.フェードイン )
					{
						App.ステージ管理.現在のアイキャッチ.進行描画する( gd );
						if( App.ステージ管理.現在のアイキャッチ.現在のフェーズ == アイキャッチ.フェーズ.オープン完了 )
						{
							this.現在のフェーズ = フェーズ.ユーザ選択;
						}
					}
					else if( this.現在のフェーズ == フェーズ.ユーザ選択 )
					{
						if( App.入力管理.シンバルが入力された() || App.入力管理.Keyboard.キーが押された( 0, Key.Return ) )
						{
							App.ステージ管理.アイキャッチを選択しクローズする( gd, nameof( 回転幕 ) );
							this.現在のフェーズ = フェーズ.フェードアウト;
						}
						else if( App.入力管理.Keyboard.キーが押された( 0, Key.Escape ) )
						{
							this.現在のフェーズ = フェーズ.キャンセル;
						}
						else if( App.入力管理.ドラムが入力された( 入力.ドラム入力種別.Tom1 ) || App.入力管理.Keyboard.キーが押された( 0, Key.Up ) )
						{
							this._ユーザリスト.前のユーザを選択する();
						}
						else if( App.入力管理.ドラムが入力された( 入力.ドラム入力種別.Tom2 ) || App.入力管理.Keyboard.キーが押された( 0, Key.Down ) )
						{
							this._ユーザリスト.次のユーザを選択する();
						}
					}
					//----------------
					#endregion
					break;

				case フェーズ.フェードアウト:
					#region " *** "
					//----------------
					this._舞台画像.進行描画する( gd, true );

					App.ステージ管理.現在のアイキャッチ.進行描画する( gd );
					if( App.ステージ管理.現在のアイキャッチ.現在のフェーズ == アイキャッチ.フェーズ.クローズ完了 )
					{
						this.現在のフェーズ = フェーズ.ユーザ切り替え;
					}
					//----------------
					#endregion
					break;

				case フェーズ.ユーザ切り替え:
					#region " *** "
					//----------------
					App.ステージ管理.現在のアイキャッチ.進行描画する( gd );

					// ログオフ
					if( null != App.ユーザ管理.ログオン中のユーザ )
					{
						// 特になし
						Log.Info( $"ユーザ「{App.ユーザ管理.ログオン中のユーザ.ユーザ名}」をログオフしました。" );
					}

					// ログイン
					App.ユーザ管理.ユーザリスト.SelectItem( this._ユーザリスト.選択中のユーザ );
					Log.Info( $"ユーザ「{App.ユーザ管理.ログオン中のユーザ.ユーザ名}」でログインしました。" );

					this.現在のフェーズ = フェーズ.確定;
					//----------------
					#endregion
					break;

				case フェーズ.確定:
				case フェーズ.キャンセル:
					break;
			}
		}

		private bool _初めての進行描画 = true;
		private 舞台画像 _舞台画像 = null;
		private 画像 _ウィンドウ画像 = null;
		private 文字列画像 _プレイヤーを選択してください = null;
		private ユーザリスト _ユーザリスト = null;
	}
}
