using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace REngine.Core
{
	public interface IMergeable<T> where T : class
	{
		public void Merge(T value);
	}
}
