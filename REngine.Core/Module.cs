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

namespace REngine.Core
{
	public sealed class CoreModule : IModule
	{
		public void Setup(IServiceRegistry registry)
		{
			registry
				.Add<EngineSettings>()
				.Add<EntityManager>()
				.Add<ILoggerFactory, DebugLoggerFactory>()
				.Add<IInput, InputImpl>()
				.Add<IEngine, EngineImpl>()
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
