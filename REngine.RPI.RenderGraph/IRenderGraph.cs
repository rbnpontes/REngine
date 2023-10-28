using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace REngine.RPI.RenderGraph
{
	public interface IRenderGraph
	{
		public IRenderGraph LoadFromFile(string filePath);
		public IRenderGraph Load(Stream stream);
		/// <summary>
		/// Execute Render Graph
		/// Best way to call this code is through RenderFeature
		/// </summary>
		/// <returns>self instance</returns>
		public IRenderGraph Execute();
	}
}
