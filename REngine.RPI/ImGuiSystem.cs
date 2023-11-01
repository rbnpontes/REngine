using REngine.Core;
using REngine.Core.DependencyInjection;
using REngine.Core.IO;
using REngine.Core.Mathematics;
using REngine.Core.Threading;
using REngine.RHI;
using REngine.RPI.Features;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.CompilerServices.RuntimeHelpers;

namespace REngine.RPI
{
#if RENGINE_IMGUI
	internal class ImGuiSystem : IImGuiSystem, IDisposable
	{
		const byte MaxMouseKeys = (byte)MouseKey.XButton2;

		private readonly IServiceProvider pProvider;
		private readonly IExecutionPipeline pExecutionPipeline;
		private readonly EngineEvents pEngineEvents;
		private readonly GraphicsSettings pGraphicsSettings;
		private readonly IRenderer pRenderer;
		private readonly IInput pInput;
		private readonly ILogger<IImGuiSystem> pLogger;
		private readonly IExecutionPipelineVar pUpdateRateVar;
		private readonly RPIEvents pRPIEvents;

		private readonly string pImGuiSettingsPath;
		private readonly object pSync = new();
		private readonly Mutex pMutex = new();

		private readonly Stopwatch pStopwatch = Stopwatch.StartNew();

		private ImGuiFeature? pFeature;

		private string pTextToInsert = string.Empty;

		private bool pDisposed = false;
		private IntPtr pImGuiCtx = IntPtr.Zero;

		public UIntSize FontSize { get; private set; } = new UIntSize();
		public byte[] FontData { get; private set; } = new byte[0];

		public IGraphicsRenderFeature Feature
		{
			get => GetFeature();
		}

		public event EventHandler? OnGui;

		public ImGuiSystem(
			IExecutionPipeline executionPipeline,
			EngineEvents engineEvents,
			GraphicsSettings graphicsSettings,
			IRenderer renderer,
			IInput input,
			ILoggerFactory factory,
			RenderSettings renderSettings,
			RPIEvents rpiEvents,
			IServiceProvider provider
		)
		{ 
			pEngineEvents = engineEvents;
			pExecutionPipeline = executionPipeline;
			pGraphicsSettings = graphicsSettings;
			pRenderer = renderer;
			pInput = input;
			pLogger = factory.Build<IImGuiSystem>();
			pRPIEvents = rpiEvents;
			pProvider = provider;

			pImGuiSettingsPath = Path.Join(EngineSettings.AppDataPath, "imgui_settings.ini");
			pUpdateRateVar = executionPipeline.GetOrCreateVar(DefaultVars.ImGuiUpdateRate);
			pUpdateRateVar.Value = renderSettings.ImGuiUpdateRate;

			engineEvents.OnStart += HandleEngineStart;
			engineEvents.OnStop += HandleEngineStop;
			input.OnInput += HandleInput;
			input.OnKeyDown += HandleKeyDown;
			input.OnKeyUp += HandleKeyUp;

			rpiEvents.OnUpdateSettings += HandleUpdateSettings;
		}

		public void Dispose()
		{
			if (pDisposed)
				return;

			SaveImGuiSettings();

			pEngineEvents.OnStart -= HandleEngineStart;
			pEngineEvents.OnStop -= HandleEngineStop;

			pInput.OnInput -= HandleInput;
			pInput.OnKeyDown -= HandleKeyDown;
			pInput.OnKeyUp -= HandleKeyUp;

			pRPIEvents.OnUpdateSettings -= HandleUpdateSettings;

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

			LoadImGuiSettings();

			var io = ImGuiNET.ImGui.GetIO();
			io.ConfigFlags |= ImGuiNET.ImGuiConfigFlags.DockingEnable;
			io.Fonts.AddFontDefault();
			io.Fonts.Build();

			io.DisplaySize.X = io.DisplaySize.Y = 1;

			var wndManager = pProvider.GetOrDefault<IWindowManager>();
			if (wndManager != null)
			{
				var videoScale = wndManager.VideoScale;
				io.FontGlobalScale = (videoScale.X + videoScale.Y) / 2.0f;
			}

			SetupKeyMap();
			AllocateFontBuffer();

			pExecutionPipeline.AddEvent(DefaultEvents.ImGuiDrawId, (_) => HandleDraw());
		}

