using REngine.RHI.NativeDriver.NativeStructs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace REngine.RHI.NativeDriver
{
	internal partial class DeviceImpl : NativeObject, IDevice
	{
		private readonly GraphicsBackend pBackend;
		private readonly object pSync = new();

		public DeviceImpl(GraphicsBackend backend, IntPtr handle) : base(handle)
		{
			pBackend = backend;
		}

		public IBuffer CreateBuffer(in BufferDesc desc)
		{
			return CreateBuffer(desc, IntPtr.Zero, 0);
		}

		public IBuffer CreateBuffer<T>(in BufferDesc desc, IEnumerable<T> values) where T : unmanaged
		{
			return CreateBuffer(desc, new ReadOnlySpan<T>(values.ToArray()));
		}

		public unsafe IBuffer CreateBuffer<T>(in BufferDesc desc, ReadOnlySpan<T> values) where T : unmanaged
		{
			fixed(T* data = values)
			{
				return CreateBuffer(desc, new IntPtr(data), (ulong)(Unsafe.SizeOf<T>() * values.Length));
			}
		}

		public unsafe IBuffer CreateBuffer<T>(in BufferDesc desc, T data) where T : struct
		{
			return CreateBuffer(desc, new IntPtr(Unsafe.AsPointer(ref data)), (ulong)Unsafe.SizeOf<T>());
		}

		public IBuffer CreateBuffer(in BufferDesc desc, IntPtr data, ulong size)
		{
			ResultNative result = new();

			BufferDescDTO.Fill(desc, out BufferDescDTO output);
			if (desc.Size == 0)
				output.size = size;

			lock (pSync)
				rengine_device_create_buffer(Handle, ref output, size, data, ref result);
			
			if(output.name != IntPtr.Zero)
				Marshal.FreeHGlobal(output.name);

			if (result.error != IntPtr.Zero)
				throw new Exception(Marshal.PtrToStringAnsi(result.error) ?? "Could not possible create Buffer");

			return new BufferImpl(result.value);
		}

		public IComputePipelineState CreateComputePipeline(ComputePipelineDesc desc)
		{
			throw new NotImplementedException();
		}

		public unsafe IPipelineState CreateGraphicsPipeline(GraphicsPipelineDesc desc)
		{
			List<IntPtr> strings2Dispose = new ();
			List<IDisposable> disposables = new();
			var inputLayouts = new ArrayPointer<PipelineInputLayoutElementDescNative>(
				desc
				.InputLayouts
				.Select(x =>
				{
					PipelineInputLayoutElementDescNative.Fill(x, out PipelineInputLayoutElementDescNative output);
					return output;
				})
				.ToArray()
			);
			var immutableSamplers = new ArrayPointer<ImmutableSamplerDescNative>(
				desc
				.Samplers
				.Select(x =>
				{
					ImmutableSamplerDescNative.Fill(x, out ImmutableSamplerDescNative output);
					if (output.name != IntPtr.Zero)
						strings2Dispose.Add(output.name);
					return output;
				})
				.ToArray()
			);
			var rtFormats = new ArrayPointer<ushort>(
				desc.Output.RenderTargetFormats.Select(x => (ushort)x).ToArray()
			);

			disposables.AddRange(new IDisposable[]
			{
				inputLayouts,
				immutableSamplers,
				rtFormats
			});

			GraphicsPipelineDescDTO.Fill(desc, out GraphicsPipelineDescDTO output);
			if (output.name != IntPtr.Zero)
				strings2Dispose.Add(output.name);
			
			output.inputLayouts = inputLayouts.Handle;
			output.immutableSamplers = immutableSamplers.Handle;
			output.output_rtFormats = rtFormats.Handle;

			ResultNative result = new();
			lock (pSync)
			{
				rengine_device_create_graphicspipeline(
					Handle,
					ref output,
					(byte)(pBackend == GraphicsBackend.OpenGL ? 1 : 0),
					ref result
				);
			}

			strings2Dispose.ForEach(x => Marshal.FreeHGlobal(x));
			disposables.ForEach(x => x.Dispose());

			if (result.error != IntPtr.Zero)
				throw new Exception(Marshal.PtrToStringAnsi(result.error) ?? "Could not possible create graphics pipeline state");

			return new GraphicsPipelineImpl(desc, result.value);
		}

		public IShader CreateShader(in ShaderCreateInfo createInfo)
		{
			List<IDisposable> disposables = new();
			ShaderCreateInfoDTO.Fill(createInfo, out StringArray macroKeys, out StringArray macroValues, out ShaderCreateInfoDTO output);
			
			disposables.Add(macroKeys);
			disposables.Add(macroValues);
			if(createInfo.ByteCode.Length > 0)
			{
				ArrayPointer<byte> byteCode = new(createInfo.ByteCode);
				disposables.Add(byteCode);
			}

			ResultNative result = new();
			lock (pSync)
				rengine_device_create_shader(Handle, ref output, ref result);

			if (output.name != IntPtr.Zero)
				Marshal.FreeHGlobal(output.name);

			disposables.ForEach(x => x.Dispose());

			if (result.error != IntPtr.Zero)
				throw new Exception(Marshal.PtrToStringAnsi(result.error) ?? "Could not possible create shader");

			return new ShaderImpl(result.value);
		}

		public ITexture CreateTexture(in TextureDesc desc)
		{
			return CreateTexture(desc, Array.Empty<ITextureData>());
		}

		public ITexture CreateTexture(in TextureDesc desc, IEnumerable<ITextureData> subresources)
		{
			ArrayPointer<TextureDataDTO> data = new(
				subresources
					.Select(x =>
					{
						TextureDataDTO.Fill(x, out TextureDataDTO output);
						return output;
					})
					.ToArray()
			);
			TextureDescDTO.Fill(desc, out TextureDescDTO output);

			ResultNative result = new();

			lock (pSync)
			{
				uint subresourcesCount = (uint)subresources.Count();
				rengine_device_create_texture(
					Handle,
					ref output,
					data.Handle,
					subresourcesCount,
					ref result
				);
			}

			if (output.name != IntPtr.Zero)
				Marshal.FreeHGlobal(output.name);
			data.Dispose();

			if (result.error != IntPtr.Zero)
				throw new Exception(Marshal.PtrToStringAnsi(result.error) ?? "Could not possible create texture");

			return new TextureImpl(result.value);
		}
	}
}
