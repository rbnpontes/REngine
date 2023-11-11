using REngine.Core.DependencyInjection;
using REngine.RHI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading.Tasks;

#if RENGINE_RENDERGRAPH
namespace REngine.RPI.RenderGraph
{
	public abstract class RenderFeatureNode : ExecutableGraphNode
	{
		private IBufferManager? pBufferProvider;
		private IRenderer? pRenderer;
		private IRenderFeature? pFeature;
		private IPipelineStateManager? pPipelineMgr;
		private IShaderManager? pShaderMgr;

		public override bool IsDirty => pFeature?.IsDirty ?? true;

		public RenderFeatureNode(string debugName) : base(debugName)
		{
		}

		protected override void OnRun(IServiceProvider provider)
		{
			if(pFeature is null)
				pFeature = GetFeature();
			if (pBufferProvider is null)
				pBufferProvider = provider.Get<IBufferManager>();
			if(pRenderer is null)
				pRenderer = provider.Get<IRenderer>();
			if(pPipelineMgr is null)
				pPipelineMgr = provider.Get<IPipelineStateManager>();
			if (pShaderMgr is null)
				pShaderMgr = provider.Get<IShaderManager>();

			base.OnRun(provider);
		}

		protected override void OnCompile(IDevice device, ICommandBuffer command)
		{
			if (pBufferProvider is null || pRenderer is null || pPipelineMgr is null || pShaderMgr is null)
				return;

			RenderFeatureSetupInfo setupInfo = new()
			{
				Driver = Driver,
				BufferManager = pBufferProvider,
				Renderer = pRenderer,
				PipelineStateManager = pPipelineMgr,
				ShaderManager = pShaderMgr
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

		public void AddReadResource(int resourceSlotId, IResource resource)
		{
#if DEBUG
			ValidateExpectedResource(GetExpectedReadResourceSlots(), resourceSlotId);
#endif
			OnAddReadResource(resourceSlotId, resource);
		}
		public void AddWriteResource(int resourceSlotId, IResource resource)
		{
#if DEBUG
			ValidateExpectedResource(GetExpectedWriteResourceSlots(), resourceSlotId);
#endif
			OnAddWriteResource(resourceSlotId, resource);
		}

		protected virtual IEnumerable<int> GetExpectedWriteResourceSlots()
		{
			return Array.Empty<int>();
		}
		protected virtual IEnumerable<int> GetExpectedReadResourceSlots()
		{
			return Array.Empty<int>();
		}

#if DEBUG
		private void ValidateExpectedResource(IEnumerable<int> expectedResources, int resourceSlot)
		{
			if (expectedResources.Contains(resourceSlot))
				return;
			throw new RenderGraphException("Invalid Resource Slot.");
		}
#endif

		protected virtual void OnAddReadResource(int resourceSlotId, IResource resource) { }
		protected virtual void OnAddWriteResource(int resourceSlotId, IResource resource) { }
	}
}
#endif