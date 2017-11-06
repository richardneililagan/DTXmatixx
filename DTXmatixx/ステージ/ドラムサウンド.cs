using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using CSCore;
using FDK;
using FDK.メディア.サウンド.WASAPI;
using SSTFormat.v3;

namespace DTXmatixx.ステージ
{
	class ドラムサウンド : IDisposable
	{
		public ドラムサウンド( int 多重度 = 4 )
		{
			this._多重度 = 多重度;
			this.初期化する();
		}
		public void Dispose()
		{
			using( Log.Block( FDKUtilities.現在のメソッド名 ) )
			{
				lock( this._Sound利用権 )
				{
					if( null != this._チップtoコンテキスト )
					{
						foreach( var kvp in this._チップtoコンテキスト )
							kvp.Value.Dispose();
						this._チップtoコンテキスト.Clear();
						this._チップtoコンテキスト = null;
					}
				}
			}
		}

		/// <summary>
		///		サブチップID = 0（SSTの規定ドラムサウンド）以外をクリアする。
		/// </summary>
		public void 初期化する()
		{
			using( Log.Block( FDKUtilities.現在のメソッド名 ) )
			{
				lock( this._Sound利用権 )
				{
					if( null != this._チップtoコンテキスト )
					{
						foreach( var kvp in this._チップtoコンテキスト )
							kvp.Value.Dispose();
					}

					this._チップtoコンテキスト = new Dictionary<(チップ種別 chipType, int サブチップID), Cコンテキスト>();

					// SSTの既定のサウンドを、subChipId = 0 としてプリセット登録する。
					this.登録する( チップ種別.LeftCrash, 0, @"$(System)sounds\drums\LeftCrash.wav" );
					this.登録する( チップ種別.Ride, 0, @"$(System)sounds\drums\Ride.wav" );
					this.登録する( チップ種別.Ride_Cup, 0, @"$(System)sounds\drums\RideCup.wav" );
					this.登録する( チップ種別.China, 0, @"$(System)sounds\drums\China.wav" );
					this.登録する( チップ種別.Splash, 0, @"$(System)sounds\drums\Splash.wav" );
					this.登録する( チップ種別.HiHat_Open, 0, @"$(System)sounds\drums\HiHatOpen.wav" );
					this.登録する( チップ種別.HiHat_HalfOpen, 0, @"$(System)sounds\drums\HiHatHalfOpen.wav" );
					this.登録する( チップ種別.HiHat_Close, 0, @"$(System)sounds\drums\HiHatClose.wav" );
					this.登録する( チップ種別.HiHat_Foot, 0, @"$(System)sounds\drums\HiHatFoot.wav" );
					this.登録する( チップ種別.Snare, 0, @"$(System)sounds\drums\Snare.wav" );
					this.登録する( チップ種別.Snare_OpenRim, 0, @"$(System)sounds\drums\SnareOpenRim.wav" );
					this.登録する( チップ種別.Snare_ClosedRim, 0, @"$(System)sounds\drums\SnareClosedRim.wav" );
					this.登録する( チップ種別.Snare_Ghost, 0, @"$(System)sounds\drums\SnareGhost.wav" );
					this.登録する( チップ種別.Bass, 0, @"$(System)sounds\drums\Bass.wav" );
					this.登録する( チップ種別.Tom1, 0, @"$(System)sounds\drums\Tom1.wav" );
					this.登録する( チップ種別.Tom1_Rim, 0, @"$(System)sounds\drums\Tom1Rim.wav" );
					this.登録する( チップ種別.Tom2, 0, @"$(System)sounds\drums\Tom2.wav" );
					this.登録する( チップ種別.Tom2_Rim, 0, @"$(System)sounds\drums\Tom2Rim.wav" );
					this.登録する( チップ種別.Tom3, 0, @"$(System)sounds\drums\Tom3.wav" );
					this.登録する( チップ種別.Tom3_Rim, 0, @"$(System)sounds\drums\Tom3Rim.wav" );
					this.登録する( チップ種別.RightCrash, 0, @"$(System)sounds\drums\RightCrash.wav" );
					this.登録する( チップ種別.LeftCymbal_Mute, 0, @"$(System)sounds\drums\LeftCymbalMute.wav" );
					this.登録する( チップ種別.RightCymbal_Mute, 0, @"$(System)sounds\drums\RightCymbalMute.wav" );
				}
			}
		}

