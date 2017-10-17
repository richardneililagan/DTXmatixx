using System;

namespace FDK.入力
{
	/// <summary>
	///		入力イベントデータの最小単位。全入力デバイスで共通。
	/// </summary>
	public class InputEvent
	{
		/// <summary>
		///		複数の同じ種類のデバイス同士を識別するための、内部デバイスID。
		/// </summary>
		public int DeviceID { get; set; }

		/// <summary>
		///		イベントが発生したキーのコード。
		///		値の意味はデバイスに依存する。
		/// </summary>
		public int Key { get; set; }

		/// <summary>
		///		キーが押されたのであれば true。
		///		「離された」プロパティとは排他。
		/// </summary>
		public bool 押された { get; set; }

		/// <summary>
		///		キーが離されたのであれば true。
		///		「押された」プロパティとは排他。
		/// </summary>
		public bool 離された { get; set; }

		/// <summary>
		///		このイベントが発生した時点の生パフォーマンスカウンタの値。
		/// </summary>
		public long TimeStamp { get; set; }

		/// <summary>
		///		入力されたキーの強さ。
		///		値の意味はデバイスに依存する。
		/// </summary>
		public int Velocity { get; set; }

		/// <summary>
		///		可読文字列形式に変換して返す。
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			return $"InputEvent[Key={Key},押された={押された},TimeStamp={TimeStamp},Velocity={Velocity}]";
		}
	}
}
