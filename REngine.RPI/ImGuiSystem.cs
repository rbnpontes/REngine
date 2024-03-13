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
using ImGuiNET;
using REngine.Core.Reflection;
using REngine.Core.Resources;
using static System.Runtime.CompilerServices.RuntimeHelpers;

namespace REngine.RPI
{
#if RENGINE_IMGUI
	internal class ImGuiSystem : IImGuiSystem
	{
		const byte MaxMouseKeys = (byte)MouseKey.XButton2;
		
		#region ImGui Key Table
		private static readonly ImGuiKey[] pKeys = new[]
		{
			ImGuiKey.None,
			ImGuiKey.Backspace,
			ImGuiKey.Tab,
			ImGuiKey.Enter,
			ImGuiKey.Enter,
			ImGuiKey.ModShift,
			ImGuiKey.ModCtrl,
			ImGuiKey.ModAlt,
			ImGuiKey.Pause,
			ImGuiKey.CapsLock,
			ImGuiKey.Escape,
			ImGuiKey.Space,
			ImGuiKey.PageUp,
			ImGuiKey.PageDown,
			ImGuiKey.End,
			ImGuiKey.Home,
			ImGuiKey.LeftArrow,
			ImGuiKey.UpArrow,
			ImGuiKey.RightArrow,
			ImGuiKey.DownArrow,
			ImGuiKey.None,
			ImGuiKey.None,
			ImGuiKey.None,
			ImGuiKey.PrintScreen,
			ImGuiKey.Insert,
			ImGuiKey.Delete,
			ImGuiKey.None,
			ImGuiKey._0,
			ImGuiKey._1,
			ImGuiKey._2, 
			ImGuiKey._3,
			ImGuiKey._4,
			ImGuiKey._5,
			ImGuiKey._6,
			ImGuiKey._7,
			ImGuiKey._8,
			ImGuiKey._9,
			ImGuiKey.A,
			ImGuiKey.B,
			ImGuiKey.C,
			ImGuiKey.D,
			ImGuiKey.E,
			ImGuiKey.F,
			ImGuiKey.G,
			ImGuiKey.H,
			ImGuiKey.I,
			ImGuiKey.J,
			ImGuiKey.K,
			ImGuiKey.L,
			ImGuiKey.M,
			ImGuiKey.N,
			ImGuiKey.O,
			ImGuiKey.P,
			ImGuiKey.Q,
			ImGuiKey.R,
			ImGuiKey.S,
			ImGuiKey.T,
			ImGuiKey.U,
			ImGuiKey.V,
			ImGuiKey.W,
			ImGuiKey.X,
			ImGuiKey.Y,
			ImGuiKey.Z,
			ImGuiKey.LeftSuper,
			ImGuiKey.RightSuper,
			ImGuiKey.None,
			ImGuiKey.None,
			ImGuiKey.Keypad0,
			ImGuiKey.Keypad1,
			ImGuiKey.Keypad2,
			ImGuiKey.Keypad3,
			ImGuiKey.Keypad4,
			ImGuiKey.Keypad5,
			ImGuiKey.Keypad6,
			ImGuiKey.Keypad7,
			ImGuiKey.Keypad8,
			ImGuiKey.Keypad9,
			ImGuiKey.KeypadMultiply,
			ImGuiKey.KeypadAdd,
			ImGuiKey.None,
			ImGuiKey.KeypadSubtract,
			ImGuiKey.KeypadDecimal,
			ImGuiKey.KeypadDivide,
			ImGuiKey.F1,
			ImGuiKey.F2,
			ImGuiKey.F3,
			ImGuiKey.F4,
			ImGuiKey.F5,
			ImGuiKey.F6,
			ImGuiKey.F7,
			ImGuiKey.F8,
			ImGuiKey.F9,
			ImGuiKey.F10,
			ImGuiKey.F11,
			ImGuiKey.F12,
			ImGuiKey.F13,
			ImGuiKey.F14,
			ImGuiKey.F15,
			ImGuiKey.F16,
			ImGuiKey.F17,
			ImGuiKey.F18,
			ImGuiKey.F19,
			ImGuiKey.F20,
			ImGuiKey.F21,
			ImGuiKey.F22,
			ImGuiKey.F23,
			ImGuiKey.F24,
			ImGuiKey.None,
			ImGuiKey.NumLock,
			ImGuiKey.ScrollLock,
			ImGuiKey.LeftShift,
			ImGuiKey.RightShift,
			ImGuiKey.LeftCtrl,
			ImGuiKey.RightCtrl,
			ImGuiKey.LeftAlt,
			ImGuiKey.RightAlt,
			ImGuiKey.None,
			ImGuiKey.None,
			ImGuiKey.None,
			ImGuiKey.None,
			ImGuiKey.None,
			ImGuiKey.None,
			ImGuiKey.None,
			ImGuiKey.None,
			ImGuiKey.None,
			ImGuiKey.None,
			ImGuiKey.None,
			ImGuiKey.None,
			ImGuiKey.None,
			ImGuiKey.None,
			ImGuiKey.None,
			ImGuiKey.None,
			ImGuiKey.None,
			ImGuiKey.None,
			ImGuiKey.Semicolon,
			ImGuiKey.Equal,
			ImGuiKey.Comma,
			ImGuiKey.Minus,
			ImGuiKey.Period,
			ImGuiKey.Slash,
			ImGuiKey.GraveAccent,
			ImGuiKey.LeftBracket,
			ImGuiKey.Backslash,
			ImGuiKey.RightBracket,
			ImGuiKey.None,
			ImGuiKey.Backslash,
			ImGuiKey.None,
			ImGuiKey.None,
			ImGuiKey.None,
			ImGuiKey.None,
			ImGuiKey.None,
			ImGuiKey.None,
			ImGuiKey.None,
			ImGuiKey.None,
			ImGuiKey.None,
			ImGuiKey.None,
			ImGuiKey.None
		};
		#endregion
		
