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

		public TextureDesc Desc
		{
			get => GetObjectDesc(Handle);
		}

		public string Name
		{
			get => Desc.Name;
		}

		public GPUObjectType ObjectType { get; private set; }

		public TextureImpl(IntPtr handle) : base(handle)
		{
			ObjectType = GetObjectTypeFromDesc(Desc);
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
			rengine_texture_getdefaultview(Handle, (byte)view, ref result);
			
			if(result.error != IntPtr.Zero)
				throw new Exception(Marshal.PtrToStringAnsi(result.error) ?? $"Can´t retrieve default view {view}. Texture View is null");

			texView = ObjectRegistry.Acquire(result.value) as TextureViewImpl;

			if(texView is null)
			{
				pTexViews[(byte)view] = texView = new TextureViewImpl(result.value);
				//texView.AddRef();
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

		public static TextureDesc GetObjectDesc(IntPtr ptr)
		{
			TextureDescDTO desc = new();
			rengine_texture_getdesc(ptr, ref desc);

			TextureDescDTO.Fill(desc, out TextureDesc output);
			return output;
		}
	}
}
