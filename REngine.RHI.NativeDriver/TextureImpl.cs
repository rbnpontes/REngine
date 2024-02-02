using REngine.RHI.NativeDriver.NativeStructs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace REngine.RHI.NativeDriver
{
	internal partial class TextureImpl : NativeObject, ITexture
	{
		private readonly TextureViewImpl?[] pTexViews = new TextureViewImpl?[(byte)TextureViewType.ShadingRate];
		private readonly TextureDesc pDesc;

		public TextureDesc Desc => pDesc;

		public string Name => pDesc.Name;

		public GPUObjectType ObjectType { get; }

		public ResourceState State
		{
			get => (ResourceState)rengine_texture_get_state(Handle);
			set => rengine_texture_set_state(Handle, (uint)value);
		}

		public ulong GPUHandle => rengine_texture_get_gpuhandle(Handle);

		public TextureImpl(IntPtr handle) : base(handle)
		{
			ObjectType = GetObjectTypeFromDesc(Desc);
			GetObjectDesc(handle, out pDesc);
		}

		protected override void BeforeRelease()
		{
			foreach(var texView in pTexViews)
			{
				if (texView is null)
					continue;
				if (!texView.IsDisposed)
				{
					texView.Dispose();
				}
			}
			Array.Fill(pTexViews, null);
		}

		public ITextureView GetDefaultView(TextureViewType view)
		{
			TextureViewImpl? texView = pTexViews[(byte)view];

			if(texView != null)
				return texView;

			ResultNative result = new();
			rengine_texture_getdefaultview	(Handle, (byte)view, ref result);
			
			if(result.error != IntPtr.Zero)
				throw new Exception(Marshal.PtrToStringAnsi(result.error) ?? $"Can´t retrieve default viewType {view}. Texture View is null");

			ValidateTextureView(view, result.value);

			texView = ObjectRegistry.Acquire(result.value) as TextureViewImpl;

			if(texView is null)
			{
				pTexViews[(byte)view] = texView = new TextureViewImpl(result.value, pDesc.Size);
				texView.AddRef();
			}

			return texView;
		}

		public static GPUObjectType GetObjectTypeFromDesc(in TextureDesc desc)
		{
			var dim = desc.Dimension;
			var flags = desc.BindFlags;

			GPUObjectType result = GPUObjectType.Unknown;
			if (dim == TextureDimension.Tex1D)
				result = GPUObjectType.Texture1D;
			else if (dim == TextureDimension.Tex1DArray)
				result = GPUObjectType.TextureArray;
			else if (dim == TextureDimension.Tex2D)
				result = GPUObjectType.Texture2D;
			else if (dim == TextureDimension.Tex2DArray)
				result = GPUObjectType.Texture2D;
			else if (dim == TextureDimension.Tex3D)
				result = GPUObjectType.Texture3D;

			if ((flags & BindFlags.RenderTarget) != 0)
				result |= GPUObjectType.RenderTarget;

			return result;
		}

		public static void ValidateTextureView(TextureViewType viewType, IntPtr texView)
		{
			if(texView == IntPtr.Zero)
				throw new NullReferenceException($"There´s no default viewType for '{viewType}'.");
		}
		public static void GetObjectDesc(IntPtr ptr, out TextureDesc output)
		{
			TextureDescDTO desc = new();
			rengine_texture_getdesc(ptr, ref desc);

			TextureDescDTO.Fill(desc, out output);
		}
	}
}
