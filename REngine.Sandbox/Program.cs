using REngine.Core;
using REngine.Core.DependencyInjection;
using REngine.RHI;
using REngine.RHI.DiligentDriver;
using REngine.Windows;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace REngine.Sandbox
{
	internal static class Program
	{
		struct Vertex
		{
			public Vector3 Position;
			public Vector2 UV;

			public Vertex()
			{
				Position = new Vector3();
				UV = new Vector2();
			}

			public Vertex(Vector3 position, Vector2 uv)
			{
				Position = position;
				UV = uv;
			}
		}
		/// <summary>
		///  The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main()
		{
			EngineApplication
				.CreateStartup<SandboxApp>()
				.Setup()
				.Start()
				.Run();
		}
		private static ITextureView CreateTexture(IDevice device)
		{
			var bitmap = new Bitmap(
				Path.Join(AppDomain.CurrentDomain.BaseDirectory, "Assets/Textures/doge.jpg")
			);
			var bitmapData = bitmap.LockBits(
				new Rectangle(0, 0, bitmap.Width, bitmap.Height),
				ImageLockMode.ReadOnly,
				PixelFormat.Format32bppArgb
			);

			var pixelData = new byte[bitmapData.Stride * bitmap.Height];
			Marshal.Copy(bitmapData.Scan0, pixelData, 0, bitmapData.Stride * bitmapData.Height);
			bitmap.UnlockBits(bitmapData);

			return device.CreateTexture(new TextureDesc
			{
				Name = "Doge Texture",
				Size = new TextureSize((uint)bitmap.Width, (uint)bitmap.Height),
				Format = TextureFormat.RGBA8UNormSRGB,
				BindFlags = BindFlags.ShaderResource,
				Usage = Usage.Immutable
			}, new ITextureData[] { 
				new ByteTextureData(pixelData, (ulong)bitmapData.Stride) 
			}).GetDefaultView(TextureViewType.ShaderResource);
		}
	}
}