		private readonly IServiceProvider pProvider;
		private readonly IExecutionPipeline pExecutionPipeline;
		private readonly EngineEvents pEngineEvents;
		private readonly GraphicsSettings pGraphicsSettings;
		private readonly IRenderer pRenderer;
		private readonly IInput pInput;
		private readonly ILogger<IImGuiSystem> pLogger;
		private readonly IExecutionPipelineVar pUpdateRateVar;
		private readonly RPIEvents pRpiEvents;

		private readonly object pSync = new();
		private readonly Mutex pMutex = new();

		private readonly Stopwatch pStopwatch = Stopwatch.StartNew();

		private ImGuiFeature? pFeature;

		private string pTextToInsert = string.Empty;

		private bool pDisposed = false;
		private IntPtr pImGuiCtx = IntPtr.Zero;

		public UIntSize FontSize { get; private set; } = new UIntSize();
		public byte[] FontData { get; private set; } = Array.Empty<byte>();

		public IGraphicsRenderFeature Feature => GetFeature();

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
			IServiceProvider provider,
			IAssetManager assetManager,
			IEngine engine
		)
		{ 
			pEngineEvents = engineEvents;
			pExecutionPipeline = executionPipeline;
			pGraphicsSettings = graphicsSettings;
			pRenderer = renderer;
			pInput = input;
			pLogger = factory.Build<IImGuiSystem>();
			pRpiEvents = rpiEvents;
			pProvider = provider;

			pUpdateRateVar = executionPipeline.GetOrCreateVar(DefaultVars.ImGuiUpdateRate);
			pUpdateRateVar.Value = renderSettings.ImGuiUpdateRate;

			engineEvents.OnStart.Once(HandleEngineStart);
			engineEvents.OnStop.Once(HandleEngineStop);
			input.OnInput += HandleInput;
			input.OnKeyDown += HandleKeyDown;
			input.OnKeyUp += HandleKeyUp;

			rpiEvents.OnUpdateSettings += HandleUpdateSettings;
		}

