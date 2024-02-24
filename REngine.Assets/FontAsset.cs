using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#if !WEB
using FreeTypeSharp;
using FreeTypeSharp.Native;
#endif
using REngine.Core.Resources;

namespace REngine.Assets
{
	internal class FontAtlasCharData
	{
		public Rectangle Bounds { get; init; }
		public int Left { get; init; }
		public int Top { get; init; }	
		public int AdvanceHorizontal { get; init; }
		public int AdvanceVertical { get; init; }
	}

	public class FontAsset : Asset
	{
		private const int DefaultFontSize = 48;
#if !WEB
		private class FontImpl : Font
		{
			private static readonly Dictionary<int, byte> sKeyPair = new ();

			private readonly FontAtlasCharData[] pCharData;

			private string pFontName = "Unknown Font";
			private readonly Size pAtlasSize;

			public override string Name { get => pFontName; }
			public override Size CharSize { get; }
			public override Size AtlasSize { get => pAtlasSize; }
			public override Image Atlas { get; }

			public FontImpl(Image atlas, Size charSize, FontAtlasCharData[] charData)
			{
				pCharData = charData;
				pAtlasSize = atlas.Size.ToSize();
				
				Atlas = atlas;
				CharSize = charSize;
			}

			private FontImpl(FontImpl impl, bool optimized)
			{
				pCharData = impl.pCharData;
				pAtlasSize = impl.pAtlasSize;

				Atlas = optimized ? Image.Empty() : impl.Atlas;
				CharSize = impl.CharSize;
			}

			public override Point GetAdvance(byte glyphIndex)
			{
				var charData = GetGlyphData(glyphIndex);
				return new Point(charData.AdvanceHorizontal, charData.AdvanceVertical);
			}

			public override Rectangle GetBounds(byte glyphIndex)
			{
				var charData = GetGlyphData(glyphIndex);
				return charData.Bounds;
			}

			public override Point GetOffset(byte glyphIndex)
			{
				var charData = GetGlyphData(glyphIndex);
				return new Point(charData.Left, charData.Top);
			}

			public override byte GetGlyphIndex(int charCode)
			{
				if(sKeyPair.TryGetValue(charCode, out byte glyphCode))
					return glyphCode;
				
				for(byte i = 0; i < CharMap.Length; ++i)
				{
					if (CharMap[i] != charCode) continue;
					sKeyPair[charCode] = i;
					return i;
				}
				throw new Exception($"Not found glyph. Char code: '{char.ConvertFromUtf32(charCode)}'");
			}

			public void SetFontName(string fontName)
			{
				pFontName = fontName;
			}

			public override Font Optimize()
			{
				return new FontImpl(this, true);
			}

			private FontAtlasCharData GetGlyphData(byte glyphIndex)
			{
				if (glyphIndex >= pCharData.Length)
					throw new Exception($"Invalid Glyph index. Value: {glyphIndex}");
				return pCharData[glyphIndex];
			}
		}
		private FreeTypeLibrary? pLib;
#endif
		private IntPtr pFace = IntPtr.Zero;
		private Font? pFont;
		
		public Font Font
		{
			get
			{
				lock (mSync)
				{
					if (pFont is null)
						throw new NullReferenceException("Font has not been loaded. Did you forget to load font ?");
					return pFont;
				}
			}
		}

		protected override void OnDispose()
		{
#if !WEB
			pLib?.Dispose();
			pLib = null;
#endif
			pFace = IntPtr.Zero;
			pFont = null;
			GC.SuppressFinalize(this);
		}

