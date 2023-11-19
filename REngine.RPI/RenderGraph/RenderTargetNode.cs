using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using REngine.Core.DependencyInjection;
using REngine.Core.Mathematics;
using REngine.RHI;
using REngine.RPI.RenderGraph.Annotations;
using REngine.RPI.RenderGraph.Nodes;

namespace REngine.RPI.RenderGraph
{
	[NodeTag("render-target")]
	public class RenderTargetNode : RenderGraphNode
	{
		public const string WidthPropertyKey = "width";
		public const string HeightPropertyKey = "height";
		public const string FormatPropertyKey = "format";

		public static readonly ulong WidthPropHash = Hash.Digest(WidthPropertyKey);
		public static readonly ulong HeightPropHash = Hash.Digest(HeightPropertyKey);
		public static readonly ulong FormatPropHash = Hash.Digest(FormatPropertyKey);

		private string pId = string.Empty;
		private uint pWidth;
		private uint pHeight;
		private TextureFormat pFormat = TextureFormat.Unknown;

		public RenderTargetNode() : base(nameof(RenderTargetNode))
		{
		}

		protected override void OnSetup(IDictionary<ulong, string> properties)
		{
			if (!properties.TryGetValue(VarNode.IdPropHash, out var pId))
				throw new RequiredNodePropertyException(VarNode.IdPropertyKey, nameof(RenderTargetNode));

			properties.TryGetValue(WidthPropHash, out var width);
			properties.TryGetValue(HeightPropHash, out var height);
			properties.TryGetValue(FormatPropHash, out var formatStr);

			if (!string.IsNullOrEmpty(width))
			{
				pWidth = uint.Parse(width);
				if (pWidth == 0)
					throw new FormatException("Width must be greater than 0");
			}

			if (!string.IsNullOrEmpty(height))
			{
				pHeight = uint.Parse(height);
				if (pHeight == 0)
					throw new FormatException("Height must be greater than 0");
			}


			if (string.IsNullOrEmpty(formatStr))
				return;

			pFormat = Enum.Parse<TextureFormat>(formatStr);
		}

		protected override void OnRun(IServiceProvider provider)
		{
			var renderer = provider.Get<IRenderer>();
			var rtMgr = provider.Get<IRenderTargetManager>();
			var varMgr = provider.Get<IVariableManager>();
			var @var = varMgr.GetVar(pId);

			if (pWidth == 0)
				pWidth = renderer.SwapChain?.Size.Width ?? 1;
			if (pHeight == 0)
				pHeight = renderer.SwapChain?.Size.Height ?? 1;

			var.Value = pFormat != TextureFormat.Unknown ? rtMgr.Allocate(pWidth, pHeight, pFormat) : rtMgr.Allocate(pWidth, pHeight);
			// Auto Remove after create RT
			Dispose();
		}
	}
}
