using REngine.Core.DependencyInjection;
using REngine.RHI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace REngine.RPI.RenderGraph
{
	public class RenderFeatureNode : ExecutableGraphNode
	{
		private readonly IRenderFeature pFeature;
		private IBufferProvider? pBufferProvider;
		private IRenderer? pRenderer;

		public override bool IsDirty => pFeature.IsDirty;

		public RenderFeatureNode(IRenderFeature feature, string debugName) : base(debugName)
		{
			pFeature = feature;
		}

		protected override void OnRun(IServiceProvider provider)
		{
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

			if(pFeature.IsDirty)
			{
				pFeature.Setup(setupInfo);
				pFeature.Compile(command);
			}
		}

		protected override void OnExecute(ICommandBuffer command)
		{
			pFeature.Execute(command);
		}

		protected override void OnDispose()
		{
			pFeature.Dispose();
		}
	}
}
