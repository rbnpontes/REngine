using REngine.Core.DependencyInjection;
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
#if !WEB && PROFILER
using Tracy;
#endif
namespace REngine.Core
{
	public sealed class CoreModule : IModule
	{
		static CoreModule()
		{
#if !WEB && PROFILER
			try
			{
				NativeLibrary.SetDllImportResolver(typeof(PInvoke.TracyCZoneCtx).Assembly,
					Runtimes.NativeReferences.DefaultDllImportResolver);
			}
			catch
			{
				// ignored
			}
#endif
		}
		public void Setup(IServiceRegistry registry)
		{
			registry
				.Add<EngineSettings>()
				.Add<AssetManagerSettings>()
				.Add<EntityManager>()
				.Add<ILoggerFactory, DebugLoggerFactory>()
#if !WEB && !ANDROID
				.Add<IAssetManager, FileAssetManager>()
#endif
				.Add<IInput, InputImpl>()
				.Add<IEngine, Engine>()
				.Add<EngineEvents>()
				.Add<IExecutionPipeline, ExecutionPipelineImpl>()
				.Add<ExecutionPipelineNodeRegistry>()
				.Add<ComponentSerializerFactory>()
				.Add<TransformSystem>()
				.Add<Transform2DSystem>()
				.Add<CameraSystem>()
				.Add<GameStateManager>();
		}
	}
}
