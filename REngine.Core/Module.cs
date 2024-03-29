﻿using REngine.Core.DependencyInjection;
using REngine.Core.IO;
using REngine.Core.SceneManagement;
using REngine.Core.Serialization;
using REngine.Core.Threading;
using REngine.Core.WorldManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using REngine.Core.Logic;
using REngine.Core.Resources;
using Tracy;

namespace REngine.Core
{
	public sealed class CoreModule : IModule
	{
		static CoreModule()
		{
			try
			{
				NativeLibrary.SetDllImportResolver(typeof(PInvoke.TracyCZoneCtx).Assembly,
					Runtimes.NativeReferences.DefaultDllImportResolver);
			}
			catch
			{
				// ignored
			}
		}
		public void Setup(IServiceRegistry registry)
		{
			registry
				.Add<EngineSettings>()
				.Add<AssetManagerSettings>()
				.Add<EntityManager>()
				.Add<ILoggerFactory, DebugLoggerFactory>()
				.Add<IAssetManager, FileAssetManager>()
				.Add<IInput, InputImpl>()
				.Add<IEngine, Engine>()
				.Add<EngineEvents>()
				.Add<ComponentSerializerFactory>()
				.Add<TransformSystem>()
				.Add<Transform2DSystem>()
				.Add<CameraSystem>()
				.Add<GameStateManager>();
			
			ThreadingModule.Setup(registry);
		}
	}
}
