#if !WEB
using ImGuiNET;
using REngine.Core;
using REngine.Core.DependencyInjection;
using REngine.Core.IO;
using REngine.RPI;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using REngine.Sandbox.BaseSample;

namespace REngine.Sandbox.Samples.BasicSamples
{
	[Sample("ImGui")]
	internal class ImGuiSample : ISample
	{
		private const double ByteColorToDecimal = 1.0 / 255.0;
		public IWindow? Window { get; set; }
		private IServiceProvider? pServiceProvider;
		private ILogger<ImGuiSample>? pLogger;
		private RenderState? pRenderState;

		private bool pMyToolActive = false;
		private Vector4 pMyColor = new();

		public void Dispose()
		{
			pServiceProvider.Get<IImGuiSystem>().OnGui -= OnGui;
			GC.SuppressFinalize(this);
		}

		public void Load(IServiceProvider provider)
		{
			pServiceProvider = provider;

			provider.Get<IImGuiSystem>().OnGui += OnGui;
			pRenderState = provider.Get<RenderState>();
			pLogger = provider.Get<ILoggerFactory>().Build<ImGuiSample>();
		}

		public void Update(IServiceProvider provider)
		{
			return;
		}

		private void OnGui(object? sender, EventArgs e)
		{
			ImGui.Begin("My First Tool", ref pMyToolActive, ImGuiWindowFlags.MenuBar);
			if (ImGui.BeginMenuBar())
			{
				if (ImGui.BeginMenu("File"))
				{
					if (ImGui.MenuItem("Open..", "Ctrl+O"))
						pLogger?.Debug("You clicked on Open");
					if (ImGui.MenuItem("Save", "Ctrl+S"))
						pLogger?.Debug("You clicked on Save");
					if (ImGui.MenuItem("Close", "Ctrl+W")) 
					{
						pLogger?.Debug("You clicked on Close");
						pMyToolActive = false;
					}
					ImGui.EndMenu();
				}
				ImGui.EndMenuBar();
			}

			// Edit a color stored as 4 floats
			ImGui.ColorEdit4("Color", ref pMyColor);
			if (pRenderState != null)
			{
				var clearColor = new Vector3(
					(float)(pRenderState.DefaultClearColor.R * ByteColorToDecimal), 
					(float)(pRenderState.DefaultClearColor.G * ByteColorToDecimal), 
					(float)(pRenderState.DefaultClearColor.B * ByteColorToDecimal)
				);
				ImGui.ColorEdit3("Clear Color", ref clearColor);
				pRenderState.DefaultClearColor = Color.FromArgb(255, (byte)(clearColor.X * 255), (byte)(clearColor.Y * 255), (byte)(clearColor.Z * 255));
			}

			// Generate samples and plot them
			float[] samples = new float[100];
			for (int n = 0; n < 100; n++)
				samples[n] = (float)Math.Sin(n * 0.2f + ImGui.GetTime() * 1.5f);
			ImGui.PlotLines("Samples", ref samples[0], 100);

			// Display contents in a scrolling region
			ImGui.TextColored(new Vector4(1, 1, 0, 1), "Important Stuff");
			ImGui.BeginChild("Scrolling");
			for (int n = 0; n < 50; n++)
				ImGui.Text($"{n}: Some text");
			ImGui.EndChild();
			ImGui.End();
		}

	}
}
#endif