using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace REngine.RHI.DiligentDriver.Adapters
{
	internal class CopyAdapter
	{
		public CopyAdapter()
		{
		}

		public void Fill(CopyTextureInfo copyInfo, out Diligent.CopyTextureAttribs output)
		{
			Diligent.Box box;

			if (copyInfo.SrcBox != null)
				new BoxAdapter().Fill(copyInfo.SrcBox.Value, out box);
			else
				box = new Diligent.Box();

			output = new Diligent.CopyTextureAttribs
			{
				SrcTexture = NativeObjectUtils.Get<Diligent.ITexture>(copyInfo.SrcTexture),
				SrcMipLevel = copyInfo.SrcMipLevel,
				SrcSlice = copyInfo.SrcSlice,
				SrcBox = copyInfo.SrcBox != null ? box : null,
				DstTexture = NativeObjectUtils.Get<Diligent.ITexture>(copyInfo.DstTexture),
				DstMipLevel = copyInfo.DstMipLevel,
				DstSlice = copyInfo.DstSlice,
				DstX = copyInfo.DstX,
				DstY = copyInfo.DstY,
				DstZ = copyInfo.DstZ
			};
		}
	}
}
