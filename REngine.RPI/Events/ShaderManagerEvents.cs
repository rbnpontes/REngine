using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using REngine.Core;

namespace REngine.RPI.Events
{
	public sealed class ShaderManagerEvents
	{
		public readonly AsyncEventEmitter OnReady = new();
		public readonly AsyncEventEmitter OnDispose = new();
		public readonly AsyncEventEmitter OnDisposed = new();

		public void ExecuteReady(IShaderManager shaderManager) => OnReady.Invoke(shaderManager);

		public void ExecuteDispose(IShaderManager shaderManager) => OnDispose.Invoke(shaderManager);

		public void ExecuteDisposed(IShaderManager shaderManager) => OnDisposed.Invoke(shaderManager);
	}
}
