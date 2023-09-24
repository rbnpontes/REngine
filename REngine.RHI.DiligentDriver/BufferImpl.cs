using REngine.RHI.DiligentDriver.Adapters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace REngine.RHI.DiligentDriver
{
	internal class BufferImpl : IBuffer, INativeObject
	{
		private Diligent.IBuffer? pBuffer;

		public event GPUObjectEvent OnDispose = new GPUObjectEvent((obj, e)=> { });

		public BufferDesc Desc
		{
			get
			{
				if (pBuffer is null)
					throw new ObjectDisposedException("Buffer is already disposed.");
				var adapter = new BufferAdapter();
				BufferDesc desc;
				adapter.Fill(pBuffer.GetDesc(), out desc);
				return desc;
			}
		}

		public string Name => pBuffer?.GetDesc().Name ?? string.Empty;

		public ulong Size
		{
			get => pBuffer?.GetDesc().Size ?? 0;
		}

		public object? Handle => pBuffer;

		public bool IsDisposed => pBuffer == null;

		public BufferImpl(Diligent.IBuffer buffer)
		{
			pBuffer = buffer;
			buffer.SetUserData(new ObjectWrapper(this));
		}

		public void Dispose()
		{
			if(pBuffer != null)
			{
				pBuffer.Dispose();
				OnDispose(this, new EventArgs());
			}
			pBuffer = null;
		}

		public IntPtr GetHandlePtr()
		{
			return pBuffer?.NativePointer ?? IntPtr.Zero;
		}
	}
}
