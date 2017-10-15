using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using SharpDX;
using SharpDX.Direct2D1;
using SharpDX.DirectInput;
using CSCore;
using FDK;
using FDK.メディア;
using FDK.メディア.サウンド.WASAPI;
using FDK.カウンタ;
using SSTFormat.v3;
using DTXmatixx.設定;
using DTXmatixx.設定.DB;

namespace DTXmatixx.ステージ.演奏
{
	class 演奏ステージ : ステージ
	{
		public const float ヒット判定バーの中央Y座標dpx = 847f;

		public enum フェーズ
		{
			フェードイン,
			表示,
			クリア,
			キャンセル通知,    // 高速進行スレッドから設定
			キャンセル時フェードアウト,
			キャンセル完了,
		}
		public フェーズ 現在のフェーズ
		{
			get;
			protected set;
		}

		public Bitmap キャプチャ画面
		{
			get;
			set;
		} = null;
		public 成績 成績
		{
			get;
			protected set;
		} = null;

		public 演奏ステージ()
		{
			this.子リスト.Add( this._背景画像 = new 画像( @"$(System)images\演奏画面.png" ) );
			this.子リスト.Add( this._レーンフレーム = new レーンフレーム() );
			this.子リスト.Add( this._曲名パネル = new 曲名パネル() );
			this.子リスト.Add( this._ヒットバー画像 = new 画像( @"$(System)images\演奏画面_ヒットバー.png" ) );
			this.子リスト.Add( this._ドラムパッド = new ドラムパッド() );
			this.子リスト.Add( this._レーンフラッシュ = new レーンフラッシュ() );
			this.子リスト.Add( this._ドラムチップ画像 = new 画像( @"$(System)images\ドラムチップ.png" ) );
			this.子リスト.Add( this._判定文字列 = new 判定文字列() );
			this.子リスト.Add( this._チップ光 = new チップ光() );
			this.子リスト.Add( this._左サイドクリアパネル = new 左サイドクリアパネル() );
			this.子リスト.Add( this._右サイドクリアパネル = new 右サイドクリアパネル() );
			this.子リスト.Add( this._判定パラメータ表示 = new 判定パラメータ表示() );
			this.子リスト.Add( this._フェーズパネル = new フェーズパネル() );
			this.子リスト.Add( this._コンボ表示 = new コンボ表示() );
			this.子リスト.Add( this._カウントマップライン = new カウントマップライン() );
			this.子リスト.Add( this._スコア表示 = new スコア表示() );
			this.子リスト.Add( this._プレイヤー名表示 = new プレイヤー名表示() );
			this.子リスト.Add( this._譜面スクロール速度表示 = new 譜面スクロール速度表示() );
			this.子リスト.Add( this._達成率表示 = new 達成率表示() );
			this.子リスト.Add( this._曲別SKILL = new 曲別SKILL() );
			this.子リスト.Add( this._FPS = new FPS() );
		}
		protected override void On活性化( グラフィックデバイス gd )
		{
			using( Log.Block( FDKUtilities.現在のメソッド名 ) )
			{
				this.キャプチャ画面 = null;

				this.成績 = new 成績( App.演奏スコア, App.ユーザ設定 );

				this._描画開始チップ番号 = -1;
				this._小節線色 = new SolidColorBrush( gd.D2DDeviceContext, Color.White );
				this._拍線色 = new SolidColorBrush( gd.D2DDeviceContext, Color.LightGray );
				this._ドラムチップ画像の矩形リスト = new 矩形リスト( @"$(System)images\ドラムチップ矩形.xml" );      // デバイスリソースは持たないので、子Activityではない。
				this._現在進行描画中の譜面スクロール速度の倍率 = App.ユーザ設定.譜面スクロール速度の倍率;
				this._ドラムチップアニメ = new LoopCounter( 0, 200, 3 );
				this._背景動画 = null;
				this._BGM = null;
				this._背景動画開始済み = false;
				this._BGM再生開始済み = false;
				//this._デコード済みWaveSource = null;	--> キャッシュなので消さない。
				this._プレイヤー名表示.名前 = App.ユーザ設定.ユーザ名;

				#region " 背景動画とBGMを生成する。"
				//----------------
				if( ( null != App.演奏スコア ) && ( App.演奏スコア.背景動画ファイル名.Nullでも空でもない() ) )
				{
					Log.Info( "背景動画とBGMを読み込みます。" );

					// 動画を子リストに追加。
					this.子リスト.Add( this._背景動画 = new 動画( App.演奏スコア.背景動画ファイル名 ) );

					// 動画から音声パートを抽出して BGM を作成。

					// todo: キャッシュへの対応
					//if( ( null != this._デコード済みWaveSource ) && this._デコード済みWaveSource.Path.Equals( App.演奏スコア.背景動画ファイル名 ) )
					//{
					//	// (A) 前回生成したBGMとパスが同じなので、前回のデコード済み WaveSource をキャッシュとして再利用する。
					//	Log.Info( "前回生成したサウンドデータを再利用します。" );
					//}
					//else
					{
						// (B) 初めての生成か、または前回生成したBGMとパスが違うので、新しくデコード済み WaveSource を生成する。
						this._デコード済みWaveSource?.Dispose();
						this._デコード済みWaveSource = SampleSourceFactory.Create( App.サウンドデバイス, App.演奏スコア.背景動画ファイル名 );
					}

					this._BGM?.Dispose();
					this._BGM = new Sound( App.サウンドデバイス, this._デコード済みWaveSource );
				}
				else
				{
					Log.Info( "背景動画とBGMはありません。" );
				}
				//----------------
				#endregion
				#region " WAVを生成する（ある場合）。"
				//----------------
				App.WAV管理 = new 曲.WAV管理();

				if( null != App.演奏スコア )
				{
					foreach( var kvp in App.演奏スコア.dicWAV )
					{
						var path = Path.Combine( App.演奏スコア.PATH_WAV, kvp.Value.ファイルパス );
						App.WAV管理.登録する( App.サウンドデバイス, kvp.Key, path, kvp.Value.多重再生する );
					}
				}
				//----------------
				#endregion

				this.現在のフェーズ = フェーズ.フェードイン;
				this._初めての進行描画 = true;
			}
		}
		protected override void On非活性化( グラフィックデバイス gd )
		{
			using( Log.Block( FDKUtilities.現在のメソッド名 ) )
			{
				#region " 現在の譜面スクロール速度をDBに保存。"
				//----------------
				using( var userdb = new UserDB() )
				{
					var user = userdb.Users.Where(
						( u ) => ( u.Id == App.ユーザ設定.ID )
						).SingleOrDefault();

					if( null != user )
					{
						user.ScrollSpeed = App.ユーザ設定.譜面スクロール速度の倍率;
						userdb.DataContext.SubmitChanges();

						Log.Info( $"現在の譜面スクロール速度({App.ユーザ設定.譜面スクロール速度の倍率})をDBに保存しました。[UserID={user.Id}]" );
					}
				}
				//----------------
				#endregion

				// 背景動画を生成した場合は子リストから削除。
				if( null != this._背景動画 )
					this.子リスト.Remove( this._背景動画 );

				//App.WAV管理?.Dispose();	--> ここではまだ解放しない。結果ステージの非活性化時に解放する。
				//App.WAV管理 = null;

				FDKUtilities.解放する( ref this._拍線色 );
				FDKUtilities.解放する( ref this._小節線色 );

				this.キャプチャ画面?.Dispose();
				this.キャプチャ画面 = null;
			}
		}

