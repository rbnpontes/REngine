using REngine.Core.Resources;
using REngine.RHI;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using REngine.RPI.Features;

namespace REngine.RPI
{
	public struct TextCreateInfo(Font font)
	{
		public Font Font = font;
		public uint FontSize = 16;
		public string Text = string.Empty;
		public Color Color = Color.White;
	}
	public interface ISpriteBatch
	{
		public SpriteFeature CreateRenderFeature();
		public Sprite CreateSprite(SpriteEffect? effect = null);
		public InstancedSprite CreateSprite(SpriteInstancedCreateInfo createInfo);
		public TextRendererBatch CreateText(in TextCreateInfo createInfo);
	}
}
