using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#if RENGINE_IMGUI
namespace REngine.RPI
{
	public interface IImGuiSystem
	{
		public IGraphicsRenderFeature Feature { get; }
		public event EventHandler? OnGui;
		public void SetFontScale(float fontScale);
		/// <summary>
		/// Scale all ImGui Styles to desired scale
		/// This is a lossless process and system does not
		/// store last scale, if you change ImGui, all sizes will be changed
		/// If you call this method twice, then Scale will be applied twice
		/// </summary>
		/// <param name="scale"></param>
		public void ScaleUi(float scale);

		public IGraphicsRenderFeature CreateRenderFeature();
	}
}
#endif