using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using REngine.Core.DependencyInjection;

namespace REngine.Assets
{
	public static class AssetsModule
	{
		static AssetsModule()
		{
			NativeLibrary.SetDllImportResolver(typeof(SFML.Audio.Music).Assembly, Core.Native.NativeReferences.DefaultDllImportResolver);
			NativeLibrary.SetDllImportResolver(typeof(FreeTypeSharp.NativeObject).Assembly, Core.Native.NativeReferences.DefaultDllImportResolver);
		}

		public static void Setup(IServiceRegistry registry)
		{

		}
	}
}
