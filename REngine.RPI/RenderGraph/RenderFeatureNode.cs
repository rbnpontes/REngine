using REngine.Core.DependencyInjection;
using REngine.RHI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#if RENGINE_RENDERGRAPH
namespace REngine.RPI.RenderGraph
{
	public abstract class RenderFeatureNode : ExecutableGraphNode
	{
		private IBufferProvider? pBufferProvider;
		private IRenderer? pRenderer;
		private IRenderFeature? pFeature;

		public override bool IsDirty => pFeature?.IsDirty ?? true;

		public RenderFeatureNode(string debugName) : base(debugName)
		{
		}

		protected override void OnRun(IServiceProvider provider)
		{
			if(pFeature is null)
				pFeature = GetFeature();
			if (pBufferProvider is null)
				pBufferProvider = provider.Get<IBufferProvider>();
			if(pRenderer is null)
				pRenderer = provider.Get<IRenderer>();

			base.OnRun(provider);
		}

		protected override void OnCompile(IDevice device, ICommandBuffer command)
		{
			if (pBufferProvider is null || pRenderer is null)
				return;

			RenderFeatureSetupInfo setupInfo = new()
			{
				Driver = Driver,
				BufferProvider = pBufferProvider,
				Renderer = pRenderer
			};

			var feature = pFeature;
			if (feature is null)
				return;
			if(feature.IsDirty)
			{
				feature.Setup(setupInfo);
				feature.Compile(command);
			}
		}

		protected override void OnExecute(ICommandBuffer command)
		{
			pFeature?.Execute(command);
		}

		protected override void OnDispose()
		{
			pFeature?.Dispose();
			pFeature = null;
		}

		protected abstract IRenderFeature GetFeature();
	}
}
#endif