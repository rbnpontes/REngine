using System.Text;
using Android.Util;
using Android.Views;
using REngine.Android.Windows;
using REngine.Core.Reflection;

namespace REngine.Core.Android;

public abstract class GameActivity<T> : BaseGameActivity where T : IEngineApplication
{
    private readonly IEngineApplication pApp = ActivatorExtended.CreateInstance<T>([]);
    public override IEngineApplication OnGetEngineApplication() => pApp;
}