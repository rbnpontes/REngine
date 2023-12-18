using REngine.Core.DependencyInjection;
using REngine.Core.Resources;
using REngine.Sandbox.Samples;

namespace REngine.Android.Sandbox
{
	public class SandboxApp : SampleApp
	{
		public override void OnSetup(IServiceRegistry registry)
		{
			var assetManager = new AssetManagerSettings();
			if (assetManager.HttpSettings is not null)
				assetManager.HttpSettings.MetadataUrl = "http://192.168.1.4/metadata";
			registry
				.Add(()=> assetManager)
				.Add<IAssetManager, HttpAssetManager>();
			
			base.OnSetup(registry);
		}

		protected override void OnGui()
		{
		}
	}

}
