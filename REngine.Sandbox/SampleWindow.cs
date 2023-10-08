using REngine.Core;
using REngine.Core.DependencyInjection;
using REngine.Sandbox.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace REngine.Sandbox
{
	internal partial class SampleWindow : Gtk.Window
	{
		private SampleItem? pLastSampleItem;
		private ISample? pLastSample;
		private IServiceProvider? pServiceProvider;

		public SampleWindow() : base(Gtk.WindowType.Toplevel) 
		{
			InitializeComponents();
		}

		private void CollectSamples()
		{
			Assembly
				.GetExecutingAssembly()
				.GetTypes()
				.Where(type => typeof(ISample).IsAssignableFrom(type) && !type.IsInterface)
				.ToList()
				.ForEach(type =>
				{
					SampleAttribute? attr = type.GetCustomAttribute<SampleAttribute>();
					var sample = new SampleItem(attr?.SampleName ?? type.Name, type);
					pStore.Add(sample);
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

			ISample? sample = Activator.CreateInstance(item.SampleType) as ISample;
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
			pStore.GetIterFirst(out Gtk.TreeIter firstIter);
			SampleItem? item = pStore.GetSample(firstIter);
			if (item != null)
				LoadSample(item);
		}

		public void EngineUpdate(IServiceProvider provider) 
		{
			pLastSample?.Update(provider);
		}
	}
}
