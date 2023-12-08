using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.Media;
using Android.Util;
using Android.Views;
using REngine.Android.Windows;
using REngine.Core;
using REngine.Core.Android;
using REngine.Core.DependencyInjection;
using REngine.Core.IO;
using REngine.Core.Resources;
using REngine.RHI;
using REngine.RHI.DiligentDriver;
using REngine.RPI;
using IWindowManager = REngine.Core.IWindowManager;
using WindowManager = REngine.Android.Windows.WindowManager;

namespace REngine.Android.Sandbox
{
	public class SandboxApp() : App
	{
		public override void OnSetup(IServiceRegistry registry)
		{
			registry
				.Add<IAssetManager, HttpAssetManager>();
		}

		public override void OnStart(IServiceProvider provider)
		{
			var assetManagerSettings = provider.Get<AssetManagerSettings>();
			if(assetManagerSettings.HttpSettings != null)
				assetManagerSettings.HttpSettings.MetadataUrl = "http://192.168.1.4/metadata";
		}

		protected override void OnGui()
		{
		}
	}

}
