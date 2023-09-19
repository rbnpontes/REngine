using REngine.Core.Resources;
using REngine.RHI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace REngine.RPI.Features
{
	internal class SpriteBatchFeature : IRenderFeature
	{
		private Dictionary<Image, ITexture> pCachedTextures = new Dictionary<Image, ITexture>();

		private IPipelineState? pDefaultPipeline;
		private IPipelineState? pTexturedPipeline;
		private IRenderer? pRenderer;

		public bool IsDirty { get; private set; }
		public bool IsDisposed { get; private set; } = false;

		public SpriteBatchFeature(SpriteBatcher batcher)
		{

		}

		public void Dispose()
		{
			if (IsDisposed)
				return;
			IsDisposed = true;
			pDefaultPipeline?.Dispose();
			pTexturedPipeline?.Dispose();

			pDefaultPipeline = pTexturedPipeline = null;
			ClearTextures();
		}

		public IRenderFeature Setup(IGraphicsDriver driver, IRenderer renderer)
		{
			throw new NotImplementedException();
		}

		public IRenderFeature Compile(ICommandBuffer command)
		{
			throw new NotImplementedException();
		}

		public IRenderFeature Execute(ICommandBuffer command)
		{
			throw new NotImplementedException();
		}

		public IRenderFeature MarkAsDirty()
		{
			IsDirty = true;
			return this;
		}

		public void ClearTextures()
		{
			foreach(var pair in pCachedTextures)
			{
				pair.Value.Dispose();
			}
			pCachedTextures.Clear();
		}
	
		private IPipelineState CreatePipeline(IDevice device, IRenderer renderer) { }
	}
}
