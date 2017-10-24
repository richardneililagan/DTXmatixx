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
		public string 曲ファイルパス
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
		public string 動画ファイルパス
		{
			get;
			protected set;
		} = null;

		/// <summary>
		///		この曲ノードに対応する難易度。
		///		0.00～9.99。
		/// </summary>
		public float 難易度
		{
			get;
			protected set;
		} = 5.0f;


		public MusicNode( string 曲ファイルパス, Node 親ノード )
		{
			this.親ノード = 親ノード;
			this.曲ファイルパス = Folder.絶対パスに含まれるフォルダ変数を展開して返す( 曲ファイルパス );

			// （まだ存在してなければ）曲DBに追加する。
			曲DB.曲を追加または更新する( this.曲ファイルパス, App.ユーザ設定 );

			// 曲DBから情報を取得する。
			var song = 曲DB.曲を取得する( this.曲ファイルパス );
			if( null != song )
			{
				this.タイトル = song.Title;
				this.難易度 = (float) song.Level;
				this.曲ファイルハッシュ = song.HashId;
			}

			// 曲ファイルと同じ場所に画像ファイルがあるなら、それをノード画像として採用する。
			var サムネイル画像ファイルパス =
				( from ファイル名 in Directory.GetFiles( Path.GetDirectoryName( this.曲ファイルパス ) )
				  where _対応するサムネイル画像名.Any( thumbファイル名 => ( Path.GetFileName( ファイル名 ).ToLower() == thumbファイル名 ) )
				  select ファイル名 ).FirstOrDefault();

			if( null != サムネイル画像ファイルパス )
			{
				this.子リスト.Add( this.ノード画像 = new テクスチャ( サムネイル画像ファイルパス ) );
			}

			// 曲ファイルと同じ場所に（対応する拡張子を持った）動画ファイルがあるなら、それを背景動画として採用する。
			this.動画ファイルパス =
				( from ファイル名 in Directory.GetFiles( Path.GetDirectoryName( this.曲ファイルパス ) )
				  where _対応する動画の拡張子.Any( 拡張子名 => ( Path.GetExtension( ファイル名 ).ToLower() == 拡張子名 ) )
				  select ファイル名 ).FirstOrDefault();
		}


		private readonly string[] _対応する動画の拡張子 = { ".mp4", ".avi" };

		private readonly string[] _対応するサムネイル画像名 = { "thumb.png", "thumb.bmp", "thumb.jpg", "thumb.jpeg" };
	}
}
