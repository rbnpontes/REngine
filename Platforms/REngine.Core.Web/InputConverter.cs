using REngine.Core.IO;

namespace REngine.Core.Web;

public static class InputConverter
{
    #region Key Maps
    private static readonly MouseKey[] sMouseButtonMap =
    [
        MouseKey.Left,
        MouseKey.Middle,
        MouseKey.Right,
        MouseKey.XButton1,
        MouseKey.XButton2
    ];

    private static readonly InputKey[] sKeys =
    [
        InputKey.None,
        InputKey.None,
        InputKey.None,
        InputKey.None,
        InputKey.None,
        InputKey.None,
        InputKey.None,
        InputKey.None,
        InputKey.Backspace,
        InputKey.Tab,
        InputKey.Enter,
        InputKey.Shift,
        InputKey.Control,
        InputKey.Alt,
        InputKey.Pause,
        InputKey.Capslock,
        InputKey.Esc,
        InputKey.PageUp,
        InputKey.Space,
        InputKey.PageDown,
        InputKey.End,
        InputKey.Home,
        InputKey.Left,
        InputKey.Up,
        InputKey.Right,
        InputKey.Down,
        InputKey.PrintScreen,
        InputKey.Insert,
        InputKey.Delete,
        InputKey.D0,
        InputKey.D1,
        InputKey.D2,
        InputKey.D3,
        InputKey.D4,
        InputKey.D5,
        InputKey.D6,
        InputKey.D7,
        InputKey.D8,
        InputKey.D9,
        InputKey.A,
        InputKey.B,
        InputKey.C,
        InputKey.D,
        InputKey.E,
        InputKey.F,
        InputKey.G,
        InputKey.H,
        InputKey.I,
        InputKey.J,
        InputKey.K,
        InputKey.L,
        InputKey.M,
        InputKey.N,
        InputKey.O,
        InputKey.P,
        InputKey.Q,
        InputKey.R,
        InputKey.S,
        InputKey.T,
        InputKey.U,
        InputKey.V,
        InputKey.W,
        InputKey.X,
        InputKey.Y,
        InputKey.Z,
        InputKey.LeftSuper,
        InputKey.RightSuper,
        InputKey.Menu,
        InputKey.NumPad0,
        InputKey.NumPad1,
        InputKey.NumPad2,
        InputKey.NumPad3,
        InputKey.NumPad4,
        InputKey.NumPad5,
        InputKey.NumPad6,
        InputKey.NumPad7,
        InputKey.NumPad8,
        InputKey.NumPad9,
        InputKey.Multiply,
        InputKey.Add,
        InputKey.Subtract,
        InputKey.Decimal,
        InputKey.Divide,
        InputKey.F1,
        InputKey.F2,
        InputKey.F3,
        InputKey.F4,
        InputKey.F5,
        InputKey.F6,
        InputKey.F7,
        InputKey.F8,
        InputKey.F9,
        InputKey.F10,
        InputKey.F11,
        InputKey.F12,
        InputKey.NumLock,
        InputKey.Scroll,
        InputKey.None,
        InputKey.None,
        InputKey.Semicolon,
        InputKey.Equal,
        InputKey.Comma,
        InputKey.Minus,
        InputKey.Period,
        InputKey.Slash,
        InputKey.OpenBrackets,
        InputKey.Backslash,
        InputKey.CloseBrackets,
        InputKey.Quotes
    ];
    #endregion
    public static MouseKey GetMouseKey(int buttonId)
    {
        if (buttonId < 0 || buttonId >= sMouseButtonMap.Length)
            throw new ArgumentOutOfRangeException(nameof(buttonId));
        return sMouseButtonMap[buttonId];
    }

    public static InputKey GetInputKey(int keyCode)
    {
        if (keyCode < 0 || keyCode >= sKeys.Length)
            throw new ArgumentOutOfRangeException(nameof(keyCode));
        return sKeys[keyCode];
    }
}