using ImGuiNET;
using REngine.Core;
using REngine.Core.DependencyInjection;
using REngine.Core.Threading;
using REngine.RPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace REngine.Sandbox
{
	internal class SampleWindow
	{
		class SampleItem 
		{
			public Type Type { get; private set; }
			public string Name { get; private set; }

			public SampleItem(Type type, string name)
			{
				Type = type;
				Name = name;
			}
		}

		private readonly List<SampleItem> pSamples = new();

		private SampleItem? pLastSampleItem;
		private ISample? pLastSample;
		private IServiceProvider? pServiceProvider;
		private IWindow? pGameWindow;

		private RenderState? pRenderState;

		private int pSelectedItemIdx = -1;

		private void CollectSamples()
		{
			HashSet<string> addedSamples = new();
			Assembly
				.GetExecutingAssembly()
				.GetTypes()
				.Where(type => typeof(ISample).IsAssignableFrom(type) && !type.IsInterface)
				.ToList()
				.ForEach(type =>
				{
					SampleAttribute? attr = type.GetCustomAttribute<SampleAttribute>();
					if (addedSamples.Contains(attr?.SampleName ?? string.Empty))
						return;
					var sample = new SampleItem(type, attr?.SampleName ?? type.Name);
					pSamples.Add(sample);
					addedSamples.Add(sample.Name);
				});
		}

		private void LoadSample(SampleItem item)
		{
			if (item == pLastSampleItem)
				return;
			if (pServiceProvider is null)
				return;

			pLastSample?.Dispose();
			pLastSample = null;

			ISample? sample = Activator.CreateInstance(item.Type) as ISample;
			if (sample is null)
				throw new InvalidCastException("Invalid Sample Type. Sample Type must implement ISample interface");

			sample.Window = pServiceProvider.Get<IWindow>();
			sample.Load(pServiceProvider);

			pLastSampleItem = item;
			pLastSample = sample;
		}

		public void EngineStart(IServiceProvider provider)
		{
			CollectSamples();

			pServiceProvider = provider;
			SampleItem? item = pSamples.FirstOrDefault();
			if (item != null)
				LoadSample(item);

			provider.Get<IImGuiSystem>().OnGui += OnGui;

			pRenderState = provider.Get<RenderState>();
			pGameWindow = provider.Get<IWindow>();
		}

		public void EngineUpdate(IServiceProvider provider) 
		{
			pLastSample?.Update(provider);
		}

		public void EngineStop()
		{
			pLastSample?.Dispose();
			pLastSample = null;
		}

		private void OnGui(object? sender, EventArgs e)
		{
			if (ImGui.Begin("Samples"))
			{
				RenderSampleList();

				if (ImGui.Button("Load Sample") && pSelectedItemIdx != -1)
					LoadSample(pSamples[pSelectedItemIdx]);

				RenderToggleVsyncButton();
				RenderFullscreenButton();

				ImGui.End();
			}
		}

		private void RenderFullscreenButton()
		{
			if (pGameWindow is null)
				return;
			string label = pGameWindow.IsFullscreen ? "Exit Fullscreen" : "Fullscreen";

			if (ImGui.Button(label))
			{
				if (pGameWindow.IsFullscreen)
					pGameWindow.ExitFullscreen();
				else
					pGameWindow.Fullscreen();
			}
		}

		private void RenderToggleVsyncButton()
		{
			if(pRenderState is null) return;

			if (ImGui.Button(pRenderState.Vsync ? "Disable Vsync" : "Enable Vsync"))
				pRenderState.Vsync = !pRenderState.Vsync;
		}

		private void RenderSampleList()
		{
			for(int i =0; i< pSamples.Count; ++i)
			{
				if (ImGui.Selectable(pSamples[i].Name, pSelectedItemIdx == i))
					pSelectedItemIdx = i;
			}
		}
	}
}
