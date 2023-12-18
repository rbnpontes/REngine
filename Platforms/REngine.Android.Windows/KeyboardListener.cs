using Android.Text;
using Android.Text.Method;
using Android.Views;
using REngine.Core.IO;

namespace REngine.Android.Windows;

internal class KeyboardListener(WindowImpl window) : IGameViewKeyboardListener
{
    private static readonly Dictionary<Keycode, InputKey> pKeys = new()
    {
        { Keycode.A , InputKey.A},
        { Keycode.AllApps, InputKey.Apps},
        { Keycode.AltLeft, InputKey.LeftAlt},
        { Keycode.AltRight, InputKey.RightAlt},
        { Keycode.Apostrophe, InputKey.Quotes},
        { Keycode.AppSwitch, InputKey.Menu },
        { Keycode.B, InputKey.B},
        { Keycode.Back, InputKey.Backspace },
        { Keycode.Bookmark, InputKey.BrowserFavorites },
        { Keycode.Break, InputKey.Pause },
        { Keycode.C, InputKey.C},
        { Keycode.CapsLock, InputKey.Capslock },
        { Keycode.ChannelDown, InputKey.Down},
        { Keycode.ChannelUp, InputKey.Up },
        { Keycode.Clear, InputKey.Clear },
        { Keycode.Comma, InputKey.Comma },
        { Keycode.CtrlLeft, InputKey.LeftControl },
        { Keycode.CtrlRight, InputKey.RightControl},
        { Keycode.D, InputKey.D},
        { Keycode.Del, InputKey.Backspace},
        { Keycode.DpadCenter, InputKey.NumPad5},
        { Keycode.DpadLeft, InputKey.NumPad4},
        { Keycode.DpadRight, InputKey.NumPad6},
        { Keycode.DpadUp, InputKey.NumPad8},
        { Keycode.DpadDown, InputKey.NumPad2 },
        { Keycode.E , InputKey.E},
        { Keycode.Enter, InputKey.Enter},
        { Keycode.Envelope, InputKey.Email},
        { Keycode.Equals, InputKey.Equal},
        { Keycode.Escape, InputKey.Esc},
        { Keycode.F, InputKey.F},
        { Keycode.F1, InputKey.F1},
        { Keycode.F2, InputKey.F2},
        { Keycode.F3, InputKey.F3},
        { Keycode.F4, InputKey.F4},
        { Keycode.F5, InputKey.F5},
        { Keycode.F6, InputKey.F6},
        { Keycode.F7, InputKey.F7},
        { Keycode.F8, InputKey.F8},
        { Keycode.F9, InputKey.F9},
        { Keycode.F10, InputKey.F10},
        { Keycode.F11, InputKey.F11},
        { Keycode.F12, InputKey.F12},
        { Keycode.Forward, InputKey.BrowserForward},
        { Keycode.ForwardDel, InputKey.Delete},
        { Keycode.G, InputKey.G},
        { Keycode.Grave, InputKey.Grave},
        { Keycode.H, InputKey.H},
        { Keycode.Help, InputKey.Help},
        { Keycode.Home, InputKey.Home},
        { Keycode.I, InputKey.I},
        { Keycode.Insert, InputKey.Insert},
        { Keycode.J, InputKey.J},
        { Keycode.K, InputKey.K},
        { Keycode.L, InputKey.L},
        { Keycode.LeftBracket, InputKey.OpenBrackets},
        { Keycode.M, InputKey.M},
        { Keycode.Menu, InputKey.Menu},
        { Keycode.MetaLeft, InputKey.LeftSuper},
        { Keycode.MetaRight, InputKey.RightSuper },
        { Keycode.Minus, InputKey.Minus},
        { Keycode.Mute, InputKey.VolumeMute},
        { Keycode.N, InputKey.N},
        { Keycode.Num, InputKey.Quotes},
        { Keycode.Num0, InputKey.D0},
        { Keycode.Num1, InputKey.D1},
        { Keycode.Num2, InputKey.D2},
        { Keycode.Num3, InputKey.D3},
        { Keycode.Num4, InputKey.D4},
        { Keycode.Num5, InputKey.D5},
        { Keycode.Num6, InputKey.D6},
        { Keycode.Num7, InputKey.D7},
        { Keycode.Num8, InputKey.D8},
        { Keycode.Num9, InputKey.D9},
        { Keycode.NumLock, InputKey.NumLock },
        { Keycode.Numpad0, InputKey.NumPad0},
        { Keycode.Numpad1, InputKey.NumPad1},
        { Keycode.Numpad2, InputKey.NumPad2},
        { Keycode.Numpad3, InputKey.NumPad3},
        { Keycode.Numpad4, InputKey.NumPad4},
        { Keycode.Numpad5, InputKey.NumPad5},
        { Keycode.Numpad6, InputKey.NumPad6},
        { Keycode.Numpad7, InputKey.NumPad7},
        { Keycode.Numpad8, InputKey.NumPad8},
        { Keycode.Numpad9, InputKey.NumPad9},
        { Keycode.NumpadAdd, InputKey.Add},
        { Keycode.NumpadComma, InputKey.Comma},
        { Keycode.NumpadDivide, InputKey.Divide},
        { Keycode.NumpadDot, InputKey.Period},
        { Keycode.NumpadEnter, InputKey.Enter},
        { Keycode.NumpadLeftParen, InputKey.D9 },
        { Keycode.NumpadMultiply, InputKey.Multiply},
        { Keycode.NumpadRightParen, InputKey.D0},
        { Keycode.NumpadSubtract, InputKey.Subtract},
        { Keycode.O, InputKey.O},
        { Keycode.P, InputKey.P},
        { Keycode.PageDown, InputKey.PageDown},
        { Keycode.PageUp, InputKey.PageUp},
        { Keycode.Period, InputKey.Period },
        { Keycode.Plus, InputKey.Plus },
        { Keycode.Pound, InputKey.D3},
        { Keycode.Q, InputKey.Q},
        { Keycode.R, InputKey.R},
        { Keycode.Refresh, InputKey.F5},
        { Keycode.RightBracket, InputKey.CloseBrackets},
        { Keycode.S, InputKey.S},
        { Keycode.ScrollLock, InputKey.Scroll},
        { Keycode.Search, InputKey.BrowserSearch},
        { Keycode.Semicolon, InputKey.Semicolon},
        { Keycode.ShiftLeft, InputKey.LeftShift},
        { Keycode.ShiftRight, InputKey.RightShift},
        { Keycode.Slash, InputKey.Slash},
        { Keycode.Space, InputKey.Space},
        { Keycode.Star, InputKey.Multiply},
        { Keycode.T, InputKey.T},
        { Keycode.Tab, InputKey.Tab},
        { Keycode.U, InputKey.U},
        { Keycode.V, InputKey.V},
        { Keycode.VolumeDown, InputKey.VolumeDown},
        { Keycode.VolumeUp, InputKey.VolumeUp},
        { Keycode.W, InputKey.W},
        { Keycode.X, InputKey.X},
        { Keycode.Y, InputKey.Y},
        { Keycode.Z, InputKey.Z}
    };
    
    public bool OnGameViewKeyDown(Keycode keyCode, int charCode)
    {
        if (keyCode == Keycode.Del)
            charCode = 8; // fix: del == backspace. Android does not set backspace char value
        if(charCode != 0)
            window.ForwardInputEvent(charCode);

        if (!pKeys.TryGetValue(keyCode, out var key)) 
            return false;
        
        window.ForwardKeyDownEvent(key);
        return true;
    }
    
    public bool OnGameViewKeyUp(Keycode keyCode)
    {
        if (!pKeys.TryGetValue(keyCode, out var key))
            return false;

        window.ForwardKeyUpEvent(key);
        return true;
    }

}