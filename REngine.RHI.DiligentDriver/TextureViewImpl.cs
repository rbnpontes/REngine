using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace REngine.RHI.DiligentDriver
{
	internal class TextureViewImpl : ITextureView, INativeObject
	{
		private Diligent.ITextureView? pTexture;
		public string Name { get => pTexture?.GetDesc().Name ?? string.Empty; }

		public object? Handle => pTexture;

		public bool IsDisposed => pTexture == null;

		public TextureViewImpl(Diligent.ITextureView texture)
		{
			pTexture = texture;
		}

		public void Dispose()
		{
			pTexture?.Dispose();
			pTexture = null;
		}
	}
}
