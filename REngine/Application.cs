using REngine.Core;
using REngine.Core.DependencyInjection;
using REngine.Core.IO;
using REngine.Core.Serialization;
using REngine.RHI;
using REngine.RHI.DiligentDriver;
using REngine.RHI.NativeDriver;
using REngine.RPI;
using REngine.Windows;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using REngine.Assets;
using REngine.Core.Runtimes;

namespace REngine.Sandbox
{
	public abstract class App : IEngineApplication
	{
		private readonly ILoggerFactory pLoggerFactory;

		public ILogger Logger { get; private set; }


		protected App(Type inheritanceType)
		{
			FileLoggerFactory fileLoggerFactory = new FileLoggerFactory(EngineSettings.LoggerPath);
#if DEBUG
			pLoggerFactory = new ComposedLoggerFactory(new ILoggerFactory[]
			{
				new DebugLoggerFactory(),
				fileLoggerFactory
			});
#else
			pLoggerFactory = fileLoggerFactory;
#endif
			Logger = pLoggerFactory.Build(inheritanceType);

			NativeReferences.Logger = pLoggerFactory.Build(typeof(NativeReferences));
			NativeReferences.PreloadLibs();
		}
		public virtual void OnExit(IServiceProvider provider)
		{
#if PROFILER
			Profiler.Instance.Dispose();
#endif
			NativeReferences.UnloadLibs();

			EngineSettings?		engineSettings		= provider.GetOrDefault<EngineSettings>();
			RenderSettings?		renderSettings		= provider.GetOrDefault<RenderSettings>();
			DriverSettings?		driverSettings		= provider.GetOrDefault<DriverSettings>();
			GraphicsSettings?	graphicsSettings	= provider.GetOrDefault<GraphicsSettings>();

			Logger.Info("Writing Settings Before Exit.");
			if(engineSettings != null)
				WriteSettings(EngineSettings.EngineSettingsPath, engineSettings);
			if(renderSettings != null)
				WriteSettings(EngineSettings.RenderSettingsPath, renderSettings);
			if(driverSettings != null)
				WriteSettings(EngineSettings.DriverSettingsPath, driverSettings);
			if(graphicsSettings != null)
				WriteSettings(EngineSettings.GraphicsSettingsPath, graphicsSettings);
		}

		private void WriteSettings<T>(string path, T data)
		{
			if(File.Exists(path))
				File.Delete(path);
			using (FileStream stream = new FileStream(path, FileMode.Create, FileAccess.Write))
			using (TextWriter writer = new StreamWriter(stream))
				writer.Write(data.ToJson());
		}

