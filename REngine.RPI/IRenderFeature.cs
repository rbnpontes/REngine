using REngine.RHI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using REngine.Core.Resources;

namespace REngine.RPI
{
	public struct RenderFeatureSetupInfo(
		IGraphicsDriver driver,
		IRenderer renderer,
		IBufferManager bufferMgr,
		IPipelineStateManager pipelineStateMgr,
		IShaderManager shaderMgr,
		IRenderTargetManager renderTargetMgr,
		GraphicsSettings graphicsSettings,
		RenderState renderState,
		IAssetManager assetManager,
		IShaderResourceBindingCache shaderResourceBindingCache)
	{
		public readonly IGraphicsDriver Driver = driver;
		public readonly IRenderer Renderer = renderer;
		public readonly IBufferManager BufferManager = bufferMgr;
		public readonly IPipelineStateManager PipelineStateManager = pipelineStateMgr;
		public readonly IShaderManager ShaderManager = shaderMgr;
		public readonly IRenderTargetManager RenderTargetManager = renderTargetMgr;
		public readonly GraphicsSettings GraphicsSettings = graphicsSettings;
		public readonly RenderState RenderState = renderState;
		public readonly IAssetManager AssetManager = assetManager;
		public readonly IShaderResourceBindingCache ShaderResourceBindingCache = shaderResourceBindingCache;
	}

	public interface IRenderFeature : IDisposable
	{
		public bool IsDirty { get; }
		public bool IsDisposed { get; }

		public IRenderFeature MarkAsDirty();
		public IRenderFeature Setup(in RenderFeatureSetupInfo execInfo);
		public IRenderFeature Compile(ICommandBuffer command);
		public IRenderFeature Execute(ICommandBuffer command);
	}

	public interface IGraphicsRenderFeature : IRenderFeature 
	{
		public ITextureView? BackBuffer { get; set; }
		public ITextureView? DepthBuffer { get; set; }
	}
}
