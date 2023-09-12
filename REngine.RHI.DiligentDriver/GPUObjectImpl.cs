using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace REngine.RHI.DiligentDriver
{
	internal abstract class GPUObjectImpl : IGPUObject, INativeObject
	{
		public abstract string Name { get; }

		public object? Handle { get; internal set; }

		public bool IsDisposed { get => Handle is null; }

		public event GPUObjectEvent OnDispose = new GPUObjectEvent((obj, e)=> { });

		public GPUObjectImpl(object? handle)
		{
			Handle = handle;
		}

		public T GetHandle<T>()
		{
			if (Handle is null)
				throw new ObjectDisposedException("GPUObject has been disposed");
			return (T)Handle;
		}

		public virtual void Dispose()
		{
			if(Handle != null)
			{
				var disposable = Handle as IDisposable;
				Handle = null;
				disposable?.Dispose();
				OnDispose(this, new EventArgs());
			}
			Handle = null;
		}
	}
}