		public virtual void OnSetupModules(List<IModule> modules)
		{
			modules.AddRange(new IModule[]
			{
				new AssetsModule(),
				new WindowsModule(),
				new RHIModule(),
				new RPIModule(),
			});
		}
		public virtual void OnSetup(IServiceRegistry registry)
		{
			// Create Application Data Path
			if (!Directory.Exists(EngineSettings.AppDataPath))
				Directory.CreateDirectory(EngineSettings.AppDataPath);

			var processorCount = Environment.ProcessorCount;
			registry.Add(() =>
			{
				using FileStream stream = new FileStream(EngineSettings.EngineSettingsPath, FileMode.OpenOrCreate, FileAccess.Read);
				var engineSettings = EngineSettings.FromStream(stream);
				if (engineSettings.JobsThreadCount == 0)
					engineSettings.JobsThreadCount = processorCount;
				engineSettings.JobsThreadCount = Math.Clamp(engineSettings.JobsThreadCount, 1, processorCount);
				return engineSettings;
			});
			registry.Add(() =>
			{
				using FileStream stream = new FileStream(EngineSettings.GraphicsSettingsPath, FileMode.OpenOrCreate, FileAccess.Read);
				return GraphicsSettings.FromStream(stream);
			});
			registry.Add(() =>
			{
				using FileStream stream = new FileStream(EngineSettings.RenderSettingsPath, FileMode.OpenOrCreate, FileAccess.Read);
				return RenderSettings.FromStream(stream);
			});

			ISwapChain? swapChain = null;

			registry.Add(() => pLoggerFactory);

			DriverFactory.OnDriverMessage += HandleGraphicsMessage;

			Logger.Info("OS Version: " + Environment.OSVersion);
			Logger.Info("Machine: " + Environment.MachineName);
			Logger.Info("TickCount:" + Environment.TickCount);
			Logger.Info("ProcessorCount: " + processorCount);
			Logger.Info("UserName: " + Environment.UserName);
			Logger.Info("UserDomainName: " + Environment.UserDomainName);
			Logger.Info("App Data Path: " + EngineSettings.AppDataPath);
			Logger.Info("Log Path: " + EngineSettings.LoggerPath);

			registry
				.Add(
					(deps) => OnSetupWindow((IWindowManager)deps[0]),
					new Type[] { typeof(IWindowManager) }
				)
				.Add((provider) =>
				{
					var graphicsSettings = provider.Get<GraphicsSettings>();
					var window = provider.Get<IWindow>();
					window.GetNativeWindow(out NativeWindow nativeWindow);

					Logger.Info(window);

					DriverSettings driverSettings = OnCreateDriverSettings(provider);
					var driver = DriverFactory.Build(
						driverSettings,
						nativeWindow,
						new SwapChainDesc(graphicsSettings)
						{
							Size = new SwapChainSize(window.Size),
							Usage = SwapChainUsage.RenderTarget
						}, out swapChain);

					Logger.Info("GraphicsBackend: " + driverSettings.Backend);
					// When format is not supported by the driver
					// Driver will search for a compatible format
					// In this case we must update graphics settings
					if (swapChain != null)
					{
						graphicsSettings.DefaultColorFormat = swapChain.Desc.Formats.Color;
						graphicsSettings.DefaultDepthFormat = swapChain.Desc.Formats.Depth;
					}
					return driver;
				})
				.Add((_) =>
				{
					if (swapChain is null)
						throw new NullReferenceException("SwapChain must be created");
					return swapChain;
				});
		}

		protected virtual IWindow OnSetupWindow(IWindowManager windowManager)
		{
			return windowManager.Create(new WindowCreationInfo
			{
				Title = "REngine",
				Size = new Size(500, 500)
			});
		}

		protected virtual DriverSettings OnCreateDriverSettings(IServiceProvider serviceProvider) {
			DriverSettings? settings;
			using(FileStream stream = new FileStream(EngineSettings.DriverSettingsPath, FileMode.OpenOrCreate, FileAccess.Read))
			using(TextReader reader = new StreamReader(stream))
				settings = reader.ReadToEnd().FromJson<DriverSettings>();
			return settings ?? new DriverSettings();
		}

		private void HandleGraphicsMessage(object? sender, MessageEventArgs args)
		{
			switch (args.Severity)
			{
				case DbgMsgSeverity.Warning:
				case DbgMsgSeverity.Error:
				case DbgMsgSeverity.FatalError:
					Logger.Critical($"Diligent Engine: {args.Severity} in {args.Function}() ({args.File}, {args.Line}): {args.Message}");
					break;
				case DbgMsgSeverity.Info:
					Logger.Info($"Diligent Engine: {args.Severity} {args.Message}");
					break;
			}
		}

		public virtual void OnStart(IServiceProvider provider)
		{
			var window = provider.GetOrDefault<IWindow>();
			var swapChain = provider.GetOrDefault<ISwapChain>();

			if (window != null)
				window.Show();
			// If main window goes to resize, we must update swapchain too
			// https://github.com/rbnpontes/REngine/issues/9
			if (window != null && swapChain != null)
				window.OnResize += (s, e) => swapChain.Resize(window.Size);
		}

		public virtual void OnUpdate(IServiceProvider provider)
		{
		}
	}
}
