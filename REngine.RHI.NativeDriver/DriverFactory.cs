using REngine.RHI.DiligentDriver;
using REngine.RHI.NativeDriver.NativeStructs;
using REngine.RHI.NativeDriver.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace REngine.RHI.NativeDriver
{
	public enum DbgMsgSeverity
	{
		Info =0,
		Warning,
		Error,
		FatalError
	}
	public class MessageEventArgs : EventArgs
	{
		public DbgMsgSeverity Severity { get; set; }
		public string Message { get; set; } = string.Empty;
		public string Function { get; set; } = string.Empty;
		public string File { get; set; } = string.Empty;
		public int Line { get; set; }
	}

	public sealed class DriverFactory
	{
		delegate void MessageEventCallback(DbgMsgSeverity severity, IntPtr msg, IntPtr func, IntPtr file, int line);
		[DllImport(Constants.Lib)]
		static extern void rengine_get_available_adapter(GraphicsBackend backend, IntPtr messageEvent, ref ResultNative result, ref uint length);
		[DllImport(Constants.Lib)]
		static extern void rengine_create_driver(ref DriverSettingsNative settings, IntPtr swapChainDesc, [In] in NativeWindow nativeWindow, ref DriverResult result);

		static readonly MessageEventCallback s_messageEventDelegate = MessageEvent;

		public static event EventHandler<MessageEventArgs>? OnDriverMessage;

		public DriverFactory()
		{

		}

		public static unsafe GraphicsAdapter[] GetAdapters(GraphicsBackend backend)
		{
			ResultNative result = new ResultNative();
			uint adaptersCount = 0;
			rengine_get_available_adapter(
				backend, 
				Marshal.GetFunctionPointerForDelegate(s_messageEventDelegate),
				ref result,
				ref adaptersCount
			);

			if(result.error != IntPtr.Zero)
			{
				string error = Marshal.PtrToStringAnsi(result.error) ?? string.Empty;
				throw new Exception(error);
			}

			GraphicsAdapter[] finalOutput = new GraphicsAdapter[adaptersCount];

			{
				Span<GraphicsAdapterNative> adapters = new Span<GraphicsAdapterNative>(result.value.ToPointer(), (int)adaptersCount);
				for(int i =0; i < adapters.Length; ++i)
				{
					GraphicsAdapterNative adapter = adapters[i];
					finalOutput[i] = new GraphicsAdapter { 
						Id = adapter.id,
						DeviceId = adapter.deviceId,
						AdapterType = (AdapterType)adapter.adapterType,
						Name = Marshal.PtrToStringAnsi(adapter.name) ?? "Unknow Device",
						VendorId = adapter.vendorId
					};
				}
			}

			NativeUtils.Free(result.value);
			return finalOutput;
		}
		
		public static unsafe IGraphicsDriver Build(DriverSettings driverSettings, in NativeWindow window, in SwapChainDesc? swapChainDesc, out ISwapChain? swapChain)
		{
			List<IDisposable> disposables = new();
			DriverResult result = new();
			DriverSettingsNative settings = new();
			DriverSettingsNative.From(driverSettings, ref settings);

			settings.messageCallback = Marshal.GetFunctionPointerForDelegate(s_messageEventDelegate);
			settings.numDeferredCtx = Math.Max((uint)Environment.ProcessorCount, 2);

#if WINDOWS
			D3D12SettingsNative d3d12Settings = new();
			D3D12SettingsNative.From(driverSettings.D3D12, ref d3d12Settings);
			{
				ArrayPointer<uint> d3d12_cpuDescriptorHeapAllocationSize = new(driverSettings.D3D12.CPUDescriptorHeapAllocationSize);
				ArrayPointer<uint> d3d12_gpuDescriptorHeapSize = new(driverSettings.D3D12.GPUDescriptorHeapSize);
				ArrayPointer<uint> d3d12_gpuDescriptorHeapDynamicSize = new(driverSettings.D3D12.GPUDescriptorHeapDynamicSize);
				ArrayPointer<uint> d3d12_dynamicDescriptorAllocationChunkSize = new(driverSettings.D3D12.DynamicDescriptorAllocationChunkSize);
				ArrayPointer<uint> d3d12_queryPoolSize = new(driverSettings.D3D12.QueryPoolSizes);

				d3d12Settings.cpuDescriptorHeapAllocationSize = d3d12_cpuDescriptorHeapAllocationSize.Handle;
				d3d12Settings.gpuDescriptorHeapSize = d3d12_gpuDescriptorHeapSize.Handle;
				d3d12Settings.gpuDescriptorHeapDynamicSize = d3d12_gpuDescriptorHeapDynamicSize.Handle;
				d3d12Settings.dynamicDescriptorAllocationChunkSize = d3d12_dynamicDescriptorAllocationChunkSize.Handle;
				d3d12Settings.queryPoolSize = d3d12_queryPoolSize.Handle;

				disposables.AddRange(new IDisposable[]
				{
					d3d12_cpuDescriptorHeapAllocationSize,
					d3d12_gpuDescriptorHeapSize,
					d3d12_gpuDescriptorHeapDynamicSize,
					d3d12_dynamicDescriptorAllocationChunkSize,
					d3d12_queryPoolSize
				});
			}
			settings.d3d12 = new IntPtr(Unsafe.AsPointer(ref d3d12Settings));
#endif
			VulkanSettingsNative vkSettings = new();
			VulkanSettingsNative.From(driverSettings.Vulkan, ref vkSettings);
			DescriptorPoolSizeNative vk_mainDescriptorPoolSize = new();
			DescriptorPoolSizeNative vk_dynamicDescriptorPoolSize = new();
			{
				StringArray vk_instanceLayers = new(driverSettings.Vulkan.InstanceLayerNames);
				StringArray vk_instanceExtensionNames = new(driverSettings.Vulkan.InstanceExtensionNames);
				StringArray vk_deviceExtensionNames = new(driverSettings.Vulkan.DeviceExtensionNames);
				StringArray vk_ignoreDebugMessageNames = new(driverSettings.Vulkan.IgnoreDebugMessageNames);

				DescriptorPoolSizeNative.From(driverSettings.Vulkan.MainDescriptorPoolSize, ref vk_mainDescriptorPoolSize);
				DescriptorPoolSizeNative.From(driverSettings.Vulkan.DynamicDescriptorPoolSize, ref vk_dynamicDescriptorPoolSize);

				ArrayPointer<uint> vk_queryPoolSizes = new(driverSettings.Vulkan.QueryPoolSizes);

				vkSettings.instanceLayerNames = vk_instanceLayers.Handle;
				vkSettings.instanceExtensionNames = vk_instanceExtensionNames.Handle;
				vkSettings.deviceExtensionNames = vk_deviceExtensionNames.Handle;
				vkSettings.ignoreDebugMessageNames = vk_ignoreDebugMessageNames.Handle;

				vkSettings.mainDescriptorPoolSize = new IntPtr(Unsafe.AsPointer(ref vk_mainDescriptorPoolSize));
				vkSettings.dynamicDescriptorPoolSize = new IntPtr(Unsafe.AsPointer(ref vk_dynamicDescriptorPoolSize));

				vkSettings.queryPoolSizes = vk_queryPoolSizes.Handle;

				disposables.AddRange(new IDisposable[] 
				{ 
					vk_instanceLayers,
					vk_instanceExtensionNames,
					vk_deviceExtensionNames,
					vk_ignoreDebugMessageNames,
					vk_queryPoolSizes
				});
			}
			settings.vulkan = new IntPtr(Unsafe.AsPointer(ref vkSettings));

			SwapChainDescNative swapChainDescNative = new();
			if(swapChainDesc != null)
				SwapChainDescNative.From(swapChainDesc.Value, ref swapChainDescNative);

			IntPtr swapChainDescHandle = swapChainDesc is null ? IntPtr.Zero : new IntPtr(Unsafe.AsPointer(ref swapChainDescNative));
			rengine_create_driver(ref settings, swapChainDescHandle, window, ref result);

			disposables.ForEach(x => x.Dispose());

			if(result.error != IntPtr.Zero)
				throw new Exception(Marshal.PtrToStringAnsi(result.error) ?? "Fatal Error. Error Unknow!");

			DriverNative driverNative = new();
			long driverNativeSize = Unsafe.SizeOf<DriverNative>();
			Buffer.MemoryCopy(result.driver.ToPointer(), Unsafe.AsPointer(ref driverNative), driverNativeSize, driverNativeSize);

			NativeUtils.Free(result.driver);
			DeviceImpl device = new (driverNative.device);

			ICommandBuffer[] commands = new ICommandBuffer[settings.numDeferredCtx];
			int ptrSize = Marshal.SizeOf<IntPtr>();

			for(int i =0; i < commands.Length; ++i)
			{
				IntPtr commandPtr = IntPtr.Add(driverNative.deferredCtx, ptrSize * i);
				commands[i] = new CommandBufferImpl(commandPtr, true);
			}

			if (swapChainDesc != null)
				swapChain = new SwapChainImpl(result.swapChain);
			else
				swapChain = null;

			return new DriverImpl(
				new CommandBufferImpl(driverNative.immediateCtx, false),
				commands,
				device,
				driverNative.factory
			);
		}

		private static void MessageEvent(DbgMsgSeverity severity, IntPtr msgPtr, IntPtr funcPtr, IntPtr filePtr, int line)
		{
			OnDriverMessage?.Invoke(null, new MessageEventArgs
			{
				File = Marshal.PtrToStringAnsi(filePtr) ?? "UnkowFile",
				Function = Marshal.PtrToStringAnsi(funcPtr) ?? "UnknowFunction()",
				Message = Marshal.PtrToStringAnsi(msgPtr) ?? string.Empty,
				Line = line,
				Severity = severity
			});
		}
	}
}
