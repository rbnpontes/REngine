using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace REngine.Assets
{
	public interface IAsset : IDisposable
	{
		public int Size { get; }
		public string Checksum { get; }

		/// <summary>
		/// Asset Name or Asset Path
		/// </summary>
		public string Name { get; set; }
		/// <summary>
		/// Save IAsset data into Stream.
		/// This method does not close Stream, you must close yourself
		/// </summary>
		/// <param name="stream"></param>
		/// <returns>A waitable task</returns>
		public Task Save(Stream stream);
		/// <summary>
		/// Load IAsset data from Stream
		/// This method does not close Stream, you must close yourself
		/// </summary>
		/// <param name="stream"></param>
		/// <returns>A readable task</returns>
		public Task Load(Stream stream);
	}
}
