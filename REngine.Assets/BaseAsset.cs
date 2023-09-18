using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace REngine.Assets
{
	public abstract class BaseAsset : IAsset
	{
		public virtual string Name { get; set; } = string.Empty;

		public abstract int Size { get; }
		public abstract string Checksum { get; }

		public abstract Task Load(Stream stream);
		public abstract Task Save(Stream stream);
		public abstract void Dispose();
	}
}
