using Diligent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace REngine.RHI.DiligentDriver
{
	internal class DeviceImpl : IDevice
	{
		private IRenderDevice? pDevice;
		public DeviceImpl(IRenderDevice device)
		{
			pDevice = device;
		}
		public void Dispose()
		{
			pDevice?.Dispose();
			pDevice = null;
		}
	}
}
