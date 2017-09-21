using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SharpDX;
using SharpDX.Direct2D1;
using SharpDX.DirectInput;
using FDK;
using FDK.メディア;
using FDK.カウンタ;
using DTXmatixx.曲;
using SSTFormat.v2;

namespace DTXmatixx.ステージ.演奏
{
	class 演奏ステージ : ステージ
	{
		public const float ヒット判定バーの中央Y座標dpx = 847f;

		public enum フェーズ
		{
			フェードイン,
			表示,
			キャンセル,
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

		public 演奏ステージ()
		{
			this.子リスト.Add( this._背景画像 = new 画像( @"$(System)images\演奏画面.png" ) );
			this.子リスト.Add( this._曲名パネル = new 曲名パネル() );
			this.子リスト.Add( this._ステータスパネル = new ステータスパネル() );
			this.子リスト.Add( this._成績パネル = new 成績パネル() );
			this.子リスト.Add( this._ヒットバー = new 画像( @"$(System)images\演奏画面_ヒットバー.png" ) );
			this.子リスト.Add( this._パッド = new ドラムパッド() );
			this.子リスト.Add( this._FPS = new FPS() );
		}

		protected override void On活性化( グラフィックデバイス gd )
		{
			using( Log.Block( FDKUtilities.現在のメソッド名 ) )
			{
				this.キャプチャ画面 = null;
				this._描画開始チップ番号 = -1;
				this._小節線色 = new SolidColorBrush( gd.D2DDeviceContext, Color.White );
				this._拍線色 = new SolidColorBrush( gd.D2DDeviceContext, Color.LightGray );

				this.現在のフェーズ = フェーズ.フェードイン;
				this._初めての進行描画 = true;
			}
		}
		protected override void On非活性化( グラフィックデバイス gd )
		{
			using( Log.Block( FDKUtilities.現在のメソッド名 ) )
			{
				FDKUtilities.解放する( ref this._拍線色 );
				FDKUtilities.解放する( ref this._小節線色 );

				this.キャプチャ画面?.Dispose();
				this.キャプチャ画面 = null;
			}
		}

		public override void 高速進行する()
		{
			if( this._初めての進行描画 )
			{
				this._フェードインカウンタ = new Counter( 0, 100, 10 );
				this._初めての進行描画 = false;
			}

			// 高速進行

			this._FPS.FPSをカウントしプロパティを更新する();

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
					// 入力
					App.Keyboard.ポーリングする();
					if( App.Keyboard.キーが押された( 0, Key.Escape ) )
					{
						#region " ESC → 演奏中断 "
						//----------------
						Log.Info( "演奏を中断します。" );
						this.現在のフェーズ = フェーズ.キャンセル;
						//----------------
						#endregion
					}
					break;

				case フェーズ.キャンセル:
					break;
			}
		}
		public override void 進行描画する( グラフィックデバイス gd )
		{
			// 進行描画

			if( this._初めての進行描画 )
				return; // まだ最初の高速進行が行われていない。

			switch( this.現在のフェーズ )
			{
				case フェーズ.フェードイン:
					{
						this._背景画像.描画する( gd, 0f, 0f );
						this._パッド.進行描画する( gd );
						this._ステータスパネル.描画する( gd );
						this._成績パネル.進行描画する( gd );
						this._曲名パネル.描画する( gd );
						this._ヒットバー.描画する( gd, 441f, ヒット判定バーの中央Y座標dpx - 4f );    // 4f がバーの厚みの半分[dpx]。
						this._キャプチャ画面を描画する( gd, ( 1.0f - this._フェードインカウンタ.現在値の割合 ) );
					}
					break;

				case フェーズ.表示:
					{
						double 演奏時刻sec = this._演奏開始からの経過時間secを返す() + gd.次のDComp表示までの残り時間sec;

						this._小節線拍線を描画する( gd, 演奏時刻sec );
						this._背景画像.描画する( gd, 0f, 0f );
						this._パッド.進行描画する( gd );
						this._ステータスパネル.描画する( gd );
						this._成績パネル.進行描画する( gd );
						this._曲名パネル.描画する( gd );
						this._ヒットバー.描画する( gd, 441f, ヒット判定バーの中央Y座標dpx - 4f );    // 4f がバーの厚みの半分[dpx]。
						this._チップを描画する( gd, 演奏時刻sec );
						this._FPS.VPSをカウントする();
						this._FPS.描画する( gd, 0f, 0f );
					}
					break;

				case フェーズ.キャンセル:
					break;
			}
		}

