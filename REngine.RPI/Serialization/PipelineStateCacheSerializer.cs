using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using REngine.RHI;

namespace REngine.RPI.Serialization
{
	internal class PipelineStateCacheSerializer : IDisposable
	{
		private readonly Stream pStream;
		private readonly IPipelineStateCache pCache;

		private bool pDisposed;

		public PipelineStateCacheSerializer(IPipelineStateCache pipelineCache, Stream stream)
		{
			pCache = pipelineCache;
			pStream = stream;
		}


		public void Dispose()
		{
			if (pDisposed) return;
			pDisposed = true;
		}

		public void Serialize(IGraphicsDriver driver)
		{
			if(pDisposed) return;

			var hashBytes = BitConverter.GetBytes(driver.AdapterInfo.ToHash());
			pCache.GetData(out var pipelineBytes);

			var buffer = new byte[hashBytes.Length + pipelineBytes.Length];
			Array.Copy(hashBytes, buffer, hashBytes.Length);
			Array.Copy(pipelineBytes, 0, buffer, hashBytes.Length, pipelineBytes.Length);

			using BinaryWriter writer = new(pStream);
			writer.Write(buffer);
		}
	}

	internal class PipelineStateCacheDeserializer : IDisposable
	{
		private readonly Stream pStream;

		private bool pDisposed;
		public PipelineStateCacheDeserializer(Stream stream)
		{
			pStream = stream;
		}

		public void Dispose()
		{
			if(pDisposed) return;
			pDisposed = true;
		}

		public void Deserialize(IGraphicsDriver driver, out byte[] pipelineData)
		{
			if (pDisposed)
				throw new ObjectDisposedException(nameof(PipelineStateCacheDeserializer));

			var hashSize = Unsafe.SizeOf<ulong>();
			using MemoryStream memory = new();
			pStream.CopyTo(memory);

			var buffer = memory.ToArray();
			if (buffer.Length <= hashSize)
			{
				pipelineData = Array.Empty<byte>();
				return;
			}

			var hash = BitConverter.ToUInt64(buffer, 0);
			var expectedHash = driver.AdapterInfo.ToHash();
			// Pipeline State Cache contents must be of the same
			// Graphics Device on Vulkan Backend, D3D12 does not care
			// but Graphics Card Driver(user gpu) must not or does not
			// handle pscache from other device, in this case we must let
			// engine create all pipelines again
			if (hash != expectedHash)
			{
				pipelineData = Array.Empty<byte>();
				return;
			}

			var data = new byte[buffer.Length - hashSize];

			Array.Copy(buffer, hashSize, data, 0, data.Length);
			pipelineData = data;
		}
	}
}
