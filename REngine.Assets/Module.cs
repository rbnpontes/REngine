using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using REngine.Core;
using REngine.Core.DependencyInjection;

namespace REngine.Assets
{
	public sealed class AssetsModule : IModule
	{
		static AssetsModule()
		{
			try
			{
				NativeLibrary.SetDllImportResolver(typeof(SFML.Audio.Music).Assembly,
					Core.Runtimes.NativeReferences.DefaultDllImportResolver);
				NativeLibrary.SetDllImportResolver(typeof(FreeTypeSharp.NativeObject).Assembly,
					Core.Runtimes.NativeReferences.DefaultDllImportResolver);
			}
			catch
			{
				// ignored
			}
		}

		public void Setup(IServiceRegistry registry)
		{

		}
	}
}
