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
	internal class FontAtlasCharData
	{
		public Rectangle Bounds { get; set; }
		public int Left { get; set; }
		public int Top { get; set; }	
		public int AdvanceHorizontal { get; set; }
		public int AdvanceVertical { get; set; }
	}

	public class FontAsset : IAsset
	{
		class FontImpl : Font
		{
			private static readonly IDictionary<int, byte> sKeyPair = new Dictionary<int, byte>();

			private readonly Image pAtlas;
			private readonly FontAtlasCharData[] pCharData;

			private string pFontName = "Unknow Font";
			public override string Name { get => pFontName; }
			public override Image Atlas { get => pAtlas; }

			public FontImpl(Image atlas, FontAtlasCharData[] charData)
			{
				pAtlas = atlas;
				pCharData = charData;
			}

			public override Point GetAdvance(byte glyphIndex)
			{
				FontAtlasCharData charData = GetGlyphData(glyphIndex);
				return new Point(charData.AdvanceHorizontal, charData.AdvanceVertical);
			}

			public override Rectangle GetBounds(byte glyphIndex)
			{
				FontAtlasCharData charData = GetGlyphData(glyphIndex);
				return charData.Bounds;
			}

			public override Point GetOffset(byte glyphIndex)
			{
				FontAtlasCharData charData = GetGlyphData(glyphIndex);
				return new Point(charData.Left, charData.Top);
			}

			public override byte GetGlyhIndex(int charCode)
			{
				if(sKeyPair.TryGetValue(charCode, out byte glyphCode))
					return glyphCode;
				
				for(byte i = 0; i < CharMap.Length; ++i)
				{
					if (CharMap[i] == charCode)
					{
						sKeyPair[charCode] = i;
						return i;
					}
				}
				throw new Exception($"Not found glyph. Char code: '{char.ConvertFromUtf32(charCode)}'");
			}

			public void SetFontName(string fontName)
			{
				pFontName = fontName;
			}

			private FontAtlasCharData GetGlyphData(byte glyphIndex)
			{
				if (glyphIndex >= pCharData.Length)
					throw new Exception($"Invalid Glyph index. Value: {glyphIndex}");
				return pCharData[glyphIndex];
			}
		}

		private FreeTypeLibrary? pLib;
		private IntPtr pFace = IntPtr.Zero;
		private Font? pFont;

		private object pSync = new();

		public int Size { get; private set; }

		public string Checksum { get; private set; } = string.Empty;

		public string Name { get; set; } = string.Empty;

		public Font Font
		{
			get
			{
				if (pFont is null)
					throw new NullReferenceException("Font has not been loaded. Did you forget to load font ?");
				return pFont;
			}
		}

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
				lock (pSync)
					BuildFont();
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
				throw new Exception($"Error has ocurred at set char size on face. Error: {err}");

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
		
		private unsafe Image GetGlyphImage(uint charCode, out int left, out int top, out uint advanceX, out uint advanceY)
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

				left = faceRec->glyph->bitmap_left;
				top = faceRec->glyph->bitmap_top;
				advanceX = (uint)faceRec->glyph->linearHoriAdvance;
				advanceY = (uint)faceRec->glyph->linearVertAdvance;
			}
			return image;
		}
		
		public unsafe Image GetGlyph(uint charCode)
		{
			return GetGlyphImage(charCode, out int left, out int top, out uint advanceX, out uint advanceY);
		}

		private void BuildFont()
		{
			List<Image> images = new();
			List<(int, int, uint, uint)> offsets = new();

			for(int i = 0; i < Font.CharMap.Length; i++) 
			{
				lock (pSync)
				{
					if(pFace == IntPtr.Zero)
						throw new ObjectDisposedException("FontAsset has been disposed");
					images.Add(GetGlyphImage(Font.CharMap[i], out int left, out int top, out uint advanceX, out uint advanceY));
					offsets.Add((left, top, advanceX, advanceY));
				}
			}

			var atlasData = Image.MakeAtlas(images, 5, 8);

			FontAtlasCharData[] charDataValues = new FontAtlasCharData[Font.CharMap.Length];
			for (int i = 0; i < Font.CharMap.Length; ++i)
			{
				(int left, int top, uint advanceX, uint advanceY) = offsets[i];
				FontAtlasCharData charData = new FontAtlasCharData();
				charData.Bounds = atlasData.Items.ElementAt(i);
				charData.Left = left;
				charData.Top = top;
				charData.AdvanceHorizontal = (int)advanceX;
				charData.AdvanceVertical = (int)advanceY;
				charDataValues[i] = charData;
			}

			var font = new FontImpl(atlasData.Image, charDataValues);
			if (!string.IsNullOrEmpty(Name))
				font.SetFontName(Name);
			Size += font.Atlas.Data.Length;
			pFont = font;
		}

		public Task Save(Stream stream)
		{
			throw new NotSupportedException("Not supported front writing");
		}
	}
}
