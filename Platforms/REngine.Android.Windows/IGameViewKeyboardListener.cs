using Android.Views;

namespace REngine.Android.Windows;

public interface IGameViewKeyboardListener
{
    public bool OnGameViewKeyDown(Keycode keyCode, int charCode);
    public bool OnGameViewKeyUp(Keycode keycode);
}