		/// <summary>
		///		進行と入力。
		/// </summary>
		public override void 高速進行する()
		{
			if( this._初めての進行描画 )
			{
				App.サウンドタイマ.リセットする();		// カウント開始
				this._フェードインカウンタ = new Counter( 0, 100, 10 );
				this._初めての進行描画 = false;
			}

			// 高速進行

			this._FPS.FPSをカウントしプロパティを更新する();

			#region " 背景動画が再生されているのにBGMがまだ再生されていないなら、すぐに再生を開始する。"
			//----------------
			if( this._背景動画開始済み && !( this._BGM再生開始済み ) )
			{
				this._BGM?.Play();
				this._BGM再生開始済み = true;
			}
			//----------------
			#endregion

			switch( this.現在のフェーズ )
			{
				case フェーズ.フェードイン:
					if( this._フェードインカウンタ.終了値に達した )
					{
						// フェードインが終わってから演奏開始。
						Log.Info( "演奏を開始します。" );
						this._描画開始チップ番号 = 0; // -1 から 0 に変われば演奏開始。
						App.サウンドタイマ.リセットする();

						this.現在のフェーズ = フェーズ.表示;
					}
					break;

				case フェーズ.表示:

					double 現在の演奏時刻sec = this._演奏開始からの経過時間secを返す();

					// AutoPlay 判定

					#region " 自動ヒット処理。"
					//----------------
					this._描画範囲のチップに処理を適用する( 現在の演奏時刻sec, ( chip, index, ヒット判定バーと描画との時間sec, ヒット判定バーと発声との時間sec, ヒット判定バーとの距離 ) => {

						var オプション設定 = App.ユーザ設定;
						var 対応表 = オプション設定.ドラムとチップと入力の対応表[ chip.チップ種別 ];
						var AutoPlay = オプション設定.AutoPlay[ 対応表.AutoPlay種別 ];

						bool チップはヒット済みである = chip.ヒット済みである;
						bool チップはMISSエリアに達している = ( ヒット判定バーと描画との時間sec > オプション設定.最大ヒット距離sec[ 判定種別.OK ] );
						bool チップは描画についてヒット判定バーを通過した = ( 0 <= ヒット判定バーと描画との時間sec );
						bool チップは発声についてヒット判定バーを通過した = ( 0 <= ヒット判定バーと発声との時間sec );

						if( チップはヒット済みである )
						{
							// 何もしない。
							return;
						}

						if( チップはMISSエリアに達している )
						{
							// MISS判定。
							if( AutoPlay && 対応表.AutoPlayON.MISS判定 )
							{
								this._チップのヒット処理を行う( chip, 判定種別.MISS, 対応表.AutoPlayON.自動ヒット時処理, ヒット判定バーと発声との時間sec );
								return;
							}
							else if( !AutoPlay && 対応表.AutoPlayOFF.MISS判定 )
							{
								this._チップのヒット処理を行う( chip, 判定種別.MISS, 対応表.AutoPlayOFF.ユーザヒット時処理, ヒット判定バーと発声との時間sec );
								return;
							}
							else
							{
								// 通過。
							}
						}

						if( チップは発声についてヒット判定バーを通過した )
						{
							// 自動ヒット判定。
							if( ( AutoPlay && 対応表.AutoPlayON.自動ヒット && 対応表.AutoPlayON.自動ヒット時処理.再生 ) ||
								( !AutoPlay && 対応表.AutoPlayOFF.自動ヒット && 対応表.AutoPlayOFF.自動ヒット時処理.再生 ) )
							{
								this._チップの発声を行う( chip, ヒット判定バーと発声との時間sec );
							}
						}

						if( チップは描画についてヒット判定バーを通過した )
						{
							// 自動ヒット判定。
							if( AutoPlay && 対応表.AutoPlayON.自動ヒット )
							{
								// Auto は Perfect 扱い
								this._チップのヒット処理を行う( chip, 判定種別.PERFECT, 対応表.AutoPlayON.自動ヒット時処理, ヒット判定バーと発声との時間sec );
								return;
							}
							else if( !AutoPlay && 対応表.AutoPlayOFF.自動ヒット )
							{
								// Auto は Perfect 扱い
								this._チップのヒット処理を行う( chip, 判定種別.PERFECT, 対応表.AutoPlayOFF.自動ヒット時処理, ヒット判定バーと発声との時間sec );
								return;
							}
							else
							{
								// 通過。
							}
						}

					} );
					//----------------
					#endregion


					// 入力

					App.Keyboard.ポーリングする();
					if( App.Keyboard.キーが押された( 0, Key.Escape ) )
					{
						#region " ESC → 演奏中断 "
						//----------------
						Log.Info( "演奏を中断します。" );
						this.BGMを停止する();
						this.現在のフェーズ = フェーズ.キャンセル通知;	// 通知
						//----------------
						#endregion
					}
					if( App.Keyboard.キーが押された( 0, Key.Up ) )
					{
						#region " 上 → 譜面スクロールを加速 "
						//----------------
						const double 最大倍率 = 8.0;
						App.ユーザ設定.譜面スクロール速度の倍率 = Math.Min( App.ユーザ設定.譜面スクロール速度の倍率 + 0.5, 最大倍率 );
						//----------------
						#endregion
					}
					if( App.Keyboard.キーが押された( 0, Key.Down ) )
					{
						#region " 下 → 譜面スクロールを減速 "
						//----------------
						const double 最小倍率 = 0.5;
						App.ユーザ設定.譜面スクロール速度の倍率 = Math.Max( App.ユーザ設定.譜面スクロール速度の倍率 - 0.5, 最小倍率 );
						//----------------
						#endregion
					}
					break;
			}
		}

