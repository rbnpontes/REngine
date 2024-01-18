using REngine.Core;
using REngine.Core.DependencyInjection;

namespace REngine.Game;

public sealed class GameModule : IModule
{
    public void Setup(IServiceRegistry registry)
    {
        registry
            .Add<SpriteSystem>()
            .Add<SpriteInstanceSystem>()
            .Add<TextSystem>();
    }
}