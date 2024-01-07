using REngine.RHI.NativeDriver.NativeStructs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace REngine.RHI.NativeDriver
{
	internal class TextureWrapper : ITexture
	{
		private readonly TextureDesc pDesc;
		public TextureDesc Desc => pDesc;
		public string Name => pDesc.Name;

		public IntPtr Handle { get; set; }

		public bool IsDisposed { get; private set; }

		public GPUObjectType ObjectType => GPUObjectType.Texture;

		public ResourceState State
		{
			get => (ResourceState)TextureImpl.rengine_texture_get_state(Handle);
			set => TextureImpl.rengine_texture_set_state(Handle, (uint)value);
		}

		public ulong GPUHandle => TextureImpl.rengine_texture_get_gpuhandle(Handle);
		public event EventHandler? OnDispose;

		public TextureWrapper(IntPtr handle)
		{
			Handle = handle;
			TextureImpl.GetObjectDesc(handle, out pDesc);
		}

		public void Dispose()
		{
			if (IsDisposed)
				return;
			OnDispose?.Invoke(this, EventArgs.Empty);
			IsDisposed = true;
			Handle = IntPtr.Zero;
			GC.SuppressFinalize(this);
		}

		public ITextureView GetDefaultView(TextureViewType view)
		{
			ResultNative result = new();
			TextureImpl.rengine_texture_getdefaultview(Handle, (byte)view, ref result);

			if (result.error != IntPtr.Zero)
				throw new NullReferenceException(Marshal.PtrToStringAnsi(result.error) ?? $"Can´t retrieve default view {view}. Texture View is null");
			TextureImpl.ValidateTextureView(view, result.value);
			return new TextureViewWrapper(result.value, new TextureSize(pDesc.Size.Width, pDesc.Size.Height));
		}
	}
}
