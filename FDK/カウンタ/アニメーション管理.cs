using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX.Animation;

namespace FDK.カウンタ
{
	public class アニメーション管理 : IDisposable
	{
		public Manager Manager
			=> this._manager;

		public Timer Timer
			=> this._timer;

		public TransitionLibrary TrasitionLibrary
			=> this._trasitionLibrary;


		public アニメーション管理()
		{
			this._manager = new Manager();
			this._timer = new Timer();
			this._trasitionLibrary = new TransitionLibrary();
		}

		public void Dispose()
		{
			FDKUtilities.解放する( ref this._trasitionLibrary );
			FDKUtilities.解放する( ref this._timer );
			FDKUtilities.解放する( ref this._manager );
		}

		public void 進行する()
		{
			this._manager.Update( this._timer.Time );
		}


		private Manager _manager = null;

		private Timer _timer = null;

		private TransitionLibrary _trasitionLibrary = null;
	}
}