		protected override void OnLoad(AssetStream stream)
		{
#if WEB
			throw new NotImplementedException();
#else
			LoadFace(stream);
			BuildFont();
#endif
		}

#if !WEB
		private void LoadFace(Stream stream)
		{
			byte[] fontData;
			using (var mem = new MemoryStream())
			{
				stream.CopyTo(mem);
				fontData = mem.ToArray();
			}
			mSize = fontData.Length;

			pLib?.Dispose();
			pLib = new FreeTypeLibrary();

			var face = GetFace(pLib, fontData);
			fontData = Array.Empty<byte>();
			GC.Collect();

			//var err = FT.FT_Set_Char_Size(face, IntPtr.Zero, new IntPtr(16*24), 300, 300);
			var err = FT.FT_Set_Pixel_Sizes(face, 0, DefaultFontSize);
			if (err != FT_Error.FT_Err_Ok)
				throw new Exception($"Error has occurred at set char size on face. Error: {err}");

			pFace = face;
		}

		private static unsafe IntPtr GetFace(FreeTypeLibrary lib, byte[] buffer)
		{
			IntPtr result;
			fixed(byte* ptr = buffer)
			{
				var err = FT.FT_New_Memory_Face(lib.Native, new IntPtr(ptr), buffer.Length * sizeof(byte), 0, out result);
				if (err != FT_Error.FT_Err_Ok)
					throw new Exception($"Error has occurred at font face loading. Error: {err}");
			}

			return result;
		}
		
		private unsafe Image GetGlyphImage(uint charCode, out int left, out int top, out uint advanceX, out uint advanceY)
		{
			var image = new Image();
			
			if (pFace == IntPtr.Zero)
				throw new NullReferenceException("Font Face has not been created. Did you forget to load Font ?");

			var glyphIdx = FT.FT_Get_Char_Index(pFace, charCode);

			var err = FT.FT_Load_Glyph(pFace, glyphIdx, FT.FT_LOAD_DEFAULT);
			if (err != FT_Error.FT_Err_Ok)
				throw new Exception($"Error has occurred at get glyph. Error: {err}");

			var faceRec = (FT_FaceRec*)pFace;

			err = FT.FT_Render_Glyph(new IntPtr(faceRec->glyph), FT_Render_Mode.FT_RENDER_MODE_NORMAL);
			if (err != FT_Error.FT_Err_Ok)
				throw new Exception($"Error has occurred at render glyph. Error: {err}");

			var data = new byte[faceRec->glyph->bitmap.width * faceRec->glyph->bitmap.rows];
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

			if(faceRec->glyph->bitmap.width > 0 && faceRec->glyph->bitmap.rows > 0)
			{
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

			left = faceRec->glyph->bitmap_left;
			top = faceRec->glyph->bitmap_top;
			advanceX = (uint)faceRec->glyph->advance.x >> 6;
			advanceY = (uint)faceRec->glyph->advance.y >> 6;
			
			return image;
		}
		
		public unsafe Image GetGlyph(uint charCode)
		{
			return GetGlyphImage(charCode, out var left, out var top, out var advanceX, out var advanceY);
		}

		private void BuildFont()
		{
			List<Image> images = [];
			List<(int, int, uint, uint)> offsets = [];

			foreach (var t in Font.CharMap)
			{
				if(pFace == IntPtr.Zero)
					throw new ObjectDisposedException("FontAsset has been disposed");
				
				images.Add(GetGlyphImage(t, out var left, out var top, out var advanceX, out var advanceY));
				offsets.Add((left, top, advanceX, advanceY));
			}

			var atlasData = Image.MakeAtlas(images, 16, 8);

			var charDataValues = new FontAtlasCharData[Font.CharMap.Length];
			for (var i = 0; i < Font.CharMap.Length; ++i)
			{
				var (left, top, advanceX, advanceY) = offsets[i];
				var charData = new FontAtlasCharData
				{
					Bounds = atlasData.Items.ElementAt(i),
					Left = left,
					Top = top,
					AdvanceHorizontal = (int)advanceX,
					AdvanceVertical = (int)advanceY
				};
				charDataValues[i] = charData;
			}

			var font = new FontImpl(atlasData.Image, new Size(16, 16), charDataValues);
			if (!string.IsNullOrEmpty(Name))
				font.SetFontName(Name);
			mSize += font.Atlas.Data.Length;
			pFont = font;
		}
#endif
	}
}
