using REngine.Core.Resources;
using REngine.RHI;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using REngine.RPI.Batches;
using REngine.RPI.Effects;
using REngine.RPI.Features;

namespace REngine.RPI
{
	public struct TextCreateInfo(Font font, ITexture fontAtlas, bool isDynamic)
	{
		public Font Font = font;
		public ITexture FontAtlas = fontAtlas;
		public bool IsDynamic = isDynamic;
	}
	public interface ISpriteBatch
	{
		public SpriteEffect DefaultEffect { get; }
		public TextEffect DefaultTextEffect { get; }
		public SpriteFeature CreateRenderFeature();
		public SpriteBatch CreateSprite();
		public DynamicSpriteInstanceBatch CreateDynamicSprite();
		public DefaultSpriteInstanceBatch CreateDefaultSprite();
		public StaticSpriteInstanceBatch CreateStaticSprite();
		public TextBatch CreateTextBatch(TextCreateInfo createInfo);
		public void RemoveBatch(SpriteBatch batch);
		public void RemoveBatch(SpriteInstanceBatch batch);
		public void RemoveBatch(TextBatch batch);
	}
}
