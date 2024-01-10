using REngine.Core.DependencyInjection;
using REngine.Core.Threading;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace REngine.Core.IO
{
	internal class InputImpl : IInput, IDisposable
	{
		const float DefaultMsWheelDeadzone = 0.34f;

		private readonly object pSync = new();
		private readonly byte[] pPressKeys = new byte[(int)InputKey.OemClear + 1];
		private readonly byte[] pMouseKeys = new byte[(int)MouseKey.XButton2 + 1];
		private readonly IServiceProvider pProvider;
		private readonly EngineEvents pEngineEvents;
		private readonly IExecutionPipeline pExecution;
		private readonly LinkedList<Action> pDisposeWndEvents = new();

		private bool pDisposed = false;

		private Vector2 pLastMousePos = new ();

		public Vector2 MousePosition { get; private set; }

		public Vector2 MouseWheel { get; private set; }
		public Vector2 MouseWheelXDeadZone { get; set; } = new(-DefaultMsWheelDeadzone, DefaultMsWheelDeadzone);
		public Vector2 MouseWheelYDeadZone { get; set; } = new(-DefaultMsWheelDeadzone, DefaultMsWheelDeadzone);

		public Vector2 MouseMovement => pLastMousePos - MousePosition;

		public event EventHandler<InputEventArgs>? OnKeyDown;
		public event EventHandler<InputEventArgs>? OnKeyPressed;
		public event EventHandler<InputEventArgs>? OnKeyUp;
		public event EventHandler<InputTextEventArgs>? OnInput;

		public event EventHandler<InputMouseEventArgs>? OnMouseDown;
		public event EventHandler<InputMouseEventArgs>? OnMousePressed;
		public event EventHandler<InputMouseEventArgs>? OnMouseUp;


		public InputImpl(
			EngineEvents engineEvents,
			IServiceProvider provider,
			IExecutionPipeline execution
		)
		{
			pEngineEvents = engineEvents;
			pProvider = provider;
			pExecution = execution;

			engineEvents.OnStart += HandleEngineStart;
			engineEvents.OnBeforeStop += HandleEngineStop;
		}

		public void Dispose()
		{
			if (pDisposed)
				return;

			pEngineEvents.OnStart -= HandleEngineStart;
			pEngineEvents.OnBeforeStop -= HandleEngineStop;

			var disposeWndEventsNode = pDisposeWndEvents.First;
			while(disposeWndEventsNode != null)
			{
				disposeWndEventsNode.Value();
				disposeWndEventsNode = disposeWndEventsNode.Next;
			}

			pDisposeWndEvents.Clear();
			pDisposed = true;

			GC.SuppressFinalize(this);
		}

		private void HandleEngineStop(object? sender, EventArgs e)
		{
			Dispose();
		}

		private void HandleEngineStart(object? sender, EventArgs e)
		{
			IWindow? wnd = pProvider.GetOrDefault<IWindow>();
			// Bind Main Window
			if (wnd != null)
				BindWindow(wnd);

			pExecution
				.AddEvent(DefaultEvents.UpdateBeginId, (_) => ApplyMouseWheelDeadZone())
				.AddEvent(DefaultEvents.UpdateEndId, (_) => UpdateKeyPresses());
		}

		private void ApplyMouseWheelDeadZone()
		{
			lock (pSync)
			{
				// Apply Deadzone
				var mouseWheelX = MouseWheel.X;
				var mouseWheelY = MouseWheel.Y;

				if (MouseWheel.X >= MouseWheelXDeadZone.X && MouseWheel.X <= MouseWheelXDeadZone.Y)
					mouseWheelX = 0;
				if (MouseWheel.Y >= MouseWheelYDeadZone.X && MouseWheel.Y <= MouseWheelYDeadZone.Y)
					mouseWheelY = 0;

				MouseWheel = new Vector2(mouseWheelX, mouseWheelY);
			}
		}

		private void UpdateKeyPresses()
		{
			lock (pSync)
			{
				// When key is down, the held value is 1
				// But at end of frame we increase to 2
				// This will make our method and events GetKeyPress and GetMousePress
				// to be triggered
				for(var i =0; i < pPressKeys.Length; ++i)
				{
					if (pPressKeys[i] != 1) continue;
					
					pPressKeys[i] = 2;
					OnKeyPressed?.Invoke(this, new InputEventArgs { Key = (InputKey)i });
				}
				for(var i =0; i < pMouseKeys.Length; ++i)
				{
					if (pMouseKeys[i] != 1) continue;
					
					pMouseKeys[i] = 2;
					OnMousePressed?.Invoke(this, new InputMouseEventArgs { Key = (MouseKey)i });
				}
			}
		}

		public IInput BindWindow(IWindow window)
		{
			window.OnKeyDown += HandleKeyDown;
			window.OnKeyUp += HandleKeyUp;
			window.OnInput += HandleInput;

			window.OnMouseDown += HandleMouseDown;
			window.OnMouseUp += HandleMouseUp;
			window.OnMouseMove += HandleMouseMove;
			window.OnMouseWheel += HandleMouseWheel;

			pDisposeWndEvents.AddLast(() =>
			{
				window.OnKeyDown -= HandleKeyDown;
				window.OnKeyUp -= HandleKeyUp;
				window.OnInput -= HandleInput;

				window.OnMouseDown -= HandleMouseDown;
				window.OnMouseUp -= HandleMouseUp;
				window.OnMouseMove -= HandleMouseMove;
				window.OnMouseWheel -= HandleMouseWheel;
			});
			return this;
		}

		private void HandleMouseWheel(object sender, WindowMouseWheelEventArgs e)
		{
			MouseWheel = e.Wheel;
		}

		private void HandleInput(object sender, WindowInputTextEventArgs e)
		{
			OnInput?.Invoke(this, new InputTextEventArgs { Text = e.Value });
		}

		private void HandleMouseMove(object sender, WindowMouseEventArgs e)
		{
			lock (pSync)
			{
				pLastMousePos = MousePosition;
				MousePosition = e.Position;
			}
		}

		private void HandleMouseUp(object sender, WindowMouseEventArgs e)
		{
			lock(pSync)
				pMouseKeys[(int)e.MouseKey] = 0;
		}

		private void HandleMouseDown(object sender, WindowMouseEventArgs e)
		{
			lock (pSync)
				pMouseKeys[(int)e.MouseKey] = 1;
		}

		private void HandleKeyUp(object sender, WindowInputEventArgs e)
		{
			int keyIdx = (int)e.Key;
			if (keyIdx >= pPressKeys.Length)
				return;

			int combinedKey = GetCombinedKey(e.Key);
			lock(pSync)
				pPressKeys[keyIdx] = pPressKeys[combinedKey] = 0;

			OnKeyUp?.Invoke(this, new InputEventArgs { Key = e.Key });
		}

		private void HandleKeyDown(object sender, WindowInputEventArgs e)
		{
			int keyIdx = (int)e.Key;
			if (keyIdx >= pPressKeys.Length)
				return;

			int combinedKey = GetCombinedKey(e.Key);
			lock (pSync)
			{
				// Only set pressed keys if key has been released
				if (pPressKeys[keyIdx] == 0)
					pPressKeys[keyIdx] = pPressKeys[combinedKey] = 1;

				// sometimes combined key returns zero, and this value is wrote with value
				// of the key. we must guarantee that this value never changes
				// otherwise we will have strange behaviours
				pPressKeys[0] = 0;
			}

			OnKeyDown?.Invoke(this, new InputEventArgs { Key = e.Key });
		}

		public bool GetKeyDown(InputKey key)
		{
			bool state = false;
			lock (pSync)
				state = pPressKeys[(int)key] > 0;
			return state;
		}

		public bool GetKeyPress(InputKey key)
		{
			bool state = false;
			lock (pSync)
				state = pPressKeys[(int)key] == 1;
			return state;
		}

		public bool GetMouseDown(MouseKey key)
		{
			bool state = false;
			lock (pSync)
				state = pMouseKeys[(int)key] > 0;
			return state;
		}

		public bool GetMousePress(MouseKey key)
		{
			bool state = false;
			lock (pSync)
				state = pMouseKeys[(int)key] == 1;
			return state;
		}
		
		private int GetCombinedKey(InputKey key)
		{
			int combinedKey = 0;
			switch(key)
			{
				case InputKey.LeftControl:
				case InputKey.RightControl:
					combinedKey = (int)InputKey.Control;
					break;
				case InputKey.LeftAlt:
				case InputKey.RightAlt:
					combinedKey = (int)InputKey.Alt;
					break;
				case InputKey.LeftShift:
				case InputKey.RightShift:
					combinedKey = (int)InputKey.Shift;
					break;
			}

			return combinedKey;
		}
	}
}
