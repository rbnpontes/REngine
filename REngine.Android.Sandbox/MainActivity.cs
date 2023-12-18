using System.Text;
using Android.Util;
using Android.Views;
using REngine.Android.Windows;
using REngine.Core;
using REngine.Core.Android;
using REngine.Core.Resources;
using WindowManager = REngine.Android.Windows.WindowManager;

namespace REngine.Android.Sandbox
{
	[Activity(Label = "@string/app_name", MainLauncher = true)]
	public class MainActivity : GameActivity<SandboxApp>
	{
	}
}