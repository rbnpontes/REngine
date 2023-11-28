using REngine.RHI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace REngine.RPI
{
	public enum BufferGroupType
	{
		Frame = 1,
		Camera,
		Object,
		Material
	}

	public interface IRenderer : IDisposable
	{
		public bool IsDisposed { get; }
		public IGraphicsDriver? Driver { get; set; }
		public ISwapChain? SwapChain { get; set; }
		/// <summary>
		/// Insert Render Feature
		/// </summary>
		/// <param name="feature"></param>
		/// <returns></returns>
		public IRenderer AddFeature(IRenderFeature feature, int zindex = -1);
		/// <summary>
		/// Insert a Batch of Render Features
		/// </summary>
		/// <param name="features"></param>
		/// <returns></returns>
		public IRenderer AddFeature(IEnumerable<IRenderFeature> features, int zindex = -1);
		public IRenderer RemoveFeature(IRenderFeature feature);
		/// <summary>
		/// Check for dirty render features and run compile
		/// </summary>
		/// <returns></returns>
		public IRenderer Compile();
		/// <summary>
		/// Execute Render Command on render features
		/// </summary>
		/// <returns></returns>
		public IRenderer Render();
		/// <summary>
		/// Executes Sort operation on Render features
		/// </summary>
		/// <returns></returns>
		public IRenderer PrepareFeatures();
	}
}
