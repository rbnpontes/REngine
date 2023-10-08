using Gtk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace REngine.Sandbox.Models
{
	internal class SampleItem 
	{
		public string Name { get; private set; }
		public Type SampleType { get; private set; }
			
		public SampleItem(string name, Type sampleType)
		{
			Name = name;
			SampleType = sampleType;
		}
	}

	internal class SamplesListStore : ListStore
	{
		private readonly Dictionary<string, SampleItem> pItems = new Dictionary<string, SampleItem>();

		public SamplesListStore() : base(new Type[] { typeof(string) })
		{ }

		public void Add(SampleItem sample)
		{
			if (pItems.ContainsKey(sample.Name))
				return;
			pItems[sample.Name] = sample;
			SetValue(
				Append(),
				0, sample.Name
			);
		}

		public SampleItem? GetSample(TreeIter iter)
		{
			string? value = GetValue(iter, 0).ToString();
			if(value is null)
				return null;

			pItems.TryGetValue(value, out var sample);
			return sample;
		}

		public static SampleItem? GetSelected(SamplesListStore store, TreeSelection selection)
		{
			selection.GetSelected(out TreeIter iter);
			return store.GetSample(iter);
		}
	}
}
