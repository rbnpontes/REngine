using REngine.RHI.DiligentDriver.Adapters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace REngine.RHI.DiligentDriver
{
	internal class TextureViewImpl : GPUObjectImpl, ITextureView
	{
		public override string Name => GetHandle<Diligent.ITextureView>().GetDesc().Name;

		public TextureViewDesc Desc
		{
			get
			{
				var adapter = new TextureAdapter();
				TextureViewDesc desc;
				adapter.Fill(GetHandle<Diligent.ITextureView>().GetDesc(), out desc);
				return desc;
			}
		}

		public TextureViewType ViewType
		{
			get => (TextureViewType)GetHandle<Diligent.ITextureView>().GetDesc().ViewType;
		}

		public ITexture Parent { get; private set; }

		public TextureViewImpl(Diligent.ITextureView textureView) : base(textureView)
		{
			var texture = textureView.GetTexture();
			if (texture.GetUserData() == null)
				Parent = new TextureImpl(texture);
			else
				Parent = ObjectWrapper.Unwrap(texture).Get<ITexture>();
		}
	}
}
