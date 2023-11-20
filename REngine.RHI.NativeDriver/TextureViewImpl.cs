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
		private readonly TextureViewDesc pDesc;
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
					throw new NullReferenceException(Marshal.PtrToStringAnsi(result.error) ?? "Could not get ITextureView parent.");
				pParent = ObjectRegistry.Acquire(result.value) as ITexture;
				pParent ??= new TextureImpl(Handle);
				return pParent;
			}
		}

		public TextureViewDesc Desc => pDesc;

		public TextureViewType ViewType => Desc.ViewType;
		public TextureSize Size { get; }

		public string Name { get; }

		public GPUObjectType ObjectType => GPUObjectType.TextureView;

		public TextureViewImpl(IntPtr handle, TextureSize size) : base(handle)
		{
			Size = size;
			GetObjectDesc(handle, out pDesc);
			Name = string.Intern(Marshal.PtrToStringAnsi(rengine_object_getname(Handle)) ?? string.Empty);
		}


		protected override void BeforeRelease()
		{
			pParent = null;
		}

		public static void GetObjectDesc(IntPtr ptr, out TextureViewDesc output)
		{
			TextureViewDescDTO desc = new();
			rengine_textureview_getdesc(ptr, ref desc);
			TextureViewDescDTO.Fill(desc, out output);
		}
	}
}
