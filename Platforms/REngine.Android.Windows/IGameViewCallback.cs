namespace REngine.Android.Windows;

public interface IGameViewCallback
{
    void OnGameViewChange(GameView view);
    void OnGameViewReady(GameView view);
    void OnGameViewDestroy(GameView view);
}