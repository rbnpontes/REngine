using REngine.RHI;
using REngine.RPI.Structs;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace REngine.RPI
{
	public class RenderState
	{
		public Color DefaultClearColor { get; set; } = Color.Black;
		public ClearDepthStencil ClearDepthFlags { get; set; } = ClearDepthStencil.Depth;
		public float DefaultClearDepthValue { get; set; } = 1f;
		public byte DefaultClearStencilValue { get; set; } = 0;

		public RendererFixedData FixedData { get; set; } = new RendererFixedData();
	}
}
