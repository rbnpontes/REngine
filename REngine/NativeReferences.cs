using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using REngine.Core.IO;
using AppContext = System.AppContext;
// ReSharper disable All

namespace REngine
{
	internal static class NativeReferences
	{
		public static void PreloadNativeLibs(ILogger logger)
		{
			string rootDir = AppContext.BaseDirectory;
			List<string> requiredLibs = new();
			List<string> sysLibs = new();

			if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
			{
				requiredLibs.AddRange(new string[]
				{
					Path.Combine(rootDir, "runtimes/win-x64/native/cimgui.dll"),
					Path.Combine(rootDir, "runtimes/win-x64/native/freetype.dll"),
					Path.Combine(rootDir, "runtimes/win-x64/native/glfw3.dll"),
					Path.Combine(rootDir, "runtimes/win-x64/native/REngine-DiligentNativeDriver.dll"),
				});
			}

			if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
			{
				requiredLibs.AddRange(new string[]
				{
					Path.Combine(rootDir, "libcimgui.so"),
					Path.Combine(rootDir, "runtimes/linux-x64/native/libfreetype.so"),
					Path.Combine(rootDir, "runtimes/linux-x64/native/libglfw.so"),
					Path.Combine(rootDir, "runtimes/linux-x64/native/libREngine-DiligentNativeDriver.so"),
				});

				sysLibs.AddRange(new string[]
				{
					"libdl.so",
					"libdl.so.2",
					"libdl.so.6"
				});
			}

			logger.Info("Pre-Loading Required Libs");
			requiredLibs.ForEach(libPath =>
			{
				string filename = Path.GetFileName(libPath);
				logger.Info($"Loading: " + filename);
				if (NativeLibrary.TryLoad(libPath, out IntPtr handle))
					logger.Success($"[Loaded]: {filename}");
				else
					logger.Critical($"[Not Loaded]: {filename}");
			});
			if (sysLibs.Count > 0)
			{
				logger.Info("Pre-Loading System Libs");
				sysLibs.ForEach(libPath =>
				{
					logger.Info($"Loading {libPath}");
					if (NativeLibrary.TryLoad(libPath, out IntPtr handle))
						logger.Success($"[Loaded]: {libPath}");
					else
						logger.Critical($"[Not Loaded]: {libPath}");
				});
			}
		}
	}
}