		public void 登録する( チップ種別 chipType, int subChipId, VariablePath サウンドファイルパス )
		{
			if( File.Exists( サウンドファイルパス.変数なしパス ) )
			{
				lock( this._Sound利用権 )
				{
					// すでに辞書に存在してるなら、解放して削除する。
					if( this._チップtoコンテキスト.ContainsKey( (chipType, subChipId) ) )
					{
						this._チップtoコンテキスト[ (chipType, subChipId) ]?.Dispose();
						this._チップtoコンテキスト.Remove( (chipType, subChipId) );
					}

					// コンテキストを作成する。
					var context = new Cコンテキスト( this._多重度 );

					// サウンドファイルを読み込んでデコードする。
					context.SampleSource = SampleSourceFactory.Create( App.サウンドデバイス, サウンドファイルパス );

					// 多重度分のサウンドを生成する。
					for( int i = 0; i < context.Sounds.Length; i++ )
						context.Sounds[ i ] = new Sound( App.サウンドデバイス, context.SampleSource );

					// コンテキストを辞書に追加する。
					this._チップtoコンテキスト.Add( (chipType, subChipId), context );

					Log.Info( $"ドラムサウンドを生成しました。[({chipType.ToString()},{subChipId}) = {サウンドファイルパス.変数付きパス}]" );
				}
			}
			else
			{
				Log.ERROR( $"サウンドファイルが存在しません。[{サウンドファイルパス.変数付きパス}]" );
			}
		}
		public void 登録する( チップ種別 chipType, int subChipId, string サウンドファイルパス )
			=> this.登録する( chipType, subChipId, サウンドファイルパス?.ToVariablePath() );

		public void 発声する( チップ種別 chipType, int subChipId, float 音量0to1 = 1f )
		{
			lock( this._Sound利用権 )
			{
				if( this._チップtoコンテキスト.TryGetValue( (chipType, subChipId), out Cコンテキスト context ) )
				{
					// 現在発声中のサウンドを全部止めるチップ種別の場合は止める。
					if( 0 != chipType.排他発声グループID() ) // グループID = 0 は対象外。
					{
						// 消音対象のコンテキストの Sounds[] を select する。
						var 停止するサウンドs =
							from kvp in this._チップtoコンテキスト
							where ( chipType.直前のチップを消音する( kvp.Key.chipType ) )
							select kvp.Value.Sounds;

						// 集めた Sounds[] をすべて停止する。
						foreach( var sounds in 停止するサウンドs )
						{
							foreach( var sound in sounds )
							{
								sound.Stop();
							}
						}
					}

					// 再生する。
					if( null != context.Sounds[ context.次に再生するSound番号 ] )
					{
						context.Sounds[ context.次に再生するSound番号 ].Volume = 音量0to1;
						context.Sounds[ context.次に再生するSound番号 ].Play();
					}

					// サウンドローテーション。
					context.次に再生するSound番号++;

					if( context.次に再生するSound番号 >= this._多重度 )
						context.次に再生するSound番号 = 0;
				}
				else
				{
					// コンテキストがないなら何もしない。
				}
			}
		}

		private class Cコンテキスト : IDisposable
		{
			public ISampleSource SampleSource = null;
			public Sound[] Sounds = null;
			public int 次に再生するSound番号 = 0;

			public Cコンテキスト( int 多重度 = 4 )
			{
				this._多重度 = 多重度;
				this.Sounds = new Sound[ this._多重度 ];
			}
			public void Dispose()
			{
				FDKUtilities.解放する( ref this.SampleSource );

				for( int i = 0; i < this.Sounds.Length; i++ )
				{
					if( this.Sounds[ i ].再生中である )
						this.Sounds[ i ].Stop();

					FDKUtilities.解放する( ref this.Sounds[ i ] );
				}
			}

			private readonly int _多重度 = 4;
		};

		private readonly int _多重度 = 4;
		private Dictionary<(チップ種別 chipType, int サブチップID), Cコンテキスト> _チップtoコンテキスト = null;
		private readonly object _Sound利用権 = new object();
	}
}