		/// <summary>
		///		描画。
		/// </summary>
		/// <param name="gd"></param>
		public override void 進行描画する( グラフィックデバイス gd )
		{
			// 進行描画

			if( this._初めての進行描画 )
				return; // まだ最初の高速進行が行われていない。

			switch( this.現在のフェーズ )
			{
				case フェーズ.フェードイン:
					{
						this._左サイドクリアパネル.クリアする( gd );
						this._左サイドクリアパネル.クリアパネル.ビットマップへ描画する( gd, ( dc, bmp ) => {
							this._プレイヤー名表示.進行描画する( dc );
							this._スコア表示.進行描画する( dc, gd.Animation, new Vector2( +280f, +120f ), this.成績 );
							this._達成率表示.描画する( dc, this.成績.達成率 );
							this._判定パラメータ表示.描画する( dc, +118f, +372f, this.成績 );
							this._曲別SKILL.進行描画する( dc, 0f );
						} );
						this._左サイドクリアパネル.描画する( gd );

						this._右サイドクリアパネル.クリアする( gd );
						this._右サイドクリアパネル.描画する( gd );

						this._レーンフレーム.描画する( gd );
						this._ドラムパッド.進行描画する( gd );
						this._背景画像.描画する( gd, 0f, 0f );
						this._譜面スクロール速度表示.進行描画する( gd, App.ユーザ設定.譜面スクロール速度の倍率 );

						this._カウントマップライン.進行描画する( gd );
						this._フェーズパネル.進行描画する( gd );
						this._曲名パネル.描画する( gd );
						this._ヒットバーを描画する( gd );
						this._キャプチャ画面を描画する( gd, ( 1.0f - this._フェードインカウンタ.現在値の割合 ) );
					}
					break;

				case フェーズ.表示:
				case フェーズ.キャンセル時フェードアウト:
					{
						#region " 譜面スクロール速度が変化している → 追い付き進行 "
						//----------------
						{
							double 倍率 = this._現在進行描画中の譜面スクロール速度の倍率;

							if( 倍率 < App.ユーザ設定.譜面スクロール速度の倍率 )
							{
								if( 0 > this._スクロール倍率追い付き用_最後の値 )
								{
									this._スクロール倍率追い付き用カウンタ = new LoopCounter( 0, 1000, 10 );    // 0→100; 全部で10×1000 = 10000ms = 10sec あれば十分だろう
									this._スクロール倍率追い付き用_最後の値 = 0;
								}
								else
								{
									while( this._スクロール倍率追い付き用_最後の値 < this._スクロール倍率追い付き用カウンタ.現在値 )
									{
										倍率 += 0.025;
										this._スクロール倍率追い付き用_最後の値++;
									}

									this._現在進行描画中の譜面スクロール速度の倍率 = Math.Min( 倍率, App.ユーザ設定.譜面スクロール速度の倍率 );
								}
							}
							else if( 倍率 > App.ユーザ設定.譜面スクロール速度の倍率 )
							{
								if( 0 > this._スクロール倍率追い付き用_最後の値 )
								{
									this._スクロール倍率追い付き用カウンタ = new LoopCounter( 0, 1000, 10 );    // 0→100; 全部で10×1000 = 10000ms = 10sec あれば十分だろう
									this._スクロール倍率追い付き用_最後の値 = 0;
								}
								else
								{
									while( this._スクロール倍率追い付き用_最後の値 < this._スクロール倍率追い付き用カウンタ.現在値 )
									{
										倍率 -= 0.025;
										this._スクロール倍率追い付き用_最後の値++;
									}

									this._現在進行描画中の譜面スクロール速度の倍率 = Math.Max( 倍率, App.ユーザ設定.譜面スクロール速度の倍率 );
								}
							}
							else
							{
								this._スクロール倍率追い付き用_最後の値 = -1;
								this._スクロール倍率追い付き用カウンタ = null;
							}
						}
						//----------------
						#endregion

						double 演奏時刻sec = this._演奏開始からの経過時間secを返す() + gd.次のDComp表示までの残り時間sec;

						if( this._背景動画開始済み )
						{
							// 背景動画チップがヒット済みなら、背景動画の進行描画を行う。
							this._背景動画?.描画する( gd, new RectangleF( 0f, 0f, gd.設計画面サイズ.Width, gd.設計画面サイズ.Height ), 1.0f );
	
							// 開始直後のデコードが重たいかもしれないので、演奏時刻をここで更新しておく。	---> 重たくても更新禁止！（譜面スクロールがガタつく原因になる）
							//演奏時刻sec = this._演奏開始からの経過時間secを返す();
						}

						this._左サイドクリアパネル.クリアする( gd );
						this._左サイドクリアパネル.クリアパネル.ビットマップへ描画する( gd, ( dc, bmp ) => {
							this._プレイヤー名表示.進行描画する( dc );
							this._スコア表示.進行描画する( dc, gd.Animation, new Vector2( +280f, +120f ), this.成績 );
							this._達成率表示.描画する( dc, this.成績.達成率 );
							this._判定パラメータ表示.描画する( dc, +118f, +372f, this.成績 );
							this._曲別SKILL.進行描画する( dc, this.成績.Skill );
						} );
						this._左サイドクリアパネル.描画する( gd );

						this._右サイドクリアパネル.クリアする( gd );
						this._右サイドクリアパネル.クリアパネル.ビットマップへ描画する( gd, ( dc, bmp ) => {
							this._コンボ表示.進行描画する( dc, gd.Animation, new Vector2( +228f + 264f/2f, +234f ), this.成績 );
						} );
						this._右サイドクリアパネル.描画する( gd );

						this._レーンフラッシュ.進行描画する( gd );
						this._小節線拍線を描画する( gd, 演奏時刻sec );
						this._レーンフレーム.描画する( gd );
						this._ドラムパッド.進行描画する( gd );
						this._背景画像.描画する( gd, 0f, 0f );
						this._譜面スクロール速度表示.進行描画する( gd, App.ユーザ設定.譜面スクロール速度の倍率 );

						double 曲の長さsec = App.演奏スコア.チップリスト[ App.演奏スコア.チップリスト.Count - 1 ].描画時刻sec;
						float 現在位置 = (float) ( 1.0 - ( 曲の長さsec - 演奏時刻sec ) / 曲の長さsec );
						this._カウントマップライン.カウント値を設定する( 現在位置, this.成績.判定toヒット数 );
						this._カウントマップライン.進行描画する( gd );
						this._フェーズパネル.現在位置 = 現在位置;
						this._フェーズパネル.進行描画する( gd );
						this._曲名パネル.描画する( gd );
						this._ヒットバーを描画する( gd );
						this._チップを描画する( gd, 演奏時刻sec );
						this._チップ光.進行描画する( gd );
						this._判定文字列.進行描画する( gd );
						this._FPS.VPSをカウントする();
						this._FPS.描画する( gd, 0f, 0f );

						if( this.現在のフェーズ == フェーズ.キャンセル時フェードアウト )
						{
							App.ステージ管理.現在のアイキャッチ.進行描画する( gd );
							if( App.ステージ管理.現在のアイキャッチ.現在のフェーズ == アイキャッチ.フェーズ.クローズ完了 )
								this.現在のフェーズ = フェーズ.キャンセル完了;
						}
					}
					break;

				case フェーズ.キャンセル通知:
					App.ステージ管理.アイキャッチを選択しクローズする( gd, nameof( アイキャッチ.半回転黒フェード ) );
					this.現在のフェーズ = フェーズ.キャンセル時フェードアウト;
					break;

				case フェーズ.キャンセル完了:
					break;

				case フェーズ.クリア:
					break;
			}
		}

