using REngine.Core.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Keys = GLFW.Keys;
namespace REngine.Windows
{
	internal static class InputConverter
	{
		private static Dictionary<Keys, InputKey> sKeyboards = new Dictionary<Keys, InputKey>
		{
			{ Keys.Space, InputKey.Space },
			{ Keys.Apostrophe, InputKey.D6 },
			{ Keys.Comma, InputKey.Comma },
			{ Keys.Minus, InputKey.Minus },
			{ Keys.Period, InputKey.Period },
			{ Keys.Slash, InputKey.Pipe },
			{ Keys.Alpha0, InputKey.D0 },
			{ Keys.Alpha1, InputKey.D1 },
			{ Keys.Alpha2, InputKey.D2 },
			{ Keys.Alpha3, InputKey.D3 },
			{ Keys.Alpha4, InputKey.D4 },
			{ Keys.Alpha5, InputKey.D5 },
			{ Keys.Alpha6, InputKey.D6 },
			{ Keys.Alpha7, InputKey.D7 },
			{ Keys.Alpha8, InputKey.D8 },
			{ Keys.Alpha9, InputKey.D9 },
			{ Keys.SemiColon, InputKey.Semicolon },
			{ Keys.Equal, InputKey.Plus },
			{ Keys.A, InputKey.A },
			{ Keys.B, InputKey.B },
			{ Keys.C, InputKey.C },
			{ Keys.D, InputKey.D },
			{ Keys.E, InputKey.E },
			{ Keys.F, InputKey.F },
			{ Keys.G, InputKey.G },
			{ Keys.H, InputKey.H },
			{ Keys.I, InputKey.I },
			{ Keys.J, InputKey.J },
			{ Keys.K, InputKey.K },
			{ Keys.L, InputKey.L },
			{ Keys.M, InputKey.M },
			{ Keys.N, InputKey.N },
			{ Keys.O, InputKey.O },
			{ Keys.P, InputKey.P },
			{ Keys.Q, InputKey.Q },
			{ Keys.R, InputKey.R },
			{ Keys.S, InputKey.S },
			{ Keys.T, InputKey.T },
			{ Keys.U, InputKey.U },
			{ Keys.V, InputKey.V },
			{ Keys.W, InputKey.W },
			{ Keys.X, InputKey.X },
			{ Keys.Y, InputKey.Y },
			{ Keys.Z, InputKey.Z },
			{ Keys.LeftBracket, InputKey.OpenBrackets },
			{ Keys.Backslash, InputKey.Backslash },
			{ Keys.RightBracket, InputKey.CloseBrackets },
			{ Keys.GraveAccent, InputKey.Tilde },
			{ Keys.Escape, InputKey.Esc },
			{ Keys.Enter, InputKey.Enter },
			{ Keys.Tab, InputKey.Tab },
			{ Keys.Backspace, InputKey.Backspace },
			{ Keys.Insert, InputKey.Insert },
			{ Keys.Delete, InputKey.Delete },
			{ Keys.Right, InputKey.Right },
			{ Keys.Left, InputKey.Left },
			{ Keys.Up, InputKey.Up },
			{ Keys.PageUp, InputKey.PageUp },
			{ Keys.PageDown, InputKey.PageDown },
			{ Keys.Home, InputKey.Home },
			{ Keys.End, InputKey.End },
			{ Keys.CapsLock, InputKey.Capslock },
			{ Keys.ScrollLock, InputKey.Scroll },
			{ Keys.NumLock, InputKey.NumLock },
			{ Keys.PrintScreen, InputKey.PrintScreen },
			{ Keys.Pause, InputKey.Pause },
			{ Keys.F1, InputKey.F1 },
			{ Keys.F2, InputKey.F2 },
			{ Keys.F3, InputKey.F3 },
			{ Keys.F4, InputKey.F4 },
			{ Keys.F5, InputKey.F5 },
			{ Keys.F6, InputKey.F6 },
			{ Keys.F7, InputKey.F7 },
			{ Keys.F8, InputKey.F8 },
			{ Keys.F9, InputKey.F9 },
			{ Keys.F10, InputKey.F10 },
			{ Keys.F11, InputKey.F11 },
			{ Keys.F12, InputKey.F12 },
			{ Keys.F13, InputKey.F13 },
			{ Keys.F14, InputKey.F14 },
			{ Keys.F15, InputKey.F15 },
			{ Keys.F16, InputKey.F16 },
			{ Keys.F17, InputKey.F17 },
			{ Keys.F18, InputKey.F18 },
			{ Keys.F19, InputKey.F19 },
			{ Keys.F20, InputKey.F20 },
			{ Keys.F21, InputKey.F21 },
			{ Keys.F22, InputKey.F22 },
			{ Keys.F23, InputKey.F23 },
			{ Keys.F24, InputKey.F24 },
			{ Keys.F25, InputKey.F25 },
			{ Keys.Numpad0, InputKey.NumPad0 },
			{ Keys.Numpad1, InputKey.NumPad1 },
			{ Keys.Numpad2, InputKey.NumPad2 },
			{ Keys.Numpad3, InputKey.NumPad3 },
			{ Keys.Numpad4, InputKey.NumPad4 },
			{ Keys.Numpad5, InputKey.NumPad5 },
			{ Keys.Numpad6, InputKey.NumPad6 },
			{ Keys.Numpad7, InputKey.NumPad7 },
			{ Keys.Numpad8, InputKey.NumPad8 },
			{ Keys.Numpad9, InputKey.NumPad9 },
			{ Keys.NumpadDecimal, InputKey.Decimal },
			{ Keys.NumpadDivide, InputKey.Divide },
			{ Keys.NumpadMultiply, InputKey.Multiply },
			{ Keys.NumpadSubtract, InputKey.Subtract },
			{ Keys.NumpadAdd, InputKey.Add },
			{ Keys.NumpadEnter, InputKey.Enter },
			{ Keys.NumpadEqual, InputKey.Equal },
			{ Keys.LeftShift, InputKey.LeftShift },
			{ Keys.LeftControl, InputKey.LeftControl },
			{ Keys.LeftAlt, InputKey.LeftAlt },
			{ Keys.LeftSuper, InputKey.LeftSuper },
			{ Keys.RightShift, InputKey.RightShift },
			{ Keys.RightControl, InputKey.RightControl },
			{ Keys.RightAlt, InputKey.RightAlt },
			{ Keys.RightSuper, InputKey.RightSuper },
			{ Keys.Menu, InputKey.Menu },
		};

		private static readonly MouseKey[] sMouseKeys = new MouseKey[]
		{
			MouseKey.Left,
			MouseKey.Right,
			MouseKey.Middle,
			MouseKey.XButton1,
			MouseKey.XButton2
		};

		public static MouseKey GetMouseKey(GLFW.MouseButton button)
		{
			return sMouseKeys[(int)button];
		}

		public static InputKey GetInputKey(GLFW.Keys keys)
		{
			if(sKeyboards.TryGetValue(keys, out var key))
				return key;
			return InputKey.None;
		}
	}
}
