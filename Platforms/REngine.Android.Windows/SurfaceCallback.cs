using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.Graphics;
using Android.Runtime;
using Android.Util;
using Android.Views;

namespace REngine.Android.Windows
{
	public sealed class SurfaceCallback : Java.Lang.Object, ISurfaceHolderCallback
	{
		private IntPtr pNativeWindow = IntPtr.Zero;
		public IntPtr NativeWindow => pNativeWindow;
		public Action? OnCreateSurface { get; set; }
		public void SurfaceChanged(ISurfaceHolder holder, Format format, int width, int height)
		{
			Log.Debug(nameof(SurfaceCallback), "Surface is Changed");
		}

		public void SurfaceCreated(ISurfaceHolder holder)
		{
			var surface = holder.Surface;
			if (surface is null)
				return;
			pNativeWindow = AndroidApis.ANativeWindow_fromSurface(JNIEnv.Handle, surface.Handle);
			OnCreateSurface?.Invoke();
			Log.Debug(nameof(SurfaceCallback), "Surface is Created");
		}

		public void SurfaceDestroyed(ISurfaceHolder holder)
		{
			Log.Debug(nameof(SurfaceCallback), "Surface is Destroyed");
		}
	}
}
