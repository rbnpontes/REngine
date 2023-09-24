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
		Fixed = 1,
		Frame,
		Object
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
		public IRenderer AddFeature(IRenderFeature feature);
		/// <summary>
		/// Insert a Batch of Render Features
		/// </summary>
		/// <param name="features"></param>
		/// <returns></returns>
		public IRenderer AddFeature(IEnumerable<IRenderFeature> features);
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
	}
}
