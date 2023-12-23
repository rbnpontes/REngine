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
	public struct SpriteInstancedCreateInfo()
	{
		public uint NumInstances = 1;
		public bool Dynamic = false;
		public InstancedSpriteEffect? Effect = null;
	}
	public interface ISpriteBatch
	{
		public SpriteFeature CreateRenderFeature();
		public Sprite CreateSprite(SpriteEffect? effect = null);
		public InstancedSprite CreateSprite(SpriteInstancedCreateInfo createInfo);
	}
}
