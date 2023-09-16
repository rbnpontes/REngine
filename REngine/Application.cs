using REngine.Core;
using REngine.Core.DependencyInjection;
using REngine.Core.IO;
using REngine.RHI;
using REngine.RHI.DiligentDriver;
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
					(deps) => ((IWindowManager)deps[0]).Create(new WindowCreationInfo
					{
						Title = "REngine Sandbox",
						Size = new Size(500, 500)
					}),
					new Type[] { typeof(IWindowManager) }
				)
				.Add((IServiceProvider provider) =>
				{
					var graphicsSettings = provider.Get<GraphicsSettings>();
					var window = provider.Get<IWindow>();
					GraphicsFactory factory = new GraphicsFactory(provider);
					factory.OnMessage += HandleGraphicsMessage;

					return factory.Create(new GraphicsFactoryCreateInfo
					{
						WindowHandle = window.Handle,
					}, new SwapChainDesc(graphicsSettings)
					{
						Size = new SwapChainSize(window.Size)
					}, out swapChain);
				})
				.Add((IServiceProvider provider) =>
				{
					if (swapChain is null)
						throw new NullReferenceException("SwapChain must be created");
					return swapChain;
				});
		}

		private void HandleGraphicsMessage(object sender, MessageEventArgs args)
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
			provider.Get<IWindow>().Show();
		}

		public virtual void OnUpdate(IServiceProvider provider)
		{
		}
	}
}
