using REngine.Core;
using REngine.Core.DependencyInjection;
using REngine.RHI;
using REngine.RHI.DiligentDriver;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

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
			EngineApplication
				.CreateStartup<SandboxApp>()
				.Setup()
				.Start()
				.Run();
		}
	}
}