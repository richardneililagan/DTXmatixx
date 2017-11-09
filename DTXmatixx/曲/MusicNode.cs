using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using SharpDX.Direct3D11;
using SSTFormat.v3;
using FDK;
using FDK.メディア;
using DTXmatixx.設定;
using DTXmatixx.データベース.曲;

namespace DTXmatixx.曲
{
	/// <summary>
	///		曲ツリー階層において「曲」を表すノード。
	/// </summary>
	class MusicNode : Node
	{
		/// <summary>
		///		この曲ノードに対応する曲ファイル。
		/// </summary>
		public VariablePath 曲ファイルパス
		{
			get;
			protected set;
		} = null;

		/// <summary>
		///		この曲ノードに対応する曲ファイルのハッシュ値。
		/// </summary>
		public string 曲ファイルハッシュ
		{
			get;
			protected set;
		} = null;

		/// <summary>
		///		この曲ノードに対応する動画ファイル。
		/// </summary>
		public VariablePath 動画ファイルパス
		{
			get;
			protected set;
		} = null;


		public MusicNode( VariablePath 曲ファイルパス, Node 親ノード )
		{
			this.親ノード = 親ノード;
			this.曲ファイルパス = 曲ファイルパス;

			// （まだ存在してなければ）曲DBに追加する。
			曲DB.曲を追加または更新する( this.曲ファイルパス, App.ユーザ管理.ログオン中のユーザ );

			// 追加後、改めて曲DBから情報を取得する。
			using( var songdb = new SongDB() )
			{
				var song = songdb.Songs.Where( ( r ) => ( r.Path == this.曲ファイルパス.変数なしパス ) ).SingleOrDefault();
				if( null != song )
				{
					this.タイトル = song.Title;
					this.サブタイトル = song.Artist;
					this.曲ファイルハッシュ = song.HashId;
					this.難易度[ 3 ] = ("FREE", (float) song.Level);       // [3]:MASTER相当。set.def 内にある MusicNode でも同じ。
				}

				// サムネイル画像を決定する。
				string サムネイル画像ファイルパス = null;
				if( song.PreImage.Nullでも空でもない() && File.Exists( song.PreImage ) )
				{
					// (A) DB に保存されている値があり、そのファイルが存在するなら、それを使う。
					サムネイル画像ファイルパス = song.PreImage;
				}
				else
				{
					// (B) DB に保存されてない場合、曲ファイルと同じ場所に画像ファイルがあるなら、それをノード画像として採用する。
					サムネイル画像ファイルパス =
						( from ファイル名 in Directory.GetFiles( Path.GetDirectoryName( this.曲ファイルパス.変数なしパス ) )
						  where _対応するサムネイル画像名.Any( thumbファイル名 => ( Path.GetFileName( ファイル名 ).ToLower() == thumbファイル名 ) )
						  select ファイル名 ).FirstOrDefault();
				}
				if( null != サムネイル画像ファイルパス )
				{
					this.子リスト.Add( this.ノード画像 = new テクスチャ( サムネイル画像ファイルパス ) );
				}
			}

			// 曲ファイルと同じ場所に（対応する拡張子を持った）動画ファイルがあるなら、それを背景動画として採用する。
			this.動画ファイルパス =
				( from ファイル名 in Directory.GetFiles( Path.GetDirectoryName( this.曲ファイルパス.変数なしパス ) )
				  where _対応する動画の拡張子.Any( 拡張子名 => ( Path.GetExtension( ファイル名 ).ToLower() == 拡張子名 ) )
				  select ファイル名 ).FirstOrDefault()?.ToVariablePath();
		}


		private readonly string[] _対応する動画の拡張子 = { ".mp4", ".avi" };

		private readonly string[] _対応するサムネイル画像名 = { "thumb.png", "thumb.bmp", "thumb.jpg", "thumb.jpeg" };
	}
}
