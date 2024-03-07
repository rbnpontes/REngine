#if DESKTOP
using REngine.Core;
using REngine.Core.Desktop;
using REngine.Sandbox.Samples;
#endif
namespace REngine.Sandbox
{
	internal static class Program
	{
		/// <summary>
		///  The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main()
		{
#if DESKTOP
			EngineApplication.Run(DesktopEngineInstance.CreateStartup<SampleApp>());
#else
			Console.WriteLine("Unsupported Platform. Build this Project with Linux or Windows Configuration");
#endif
		}
	}
}