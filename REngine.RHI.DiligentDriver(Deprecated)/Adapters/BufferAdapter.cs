using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace REngine.RHI.DiligentDriver.Adapters
{
	internal class BufferAdapter
	{
		public BufferAdapter() { }

		public void Fill(in BufferDesc desc, out Diligent.BufferDesc output)
		{
			output = new Diligent.BufferDesc();
			output.Name = desc.Name;
			output.Size = desc.Size;
			output.BindFlags = (Diligent.BindFlags)desc.BindFlags;
			output.Usage = (Diligent.Usage)desc.Usage;
			output.CPUAccessFlags = (Diligent.CpuAccessFlags)desc.AccessFlags;
			output.Mode = (Diligent.BufferMode)desc.Mode;
			output.ElementByteStride = desc.ElementByteStride;
		}
		public void Fill(in Diligent.BufferDesc desc, out BufferDesc output)
		{
			output = new BufferDesc();
			output.Name = desc.Name;
			output.Size = desc.Size;
			output.BindFlags = (BindFlags)desc.BindFlags;
			output.AccessFlags = (CpuAccessFlags)desc.CPUAccessFlags;
			output.Usage = (Usage)desc.Usage;
			output.Mode = (BufferMode)desc.Mode;
			output.ElementByteStride = desc.ElementByteStride;
		}
	}
}
