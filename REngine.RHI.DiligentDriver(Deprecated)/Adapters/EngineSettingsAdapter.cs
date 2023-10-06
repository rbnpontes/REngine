using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace REngine.RHI.DiligentDriver.Adapters
{
	internal class EngineSettingsAdapter
	{
		public void FillDefault(GraphicsDriverSettings settings, ref Diligent.EngineCreateInfo ci)
		{

		}
		public void Fill(GraphicsDriverSettings settings, ref Diligent.EngineD3D12CreateInfo ci)
		{
			ci.CPUDescriptorHeapAllocationSize = settings.D3D12.CPUDescriptorHeapAllocationSize;
			ci.GPUDescriptorHeapSize = settings.D3D12.GPUDescriptorHeapSize;
			ci.GPUDescriptorHeapDynamicSize = settings.D3D12.GPUDescriptorHeapDynamicSize;
			ci.DynamicDescriptorAllocationChunkSize = settings.D3D12.DynamicDescriptorAllocationChunkSize;
			ci.DynamicHeapPageSize = settings.D3D12.DynamicHeapPageSize;
			ci.NumDynamicHeapPagesToReserve = settings.D3D12.NumDynamicHeapPagesToReserve;
			ci.DynamicHeapPageSize = settings.D3D12.DynamicHeapPageSize;
			ci.NumDynamicHeapPagesToReserve = settings.D3D12.NumDynamicHeapPagesToReserve;
			ci.QueryPoolSizes = settings.D3D12.QueryPoolSizes;
		}
		public void Fill(GraphicsDriverSettings settings, ref Diligent.EngineVkCreateInfo ci)
		{

		}
	}
}
