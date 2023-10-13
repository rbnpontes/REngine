using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace REngine.Core.Threading
{
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
	internal class NodeAttribute : Attribute
	{
		public string Tag { get; set; }

		public NodeAttribute(string tag) 
		{ 
			Tag = tag;
		}
	}
}
