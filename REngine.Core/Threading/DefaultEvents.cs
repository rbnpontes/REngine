using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace REngine.Core.Threading
{
	public static class DefaultEvents
	{
		public const string FrameId = "@engine/frame";
		// Update Events
		public const string UpdateBeginId = "@engine/update_begin";
		public const string UpdateId = "@engine/update";
		public const string UpdateEndId = "@engine/update_end";
		// Sprite Batch Events
		public const string SpriteBatchTaskId = "@spritebatch/task";
		public const string SpriteBatchDrawId = "@spritebatch/draw";
		public const string SpriteBatchBuildTexId = "@spritebatch/build_tex";
		public const string SpriteBatchOrganizeInstanceId = "@spritebatch/organize_instance";
		// Windows Manager Events
		public const string WindowsUpdateId = "@wndmgr/update";
		public const string WindowsInvalidateId = "@wndmgr/invalidate";
		// Render Events
		public const string RenderBeginId = "@render/begin";
		public const string RenderId = "@render";
		public const string RenderEndId = "@render/end";
		public const string RenderPrepareId = "@render/prepare";
		public const string SwapChainPresentId = "@render/swapchain_present";
		// ImGui Events
		public const string ImGuiDrawId = "@imgui/draw";
		// Scene Management Events
		public const string SceneMgtUpdateCameras = "@scenemgt/update_cameras";
	}
}
