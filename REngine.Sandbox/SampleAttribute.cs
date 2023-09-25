using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace REngine.Sandbox
{
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
	public class SampleAttribute : Attribute
	{
		public string SampleName { get; private set; }
		public SampleAttribute(string sampleName)
		{
			SampleName = sampleName;
		}
	}
}
