using REngine.RHI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using REngine.Core.Reflection;

namespace REngine.RPI.Features
{
	public class BasicFeaturesFactory(IServiceProvider serviceProvider)
	{
		public ICubeRenderFeature CreateCubeFeature()
		{
			return ActivatorExtended.CreateInstance<CubeRenderFeature>(serviceProvider) 
			       ?? throw new NullReferenceException($"Could not possible to Create {nameof(ICubeRenderFeature)}");
		}
	}
}