		public void 演奏を停止する()
		{
			using( Log.Block( FDKUtilities.現在のメソッド名 ) )
			{
				this._描画開始チップ番号 = -1;   // 演奏停止

				this.BGMを停止する();
				this._背景動画開始済み = false;

				//this._コンボ.COMBO値 = 0;
			}
		}
		/// <remarks>
		///		演奏クリア時には、次の結果ステージに入ってもBGMが鳴り続ける。
		///		そのため、後からBGMだけを別個に停止するためのメソッドが必要になる。
		/// </remarks>
		public void BGMを停止する()
		{
			using( Log.Block( FDKUtilities.現在のメソッド名 ) )
			{
				this._BGM?.Stop();
				this._BGM?.Dispose();
				this._BGM = null;

				//this._デコード済みWaveSource?.Dispose();	--> ここではまだ解放しない。
				//this._デコード済みWaveSource = null;
			}
		}
		public void BGMのキャッシュを解放する()
		{
			using( Log.Block( FDKUtilities.現在のメソッド名 ) )
			{
				this.BGMを停止する();
				FDKUtilities.解放する( ref this._デコード済みWaveSource );
			}
		}

		private bool _初めての進行描画 = true;

		private 画像 _背景画像 = null;
		private レーンフレーム _レーンフレーム = null;
		private 曲名パネル _曲名パネル = null;
		private ドラムパッド _ドラムパッド = null;
		private レーンフラッシュ _レーンフラッシュ = null;
		private 判定文字列 _判定文字列 = null;
		private チップ光 _チップ光 = null;
		private 左サイドクリアパネル _左サイドクリアパネル = null;
		private 右サイドクリアパネル _右サイドクリアパネル = null;
		private フェーズパネル _フェーズパネル = null;
		private コンボ表示 _コンボ表示 = null;
		private カウントマップライン _カウントマップライン = null;
		private スコア表示 _スコア表示 = null;
		private 判定パラメータ表示 _判定パラメータ表示 = null;
		private プレイヤー名表示 _プレイヤー名表示 = null;
		private 譜面スクロール速度表示 _譜面スクロール速度表示 = null;
		private 達成率表示 _達成率表示 = null;
		private 曲別SKILL _曲別SKILL = null;
		private FPS _FPS = null;
		/// <summary>
		///		読み込み画面: 0 ～ 1: 演奏画面
		/// </summary>
		private Counter _フェードインカウンタ = null;

