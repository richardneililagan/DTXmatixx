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

		public bool 可視の初期値
		{
			get
			{
				return (
					// ↓これらは不可視。
					( this._chip.チップ種別 == チップ種別.BPM ) ||
					( this._chip.チップ種別 == チップ種別.背景動画 ) ||
					( this._chip.チップ種別 == チップ種別.小節メモ ) ||
					( this._chip.チップ種別 == チップ種別.小節の先頭 ) ||
					( this._chip.チップ種別 == チップ種別.SE ) ||
					( this._chip.チップ種別 == チップ種別.Unknown )
					) ? false : true;
			}
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
			this.可視 = this.可視の初期値;
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
