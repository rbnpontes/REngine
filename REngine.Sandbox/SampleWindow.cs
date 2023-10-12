using REngine.Core;
using REngine.Core.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace REngine.Sandbox
{
	internal partial class SampleWindow
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

		public SampleWindow() : base() 
		{
		}

		public void Init()
		{
			CollectSamples();
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
					var sample = new SampleItem(type, attr?.SampleName ?? type.Name);
					pSamples.Add(sample);
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
		}

		public void EngineUpdate(IServiceProvider provider) 
		{
			pLastSample?.Update(provider);
		}
	}
}
