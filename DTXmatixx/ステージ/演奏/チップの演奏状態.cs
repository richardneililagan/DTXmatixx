using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SSTFormat.v3;

namespace DTXmatixx.ステージ.演奏
{
	/// <summary>
	///		チップに対応する、チップの演奏情報。
	/// </summary>
	class チップの演奏状態 : IDisposable
	{
		public bool 可視 { get; set; } = true;
		public bool 不可視
		{
			get => !this.可視;
			set => this.可視 = !value;
		}

		public bool ヒット済みである { get; set; } = false;
		public bool ヒットされていない
		{
			get => !this.ヒット済みである;
			set => this.ヒット済みである = !value;
		}

		public bool 発声済みである { get; set; } = false;
		public bool 発声されていない
		{
			get => !this.発声済みである;
			set => this.発声済みである = !value;
		}

		public チップの演奏状態( チップ chip )
		{
			this._chip = chip;
		}
		public void Dispose()
		{
			this._chip = null;
		}
		public void CopyFrom( チップの演奏状態 srcChipStatus )
		{
			this.可視 = srcChipStatus.可視;
			this.ヒット済みである = srcChipStatus.ヒット済みである;
			this.発声済みである = srcChipStatus.発声済みである;
		}
		public void ヒット前の状態にする()
		{
			this.可視 = !( App.ユーザ設定.ドラムとチップと入力の対応表.対応表[ this._chip.チップ種別 ].不可視 );
			this.ヒット済みである = false;
			this.発声済みである = false;
		}
		public void ヒット済みの状態にする()
		{
			this.可視 = false;
			this.ヒット済みである = true;
			this.発声済みである = true;
		}

		protected チップ _chip = null;
	}
}
