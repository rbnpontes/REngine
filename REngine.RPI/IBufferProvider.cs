using REngine.RHI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace REngine.RPI
{
	public interface IBufferProvider
	{
		/// <summary>
		/// Return a Constant Buffer by Group Frequency type
		/// Constant Buffer from IBufferProvider must not be disposed
		/// BufferProvider cares this.
		/// </summary>
		/// <param name="groupType"></param>
		/// <returns></returns>
		public IBuffer GetBuffer(BufferGroupType groupType);
		/// <summary>
		/// Return a Instancing Buffer by specific size
		/// This method can return a buffer that has size bigger than is need
		/// </summary>
		/// <param name="bufferSize"></param>
		/// <returns></returns>
		public IBuffer GetInstancingBuffer(ulong bufferSize);
		/// <summary>
		/// Free allocated instancing buffers
		/// </summary>
		/// <returns></returns>
		public IBufferProvider DisposeInstancingBuffers();
	}
}
