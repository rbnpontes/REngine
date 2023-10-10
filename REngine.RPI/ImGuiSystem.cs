using REngine.Core;
using REngine.Core.IO;
using REngine.Core.Mathematics;
using REngine.Core.Threading;
using REngine.RHI;
using REngine.RPI.Features;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace REngine.RPI
{
	internal class ImGuiSystem : IImGuiSystem, IDisposable
	{
		const byte MaxMouseKeys = (byte)MouseKey.XButton2;

		private readonly IExecutionPipeline pExecutionPipeline;
		private readonly EngineEvents pEngineEvents;
		private readonly IEngine pEngine;
		private readonly GraphicsSettings pGraphicsSettings;
		private readonly IRenderer pRenderer;
		private readonly IInput pInput;

		private ImGuiFeature? pFeature;

		private bool pDisposed = false;
		private IntPtr pImGuiCtx = IntPtr.Zero;

		public UIntSize FontSize { get; private set; } = new UIntSize();
		public byte[] FontData { get; private set; } = new byte[0];

		public IRenderFeature Feature
		{
			get => GetFeature();
		}

		public event EventHandler? OnGui;

		public ImGuiSystem(
			IEngine engine,
			IExecutionPipeline executionPipeline,
			EngineEvents engineEvents,
			GraphicsSettings graphicsSettings,
			IRenderer renderer,
			IInput input
		) 
		{ 
			pEngine = engine;
			pEngineEvents = engineEvents;
			pExecutionPipeline = executionPipeline;
			pGraphicsSettings = graphicsSettings;
			pRenderer = renderer;
			pInput = input;

			engineEvents.OnStart += HandleEngineStart;
			engineEvents.OnStop += HandleEngineStop;
		}

		public void Dispose()
		{
			if (pDisposed)
				return;

			pEngineEvents.OnStart -= HandleEngineStart;
			pEngineEvents.OnStop -= HandleEngineStop;

			pExecutionPipeline.AddEvent(DefaultEvents.ImGuiDrawId, (_) => HandleDraw());

			if(pImGuiCtx != IntPtr.Zero)
				ImGuiNET.ImGui.DestroyContext(pImGuiCtx);
			pImGuiCtx = IntPtr.Zero;

			pDisposed = true;
			GC.SuppressFinalize(this);
		}

		private void HandleEngineStop(object? sender, EventArgs e)
		{
			Dispose();
		}

		private void HandleEngineStart(object? sender, EventArgs e)
		{
			pImGuiCtx = ImGuiNET.ImGui.CreateContext();	
			ImGuiNET.ImGui.SetCurrentContext(pImGuiCtx);

			var io = ImGuiNET.ImGui.GetIO();
			io.ConfigFlags |= ImGuiNET.ImGuiConfigFlags.DockingEnable;
			io.Fonts.AddFontDefault();
			io.Fonts.Build();

			io.DisplaySize.X = io.DisplaySize.Y = 1;

			AllocateFontBuffer();

			pExecutionPipeline.AddEvent(DefaultEvents.ImGuiDrawId, (_) => HandleDraw());
		}

		private unsafe void AllocateFontBuffer()
		{
			var io = ImGuiNET.ImGui.GetIO();

			unsafe
			{
				io.Fonts.GetTexDataAsRGBA32(out byte* pixelData, out int width, out int height, out int bpp);

				FontSize = new UIntSize((uint)width, (uint)height);

				FontData = new byte[width * height * bpp];

				Marshal.Copy(new IntPtr(pixelData), FontData, 0, FontData.Length);
			}

			io.Fonts.ClearTexData();
		}

		private void HandleDraw()
		{
			if (pDisposed)
				return;

			var io = ImGuiNET.ImGui.GetIO();
			io.DeltaTime = (float)pEngine.DeltaTime;
			io.MousePos = pInput.MousePosition;
			for (byte i = 1; i < MaxMouseKeys; ++i)
				io.MouseDown[i - 1] = pInput.GetMouseDown((MouseKey)i);

			ImGuiNET.ImGui.SetCurrentContext(pImGuiCtx);

			ImGuiNET.ImGui.NewFrame();
#if DEBUG
			bool demoWnd = true;
			ImGuiNET.ImGui.ShowDemoWindow(ref demoWnd);
#endif
			OnGui?.Invoke(this, EventArgs.Empty);

			ImGuiNET.ImGui.EndFrame();
		}

		private ImGuiFeature GetFeature()
		{
			if (pFeature is null || pFeature.IsDisposed)
				pFeature = AllocateFeature();
			return pFeature;
		}

		private ImGuiFeature AllocateFeature()
		{
			return new ImGuiFeature(this, pGraphicsSettings, pRenderer);
		}
	}
}
