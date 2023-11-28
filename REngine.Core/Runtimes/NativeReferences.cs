using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using REngine.Core.IO;

namespace REngine.Core.Runtimes
{
	public static class NativeReferences
	{
		private static readonly Dictionary<int, IntPtr> sLoadedLibs = new();
		public static ILogger? Logger { get; set; }
		public static IntPtr DefaultDllImportResolver(string libName, Assembly assembly, DllImportSearchPath? searchPath)
		{
			var libHashCode = libName.GetHashCode();
			if(sLoadedLibs.TryGetValue(libHashCode, out var result))
				return result;

			Logger?.Debug($"Resolving Native Library: {libName}");
			var libs = Array.Empty<string>();

			if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
			{
				List<string> libParts = new();
				List<string> pathParts = new()
				{
					Environment.SystemDirectory,
					Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "SysWOW64"),
					Path.Join(AppDomain.CurrentDomain.BaseDirectory, "runtimes/win-x64/native")
				};

				foreach (var libPath in pathParts)
				{
					var files = Directory.GetFiles(libPath);
					libParts.AddRange(files.Where(file => file.Contains(libName)));
				}
				libs = libParts.ToArray();
			} 
			else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
			{
				List<string> libPaths = new();

				var pathParts = (Environment.GetEnvironmentVariable("LD_LIBRARY_PATH") ?? string.Empty)
					.Split(":").ToList();
				pathParts.Add(AppDomain.CurrentDomain.BaseDirectory);
				pathParts.Add(Path.Join(AppDomain.CurrentDomain.BaseDirectory, "runtimes/linux-x64/native"));

				foreach (var files in from pathPart in pathParts where !string.IsNullOrEmpty(pathPart) select Directory.GetFiles(pathPart))
				{
					libPaths.AddRange(files.Where(file => file.Contains(libName)));
				}
				libs = libPaths.ToArray();
			}
			else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
			{
				List<string> libPaths = new();
				var pathParts = (Environment.GetEnvironmentVariable("DYLD_LIBRARY_PATH") ?? string.Empty).Split(":").ToList();
				pathParts.Add(AppDomain.CurrentDomain.BaseDirectory);
				pathParts.Add(Path.Join(AppDomain.CurrentDomain.BaseDirectory, "runtimes/osx/native"));
				pathParts.Add(Path.Join(AppDomain.CurrentDomain.BaseDirectory, "runtimes/osx-x64/native"));

				foreach (var files in from pathPart in pathParts where !string.IsNullOrEmpty(pathPart) select Directory.GetFiles(pathPart))
				{
					libPaths.AddRange(files.Where(file => file.Contains(libName)));
				}
				libs = libPaths.ToArray();
			}

			Logger?.Debug("Found Libraries: "+string.Join(Environment.NewLine, libs));
			foreach (var lib in libs)
			{
				Logger?.Debug("[Loading]: " + lib);
				if (NativeLibrary.TryLoad(lib, out result))
				{
					Logger?.Success("[Loaded]: " + lib);
					sLoadedLibs[libHashCode] = result;
					return result;
				}

				Logger?.Debug("[Failed]: " + lib);
			}

			Logger?.Debug("Trying again.");
			foreach (var lib in libs)
			{
				try
				{
					result = NativeLibrary.Load(lib);
					sLoadedLibs[libHashCode] = result;
					return result;
				}
				catch (Exception ex)
				{
					Logger?.Critical(ex);
				}
			}

			Logger?.Debug("Fallback to default loading");
			if (!NativeLibrary.TryLoad(libName, assembly,
				    DllImportSearchPath.ApplicationDirectory | DllImportSearchPath.UserDirectories |
				    DllImportSearchPath.UseDllDirectoryForDependencies,
				    out result)) throw new FileLoadException($"Failed to load native lib {libName}.");
			sLoadedLibs[libHashCode] = result;
			return result;
		}

		public static void PreloadLibs()
		{
			List<string> libs2Load = new();
			List<string> searchPaths = new()
			{
				AppContext.BaseDirectory
			};

			if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
			{
				libs2Load.AddRange(new string[]
				{
					"cimgui.dll",
					"freetype.dll",
					"glfw3.dll",
					"REngine-DiligentNativeDriver.dll",
					"csfml-audio.dll",
					"TracyClient.dll"
				});

				searchPaths.AddRange(new string[]
				{
					Path.Join(AppDomain.CurrentDomain.BaseDirectory, "runtimes/win-x64/native"),
					Path.Join(AppDomain.CurrentDomain.BaseDirectory, "runtimes/win-x86/native"),
					Path.Join(AppDomain.CurrentDomain.BaseDirectory, "runtimes/win10-x64/native"),
					Path.Join(AppDomain.CurrentDomain.BaseDirectory, "runtimes/win10-x86/native"),
					Environment.SystemDirectory,
					Environment.GetFolderPath(Environment.SpecialFolder.Windows),
					Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "SysWOW64"),
				});
			}

