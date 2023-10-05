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
		private readonly ITextureView?[] pTexViews = new ITextureView?[(byte)TextureViewType.ShadingRate];

		public TextureDesc Desc
		{
			get
			{
				TextureDescDTO desc = new();
				rengine_texture_getdesc(Handle, ref desc);

				TextureDescDTO.Fill(desc, out TextureDesc output);
				return output;
			}
		}

		public string Name
		{
			get => Desc.Name;
		}

		public TextureImpl(IntPtr handle) : base(handle)
		{
		}

		protected override void BeforeRelease()
		{
			Array.Fill(pTexViews, null);
		}

		public ITextureView GetDefaultView(TextureViewType view)
		{
			ITextureView? texView = pTexViews[(byte)view];

			if(texView != null)
				return texView;

			ResultNative result = new();
			rengine_texture_getdefaultview(Handle, (byte)view, ref result);
			
			if(result.error != IntPtr.Zero)
				throw new Exception(Marshal.PtrToStringAnsi(result.error) ?? $"Can´t retrieve default view {view}. Texture View is null");

			texView = ObjectRegistry.Acquire(result.value) as ITextureView;

			if(texView is null)
				pTexViews[(byte)view] = texView = new TextureViewImpl(result.value);

			return texView;
		}
	}
}
