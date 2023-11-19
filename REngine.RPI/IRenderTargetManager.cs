using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using REngine.RHI;

namespace REngine.RPI
{
	public interface IRenderTargetManager
	{
		public ITexture GetDummyTexture();
		public ITexture Allocate(uint width, uint height);
		public ITexture Allocate(uint width, uint height, TextureFormat format);
	}
}