		private async Task Dispose()
		{
			await EngineGlobals.MainDispatcher.Yield();
			if (pDisposed)
				return;

			var task = SaveImGuiSettings();

			pInput.OnInput -= HandleInput;
			pInput.OnKeyDown -= HandleKeyDown;
			pInput.OnKeyUp -= HandleKeyUp;

			pRpiEvents.OnUpdateSettings -= HandleUpdateSettings;

			pExecutionPipeline.AddEvent(DefaultEvents.ImGuiDrawId, (_) => HandleDraw());

			await task;
			if(pImGuiCtx != IntPtr.Zero)
				ImGuiNET.ImGui.DestroyContext(pImGuiCtx);
			pImGuiCtx = IntPtr.Zero;

			pDisposed = true;
			GC.SuppressFinalize(this);
		}

		private async Task HandleEngineStop(object sender)
		{
			await Dispose();
		}

		private async Task HandleEngineStart(object sender)
		{
			await EngineGlobals.MainDispatcher.Yield();
			InitImGui();
			pExecutionPipeline.AddEvent(DefaultEvents.ImGuiDrawId, (_) => HandleDraw());
		}

		private unsafe void InitImGui()
		{
			pImGuiCtx = ImGuiNET.ImGui.CreateContext();	
			ImGuiNET.ImGui.SetCurrentContext(pImGuiCtx);

			LoadImGuiSettings();

			var io = ImGuiNET.ImGui.GetIO();
			io.ConfigFlags |= ImGuiNET.ImGuiConfigFlags.DockingEnable;
			var fontConfig = new ImFontConfig
			{
				FontDataOwnedByAtlas = 1,
				OversampleH = 2,
				OversampleV = 1,
				GlyphMaxAdvanceX = float.MaxValue,
				RasterizerMultiply = 1.0f,
				RasterizerDensity = 1.0f,
				EllipsisChar = ushort.MaxValue,
				SizePixels = 13
			};
#if ANDROID
			fontConfig.SizePixels = 24;
#endif
			io.Fonts.AddFontDefault(new ImFontConfigPtr(&fontConfig));
			io.Fonts.Build();

			io.DisplaySize.X = io.DisplaySize.Y = 1;

			var wndManager = pProvider.GetOrDefault<IWindowManager>();
			if (wndManager != null)
			{
				var videoScale = wndManager.VideoScale;
				io.FontGlobalScale = (videoScale.X + videoScale.Y) / 2.0f;
			}
            
			AllocateFontBuffer();
		}

