using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using REngine.Core.DependencyInjection;
using REngine.Core.Mathematics;
using REngine.RHI;
using REngine.RPI.Features;
using REngine.RPI.RenderGraph.Annotations;

namespace REngine.RPI.RenderGraph
{
	[NodeTag("clear")]
	public sealed class ClearNode : RenderFeatureNode
	{
		public const string ColorPropertyKey = "color";
		public const string DepthValuePropertyKey = "depth";
		public const string StencilValuePropertyKey = "stencil";
		public const string ColorBufferPropertyKey = "colorbuffer";
		public const string DepthBufferPropertyKey = "depthbuffer";

		public readonly ulong ColorPropHash = Hash.Digest(ColorPropertyKey);
		public readonly ulong DepthPropHash = Hash.Digest(DepthValuePropertyKey);
		public readonly ulong StencilPropHash = Hash.Digest(StencilValuePropertyKey);
		public readonly ulong ColorBufferPropHash = Hash.Digest(ColorBufferPropertyKey);
		public readonly ulong DepthBufferPropHash = Hash.Digest(DepthBufferPropertyKey);

		private readonly ClearRenderFeature pFeature = new();

		private ulong pColorBufferId;
		private ulong pDepthBufferId;

		private IResource? pColorBufferResource;
		private IResource? pDepthBufferResource;
		private IResourceManager? pResourceManager;
		public ClearNode() : base(nameof(ClearNode))
		{
		}

		protected override void OnSetup(IDictionary<ulong, string> properties)
		{
			properties.TryGetValue(ColorPropHash, out var color);
			properties.TryGetValue(DepthPropHash, out var depth);
			properties.TryGetValue(StencilPropHash, out var stencil);
			properties.TryGetValue(ColorBufferPropHash, out var colorBuffer);
			properties.TryGetValue(DepthBufferPropHash, out var depthBuffer);

			if (!string.IsNullOrEmpty(color))
				pFeature.ClearColor = Color.FromName(color);
			if(!string.IsNullOrEmpty(depth))
				pFeature.ClearDepthValue = float.Parse(depth);
			if(!string.IsNullOrWhiteSpace(stencil))
				pFeature.ClearStencilValue = byte.Parse(stencil);
			if (!string.IsNullOrEmpty(colorBuffer))
				pColorBufferId = Hash.Digest(colorBuffer);
			if(!string.IsNullOrEmpty(depthBuffer))
				pDepthBufferId = Hash.Digest(depthBuffer);
		}

		protected override void OnRun(IServiceProvider provider)
		{
			pResourceManager ??= provider.Get<IResourceManager>();
			pColorBufferResource ??= pResourceManager.GetResource(pColorBufferId);
			pDepthBufferResource ??= pResourceManager.GetResource(pDepthBufferId);

			if (pColorBufferResource != null)
			{
				var value = pColorBufferResource.Value;
				pFeature.ColorBuffer = value switch
				{
					ITexture tex => tex.GetDefaultView(TextureViewType.RenderTarget),
					ITextureView texView when texView.Desc.ViewType != TextureViewType.RenderTarget => throw
						new InvalidOperationException(
							$"ColorBuffer resource view must be {TextureViewType.RenderTarget}."),
					ITextureView texView => texView,
					_ => pFeature.ColorBuffer
				};
			}

			if (pDepthBufferResource != null)
			{
				var value = pDepthBufferResource.Value;
				pFeature.DepthBuffer = value switch
				{
					ITexture tex => tex.GetDefaultView(TextureViewType.DepthStencil),
					ITextureView texView when texView.Desc.ViewType != TextureViewType.RenderTarget => throw
						new InvalidOperationException(
							$"DepthBuffer resource view must be {TextureViewType.DepthStencil}."),
					ITextureView texView => texView,
					_ => pFeature.DepthBuffer
				};
			}

			base.OnRun(provider);
		}

		protected override IRenderFeature GetFeature()
		{
			return pFeature;
		}
	}
}