		private bool _初めての進行描画 = true;
		private 画像 _背景画像 = null;
		private 曲名パネル _曲名パネル = null;
		private ステータスパネル _ステータスパネル = null;
		private 成績パネル _成績パネル = null;
		private 画像 _ヒットバー = null;
		private ドラムパッド _パッド = null;
		private FPS _FPS = null;
		/// <summary>
		///		読み込み画面: 0 ～ 1: 演奏画面
		/// </summary>
		private Counter _フェードインカウンタ = null;
		private SolidColorBrush _小節線色 = null;
		private SolidColorBrush _拍線色 = null;

		/// <summary>
		///		<see cref="スコア.チップリスト"/> のうち、描画を始めるチップのインデックス番号。
		///		未演奏時・演奏終了時は -1 。
		/// </summary>
		/// <remarks>
		///		演奏開始直後は 0 で始まり、対象番号のチップが描画範囲を流れ去るたびに +1 される。
		///		このメンバの更新は、高頻度進行タスクではなく、進行描画メソッドで行う。（低精度で構わないので）
		/// </remarks>
		private int _描画開始チップ番号 = -1;
		private double _現在進行描画中の譜面スクロール速度の倍率 = 1.0;

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
		private double _演奏開始からの経過時間secを返す()
		{
			return App.サウンドタイマ.現在時刻sec;
		}

		/// <summary>
		///		<see cref="_描画開始チップ番号"/> から画面上端にはみ出すまでの間の各チップに対して、指定された処理を適用する。
		/// </summary>
		/// <param name="適用する処理">
		///		引数は、順に、対象のチップ、チップ番号、ヒット判定バーとの時間sec、ヒット判定バーとの距離
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
				double ヒット判定バーとの距離 = スコア.指定された時間secに対応する符号付きピクセル数を返す( this._現在進行描画中の譜面スクロール速度の倍率, ヒット判定バーと描画との時間sec );

				// 終了判定。
				bool チップは画面上端より上に出ている = ( ( ヒット判定バーの中央Y座標dpx + ヒット判定バーとの距離 ) < -40.0 );   // -40 はチップが隠れるであろう適当なマージン。
				if( チップは画面上端より上に出ている )
					break;

