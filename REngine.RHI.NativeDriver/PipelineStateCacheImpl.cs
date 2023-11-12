using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace REngine.RHI.NativeDriver
{
	internal class PipelineStateCacheImpl : NativeObject, IPipelineStateCache
	{
		public GPUObjectType ObjectType => GPUObjectType.PipelineStateCache;

		public string Name => "REngine Pipeline State Cache";

		[DllImport(Constants.Lib)]
		static extern IntPtr rengine_pscache_getdata(IntPtr cache);
		public PipelineStateCacheImpl(IntPtr handle) : base(handle)
		{
		}

		public void GetData(out byte[] data)
		{
			AssertDispose();
			IntPtr blobPtr = rengine_pscache_getdata(Handle);
			if(blobPtr == IntPtr.Zero)
			{
				data = Array.Empty<byte>();
				return;
			}

			using (DataBlob blob = new(blobPtr))
				blob.GetData(out data);
		}
	}
}
