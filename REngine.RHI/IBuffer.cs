using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace REngine.RHI
{
	public struct BufferDesc
	{
		public string Name;
		public ulong Size;
		public BindFlags BindFlags;
		public Usage Usage;
		public CpuAccessFlags AccessFlags;
		public BufferMode Mode;
		public uint ElementByteStride;

		public BufferDesc()
		{
			Name = string.Empty;
			Size = 0;
			BindFlags = BindFlags.None;
			Usage = Usage.Default;
			AccessFlags = CpuAccessFlags.None;
			Mode = BufferMode.Undefined;
			ElementByteStride = 0;
		}
	}

	public interface IBuffer : IGPUObject
	{
		public BufferDesc Desc { get; }
	}
}
