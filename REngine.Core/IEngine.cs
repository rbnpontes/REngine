using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace REngine.Core
{
	public interface IEngine
	{
		public bool IsStopped { get; }
		public double DeltaTime { get; }
		public double ElapsedTime { get; }
		/// <summary>
		/// Check if Virtual Keyboard is Visible
		/// This property only works on Android and iOS only
		/// </summary>
		public bool IsKeyboardVisible { get; }
		/// <summary>
		/// Test if this thread you are calling is Main Thread
		/// </summary>
		public bool IsMainThread { get; }
		public IEngine Start();
		public IEngine ExecuteFrame();
		public IEngine Stop();
		/// <summary>
		/// Android and iOS Only
		/// </summary>
		/// <returns></returns>
		public IEngine ShowKeyboard();
		/// <summary>
		/// Android and iOS Only
		/// </summary>
		/// <returns></returns>
		public IEngine HideKeyboard();
	}
}
