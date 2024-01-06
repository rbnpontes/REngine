using REngine.Core;
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

	public sealed partial class DriverFactory
	{
		private static readonly MessageEventCallback sMessageEventDelegate = MessageEvent;

		public static event EventHandler<MessageEventArgs>? OnDriverMessage;

		public static unsafe IGraphicsAdapter[] GetAdapters(GraphicsBackend backend)
		{
			ResultNative result = new();
			uint adaptersCount = 0;
			rengine_get_available_adapter(
				backend, 
				Marshal.GetFunctionPointerForDelegate(sMessageEventDelegate),
				ref result,
				ref adaptersCount
			);

			if(result.error != IntPtr.Zero)
			{
				string error = Marshal.PtrToStringAnsi(result.error) ?? string.Empty;
				throw new Exception(error);
			}

			var finalOutput = new GraphicsAdapter[adaptersCount];

			{
				Span<GraphicsAdapterNative> adapters = new (result.value.ToPointer(), (int)adaptersCount);
				for(var i =0; i < adapters.Length; ++i)
				{
					var adapter = adapters[i];
					finalOutput[i] = new GraphicsAdapter();
					GraphicsAdapterNative.Fill(adapter, finalOutput[i]);
				}
			}

			NativeUtils.rengine_free_block(result.value);
			NativeUtils.rengine_stringdb_free();
			ObjectRegistry.ClearRegistry();
			// ReSharper disable once CoVariantArrayConversion
			return finalOutput;
		}
		
		public static unsafe IGraphicsDriver Build(DriverSettings driverSettings, in NativeWindow window, in SwapChainDesc? swapChainDesc, out ISwapChain? swapChain)
		{
			List<IDisposable> disposables = new();
			DriverResult result = new();
			DriverSettingsNative settings = new();
			DriverSettingsNative.From(driverSettings, ref settings);

			settings.messageCallback = Marshal.GetFunctionPointerForDelegate(sMessageEventDelegate);
			settings.numDeferredCtx = Math.Max((uint)Environment.ProcessorCount, 2);

			if (driverSettings.Backend == GraphicsBackend.OpenGL)
				settings.numDeferredCtx = 0;

			var adapter = FindBestAdapter(GetAdapters(driverSettings.Backend), settings.adapterId);
			settings.adapterId = adapter.Id;

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

			var swapChainDescHandle = swapChainDesc is null ? IntPtr.Zero : new IntPtr(Unsafe.AsPointer(ref swapChainDescNative));
			rengine_create_driver(ref settings, swapChainDescHandle, window, ref result);

			disposables.ForEach(x => x.Dispose());

			if(result.error != IntPtr.Zero)
				throw new Exception(Marshal.PtrToStringAnsi(result.error) ?? "Fatal Error. Error Unknown!");

			DriverNative driverNative = new();
			long driverNativeSize = Unsafe.SizeOf<DriverNative>();
			Buffer.MemoryCopy(result.driver.ToPointer(), Unsafe.AsPointer(ref driverNative), driverNativeSize, driverNativeSize);

			if(driverNative.device == IntPtr.Zero)
			{
				NativeUtils.rengine_free(result.driver);
				NativeUtils.rengine_free_block(driverNative.contexts);

				throw new NullReferenceException("Driver device is null.");
			}

			DeviceImpl device = new (settings.backend, driverNative.device);

			var commands = new ICommandBuffer[settings.numDeferredCtx + 1];
			{
				ReadOnlySpan<IntPtr> commandPointers = new(driverNative.contexts.ToPointer(), commands.Length);
				for(var i =0; i < commands.Length; ++i)
				{
					var ptr = commandPointers[i];
					if (ptr == IntPtr.Zero)
					{
						NativeUtils.rengine_free(result.driver);
						NativeUtils.rengine_free_block(driverNative.contexts);
						throw new NullReferenceException($"ICommandBuffer at index {i} is null.");
					}
					commands[i] = new CommandBufferImpl(commandPointers[i], i > 0);
				}
			}

			var immediateCmd = commands[0];
			var deferredCmd = new ICommandBuffer[commands.Length - 1];

			Array.Copy(commands, 1, deferredCmd, 0, commands.Length - 1);

			if (swapChainDesc != null)
			{
				if (result.swapChain == IntPtr.Zero)
					throw new NullReferenceException("Error has occurred at SwapChain creation. SwapChain is null");
				swapChain = new SwapChainImpl(result.swapChain);
			}
			else
				swapChain = null;

			NativeUtils.rengine_free(result.driver);
			NativeUtils.rengine_free_block(driverNative.contexts);
			NativeUtils.rengine_stringdb_free();

			return new DriverImpl(
				immediateCmd,
				deferredCmd,
				device,
				driverNative.factory,
				adapter
			) { Backend = driverSettings.Backend };
		}

		private static void MessageEvent(DbgMsgSeverity severity, IntPtr msgPtr, IntPtr funcPtr, IntPtr filePtr, int line)
		{
			OnDriverMessage?.Invoke(null, new MessageEventArgs
			{
				File = Marshal.PtrToStringAnsi(filePtr) ?? "UnknownFile",
				Function = Marshal.PtrToStringAnsi(funcPtr) ?? "UnknownFunction()",
				Message = Marshal.PtrToStringAnsi(msgPtr) ?? string.Empty,
				Line = line,
				Severity = severity
			});
		}

		private static IGraphicsAdapter FindBestAdapter(IReadOnlyList<IGraphicsAdapter> adapters, uint adapterId)
		{
			if (adapterId == uint.MaxValue)
			{
				var discreteAdapters = adapters.Where(x => x.AdapterType == AdapterType.Discrete);
				
				var graphicsAdapters = discreteAdapters as IGraphicsAdapter[] ?? discreteAdapters.ToArray();
				if (graphicsAdapters.Length == 0)
				{
					// Prefer Integrated over Software
					var adapter = adapters.FirstOrDefault(x => x.AdapterType == AdapterType.Integrated);
					return adapter ?? adapters.FirstOrDefault(x => x.AdapterType == AdapterType.Software) ?? adapters[0];
				}

				// Sort Adapters by their Total Memory, then pick adapter with the highest memory
				discreteAdapters = graphicsAdapters.OrderByDescending(
					x => x.LocalMemory + x.HostVisibleMemory + x.UnifiedMemory
				);
				return discreteAdapters.First();
			}

			if (adapterId >= adapters.Count)
				throw new IndexOutOfRangeException("Adapter Id value is greater than available adapters on your system");
			return adapters[(int)adapterId];
		}
	}
}