			if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
			{
				libs2Load.AddRange(new string[]
				{
					"libcimgui.so",
					"libfreetype.so",
					"libglfw.so",
					"libREngine-DiligentNativeDriver.so",
					"libcsfml-audio.so",
					"libdl.so",
					"libFLAC.so"
				});


				searchPaths.Add(
					Path.Join(AppDomain.CurrentDomain.BaseDirectory, "runtimes/linux-x64/native")
				);
				searchPaths.AddRange(
					(Environment.GetEnvironmentVariable("LD_LIBRARY_PATH") ?? string.Empty)
					.Split(':').Where(x => !string.IsNullOrEmpty(x)).ToList()
				);
			}

			if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
			{
				libs2Load.AddRange(new string[]
				{
					"libcimgui.dylib",
					"libfreetype.dylib",
					"libglfw.dylib",
					"libREngine-DiligentNativeDriver.dylib",
					"libcsfml-audio.dylib",
					"libdl.dylib",
					"libFLAC.dylib"
				});
				searchPaths.AddRange(
					new string[]
					{
						Path.Join(AppDomain.CurrentDomain.BaseDirectory, "runtimes/osx-x64/native"),
						Path.Join(AppDomain.CurrentDomain.BaseDirectory, "runtimes/osx/native")
					}
				);
				searchPaths.AddRange(
					(Environment.GetEnvironmentVariable("DYLD_LIBRARY_PATH") ?? string.Empty)
					.Split(':').Where(x => !string.IsNullOrEmpty(x)).ToList()
				);
			}

			Logger?.Debug("Preloading Libs: " + string.Join(Environment.NewLine, libs2Load));
			foreach (var lib in libs2Load)
			{
				foreach (var searchPath in searchPaths)
				{
					if(!Directory.Exists(searchPath))
						continue;

					var loaded = false;
					var files = Directory.GetFiles(searchPath).Where(x => x.Contains(lib));
					foreach (var file in files)
					{
						loaded = TryInsertLib(lib, searchPath, file);
						if (loaded)
							break;
					}

					if (loaded)
						break;
				}
			}

			return;

			bool TryInsertLib(string libName, string basePath, string libPath)
			{
				IntPtr result;
				try
				{
					result = NativeLibrary.Load(libPath);
					Logger?.Success($"[LOADED]: {libName}");
				}
				catch (Exception e)
				{
					Logger?.Error($"[FAILED TO LOAD]: {libName}", e);
					return false;
				}

				var libHashCode = libName.GetHashCode();
				InsertLib(libHashCode, result);

				libPath = libPath.Replace(basePath, string.Empty)
					.Replace("\\", string.Empty)
					.Replace("/", string.Empty); // remove any file delimiter

				libHashCode = libName.GetHashCode();
				InsertLib(libHashCode, result);

				if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ||
				    RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
				{
					libPath = libPath.Replace("lib", string.Empty);
					InsertLib(libPath.GetHashCode(), result); // Insert Lib without 'lib' prefix
				}

				var dotIdx = libPath.IndexOf(".", StringComparison.Ordinal);
				if (dotIdx != -1)
					libPath = libPath[..dotIdx];
				InsertLib(libPath.GetHashCode(), result); // Insert Lib without full extension and lib version, like: libname.so.1, libname.dylib.1 or libname.dll

				return true;
			}

			void InsertLib(int hashCode, IntPtr libHandle)
			{
				if (libHandle == IntPtr.Zero)
					return;
				sLoadedLibs.TryAdd(hashCode, libHandle);
			}
		}

		public static void UnloadLibs()
		{
			HashSet<IntPtr> libs2Unload = new();
			foreach (var libPair in sLoadedLibs)
				libs2Unload.Add(libPair.Value);

			sLoadedLibs.Clear();

			foreach (var lib in libs2Unload)
				NativeLibrary.Free(lib);

			Logger?.Debug($"Unloaded {libs2Unload.Count} Native Libs");
		}
	}
}
