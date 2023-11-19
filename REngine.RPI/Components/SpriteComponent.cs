using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using REngine.Core.DependencyInjection;
using REngine.Core.Resources;
using REngine.Core.WorldManagement;

namespace REngine.RPI.Components
{
	public abstract class BaseSpriteComponent : Component
	{
		protected readonly ISpriteBatch mSpriteBatch;

		internal BaseSpriteComponent(IServiceProvider provider)
		{
			mSpriteBatch = provider.Get<ISpriteBatch>();
			mSpriteBatch.OnDraw += OnDraw;
		}

		protected abstract void OnDraw(object? sender, EventArgs e);
	}

	//public sealed class SpriteComponent : BaseSpriteComponent
	//{
	//	public byte TextureSlot { get; set; }
	//	protected override void OnDraw(object? sender, EventArgs e)
	//	{
	//	}
	//}
}
