using REngine.RHI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using REngine.Core.Mathematics;

namespace REngine.RPI.RenderGraph
{
	public abstract class GraphicsRenderFeatureNode : RenderFeatureNode
	{
		public const string BackBufferSlotName = "backbuffer";
		public const string DepthBufferSlotName = "depthbuffer";

		public static readonly ulong BackBufferSlotHash = Hash.Digest(BackBufferSlotName);
		public static readonly ulong DepthBufferSlotHash = Hash.Digest(DepthBufferSlotName);
		protected IResource? BackBufferResource { get; set; }
		protected IResource? DepthBufferResource { get; set; }

		private bool pDirtyBindings = false;

		protected GraphicsRenderFeatureNode(string debugName) : base(debugName)
		{
		}

		protected override void OnExecute(ICommandBuffer command)
		{
			if(!pDirtyBindings)
			{
				base.OnExecute(command);
				return;
			}

			if(GetFeature() is IGraphicsRenderFeature feature)
			{
				if (BackBufferResource != null && BackBufferResource.Value?.ObjectType == GPUObjectType.TextureView)
					feature.BackBuffer = (ITextureView)BackBufferResource.Value;
				if(DepthBufferResource != null && DepthBufferResource.Value?.ObjectType == GPUObjectType.TextureView)
					feature.DepthBuffer = (ITextureView)DepthBufferResource.Value;
				pDirtyBindings = false;
			}

			base.OnExecute(command);
		}

		protected override void OnAddWriteResource(ulong resourceSlotId, IResource resource)
		{
			if(resourceSlotId == BackBufferSlotHash)
			{
				if (BackBufferResource != null)
					BackBufferResource.ValueChanged -= HandleResourceChanges;

				BackBufferResource = resource;
				pDirtyBindings = true;
			}

			if(resourceSlotId == DepthBufferSlotHash)
			{
				if(DepthBufferResource != null)
					DepthBufferResource.ValueChanged -= HandleResourceChanges;

				DepthBufferResource = resource;
				pDirtyBindings = true;
			}
			
			if(pDirtyBindings)
				resource.ValueChanged += HandleResourceChanges;
		}

		private void HandleResourceChanges(object? sender, ResourceChangeEventArgs e)
		{
			pDirtyBindings = true;
		}

		protected override void OnDispose()
		{
			if (BackBufferResource != null)
				BackBufferResource.ValueChanged -= HandleResourceChanges;
			if(DepthBufferResource != null)
				DepthBufferResource.ValueChanged -= HandleResourceChanges;
			base.OnDispose();
		}
	}
}
