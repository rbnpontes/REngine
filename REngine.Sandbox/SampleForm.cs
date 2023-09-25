using REngine.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace REngine.Sandbox
{
	public partial class SampleForm : Form
	{
		public class SampleItem
		{
			public string Name { get; set; }
			public Type SampleType { get; set; }

			public SampleItem(Type type)
			{
				SampleType = type;
				Name = type.GetCustomAttribute<SampleAttribute>()?.SampleName ?? type.Name;
			}

			public override string ToString()
			{
				return Name;
			}
		}

		private ISample? pSample;
		private Dictionary<string, SampleItem> pSamples = new Dictionary<string, SampleItem>();

		public Control GameContent { get => pSwapChainField; }
		public ISample? CurrentSample 
		{
			get => pSample;
			set
			{
				LoadSample(value);
			}
		}
		public IServiceProvider? ServiceProvider { get; set; }
		public IWindow? GameWindow { get; set; }

		public SampleForm()
		{
			InitializeComponent();
		}

		protected override void OnClosed(EventArgs e)
		{
			base.OnClosed(e);
			Dispose();
		}

		private void HandleLoad(object sender, EventArgs e)
		{
			// Collect Sample Types
			Assembly
				.GetExecutingAssembly()
				.GetTypes()
				.Where(type => typeof(ISample).IsAssignableFrom(type) && !type.IsInterface)
				.ToList()
				.ForEach(type =>
				{
					var sample = new SampleItem(type);
					pSamples.Add(sample.Name, sample);
					pSamplesList.Items.Add(sample.Name);
				});
		}

		public void EngineStart(IServiceProvider provider)
		{
			ServiceProvider = provider;
			if (pSamplesList.Items.Count > 0)
				LoadSampleItem((string)pSamplesList.Items[0]);
		}

		private void LoadSample(ISample sample)
		{
			if (ServiceProvider is null)
				return;

			pSample?.Dispose();
			pSample = sample;
			pSample.Window = GameWindow;
			pSample.Load(ServiceProvider);
		}

		private void LoadSampleItem(string sampleItem)
		{
			SampleItem? sample;
			if (!pSamples.TryGetValue(sampleItem, out sample))
				return;

			ISample? targetSample = Activator.CreateInstance(sample.SampleType) as ISample;
			if (targetSample != null)
				LoadSample(targetSample);
		}

		private void HandleLoadSample(object sender, EventArgs e)
		{
			var selectedItem = pSamplesList.SelectedItem as string;
			if (selectedItem is null)
				return;
			LoadSampleItem(selectedItem);
		}
	}
}
