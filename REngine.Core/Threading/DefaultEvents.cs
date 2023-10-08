using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace REngine.Core.Threading
{
	public static class DefaultEvents
	{
		public const string FrameId = "frame";
		public const string UpdateBeginId = "update_begin";
		public const string SpriteBatchTaskId = "spritebatch_task";
		public const string SpriteBatchDrawId = "spritebatch_draw";
		public const string SpriteBatchBuildTexId = "spritebatch_build_tex";
		public const string SpriteBatchOrganizeInstanceId = "spritebatch_organize_instance";
		public const string UpdateSwapChainWndTaskId = "update_swapchain_wnd_task";
		public const string SwapChainPresentId = "swapchain_present";
		public const string WindowsUpdateId = "windows_update";
		public const string UpdateId = "update";
		public const string RenderBeginId = "render_begin";
		public const string RenderId = "render";
		public const string RenderEndId = "render_end";
		public const string WindowsInvalidateId = "windows_invalidate";
		public const string ImGuiDrawId = "imgui_draw";
	}
}