		private double _現在進行描画中の譜面スクロール速度の倍率 = 1.0;
		private LoopCounter _スクロール倍率追い付き用カウンタ = null;
		private int _スクロール倍率追い付き用_最後の値 = -1;

		/// <summary>
		///		<see cref="スコア表示.チップリスト"/> のうち、描画を始めるチップのインデックス番号。
		///		未演奏時・演奏終了時は -1 。
		/// </summary>
		/// <remarks>
		///		演奏開始直後は 0 で始まり、対象番号のチップが描画範囲を流れ去るたびに +1 される。
		///		このメンバの更新は、高頻度進行タスクではなく、進行描画メソッドで行う。（低精度で構わないので）
		/// </remarks>
		private int _描画開始チップ番号 = -1;

		private double _演奏開始からの経過時間secを返す()
		{
			return App.サウンドタイマ.現在時刻sec;
		}

		/// <summary>
		///		<see cref="_描画開始チップ番号"/> から画面上端にはみ出すまでの間の各チップに対して、指定された処理を適用する。
		/// </summary>
		/// <param name="適用する処理">
		///		引数は、順に、対象のチップ, チップ番号, ヒット判定バーと描画との時間sec, ヒット判定バーと発声との時間sec, ヒット判定バーとの距離dpx。
		///		時間と距離はいずれも、負数ならバー未達、0でバー直上、正数でバー通過。
		///	</param>
		private void _描画範囲のチップに処理を適用する( double 現在の演奏時刻sec, Action<チップ, int, double, double, double> 適用する処理 )
		{
			var スコア = App.演奏スコア;
			if( null == スコア )
				return;

			for( int i = this._描画開始チップ番号; ( 0 <= i ) && ( i < スコア.チップリスト.Count ); i++ )
			{
				var チップ = スコア.チップリスト[ i ];

				// ヒット判定バーとチップの間の、時間 と 距離 を算出。→ いずれも、負数ならバー未達、0でバー直上、正数でバー通過。
				double ヒット判定バーと描画との時間sec = 現在の演奏時刻sec - チップ.描画時刻sec;
				double ヒット判定バーと発声との時間sec = 現在の演奏時刻sec - チップ.発声時刻sec;
				double ヒット判定バーとの距離dpx = スコア.指定された時間secに対応する符号付きピクセル数を返す( this._現在進行描画中の譜面スクロール速度の倍率, ヒット判定バーと描画との時間sec );

				// 終了判定。
				bool チップは画面上端より上に出ている = ( ( ヒット判定バーの中央Y座標dpx + ヒット判定バーとの距離dpx ) < -40.0 );   // -40 はチップが隠れるであろう適当なマージン。
				if( チップは画面上端より上に出ている )
					break;

				// 処理実行。開始判定（描画開始チップ番号の更新）もこの中で。
				適用する処理( チップ, i, ヒット判定バーと描画との時間sec, ヒット判定バーと発声との時間sec, ヒット判定バーとの距離dpx );
			}
		}

