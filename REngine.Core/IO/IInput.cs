using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace REngine.Core.IO
{
	public class InputEventArgs : EventArgs
	{
		public InputKey Key { get; internal set; }
	}
	public class InputMouseEventArgs : EventArgs
	{
		public MouseKey Key { get; internal set; }
	}
	public class InputTextEventArgs : EventArgs
	{
		public string Text { get; internal set; } = string.Empty;
	}

	public interface IInput
	{
		/// <summary>
		/// Listen events from Window and update input system
		/// </summary>
		/// <param name="window"></param>
		/// <returns>self instance</returns>
		public IInput BindWindow(IWindow window);

		public bool GetKeyDown(InputKey key);
		public bool GetKeyPress(InputKey key);
		public bool GetMouseDown(MouseKey key);
		public bool GetMousePress(MouseKey key);

		public Vector2 MousePosition { get; }
		public Vector2 MouseWheel { get; }
		public Vector2 MouseWheelXDeadZone { get; set; }
		public Vector2 MouseWheelYDeadZone { get; set; }
		public Vector2 MouseMovement { get; }

		public event EventHandler<InputEventArgs>? OnKeyDown;
		public event EventHandler<InputEventArgs>? OnKeyPressed;
		public event EventHandler<InputEventArgs>? OnKeyUp;
		public event EventHandler<InputTextEventArgs>? OnInput;

		public event EventHandler<InputMouseEventArgs>? OnMouseDown;
		public event EventHandler<InputMouseEventArgs>? OnMousePressed;
		public event EventHandler<InputMouseEventArgs>? OnMouseUp;
	}
}
