using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace REngine.RHI.NativeDriver
{
	internal class DataBlob : NativeObject
	{
		[DllImport(Constants.Lib)]
		static extern ulong rengine_datablob_getlength(IntPtr blob);
		[DllImport(Constants.Lib)]
		static extern IntPtr rengine_datablob_getdata(IntPtr blob);

		public DataBlob(IntPtr handle) : base(handle)
		{
		}

		public unsafe void GetData(out byte[] data)
		{
			AssertDispose();

			ulong size = rengine_datablob_getlength(Handle);
			IntPtr rawData = rengine_datablob_getdata(Handle);

			if(size == 0 || rawData == IntPtr.Zero)
			{
				data = Array.Empty<byte>();
				return;
			}

			ReadOnlySpan<byte> buffer = new(rawData.ToPointer(), (int)size);
			data = buffer.ToArray();
		}
	}
}