		private 画像 _ヒットバー画像 = null;
		private void _ヒットバーを描画する( グラフィックデバイス gd )
		{
			this._ヒットバー画像.描画する( gd, 441f, ヒット判定バーの中央Y座標dpx - 4f );    // 4f がバーの厚みの半分[dpx]。
		}

		// 小節線・拍線 と チップ は描画階層（奥行き）が異なるので、別々のメソッドに分ける。
		private SolidColorBrush _小節線色 = null;
		private SolidColorBrush _拍線色 = null;
		private void _小節線拍線を描画する( グラフィックデバイス gd, double 現在の演奏時刻sec )
		{
			gd.D2DBatchDraw( ( dc ) => {

				this._描画範囲のチップに処理を適用する( 現在の演奏時刻sec, ( chip, index, ヒット判定バーと描画との時間sec, ヒット判定バーと発声との時間sec, ヒット判定バーとの距離dpx ) => {

					if( chip.チップ種別 == チップ種別.小節線 )
					{
						float 上位置dpx = (float) ( ヒット判定バーの中央Y座標dpx + ヒット判定バーとの距離dpx - 1f );   // -1f は小節線の厚みの半分。
						dc.DrawLine( new Vector2( 441f, 上位置dpx ), new Vector2( 441f + 780f, 上位置dpx ), this._小節線色, strokeWidth: 3f );
					}
					else if( chip.チップ種別 == チップ種別.拍線 )
					{
						float 上位置dpx = (float) ( ヒット判定バーの中央Y座標dpx + ヒット判定バーとの距離dpx - 1f );   // -1f は拍線の厚みの半分。
						dc.DrawLine( new Vector2( 441f, 上位置dpx ), new Vector2( 441f + 780f, 上位置dpx ), this._拍線色, strokeWidth: 1f );
					}

				} );

			} );
		}

