using Diligent;
using REngine.Core.DependencyInjection;
using REngine.RHI.DiligentDriver.Adapters;
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
		private GraphicsDriverImpl pDriver;

		public DeviceImpl(GraphicsDriverImpl driver, IRenderDevice device)
		{
			pDevice = device;
			pDriver = driver;
		}

		public IComputePipelineState CreateComputePipeline(ComputePipelineDesc desc)
		{
			throw new NotImplementedException();
		}

		public IPipelineState CreateGraphicsPipeline(GraphicsPipelineDesc desc)
		{
			if (pDevice is null)
				throw new ObjectDisposedException("Can´t create Graphics Pipeline. Device has been already disposed.");

			var adapter = new PipelineStateAdapter(
				pDriver.ServiceProvider.Get<GraphicsSettings>(),
				pDriver.Backend == GraphicsBackend.OpenGL
			);

			GraphicsPipelineStateCreateInfo ci;
			adapter.Fill(desc, out ci);

			var pipeline = pDevice.CreateGraphicsPipelineState(ci);
			return new PipelineStateImpl(pipeline, desc);
		}

		public IShader CreateShader(in ShaderCreateInfo createInfo)
		{
			if (pDevice is null)
				throw new ObjectDisposedException("Can´t create Shader. Device has been already disposed.");
			Diligent.ShaderCreateInfo shaderCI;
			var adapter = new ShaderAdapter();
			adapter.Fill(in createInfo, out shaderCI);

			Diligent.IShader shader = pDevice.CreateShader(shaderCI, out _);
			return new ShaderImpl(shader);
		}

		public void Dispose()
		{
			pDevice?.Dispose();
			pDevice = null;
		}
	}
}
