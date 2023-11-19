using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using REngine.Core.DependencyInjection;
using REngine.Core.Mathematics;
using REngine.RHI;
using REngine.RPI.RenderGraph.Annotations;
using REngine.RPI.RenderGraph.Nodes;

namespace REngine.RPI.RenderGraph
{
	public abstract class ViewportRenderTargetNode : RenderGraphNode
	{
		public const string ScalePropertyKey = "scale";
		public static readonly ulong ScalePropHash = Hash.Digest(ScalePropertyKey);

		protected ulong mId;
		protected float mScale = 1;

		protected IResource? mResource;
		protected IResourceManager? mResourceManager;
		protected IRenderer? mRenderer;
		protected IRenderTargetManager? mRenderTargetManager;
		protected IServiceProvider? mServiceProvider;

		internal ViewportRenderTargetNode(string debugName) : base(debugName)
		{
		}

		protected override void OnSetup(IDictionary<ulong, string> properties)
		{
			if (!properties.TryGetValue(VarNode.IdPropHash, out var id))
				throw new RequiredNodePropertyException(VarNode.IdPropertyKey, nameof(ViewportRenderTargetNode));
			mId = Hash.Digest(id);
			
			properties.TryGetValue(ScalePropHash, out var scale);

			if(!string.IsNullOrEmpty(scale))
				mScale = uint.Parse(scale);
		}
		protected override void OnRun(IServiceProvider provider)
		{
			if (mResource is not null)
				return;

			CreateRenderTarget(provider);
		}

		private void CreateRenderTarget(IServiceProvider provider)
		{
			mRenderer ??= provider.Get<IRenderer>();
			mRenderTargetManager ??= provider.Get<IRenderTargetManager>();
			mResourceManager ??= provider.Get<IResourceManager>();
			mServiceProvider = provider;

			var swapChain = mRenderer.SwapChain;
			if (swapChain is null)
				return;

			mResource ??= mResourceManager.GetResource(mId);
			mResource.Value = OnCreateRenderTarget(swapChain, mRenderTargetManager);

			swapChain.OnResize += HandleSwapChainResize;
		}

		private void HandleSwapChainResize(object? sender, SwapChainResizeEventArgs e)
		{
			if (mServiceProvider is null)
				return;

			if(sender is ISwapChain swapChain)
				swapChain.OnResize -= HandleSwapChainResize;

			mResource?.Value?.Dispose();
			CreateRenderTarget(mServiceProvider);
		}

		protected abstract ITexture
			OnCreateRenderTarget(ISwapChain swapChain, IRenderTargetManager renderTargetManager);

		public static void GetScaledRtSize(float scale, ref SwapChainSize size)
		{
			var w = scale * size.Width;
			var h = scale * size.Height;

			size.Width = (uint)w;
			size.Height = (uint)h;
		}
	}
	[NodeTag("colorbuffer")]
	public sealed class ColorBufferNode : ViewportRenderTargetNode
	{
		public ColorBufferNode() : base(nameof(ColorBufferNode)){}

		protected override ITexture OnCreateRenderTarget(ISwapChain swapChain, IRenderTargetManager renderTargetManager)
		{
			var size = swapChain.Size;
			GetScaledRtSize(mScale, ref size);
			return renderTargetManager.Allocate(size.Width, size.Height, swapChain.Desc.Formats.Color);
		}
	}

	[NodeTag("depthbuffer")]
	public sealed class DepthBufferNode : ViewportRenderTargetNode
	{
		public DepthBufferNode() : base(nameof(DepthBufferNode)) { }

		protected override ITexture OnCreateRenderTarget(ISwapChain swapChain, IRenderTargetManager renderTargetManager)
		{
			var size = swapChain.Size;
			GetScaledRtSize(mScale, ref size);

			var depthFormat = swapChain.Desc.Formats.Depth;
			if (depthFormat == TextureFormat.Unknown && mServiceProvider != null)
				depthFormat = mServiceProvider.Get<GraphicsSettings>().DefaultDepthFormat;
			return renderTargetManager.AllocateDepth(size.Width, size.Height, depthFormat);
		}
	}
}
