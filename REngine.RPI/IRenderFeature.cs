using REngine.RHI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace REngine.RPI
{
	public struct RenderFeatureSetupInfo
	{
		public IGraphicsDriver Driver;
		public IRenderer Renderer;
		public IBufferManager BufferManager;
		public IPipelineStateManager PipelineStateManager;
		public IShaderManager ShaderManager;

		public RenderFeatureSetupInfo(
			IGraphicsDriver driver, 
			IRenderer renderer, 
			IBufferManager bufferMgr,
			IPipelineStateManager pipelineStateMgr,
			IShaderManager shaderMgr)
		{
			Driver = driver;
			Renderer = renderer;
			BufferManager = bufferMgr;
			PipelineStateManager = pipelineStateMgr;
			ShaderManager = shaderMgr;
		}
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
