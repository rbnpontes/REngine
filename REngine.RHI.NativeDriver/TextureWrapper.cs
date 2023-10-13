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
			get
			{
				TextureDescDTO desc = new();
				TextureImpl.rengine_texture_getdesc(Handle, ref desc);

				TextureDescDTO.Fill(desc, out TextureDesc output);
				return output;
			}
		}

		public string Name { get => Desc.Name; }

		public IntPtr Handle { get; set; }

		public bool IsDisposed { get; private set; }

		public event EventHandler? OnDispose;

		public TextureWrapper(IntPtr handle)
		{
			Handle = handle;
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
			return new TextureViewWrapper(result.value);
		}
	}
}