		private 画像 _ドラムチップ画像 = null;
		private 矩形リスト _ドラムチップ画像の矩形リスト = null;
		private LoopCounter _ドラムチップアニメ = null;
		private void _チップを描画する( グラフィックデバイス gd, double 現在の演奏時刻sec )
		{
			Debug.Assert( null != this._ドラムチップ画像の矩形リスト );

			this._描画範囲のチップに処理を適用する( 現在の演奏時刻sec, ( chip, index, ヒット判定バーと描画との時間sec, ヒット判定バーと発声との時間sec, ヒット判定バーとの距離dpx ) => {

				float 縦中央位置dpx = (float) ( ヒット判定バーの中央Y座標dpx + ヒット判定バーとの距離dpx );
				float 消滅割合 = 0f;

				#region " チップがヒット判定バーを通過してたら、通過距離に応じて 0→1の消滅割合を付与する。0で完全表示、1で完全消滅、通過してなければ 0。"
				//----------------
				const float 消滅を開始するヒット判定バーからの距離dpx = 20f;
				const float 消滅開始から完全消滅するまでの距離dpx = 70f;
				if( 消滅を開始するヒット判定バーからの距離dpx < ヒット判定バーとの距離dpx )
				{
					消滅割合 = Math.Min( 1f, (float) ( ( ヒット判定バーとの距離dpx - 消滅を開始するヒット判定バーからの距離dpx ) / 消滅開始から完全消滅するまでの距離dpx ) );
				}
				//----------------
				#endregion

				#region " チップが描画開始チップであり、かつ、そのY座標が画面下端を超えたなら、描画開始チップ番号を更新する。"
				//----------------
				if( ( index == this._描画開始チップ番号 ) &&
					( gd.設計画面サイズ.Height + 40.0 < 縦中央位置dpx ) )   // +40 はチップが隠れるであろう適当なマージン。
				{
					this._描画開始チップ番号++;

					if( App.演奏スコア.チップリスト.Count <= this._描画開始チップ番号 )
					{
						this.現在のフェーズ = フェーズ.クリア;
						this._描画開始チップ番号 = -1;    // 演奏完了。
						return;
					}
				}
				//----------------
				#endregion

				if( chip.不可視 )
					return;

				float 音量0to1 = 1f;      // chip.音量 / (float) チップ.最大音量;		matixx では音量無視。

				var lane = App.システム設定.チップto表示レーン[ chip.チップ種別 ];
				if( lane != 表示レーン種別.Unknown )
				{
					// xml の記述ミスの検出用。
					Debug.Assert( null != this._ドラムチップ画像の矩形リスト[ lane.ToString() ] );
					Debug.Assert( null != this._ドラムチップ画像の矩形リスト[ lane.ToString() + "_back" ] );

					var 縦方向中央位置dpx = this._ドラムチップ画像の矩形リスト[ "縦方向中央位置" ]?.Height ?? 0f;

					#region " パッド絵 "
					//----------------
					{
						var 矩形 = this._ドラムチップ画像の矩形リスト[ lane.ToString() + "_back" ].Value;
						var 矩形中央 = new Vector2( 矩形.Width / 2f, 矩形.Height / 2f );
						var 割合 = this._ドラムチップアニメ.現在値の割合;   // 0→1のループ

						var 変換行列2D = ( 0 >= 消滅割合 ) ? Matrix3x2.Identity : Matrix3x2.Scaling( 1f - 消滅割合, 1f, 矩形中央 );

						// 拡大縮小回転
						switch( lane )
						{
							case 表示レーン種別.LeftCrash:
							case 表示レーン種別.HiHat:
							case 表示レーン種別.Foot:
							case 表示レーン種別.Tom3:
							case 表示レーン種別.RightCrash:
								{
									float v = (float) ( Math.Sin( 2 * Math.PI * 割合 ) * 0.2 );
									変換行列2D = 変換行列2D * Matrix3x2.Scaling( (float) ( 1 + v ), (float) ( 1 - v ) * 音量0to1, 矩形中央 );
								}
								break;

							case 表示レーン種別.Bass:
								{
									float r = (float) ( Math.Sin( 2 * Math.PI * 割合 ) * 0.2 );
									変換行列2D = 変換行列2D *
										Matrix3x2.Scaling( 1f, 音量0to1, 矩形中央 ) *
										Matrix3x2.Rotation( (float) ( r * Math.PI ), 矩形中央 );
								}
								break;

							default:
								変換行列2D = 変換行列2D * Matrix3x2.Scaling( 1f, 音量0to1, 矩形中央 );
								break;
						}

						// 移動
						変換行列2D = 変換行列2D *
							Matrix3x2.Translation(
								x: レーンフレーム.領域.Left + レーンフレーム.レーンtoチップの左端位置dpx[ lane ],
								y: 縦中央位置dpx - 縦方向中央位置dpx * 音量0to1 );

						this._ドラムチップ画像.描画する(
							gd,
							変換行列2D,
							転送元矩形: 矩形,
							不透明度0to1: 1f - 消滅割合 );
					}
					//----------------
					#endregion

					#region " チップ本体 "
					//----------------
					{
						var 矩形 = this._ドラムチップ画像の矩形リスト[ lane.ToString() ].Value;
						var 矩形中央 = new Vector2( 矩形.Width / 2f, 矩形.Height / 2f );

						var 変換行列2D =
							( ( 0 >= 消滅割合 ) ? Matrix3x2.Identity : Matrix3x2.Scaling( 1f - 消滅割合, 1f, 矩形中央 ) ) *
							Matrix3x2.Scaling( 1f, 音量0to1, 矩形中央 ) *
							Matrix3x2.Translation(
								x: レーンフレーム.領域.Left + レーンフレーム.レーンtoチップの左端位置dpx[ lane ],
								y: 縦中央位置dpx - 縦方向中央位置dpx * 音量0to1 );

						this._ドラムチップ画像.描画する(
							gd,
							変換行列2D,
							転送元矩形: 矩形,
							不透明度0to1: 1f - 消滅割合 );
					}
					//----------------
					#endregion
				}

			} );
		}

