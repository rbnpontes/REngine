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
		public TextureDesc Desc
		{
			get => TextureImpl.GetObjectDesc(Handle);
		}

		public string Name { get => Desc.Name; }

		public IntPtr Handle { get; set; }

		public bool IsDisposed { get; private set; }

		public GPUObjectType ObjectType { get; private set; }

		public event EventHandler? OnDispose;

		public TextureWrapper(IntPtr handle)
		{
			Handle = handle;
			ObjectType = TextureImpl.GetObjectTypeFromDesc(Desc);
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
			return new TextureViewWrapper(result.value);
		}
	}
}
