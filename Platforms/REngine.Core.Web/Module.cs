using REngine.Core.DependencyInjection;
using REngine.Core.Desktop;
using REngine.Core.Resources;

namespace REngine.Core.Web;

public sealed class WebModule : IModule
{
    public void Setup(IServiceRegistry registry)
    {
        registry
            .Add<IWindowManager, WebWindowManager>();
    }
}