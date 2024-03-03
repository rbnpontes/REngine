// TODO: fix imgui system on web
//using ImGuiNET;
using REngine.Core;
using REngine.Core.DependencyInjection;
using REngine.RPI;
using System.Reflection;
using ImGuiNET;
using REngine.Core.Reflection;
using REngine.Core.Resources;
using REngine.Sandbox.BaseSample;

namespace REngine.Sandbox.Samples
{
	internal class SampleWindow
	{
		private class SampleItem(Type type, string name)
		{
			public Type Type { get; private set; } = type;
			public string Name { get; private set; } = name;
		}

		private readonly List<SampleItem> pSamples = [];
		private readonly object pSync = new();
		
		private SampleItem? pLastSampleItem;
		private ISample? pLastSample;
		private bool pSampleReady;
		
		private IServiceProvider? pServiceProvider;
		private IWindow? pGameWindow;

		private RenderState? pRenderState;

		private int pSelectedItemIdx = -1;

		private void CollectSamples()
		{
			HashSet<string> addedSamples = [];
			CollectSamples(AppDomain.CurrentDomain.GetAssemblies(), addedSamples);
		}

		private void CollectSamples(IEnumerable<Assembly> assemblies, ISet<string> addedSamples)
		{
			foreach (var assembly in assemblies)
				CollectSamples(assembly, addedSamples);
		}

		private void CollectSamples(Assembly assembly, ISet<string> addedSamples)
		{
			assembly.GetTypes()
				.Where(type => typeof(ISample).IsAssignableFrom(type) && !type.IsInterface)
				.ToList()
				.ForEach(type =>
				{
					var attr = type.GetCustomAttribute<SampleAttribute>();
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

			lock (pSync)
			{
				pLastSample?.Dispose();
				pLastSample = null;

				if (ActivatorExtended.CreateInstance(pServiceProvider, item.Type) is not ISample sample)
					throw new InvalidCastException("Invalid Sample Type. Sample Type must implement ISample interface");

				sample.Window = pServiceProvider.Get<IWindow>();
				
				// Force Assets Unload
				pServiceProvider.Get<IAssetManager>().UnloadAssets();
				
				pLastSampleItem = item;
				pLastSample = sample;

				pSampleReady = false;
			}
		}

		public void EngineStart(IServiceProvider provider)
		{
			CollectSamples();

			pServiceProvider = provider;
			var item = pSamples.FirstOrDefault();
			if (item != null)
				LoadSample(item);

			provider.Get<IImGuiSystem>().OnGui += OnGui;

			pRenderState = provider.Get<RenderState>();
			pGameWindow = provider.Get<IWindow>();
		}

		public void EngineUpdate(IServiceProvider provider) 
		{
			if (pIsLoadingSample)
				return;
			lock (pSync)
			{
				if (pSampleReady)
					pLastSample?.Update(provider);
				else
				{
					pIsLoadingSample = true;
					Task.Run(() => HandleLoadSample(pLastSample));
				}
			}
		}

		private bool pIsLoadingSample;
		private async Task HandleLoadSample(ISample sample)
		{
			Monitor.Enter(pSync);
			await sample.Load(pServiceProvider);
			pSampleReady = true;
			Monitor.Exit(pSync);
			pIsLoadingSample = false;
		}

		public void EngineStop()
		{
			pLastSample?.Dispose();
			pLastSample = null;
		}

		private bool pOpenedWindow = true;
		private void OnGui(object? sender, EventArgs e)
		{
			if (ImGui.Begin("Samples", ref pOpenedWindow))
			{
				RenderSampleList();

				if (ImGui.Button("Load Sample") && pSelectedItemIdx != -1)
					LoadSample(pSamples[pSelectedItemIdx]);

				RenderToggleVsyncButton();
				RenderFullscreenButton();

			}
			ImGui.End();
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
