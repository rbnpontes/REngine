using REngine.RHI.NativeDriver.NativeStructs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace REngine.RHI.NativeDriver
{
	internal partial class TextureViewImpl : NativeObject, ITextureView
	{
		private ITexture? pParent;
		
		public ITexture Parent
		{
			get
			{
				if (pParent != null)
					return pParent;

				ResultNative result = new();
				rengine_textureview_getparent(Handle, ref result);

				if (result.error != IntPtr.Zero)
					throw new Exception(Marshal.PtrToStringAnsi(result.error) ?? "Could not get ITextureView parent.");
				pParent = ObjectRegistry.Acquire(result.value) as ITexture;
				pParent ??= new TextureImpl(Handle);
				return pParent;
			}
		}

		public TextureViewDesc Desc
		{
			get
			{
				TextureViewDescDTO desc = new();
				rengine_textureview_getdesc(Handle, ref desc);
				TextureViewDescDTO.Fill(desc, out TextureViewDesc output);
				return output;
			}
		}

		public TextureViewType ViewType => Desc.ViewType;

		public string Name => Marshal.PtrToStringAnsi(rengine_object_getname(Handle)) ?? string.Empty;
		
		public TextureViewImpl(IntPtr handle) : base(handle)
		{
		}


		protected override void BeforeRelease()
		{
			pParent = null;
		}
	}
}