		private 動画 _背景動画 = null;
		private bool _背景動画開始済み = false;
		/// <remarks>
		///		停止と解放は、演奏ステージクラスの非活性化後に、外部から行われる。
		///		<see cref="SST.ステージ.演奏.演奏ステージ.BGMを停止する"/>
		///		<see cref="SST.ステージ.演奏.演奏ステージ.BGMのキャッシュを解放する"/>
		/// </remarks>
		private Sound _BGM = null;
		private bool _BGM再生開始済み = false;
		/// <summary>
		///		BGM の生成もとになるデコード済みサウンドデータ。
		///	</summary>
		///	<remarks>
		///		活性化と非活性化に関係なく、常に最後にデコードしたデータを持つ。（キャッシュ）
		///		演奏ステージインスタンスを破棄する際に、このインスタンスもDisposeすること。
		/// </remarks>
		private ISampleSource _デコード済みWaveSource = null;

		private void _チップのヒット処理を行う( チップ chip, 判定種別 judge, ドラムとチップと入力の対応表.Column.Columnヒット処理 ヒット処理表, double ヒット判定バーと発声との時間sec )
		{
			chip.ヒット済みである = true;

			if( ヒット処理表.再生 )
			{
				#region " チップの発声を行う。"
				//----------------
				if( chip.発声されていない )
					this._チップの発声を行う( chip, ヒット判定バーと発声との時間sec );
				//----------------
				#endregion
			}
			if( ヒット処理表.判定 )
			{
				#region " チップの判定処理を行う。"
				//----------------
				var 対応表 = App.ユーザ設定.ドラムとチップと入力の対応表[ chip.チップ種別 ];

				if( judge != 判定種別.MISS )
				{
					// PERFECT～OK
					this._チップ光.表示を開始する( 対応表.表示レーン種別 );
					this._ドラムパッド.ヒットする( 対応表.表示レーン種別 );
					this._レーンフラッシュ.開始する( 対応表.表示レーン種別 );
				}

				this._判定文字列.表示を開始する( 対応表.表示レーン種別, judge );
				this.成績.ヒット数を加算する( judge );
				//----------------
				#endregion
			}
			if( ヒット処理表.非表示 )
			{
				#region " チップを非表示にする。"
				//----------------
				if( judge != 判定種別.MISS )
				{
					chip.可視 = false;        // PERFECT～POOR チップは非表示。
				}
				else
				{
					// MISSチップは最後まで表示し続ける。
				}
				//----------------
				#endregion
			}
		}
		private void _チップの発声を行う( チップ chip, double 再生開始位置sec )
		{
			if( chip.発声済みである )
				return;

			chip.発声済みである = true;

			if( 0 == chip.チップサブID )
			{
				// (A) SSTF 準拠
				if( chip.チップ種別 == チップ種別.背景動画 )
				{
					// (A-a) 背景動画
					App.サウンドタイマ.一時停止する();       // 止めても止めなくてもカクつくだろうが、止めておけば譜面は再開時にワープしない。

					this._背景動画?.再生を開始する();
					this._背景動画開始済み = true;

					this._BGM?.Play( 再生開始位置sec );
					this._BGM再生開始済み = true;

					App.サウンドタイマ.再開する();
				}
				else
				{
					// (A-b) ドラムサウンド

					// BGM以外のサウンドについては、常に最初から再生する。
					App.ドラムサウンド.発声する( chip.チップ種別, 0, ( chip.音量 / (float) チップ.最大音量 ) );
				}
			}
			else
			{
				// (B) DTX 準拠
				App.WAV管理.発声する( chip.チップサブID, chip.チップ種別, ( chip.音量 / (float) チップ.最大音量 ) );
			}
		}

		private void _キャプチャ画面を描画する( グラフィックデバイス gd, float 不透明度 = 1.0f )
		{
			Debug.Assert( null != this.キャプチャ画面, "キャプチャ画面が設定されていません。" );

			gd.D2DBatchDraw( ( dc ) => {
				dc.DrawBitmap(
					this.キャプチャ画面,
					new RectangleF( 0f, 0f, gd.設計画面サイズ.Width, gd.設計画面サイズ.Height ),
					不透明度,
					BitmapInterpolationMode.Linear );
			} );
		}
	}
}