		private void HandleUpdateSettings(object? sender, RenderUpdateSettingsEventArgs e)
		{
			pUpdateRateVar.Value = e.Settings.ImGuiUpdateRate;
		}

		private void SetupKeyMap()
		{
			var io = ImGuiNET.ImGui.GetIO();
			io.KeyMap[(int)ImGuiNET.ImGuiKey.Tab]				= (int)InputKey.Tab;
			io.KeyMap[(int)ImGuiNET.ImGuiKey.LeftArrow]			= (int)InputKey.Left;
			io.KeyMap[(int)ImGuiNET.ImGuiKey.RightArrow]		= (int)InputKey.Right;
			io.KeyMap[(int)ImGuiNET.ImGuiKey.UpArrow]			= (int)InputKey.Up;
			io.KeyMap[(int)ImGuiNET.ImGuiKey.DownArrow]			= (int)InputKey.Down;
			io.KeyMap[(int)ImGuiNET.ImGuiKey.PageUp]			= (int)InputKey.PageUp;
			io.KeyMap[(int)ImGuiNET.ImGuiKey.PageDown]			= (int)InputKey.PageDown;
			io.KeyMap[(int)ImGuiNET.ImGuiKey.Home]				= (int)InputKey.Home;
			io.KeyMap[(int)ImGuiNET.ImGuiKey.End]				= (int)InputKey.End;
			io.KeyMap[(int)ImGuiNET.ImGuiKey.Insert]			= (int)InputKey.Insert;
			io.KeyMap[(int)ImGuiNET.ImGuiKey.Delete]			= (int)InputKey.Delete;
			io.KeyMap[(int)ImGuiNET.ImGuiKey.Backspace]			= (int)InputKey.Backspace;
			io.KeyMap[(int)ImGuiNET.ImGuiKey.Space]				= (int)InputKey.Space;
			io.KeyMap[(int)ImGuiNET.ImGuiKey.Enter]				= (int)InputKey.Enter;
			io.KeyMap[(int)ImGuiNET.ImGuiKey.Escape]			= (int)InputKey.Esc;
			io.KeyMap[(int)ImGuiNET.ImGuiKey.LeftCtrl]			= (int)InputKey.LeftControl;
			io.KeyMap[(int)ImGuiNET.ImGuiKey.LeftShift]			= (int)InputKey.LeftShift;
			io.KeyMap[(int)ImGuiNET.ImGuiKey.LeftAlt]			= (int)InputKey.LeftAlt;
			io.KeyMap[(int)ImGuiNET.ImGuiKey.LeftSuper]			= (int)InputKey.LeftSuper;
			io.KeyMap[(int)ImGuiNET.ImGuiKey.RightCtrl]			= (int)InputKey.RightControl;
			io.KeyMap[(int)ImGuiNET.ImGuiKey.RightShift]		= (int)InputKey.RightShift;
			io.KeyMap[(int)ImGuiNET.ImGuiKey.RightAlt]			= (int)InputKey.RightAlt;
			io.KeyMap[(int)ImGuiNET.ImGuiKey.RightSuper]		= (int)InputKey.RightSuper;
			io.KeyMap[(int)ImGuiNET.ImGuiKey.Menu]				= (int)InputKey.Menu;
			io.KeyMap[(int)ImGuiNET.ImGuiKey._0]				= (int)InputKey.D0;
			io.KeyMap[(int)ImGuiNET.ImGuiKey._1]				= (int)InputKey.D1;
			io.KeyMap[(int)ImGuiNET.ImGuiKey._2]				= (int)InputKey.D1;
			io.KeyMap[(int)ImGuiNET.ImGuiKey._3]				= (int)InputKey.D3;
			io.KeyMap[(int)ImGuiNET.ImGuiKey._4]				= (int)InputKey.D4;
			io.KeyMap[(int)ImGuiNET.ImGuiKey._5]				= (int)InputKey.D5;
			io.KeyMap[(int)ImGuiNET.ImGuiKey._6]				= (int)InputKey.D6;
			io.KeyMap[(int)ImGuiNET.ImGuiKey._7]				= (int)InputKey.D7;
			io.KeyMap[(int)ImGuiNET.ImGuiKey._8]				= (int)InputKey.D8;
			io.KeyMap[(int)ImGuiNET.ImGuiKey._9]				= (int)InputKey.D9;
			io.KeyMap[(int)ImGuiNET.ImGuiKey.A]					= (int)InputKey.A;
			io.KeyMap[(int)ImGuiNET.ImGuiKey.B]					= (int)InputKey.B;
			io.KeyMap[(int)ImGuiNET.ImGuiKey.C]					= (int)InputKey.C;
			io.KeyMap[(int)ImGuiNET.ImGuiKey.D]					= (int)InputKey.D;
			io.KeyMap[(int)ImGuiNET.ImGuiKey.E]					= (int)InputKey.E;
			io.KeyMap[(int)ImGuiNET.ImGuiKey.F]					= (int)InputKey.F;
			io.KeyMap[(int)ImGuiNET.ImGuiKey.G]					= (int)InputKey.G;
			io.KeyMap[(int)ImGuiNET.ImGuiKey.H]					= (int)InputKey.H;
			io.KeyMap[(int)ImGuiNET.ImGuiKey.I]					= (int)InputKey.I;
			io.KeyMap[(int)ImGuiNET.ImGuiKey.J]					= (int)InputKey.J;
			io.KeyMap[(int)ImGuiNET.ImGuiKey.K]					= (int)InputKey.K;
			io.KeyMap[(int)ImGuiNET.ImGuiKey.L]					= (int)InputKey.L;
			io.KeyMap[(int)ImGuiNET.ImGuiKey.M]					= (int)InputKey.M;
			io.KeyMap[(int)ImGuiNET.ImGuiKey.N]					= (int)InputKey.N;
			io.KeyMap[(int)ImGuiNET.ImGuiKey.O]					= (int)InputKey.O;
			io.KeyMap[(int)ImGuiNET.ImGuiKey.P]					= (int)InputKey.P;
			io.KeyMap[(int)ImGuiNET.ImGuiKey.Q]					= (int)InputKey.Q;
			io.KeyMap[(int)ImGuiNET.ImGuiKey.R]					= (int)InputKey.R;
			io.KeyMap[(int)ImGuiNET.ImGuiKey.S]					= (int)InputKey.S;
			io.KeyMap[(int)ImGuiNET.ImGuiKey.T]					= (int)InputKey.T;
			io.KeyMap[(int)ImGuiNET.ImGuiKey.V]					= (int)InputKey.V;
			io.KeyMap[(int)ImGuiNET.ImGuiKey.U]					= (int)InputKey.U;
			io.KeyMap[(int)ImGuiNET.ImGuiKey.W]					= (int)InputKey.W;
			io.KeyMap[(int)ImGuiNET.ImGuiKey.X]					= (int)InputKey.X;
			io.KeyMap[(int)ImGuiNET.ImGuiKey.Y]					= (int)InputKey.Y;
			io.KeyMap[(int)ImGuiNET.ImGuiKey.Z]					= (int)InputKey.Z;
			io.KeyMap[(int)ImGuiNET.ImGuiKey.F1]				= (int)InputKey.F1;
			io.KeyMap[(int)ImGuiNET.ImGuiKey.F2]				= (int)InputKey.F2;
			io.KeyMap[(int)ImGuiNET.ImGuiKey.F3]				= (int)InputKey.F3;
			io.KeyMap[(int)ImGuiNET.ImGuiKey.F4]				= (int)InputKey.F4;
			io.KeyMap[(int)ImGuiNET.ImGuiKey.F5]				= (int)InputKey.F5;
			io.KeyMap[(int)ImGuiNET.ImGuiKey.F6]				= (int)InputKey.F6;
			io.KeyMap[(int)ImGuiNET.ImGuiKey.F7]				= (int)InputKey.F7;
			io.KeyMap[(int)ImGuiNET.ImGuiKey.F8]				= (int)InputKey.F8;
			io.KeyMap[(int)ImGuiNET.ImGuiKey.F9]				= (int)InputKey.F9;
			io.KeyMap[(int)ImGuiNET.ImGuiKey.F10]				= (int)InputKey.F10;
			io.KeyMap[(int)ImGuiNET.ImGuiKey.F11]				= (int)InputKey.F11;
			io.KeyMap[(int)ImGuiNET.ImGuiKey.F12]				= (int)InputKey.F12;
			io.KeyMap[(int)ImGuiNET.ImGuiKey.Apostrophe]		= (int)InputKey.Quotes;
			io.KeyMap[(int)ImGuiNET.ImGuiKey.Comma]				= (int)InputKey.Comma;
			io.KeyMap[(int)ImGuiNET.ImGuiKey.Minus]				= (int)InputKey.Minus;
			io.KeyMap[(int)ImGuiNET.ImGuiKey.Period]			= (int)InputKey.Period;
			io.KeyMap[(int)ImGuiNET.ImGuiKey.Slash]				= (int)InputKey.Backslash;
			io.KeyMap[(int)ImGuiNET.ImGuiKey.Semicolon]			= (int)InputKey.Semicolon;
			io.KeyMap[(int)ImGuiNET.ImGuiKey.Equal]				= (int)InputKey.Plus;
			io.KeyMap[(int)ImGuiNET.ImGuiKey.LeftBracket]		= (int)InputKey.OpenBrackets;
			io.KeyMap[(int)ImGuiNET.ImGuiKey.RightBracket]		= (int)InputKey.CloseBrackets;
			io.KeyMap[(int)ImGuiNET.ImGuiKey.GraveAccent]		= (int)InputKey.Tilde;
			io.KeyMap[(int)ImGuiNET.ImGuiKey.CapsLock]			= (int)InputKey.Capslock;
			io.KeyMap[(int)ImGuiNET.ImGuiKey.ScrollLock]		= (int)InputKey.Scroll;
			io.KeyMap[(int)ImGuiNET.ImGuiKey.NumLock]			= (int)InputKey.NumLock;
			io.KeyMap[(int)ImGuiNET.ImGuiKey.PrintScreen]		= (int)InputKey.PrintScreen;
			io.KeyMap[(int)ImGuiNET.ImGuiKey.Pause]				= (int)InputKey.Pause;
			io.KeyMap[(int)ImGuiNET.ImGuiKey.Keypad0]			= (int)InputKey.NumPad0;
			io.KeyMap[(int)ImGuiNET.ImGuiKey.Keypad1]			= (int)InputKey.NumPad1;
			io.KeyMap[(int)ImGuiNET.ImGuiKey.Keypad2]			= (int)InputKey.NumPad2;
			io.KeyMap[(int)ImGuiNET.ImGuiKey.Keypad3]			= (int)InputKey.NumPad3;
			io.KeyMap[(int)ImGuiNET.ImGuiKey.Keypad4]			= (int)InputKey.NumPad4;
			io.KeyMap[(int)ImGuiNET.ImGuiKey.Keypad5]			= (int)InputKey.NumPad5;
			io.KeyMap[(int)ImGuiNET.ImGuiKey.Keypad6]			= (int)InputKey.NumPad6;
			io.KeyMap[(int)ImGuiNET.ImGuiKey.Keypad7]			= (int)InputKey.NumPad7;
			io.KeyMap[(int)ImGuiNET.ImGuiKey.Keypad8]			= (int)InputKey.NumPad8;
			io.KeyMap[(int)ImGuiNET.ImGuiKey.Keypad9]			= (int)InputKey.NumPad9;
			io.KeyMap[(int)ImGuiNET.ImGuiKey.KeypadDecimal]		= (int)InputKey.Decimal;
			io.KeyMap[(int)ImGuiNET.ImGuiKey.KeypadDivide]		= (int)InputKey.Divide;
			io.KeyMap[(int)ImGuiNET.ImGuiKey.KeypadMultiply]	= (int)InputKey.Multiply;
			io.KeyMap[(int)ImGuiNET.ImGuiKey.KeypadSubtract]	= (int)InputKey.Subtract;
			io.KeyMap[(int)ImGuiNET.ImGuiKey.KeypadAdd]			= (int)InputKey.Add;
			io.KeyMap[(int)ImGuiNET.ImGuiKey.KeypadEnter]		= (int)InputKey.Enter;
			io.KeyMap[(int)ImGuiNET.ImGuiKey.KeypadEqual]		= (int)InputKey.Plus;
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

		private void LoadImGuiSettings()
		{
			if (!File.Exists(pImGuiSettingsPath))
				return;
			pLogger.Info("Loading ImGui Settings: " + pImGuiSettingsPath);
			using (FileStream stream = new(pImGuiSettingsPath, FileMode.Open, FileAccess.Read))
			{
				char[] buffer = new char[stream.Length];
				int nextIdx = 0;
				while(nextIdx < buffer.Length)
				{
					buffer[nextIdx] = (char)stream.ReadByte();
					++nextIdx;
				}
				pLogger.Debug("\n" + new string(buffer));

				ImGuiNET.ImGui.LoadIniSettingsFromMemory(buffer);
			}
		}

		private void SaveImGuiSettings()
		{
			pLogger.Info("Saving ImGui Settings");
			using(FileStream stream = new(pImGuiSettingsPath, FileMode.OpenOrCreate, FileAccess.Write))
			{
				using(TextWriter writer = new StreamWriter(stream))
				{
					string settings = ImGuiNET.ImGui.SaveIniSettingsToMemory();
					pLogger.Debug("\n"+settings);
					writer.Write(settings);
				}
			}
		}

		private void HandleInput(object? sender, InputTextEventArgs e)
		{
			string text = e.Text;
			char c = text.ElementAt(0);
			
			// ImGui only accepts ascii chars
			if (!char.IsAscii(c))
				return;

			lock (pSync)
			{
				pTextToInsert += text;
			}
		}

		private void HandleKeyUp(object? sender, InputEventArgs e)
		{
			ImGuiNET.ImGui.GetIO().KeysDown[(int)e.Key] = false;
		}

		private void HandleKeyDown(object? sender, InputEventArgs e)
		{
			ImGuiNET.ImGui.GetIO().KeysDown[(int)e.Key] = true;
		}

		private double pLastElapsed = 0;
		private void HandleDraw()
		{
			if (pDisposed)
				return;
			var swapChain = pRenderer.SwapChain;
			// If there's not swap chain, there's no reason to render
			if (swapChain is null)
				return;

			pMutex.WaitOne();

			var io = ImGuiNET.ImGui.GetIO();
			double curr = pStopwatch.Elapsed.TotalMilliseconds * 0.001;
			io.DeltaTime = (float)(curr - pLastElapsed);
			pLastElapsed = curr;

			io.DisplaySize.X = swapChain.Size.Width;
			io.DisplaySize.Y = swapChain.Size.Height;
			io.MousePos = pInput.MousePosition;
			io.MouseWheel = pInput.MouseWheel.Y;
			io.MouseWheelH = pInput.MouseWheel.X;
			for (byte i = 1; i < MaxMouseKeys; ++i)
				io.MouseDown[i - 1] = pInput.GetMouseDown((MouseKey)i);

			io.KeyCtrl = pInput.GetKeyDown(InputKey.Control);
			io.KeyShift = pInput.GetKeyDown(InputKey.Shift);
			io.KeyAlt = pInput.GetKeyDown(InputKey.Alt);

			lock (pSync)
			{
				io.AddInputCharactersUTF8(pTextToInsert);
				pTextToInsert = string.Empty;
			}

			ImGuiNET.ImGui.SetCurrentContext(pImGuiCtx);

			ImGuiNET.ImGui.NewFrame();
#if DEBUG
			ImGuiNET.ImGui.ShowDemoWindow();
#endif
			OnGui?.Invoke(this, EventArgs.Empty);

			ImGuiNET.ImGui.EndFrame();

			ImGuiNET.ImGui.Render();

			pMutex.ReleaseMutex();
		}

		public void BeginRender()
		{
			pMutex.WaitOne();
		}
		public void EndRender()
		{
			pMutex.ReleaseMutex();
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
#endif
}
