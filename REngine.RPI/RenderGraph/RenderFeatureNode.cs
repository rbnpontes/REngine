using REngine.Core.DependencyInjection;
using REngine.RHI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading.Tasks;
using REngine.Core.Resources;

#if RENGINE_RENDERGRAPH
namespace REngine.RPI.RenderGraph
{
	public abstract class RenderFeatureNode : ExecutableGraphNode
	{
		private IBufferManager? pBufferMgr;
		private IRenderer? pRenderer;
		private IRenderFeature? pFeature;
		private IPipelineStateManager? pPipelineMgr;
		private IShaderManager? pShaderMgr;
		private IRenderTargetManager? pRenderTargetMgr;
		private GraphicsSettings? pGraphicsSettings;
		private RenderState? pRenderState;
		private IAssetManager? pAssetManager;
		private IShaderResourceBindingCache? pShaderResourceBindingCache;

		public override bool IsDirty => pFeature?.IsDirty ?? true;

		protected RenderFeatureNode(string debugName) : base(debugName)
		{
		}

		protected override void OnRun(IServiceProvider provider)
		{
			pFeature ??= GetFeature();
			pBufferMgr ??= provider.Get<IBufferManager>();
			pRenderer ??= provider.Get<IRenderer>();
			pPipelineMgr ??= provider.Get<IPipelineStateManager>();
			pShaderMgr ??= provider.Get<IShaderManager>();
			pRenderTargetMgr ??= provider.Get<IRenderTargetManager>();
			pGraphicsSettings ??= provider.Get<GraphicsSettings>();
			pRenderState ??= provider.Get<RenderState>();
			pAssetManager ??= provider.Get<IAssetManager>();
			pShaderResourceBindingCache ??= provider.Get<IShaderResourceBindingCache>();

			base.OnRun(provider);
		}

		protected override void OnCompile(IDevice device, ICommandBuffer command)
		{
			if (pBufferMgr is null || pRenderer is null || pPipelineMgr is null || pShaderMgr is null)
				return;

			// TODO: refactor this to a better approach because
			// when occurs next refactor this will break
			RenderFeatureSetupInfo setupInfo = new(
				Driver,
				pRenderer,
				pBufferMgr,
				pPipelineMgr,
				pShaderMgr,
				pRenderTargetMgr,
				pGraphicsSettings,
				pRenderState,
				pAssetManager,
				pShaderResourceBindingCache
			);

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