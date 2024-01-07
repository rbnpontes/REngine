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
		public ulong LocalMemory { get; set; }
		public ulong HostVisibleMemory { get; set; }
		public ulong UnifiedMemory { get; set; }
		public ulong MaxMemoryAlloc { get; set; }
		public CpuAccessFlags UnifiedMemoryCpuAccess { get; set; }
		public BindFlags MemorylessTextureBindFlags { get; set; }
		public ulong ToHash()
		{
			var hash = Hash.Combine(Id, DeviceId, VendorId);
			hash = Hash.Combine(hash, Hash.Digest(Name));
			hash = Hash.Combine(hash, (ulong)AdapterType);
			hash = Hash.Combine(hash, LocalMemory);
			hash = Hash.Combine(hash, HostVisibleMemory);
			hash = Hash.Combine(hash, UnifiedMemory);
			hash = Hash.Combine(hash, MaxMemoryAlloc);
			hash = Hash.Combine(hash, (ulong)UnifiedMemoryCpuAccess);
			hash = Hash.Combine(hash, (ulong)MemorylessTextureBindFlags);
			return hash;
		}

		public override string ToString()
		{
			StringBuilder builder = new();
			builder.AppendLine("Adapter Info")
				.AppendLine($"Hash: {ToHash()}")
				.AppendLine($"Name: {Name}")
				.AppendLine($"Id: {Id}")
				.AppendLine($"Device Id: {DeviceId}")
				.AppendLine($"Vendor Id: {VendorId}")
				.AppendLine($"Type: {AdapterType}")
				.AppendLine($"Local Memory: {LocalMemory}")
				.AppendLine($"Host Visible Memory: {HostVisibleMemory}")
				.AppendLine($"Unified Memory: {UnifiedMemory}")
				.AppendLine($"Max Memory Alloc: {MaxMemoryAlloc}")
				.AppendLine($"Unified Memory CPU Access: {UnifiedMemoryCpuAccess}")
				.AppendLine($"Memoryless Texture Bind Flags: {MemorylessTextureBindFlags}");
			return builder.ToString();
		}
	}
}
