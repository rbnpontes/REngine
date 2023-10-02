using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace REngine.RHI.NativeDriver
{
	internal class TextureViewImpl : NativeObject, ITextureView
	{
		public ITexture Parent => throw new NotImplementedException();

		public TextureViewDesc Desc => throw new NotImplementedException();

		public TextureViewType ViewType => throw new NotImplementedException();

		public string Name => throw new NotImplementedException();

		public event GPUObjectEvent? OnDispose;
		
		public TextureViewImpl(IntPtr handle) : base(handle)
		{
		}

	}
}