				// 処理実行。開始判定（描画開始チップ番号の更新）もこの中で。
				適用する処理( チップ, i, ヒット判定バーと描画との時間sec, ヒット判定バーと発声との時間sec, ヒット判定バーとの距離 );
			}
		}

		// 小節線・拍線 と チップ は描画階層（奥行き）が異なるので、別々のメソッドに分ける。

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

		private void _チップを描画する( グラフィックデバイス gd, double 現在の演奏時刻sec )
		{
			//if( null == this._チップ画像 )
			//	return;

			//this._チップ画像.加算合成 = false;

			//this._描画範囲のチップに処理を適用する( 現在の演奏時刻sec, ( chip, index, ヒット判定バーと描画との時間sec, ヒット判定バーと発声との時間sec, ヒット判定バーとの距離 ) => {

			//	float 縦中央位置 = (float) ( ヒット判定バーの中央Y座標 + ヒット判定バーとの距離 );

			//	#region " チップが描画開始チップであり、かつ、そのY座標が画面下端を超えたなら、描画開始チップ番号を更新する。"
			//	//----------------
			//	if( ( index == this._描画開始チップ番号 ) &&
			//		( gd.設計画面サイズ.Height + 40.0 < 縦中央位置 ) )   // +40 はチップが隠れるであろう適当なマージン。
			//	{
			//		this._描画開始チップ番号++;

			//		if( App.演奏スコア.チップリスト.Count <= this._描画開始チップ番号 )
			//		{
			//			this.現在のフェーズ = フェーズ.クリア時フェードアウト;
			//			this._描画開始チップ番号 = -1;    // 演奏完了。
			//			return;
			//		}
			//	}
			//	//----------------
			//	#endregion

			//	if( chip.不可視 )
			//		return;

			//	#region " チップを個別に描画する。"
			//	//----------------
			//	float 音量0to1 = chip.音量 / (float) チップ.最大音量;

			//	switch( chip.チップ種別 )
			//	{
			//		case チップ種別.LeftCrash:
			//			_単画チップを１つ描画する( 表示レーン種別.LeftCrash, this._チップ画像の矩形リスト[ nameof( チップ種別.LeftCrash ) ], 縦中央位置, 音量0to1 );
			//			break;

			//		case チップ種別.HiHat_Close:
			//			_アニメチップを１つ描画する( 表示レーン種別.HiHat, this._チップ画像の矩形リスト[ nameof( チップ種別.HiHat_Close ) ], 縦中央位置, 音量0to1 );
			//			break;

			//		case チップ種別.HiHat_HalfOpen:
			//			_アニメチップを１つ描画する( 表示レーン種別.HiHat, this._チップ画像の矩形リスト[ nameof( チップ種別.HiHat_Close ) ], 縦中央位置, 音量0to1 );
			//			_単画チップを１つ描画する( 表示レーン種別.Foot, this._チップ画像の矩形リスト[ nameof( チップ種別.HiHat_HalfOpen ) ], 縦中央位置, 1.0f );
			//			break;

			//		case チップ種別.HiHat_Open:
			//			_アニメチップを１つ描画する( 表示レーン種別.HiHat, this._チップ画像の矩形リスト[ nameof( チップ種別.HiHat_Close ) ], 縦中央位置, 音量0to1 );
			//			_単画チップを１つ描画する( 表示レーン種別.Foot, this._チップ画像の矩形リスト[ nameof( チップ種別.HiHat_Open ) ], 縦中央位置, 1.0f );
			//			break;

			//		case チップ種別.HiHat_Foot:
			//			_単画チップを１つ描画する( 表示レーン種別.Foot, this._チップ画像の矩形リスト[ nameof( チップ種別.HiHat_Foot ) ], 縦中央位置, 1.0f );
			//			break;

			//		case チップ種別.Snare:
			//			_アニメチップを１つ描画する( 表示レーン種別.Snare, this._チップ画像の矩形リスト[ nameof( チップ種別.Snare ) ], 縦中央位置, 音量0to1 );
			//			break;

			//		case チップ種別.Snare_ClosedRim:
			//			_単画チップを１つ描画する( 表示レーン種別.Snare, this._チップ画像の矩形リスト[ nameof( チップ種別.Snare_ClosedRim ) ], 縦中央位置, 1.0f );
			//			break;

			//		case チップ種別.Snare_OpenRim:
			//			_単画チップを１つ描画する( 表示レーン種別.Snare, this._チップ画像の矩形リスト[ nameof( チップ種別.Snare_OpenRim ) ], 縦中央位置, 音量0to1 );
			//			// ↓ないほうがいいかも。
			//			//_単画チップを１つ描画する( 表示レーン種別.Snare, this._チップ画像の矩形リスト[ nameof( チップ種別.Snare ) ], 縦中央位置, 音量0to1 );
			//			break;

			//		case チップ種別.Snare_Ghost:
			//			_単画チップを１つ描画する( 表示レーン種別.Snare, this._チップ画像の矩形リスト[ nameof( チップ種別.Snare_Ghost ) ], 縦中央位置, 1.0f );
			//			break;

			//		case チップ種別.Bass:
			//			_アニメチップを１つ描画する( 表示レーン種別.Bass, this._チップ画像の矩形リスト[ nameof( チップ種別.Bass ) ], 縦中央位置, 音量0to1 );
			//			break;

			//		case チップ種別.Tom1:
			//			_アニメチップを１つ描画する( 表示レーン種別.Tom1, this._チップ画像の矩形リスト[ nameof( チップ種別.Tom1 ) ], 縦中央位置, 音量0to1 );
			//			break;

			//		case チップ種別.Tom1_Rim:
			//			_単画チップを１つ描画する( 表示レーン種別.Tom1, this._チップ画像の矩形リスト[ nameof( チップ種別.Tom1_Rim ) ], 縦中央位置, 1.0f );
			//			break;

			//		case チップ種別.Tom2:
			//			_アニメチップを１つ描画する( 表示レーン種別.Tom2, this._チップ画像の矩形リスト[ nameof( チップ種別.Tom2 ) ], 縦中央位置, 音量0to1 );
			//			break;

			//		case チップ種別.Tom2_Rim:
			//			_単画チップを１つ描画する( 表示レーン種別.Tom2, this._チップ画像の矩形リスト[ nameof( チップ種別.Tom2_Rim ) ], 縦中央位置, 1.0f );
			//			break;

			//		case チップ種別.Tom3:
			//			_アニメチップを１つ描画する( 表示レーン種別.Tom3, this._チップ画像の矩形リスト[ nameof( チップ種別.Tom3 ) ], 縦中央位置, 音量0to1 );
			//			break;

			//		case チップ種別.Tom3_Rim:
			//			_単画チップを１つ描画する( 表示レーン種別.Tom3, this._チップ画像の矩形リスト[ nameof( チップ種別.Tom3_Rim ) ], 縦中央位置, 1.0f );
			//			break;

			//		case チップ種別.RightCrash:
			//			_単画チップを１つ描画する( 表示レーン種別.RightCrash, this._チップ画像の矩形リスト[ nameof( チップ種別.RightCrash ) ], 縦中央位置, 音量0to1 );
			//			break;

			//		case チップ種別.China:
			//			if( App.ユーザ管理.選択されているユーザ.オプション設定.表示レーンの左右.Chinaは左 )
			//			{
			//				_単画チップを１つ描画する( 表示レーン種別.LeftCrash, this._チップ画像の矩形リスト[ "LeftChina" ], 縦中央位置, 音量0to1 );
			//			}
			//			else
			//			{
			//				_単画チップを１つ描画する( 表示レーン種別.RightCrash, this._チップ画像の矩形リスト[ "RightChina" ], 縦中央位置, 音量0to1 );
			//			}
			//			break;

			//		case チップ種別.Ride:
			//			if( App.ユーザ管理.選択されているユーザ.オプション設定.表示レーンの左右.Rideは左 )
			//			{
			//				_単画チップを１つ描画する( 表示レーン種別.LeftCrash, this._チップ画像の矩形リスト[ "LeftRide" ], 縦中央位置, 音量0to1 );
			//			}
			//			else
			//			{
			//				_単画チップを１つ描画する( 表示レーン種別.RightCrash, this._チップ画像の矩形リスト[ "RightRide" ], 縦中央位置, 音量0to1 );
			//			}
			//			break;

			//		case チップ種別.Ride_Cup:
			//			if( App.ユーザ管理.選択されているユーザ.オプション設定.表示レーンの左右.Rideは左 )
			//			{
			//				_単画チップを１つ描画する( 表示レーン種別.LeftCrash, this._チップ画像の矩形リスト[ "LeftRide_Cup" ], 縦中央位置, 音量0to1 );
			//			}
			//			else
			//			{
			//				_単画チップを１つ描画する( 表示レーン種別.RightCrash, this._チップ画像の矩形リスト[ "RightRide_Cup" ], 縦中央位置, 音量0to1 );
			//			}
			//			break;

			//		case チップ種別.Splash:
			//			if( App.ユーザ管理.選択されているユーザ.オプション設定.表示レーンの左右.Splashは左 )
			//			{
			//				_単画チップを１つ描画する( 表示レーン種別.LeftCrash, this._チップ画像の矩形リスト[ "LeftSplash" ], 縦中央位置, 音量0to1 );
			//			}
			//			else
			//			{
			//				_単画チップを１つ描画する( 表示レーン種別.RightCrash, this._チップ画像の矩形リスト[ "RightSplash" ], 縦中央位置, 音量0to1 );
			//			}
			//			break;

			//		case チップ種別.LeftCymbal_Mute:
			//			_単画チップを１つ描画する( 表示レーン種別.LeftCrash, this._チップ画像の矩形リスト[ "LeftCymbal_Mute" ], 縦中央位置, 1.0f );
			//			break;

			//		case チップ種別.RightCymbal_Mute:
			//			_単画チップを１つ描画する( 表示レーン種別.RightCrash, this._チップ画像の矩形リスト[ "RightCymbal_Mute" ], 縦中央位置, 1.0f );
			//			break;
			//	}
			//	//----------------
			//	#endregion

			//} );

			//#region " ローカル関数 "
			////----------------
			//void _単画チップを１つ描画する( 表示レーン種別 lane, RectangleF? 元矩形, float 上位置, float 音量0to1 )
			//{
			//	if( null == 元矩形 )
			//		return;

			//	var 画像範囲 = (RectangleF) 元矩形;

			//	this._チップ画像?.描画する(
			//		gd,
			//		左位置: レーンフレームの左端位置 + レーンフレーム.レーンto横中央相対位置[ lane ] - ( 画像範囲.Width / 2f ),
			//		上位置: 上位置 - ( ( 画像範囲.Height / 2f ) * 音量0to1 ),
			//		転送元矩形: 元矩形,
			//		Y方向拡大率: 音量0to1 );
			//}
			//void _アニメチップを１つ描画する( 表示レーン種別 lane, RectangleF? 画像範囲orNull, float Y, float 音量0to1 )
			//{
			//	if( null == 画像範囲orNull )
			//		return;

			//	var 画像範囲 = (RectangleF) 画像範囲orNull;

			//	float チップ1枚の高さ = 18f;
			//	画像範囲.Offset( 0f, this._チップアニメ.現在値 * 15f );   // 下端3pxは下のチップと共有する前提のデザインなので、18f-3f = 15f。
			//	画像範囲.Height = チップ1枚の高さ;
			//	float 左位置 = レーンフレームの左端位置 + レーンフレーム.レーンto横中央相対位置[ lane ] - ( 画像範囲.Width / 2f );
			//	float 上位置 = Y - ( チップ1枚の高さ / 2f ) * 音量0to1;

			//	this._チップ画像?.描画する( gd, 左位置, 上位置, 転送元矩形: 画像範囲, Y方向拡大率: 音量0to1 );
			//}
			////----------------
			//#endregion
		}
	}
}
