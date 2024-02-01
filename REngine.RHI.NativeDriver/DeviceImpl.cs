using REngine.RHI.NativeDriver.NativeStructs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using REngine.RHI.NativeDriver.Utils;

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
#if RENGINE_VALIDATIONS
			ValidateBufferDesc(desc);
#endif
			ResultNative result = new();

			BufferDescDTO.Fill(desc, out BufferDescDTO output);
			if (desc.Size == 0)
				output.size = size;

			lock (pSync)
				rengine_device_create_buffer(Handle, ref output, size, data, ref result);
			
			if(output.name != IntPtr.Zero)
				Marshal.FreeHGlobal(output.name);

			if (result.error != IntPtr.Zero)
				throw new Exception(Marshal.PtrToStringAnsi(result.error) ?? $"Could not possible create {nameof(IBuffer)}");

			if (result.value == IntPtr.Zero)
				throw new NullReferenceException($"Could not possible to create {nameof(IBuffer)}");
			return new BufferImpl(result.value);
		}

#if RENGINE_VALIDATIONS
		private void ValidateBufferDesc(in BufferDesc desc)
		{
			if ((desc.BindFlags & BindFlags.UniformBuffer) != 0)
				ValidateUniformBuffer(desc);
		}

		private void ValidateUniformBuffer(in BufferDesc desc)
		{
			if ((desc.BindFlags & BindFlags.VertexBuffer) != 0 || (desc.BindFlags & BindFlags.IndexBuffer) != 0)
				throw new Exception(
					"Is not possible to create a Uniform Buffer with VertexBuffer or IndexBuffer bind flags");
		}
#endif
		
		public IComputePipelineState CreateComputePipeline(ComputePipelineDesc desc)
		{
			List<IntPtr> strings2Dispose = new();
			List<IDisposable> disposables = new();
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

			disposables.Add(immutableSamplers);

			ComputePipelineDescDTO.Fill(desc, out ComputePipelineDescDTO output);
			if (output.name != IntPtr.Zero)
				strings2Dispose.Add(output.name);
			output.samplers = immutableSamplers.Handle;

			ResultNative result = new();
			lock (pSync)
			{
				rengine_device_create_computepipeline(
					Handle,
					ref output,
					(byte)(pBackend == GraphicsBackend.OpenGL ? 1 : 0),
					ref result
				);
			}

			if(result.error != IntPtr.Zero)
				throw new Exception("Can´t create Compute Pipeline. "+(Marshal.PtrToStringAnsi(result.error) ?? "Reason is Unknow"));

			return new ComputePipelineImpl(desc, result.value);
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

			if (result.value == IntPtr.Zero)
				throw new NullReferenceException("Could not possible create graphics pipeline state");
			return new GraphicsPipelineImpl(desc, result.value);
		}

		public IPipelineStateCache CreatePipelineStateCache()
		{
			return CreatePipelineStateCache(Array.Empty<byte>());
		}

		public unsafe IPipelineStateCache CreatePipelineStateCache(byte[] initialData)
		{
			if (!(pBackend == GraphicsBackend.Vulkan || pBackend == GraphicsBackend.D3D12))
				throw new NotSupportedException("Pipeline State Cache is only supported on modern backends (Vulkan or D3D12)");

			ResultNative result = new();
			ReadOnlySpan<byte> buffer = new(initialData);

			fixed(byte* bufferPtr = buffer)
			{
				rengine_device_create_pscache(
					Handle,
					initialData.Length > 0 ? new IntPtr(bufferPtr) : IntPtr.Zero,
					(ulong)initialData.Length,
					ref result
				);
			}

			if (result.error != IntPtr.Zero)
				throw new Exception(Marshal.PtrToStringAnsi(result.error) ?? "Could not possible to create Pipeline State Cache");
			return new PipelineStateCacheImpl(result.value);
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
				throw new Exception(Marshal.PtrToStringAnsi(result.error) ?? "Could not possible to create shader");

			if (result.value == IntPtr.Zero)
				throw new NullReferenceException("Could not possible to create shader");
			return new ShaderImpl(result.value, createInfo);
		}

		public ITexture CreateTexture(in TextureDesc desc)
		{
			return CreateTexture(desc, Array.Empty<ITextureData>());
		}

		public unsafe ITexture CreateTexture(in TextureDesc desc, IEnumerable<ITextureData> subresources)
		{
			ReadOnlySpan<TextureDataDTO> data = new(subresources
				.Select(x =>
				{
					TextureDataDTO.Fill(x, out var output);
					return output;
				}).ToArray()
			);
			TextureDescDTO.Fill(desc, out TextureDescDTO output);

			ResultNative result = new();

			lock (pSync)
			{
				fixed (TextureDataDTO* dataPtr = data)
				{
					var subResourcesCount = (uint)subresources.Count();
					rengine_device_create_texture(
						Handle,
						ref output,
						new IntPtr(dataPtr),
						subResourcesCount,
						ref result
					);
				}
			}

			if (output.name != IntPtr.Zero)
				Marshal.FreeHGlobal(output.name);

			if (result.error != IntPtr.Zero)
				throw new Exception(Marshal.PtrToStringAnsi(result.error) ?? "Could not possible create texture");

			if (result.value == IntPtr.Zero)
				throw new NullReferenceException("Could not possible create texture");
			return new TextureImpl(result.value);
		}
	}
}
