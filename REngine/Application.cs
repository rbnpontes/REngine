using REngine.Core;
using REngine.Core.DependencyInjection;
using REngine.Core.IO;
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

namespace REngine.Sandbox
{
	public abstract class App : IEngineApplication
	{
		private LinkedList<MessageEventArgs> pTmpGraphicsLogs = new LinkedList<MessageEventArgs>();

		public ILogger Logger { get; private set; }
		
		public App(Type inheritanceType)
		{
			Logger = new DebugLoggerFactory().Build(inheritanceType);
		}
		public virtual void OnExit(IServiceProvider provider)
		{
		}

		public virtual void OnSetup(IServiceRegistry registry)
		{
			WindowsModule.Setup(registry);
			RHIModule.Setup(registry);
			RPIModule.Setup(registry);
			ISwapChain? swapChain = null;

			registry
				.Add(
					(deps) => OnSetupWindow((IWindowManager)deps[0]),
					new Type[] { typeof(IWindowManager) }
				)
				.Add((IServiceProvider provider) =>
				{
					var graphicsSettings = provider.Get<GraphicsSettings>();
					var window = provider.Get<IWindow>();

#if WINDOWS
					NativeWindow nativeWindow = new() { Hwnd = window.Handle };
#endif
					DriverFactory.OnDriverMessage += HandleGraphicsMessage;
					var driver = DriverFactory.Build(
						new DriverSettings { },
						nativeWindow,
						new SwapChainDesc(graphicsSettings)
						{
							Size = new SwapChainSize(window.Size)
						}, out swapChain);

					return driver;
				})
				.Add((IServiceProvider provider) =>
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
