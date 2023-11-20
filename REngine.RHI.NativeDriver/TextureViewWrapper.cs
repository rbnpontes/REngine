using REngine.RHI.NativeDriver.NativeStructs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace REngine.RHI.NativeDriver
{
	internal class TextureViewWrapper : ITextureView
	{
		private readonly TextureViewDesc pDesc;

		public ITexture Parent
		{
			get
			{
				ResultNative result = new();
				TextureViewImpl.rengine_textureview_getparent(Handle, ref result);

				if (result.error != IntPtr.Zero)
					throw new NullReferenceException(Marshal.PtrToStringAnsi(result.error) ?? "Could not get ITextureView parent.");
				return new TextureWrapper(result.value);
			}
		}

		public TextureViewDesc Desc => pDesc;

		public TextureViewType ViewType => Desc.ViewType;
		public TextureSize Size { get; set; }

		public string Name { get; }

		public IntPtr Handle { get; set; }

		public bool IsDisposed { get; private set; }

		public GPUObjectType ObjectType => GPUObjectType.TextureView;

		public event EventHandler? OnDispose;

		public TextureViewWrapper(IntPtr handle, TextureSize size)
		{
			Handle = handle;
			Size = size;
			Name = string.Intern(Marshal.PtrToStringAnsi(NativeObject.rengine_object_getname(Handle)) ?? string.Empty);
			TextureViewImpl.GetObjectDesc(handle, out pDesc);
		}

		public void Dispose()
		{
			if (IsDisposed)
				return;
			OnDispose?.Invoke(this, EventArgs.Empty);
			IsDisposed = true;
			GC.SuppressFinalize(this);
		}

	}
}
