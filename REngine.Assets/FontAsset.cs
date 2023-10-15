using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FreeTypeSharp;
using FreeTypeSharp.Native;
using REngine.Core.Resources;

namespace REngine.Assets
{
	public class FontAtlasCharData
	{
		public Rectangle Bounds { get; set; }
	}
	public class FontAtlas
	{
		public Image Atlas { get; set; }
		public FontAtlasCharData[] CharData { get; private set; } = new FontAtlasCharData[FontAsset.CharMap.Length];
		public FontAtlas(Image atlas) 
		{ 
			Atlas = atlas;
		}
	}
	public class FontAsset : IAsset
	{
		public const string CharMap = "ABCDEFGHIJKLMNOPQRSTUVXYWZÇabcdefghijklmnopqrstuvxywzç1234567890-=_'\"/\\*+,.;~][´`^|<>?!@#$%¨&(){}ºª§";
		private FreeTypeLibrary? pLib;
		private IntPtr pFace = IntPtr.Zero;
		private object pSync = new();

		public int Size { get; private set; }

		public string Checksum { get; private set; } = string.Empty;

		public string Name { get; set; } = string.Empty;

		public void Dispose()
		{
			pLib?.Dispose();
			pFace = IntPtr.Zero;
		}

		public Task Load(Stream stream)
		{
			return Task.Run(() =>
			{
				lock (pSync)
					LoadFace(stream);
			});
		}

		private void LoadFace(Stream stream)
		{
			byte[] fontData;
			fontData = new byte[stream.Length];
			using (BufferedStream buffer = new BufferedStream(stream))
			{
				using (MemoryStream mem = new())
				{
					buffer.CopyTo(mem);
					fontData = mem.ToArray();
				}
			}

			Size = fontData.Length;

			pLib?.Dispose();
			pLib = new();

			var face = GetFace(pLib, fontData);
			fontData = Array.Empty<byte>();
			GC.Collect();

			var size = (IntPtr)(6 << 6);
			var err = FT.FT_Set_Char_Size(face, size, size, 300, 300);
			if (err != FT_Error.FT_Err_Ok)
			{
				throw new Exception($"Error has ocurred at set char size on face. Error: {err}");
			}

			pFace = face;
		}

		private unsafe IntPtr GetFace(FreeTypeLibrary lib, byte[] buffer)
		{
			IntPtr result;
			fixed(byte* ptr = buffer)
			{
				var err = FT.FT_New_Memory_Face(lib.Native, new IntPtr(ptr), buffer.Length * sizeof(byte), 0, out result);
				if (err != FT_Error.FT_Err_Ok)
					throw new Exception($"Error has ocurred at font face loading. Error: {err}");
			}

			return result;
		}

		public unsafe Image GetGlyph(uint charCode)
		{
			Image image = new Image();
			lock (pSync)
			{
				if (pFace == IntPtr.Zero)
					throw new NullReferenceException("Font Face has not been created. Did you forget to load Font ?");

				uint glyphIdx = FT.FT_Get_Char_Index(pFace, charCode);

				var err = FT.FT_Load_Glyph(pFace, glyphIdx, FT.FT_LOAD_DEFAULT);
				if (err != FT_Error.FT_Err_Ok)
					throw new Exception($"Error has ocurred at get glyph. Error: {err}");

				FT_FaceRec* faceRec = (FT_FaceRec*)pFace;

				err = FT.FT_Render_Glyph(new IntPtr(faceRec->glyph), FT_Render_Mode.FT_RENDER_MODE_NORMAL);
				if (err != FT_Error.FT_Err_Ok)
					throw new Exception($"Error has ocurred at render glyph. Error: {err}");

				byte[] data = new byte[faceRec->glyph->bitmap.width * faceRec->glyph->bitmap.rows];
				fixed (byte* dataPtr = data)
				{
					long size = data.Length * sizeof(byte);
					Buffer.MemoryCopy(
							faceRec->glyph->bitmap.buffer.ToPointer(),
							dataPtr,
							size,
							size
						);
				}

				image.SetData(new ImageDataInfo
				{
					Components = 1,
					Size = new ImageSize(
						(ushort)faceRec->glyph->bitmap.width,
						(ushort)faceRec->glyph->bitmap.rows
					),
					Data = data
				});
			}
			return image;
		}

		public Task<FontAtlas> BuildAtlas()
		{
			return Task.Run(() =>
			{
				List<Image> images = new();
				for(int i =0; i < CharMap.Length; ++i)
				{
					lock (pSync)
					{
						if (pFace == IntPtr.Zero)
							throw new ObjectDisposedException("FontAsset has been disposed");
					}
					images.Add(GetGlyph(CharMap[i]));
				}

				var atlasData = Image.MakeAtlas(images, 5, 8);
				FontAtlas result = new(atlasData.Image);
				for(int i =0; i < CharMap.Length; ++i)
				{
					FontAtlasCharData charData = new FontAtlasCharData();
					charData.Bounds = atlasData.Items.ElementAt(i);
					result.CharData[i] = charData;
				}

				return result;
			});
		}

		public Task Save(Stream stream)
		{
			throw new NotSupportedException("Not supported front writing");
		}
	}
}
