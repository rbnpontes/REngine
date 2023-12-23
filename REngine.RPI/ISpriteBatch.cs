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
	public interface ISpriteBatch
	{
		public SpriteFeature CreateRenderFeature();
		public Sprite CreateSprite(SpriteEffect? effect = null);
		public SpriteInstance CreateInstancedBatch(uint numInstances, bool dynamic = false);
	}
}
