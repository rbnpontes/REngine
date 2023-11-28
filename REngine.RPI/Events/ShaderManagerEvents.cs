using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace REngine.RPI.Events
{
	public sealed class ShaderManagerEvents
	{
		public event EventHandler? OnReady;
		public event EventHandler? OnDispose;
		public event EventHandler? OnDisposed;

		public void ExecuteReady(IShaderManager shaderManager)
		{
			OnReady?.Invoke(shaderManager, EventArgs.Empty);
		}

		public void ExecuteDispose(IShaderManager shaderManager)
		{
			OnDispose?.Invoke(shaderManager, EventArgs.Empty);
		}

		public void ExecuteDisposed(IShaderManager shaderManager)
		{
			OnDisposed?.Invoke(shaderManager, EventArgs.Empty);
		}
	}
}
