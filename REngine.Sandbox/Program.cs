using REngine.Core;
using REngine.Core.DependencyInjection;
using REngine.RHI;
using REngine.RHI.DiligentDriver;
using REngine.Windows;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace REngine.Sandbox
{
	internal static class Program
	{
		struct Vertex
		{
			public Vector3 Position;
			public Vector2 UV;

			public Vertex()
			{
				Position = new Vector3();
				UV = new Vector2();
			}

			public Vertex(Vector3 position, Vector2 uv)
			{
				Position = position;
				UV = uv;
			}
		}
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