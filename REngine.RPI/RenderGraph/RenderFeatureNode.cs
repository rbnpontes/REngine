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

		protected RenderFeatureNode(string debugName) : base(debugName)
		{
		}

		protected override void OnRun(IServiceProvider provider)
		{
			pFeature ??= GetFeature();
			pBufferProvider ??= provider.Get<IBufferManager>();
			pRenderer ??= provider.Get<IRenderer>();
			pPipelineMgr ??= provider.Get<IPipelineStateManager>();
			pShaderMgr ??= provider.Get<IShaderManager>();

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
			if (!feature.IsDirty)
				return;

			feature.Setup(setupInfo);
			feature.Compile(command);
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

		public void AddReadResource(ulong resourceSlotId, IResource resource)
		{
#if DEBUG
			ValidateExpectedResource(GetExpectedReadResourceSlots(), resourceSlotId);
#endif
			OnAddReadResource(resourceSlotId, resource);
		}
		public void AddWriteResource(ulong resourceSlotId, IResource resource)
		{
#if DEBUG
			ValidateExpectedResource(GetExpectedWriteResourceSlots(), resourceSlotId);
#endif
			OnAddWriteResource(resourceSlotId, resource);
		}

		protected virtual IEnumerable<ulong> GetExpectedWriteResourceSlots()
		{
			return Array.Empty<ulong>();
		}
		protected virtual IEnumerable<ulong> GetExpectedReadResourceSlots()
		{
			return Array.Empty<ulong>();
		}

#if DEBUG
		private static void ValidateExpectedResource(IEnumerable<ulong> expectedResources, ulong resourceSlot)
		{
			if (expectedResources.Contains(resourceSlot))
				return;
			throw new RenderGraphException("Invalid Resource Slot.");
		}
#endif

		protected virtual void OnAddReadResource(ulong resourceSlotId, IResource resource) { }
		protected virtual void OnAddWriteResource(ulong resourceSlotId, IResource resource) { }
	}
}
#endif