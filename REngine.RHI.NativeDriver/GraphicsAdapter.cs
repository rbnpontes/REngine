using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using REngine.Core.Mathematics;

namespace REngine.RHI.NativeDriver
{
	internal class GraphicsAdapter : IGraphicsAdapter
	{
		public uint Id { get; set; }
		public uint DeviceId { get; set; }
		public uint VendorId { get; set; }
		public string Name { get; set; } = string.Empty;
		public AdapterType AdapterType { get; set; }
		public ulong ToHash()
		{
			var hash = Hash.Combine(Id, DeviceId, VendorId);
			hash = Hash.Combine(hash, Hash.Digest(Name));
			return Hash.Combine(hash, (ulong)AdapterType);
		}
	}
}