		private void HandleUpdateSettings(object? sender, EventArgs e)
		{
			if(sender is RenderSettings settings)
				pUpdateRateVar.Value = settings.ImGuiUpdateRate;
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
			if (!File.Exists(EngineSettings.ImGuiSettingsPath))
				return;
			pLogger.Debug("Loading ImGui Settings");
			using FileStream stream = new(EngineSettings.ImGuiSettingsPath, FileMode.Open, FileAccess.Read);
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

		private async Task SaveImGuiSettings()
		{
			pLogger.Info("Saving ImGui Settings");
			await using FileStream stream = new(EngineSettings.ImGuiSettingsPath, FileMode.OpenOrCreate, FileAccess.Write);
			await using TextWriter writer = new StreamWriter(stream);
			var settings = ImGuiNET.ImGui.SaveIniSettingsToMemory();
			await writer.WriteAsync(settings);
		}

		private void HandleInput(object? sender, InputTextEventArgs e)
		{
			var text = e.Text;
			if (string.IsNullOrEmpty(text))
				return;
			var c = text.ElementAt(0);
			
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
			ImGui.GetIO().AddKeyEvent(pKeys[(int)e.Key], false);
		}

		private void HandleKeyDown(object? sender, InputEventArgs e)
		{
			ImGui.GetIO().AddKeyEvent(pKeys[(int)e.Key], true);
		}

		public void SetFontScale(float scale)
		{
			var io = ImGui.GetIO();
			io.FontGlobalScale = scale;
		}

		public void ScaleUi(float scale)
		{
			ImGui.GetStyle().ScaleAllSizes(scale);
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

			io.DisplaySize = new Vector2(swapChain.Size.Width, swapChain.Size.Height);
			
			FillMouseEvents(ref io);

			io.KeyCtrl = pInput.GetKeyDown(InputKey.Control);
			io.KeyShift = pInput.GetKeyDown(InputKey.Shift);
			io.KeyAlt = pInput.GetKeyDown(InputKey.Alt);

			lock (pSync)
			{
				if(!string.IsNullOrEmpty(pTextToInsert))
					io.AddInputCharactersUTF8(pTextToInsert);
				pTextToInsert = string.Empty;
			}

			ImGuiNET.ImGui.SetCurrentContext(pImGuiCtx);

			ImGuiNET.ImGui.NewFrame();
#if ANDROID
			CheckKeyboard(ref io);
#endif
#if DEBUG
			ImGuiNET.ImGui.ShowDemoWindow();
#endif
			OnGui?.Invoke(this, EventArgs.Empty);

			ImGuiNET.ImGui.EndFrame();

			ImGuiNET.ImGui.Render();

			pMutex.ReleaseMutex();
		}

		private void FillMouseEvents(ref ImGuiIOPtr io)
		{
			io.AddMousePosEvent(pInput.MousePosition.X, pInput.MousePosition.Y);
			io.AddMouseWheelEvent(pInput.MouseWheel.X, pInput.MouseWheel.Y);

			var leftDown = pInput.GetMouseDown(MouseKey.Left);
			var rightDown = pInput.GetMouseDown(MouseKey.Right);
			var middleDown = pInput.GetMouseDown(MouseKey.Middle);
			var button1Down = pInput.GetMouseDown(MouseKey.XButton1);
			var button2Down = pInput.GetMouseDown(MouseKey.XButton2);
#if ANDROID
			var hasAnyKeyDown = leftDown || rightDown || middleDown || button1Down || button2Down;
			if(hasAnyKeyDown)
				io.AddMouseSourceEvent(ImGuiMouseSource.TouchScreen);
#endif
			
			io.AddMouseButtonEvent(0, leftDown);
			io.AddMouseButtonEvent(1, rightDown);
			io.AddMouseButtonEvent(2, middleDown);
			io.AddMouseButtonEvent(3, button1Down);
			io.AddMouseButtonEvent(4, button2Down);
			
		}
#if ANDROID
		private void CheckKeyboard(ref ImGuiIOPtr io)
		{
			var wantTextInput = io is { WantCaptureKeyboard: true, WantTextInput: true };
			if (!wantTextInput && pEngine.IsKeyboardVisible)
				pEngine.HideKeyboard();
			else if(wantTextInput && !pEngine.IsKeyboardVisible)
				pEngine.ShowKeyboard();
		}
#endif
		
		public void BeginRender()
		{
			pMutex.WaitOne();
		}
		public void EndRender()
		{
			pMutex.ReleaseMutex();
		}

		public IGraphicsRenderFeature CreateRenderFeature() => AllocateFeature();
		private ImGuiFeature GetFeature()
		{
			if (pFeature is null || pFeature.IsDisposed)
				pFeature = AllocateFeature();
			return pFeature;
		}

		private ImGuiFeature AllocateFeature()
		{
			return ActivatorExtended.CreateInstance<ImGuiFeature>(pProvider) ?? throw new NullReferenceException("Error has occurred while is creating render feature");
		}
	}
#endif
}
