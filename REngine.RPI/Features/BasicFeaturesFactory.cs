using REngine.RHI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace REngine.RPI.Features
{
	public class BasicFeaturesFactory
	{
		private GraphicsSettings pSettings;
		public BasicFeaturesFactory(GraphicsSettings settings) 
		{
			pSettings = settings;
		}

		public ICubeRenderFeature CreateCubeFeature()
		{
			return new CubeRenderFeature(pSettings);
		}
	}
}
