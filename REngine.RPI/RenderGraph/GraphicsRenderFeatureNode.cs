using REngine.RHI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace REngine.RPI.RenderGraph
{
	public abstract class GraphicsRenderFeatureNode : RenderFeatureNode
	{
		public const string BackbufferSlotName = "backbuffer";
		public const string DepthbufferSlotName = "depthbuffer";

		protected IResource? BackBufferResource { get; set; }
		protected IResource? DepthBufferResource { get; set; }

		private bool pDirtyBindings = false;

		public GraphicsRenderFeatureNode(string debugName) : base(debugName)
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

		protected override void OnAddWriteResource(int resourceSlotId, IResource resource)
		{
			if(resourceSlotId == BackbufferSlotName.GetHashCode())
			{
				if (BackBufferResource != null)
					BackBufferResource.ValueChanged -= HandleResourceChanges;

				BackBufferResource = resource;
				pDirtyBindings = true;
			}

			if(resourceSlotId == DepthbufferSlotName.GetHashCode())
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
