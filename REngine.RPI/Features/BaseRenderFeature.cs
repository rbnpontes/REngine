using REngine.RHI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace REngine.RPI.Features
{
	public enum RenderFeatureState 
	{
		None=0,
		Setup,
		Compile,
		Execute
	}

	public class RenderFeatureException : Exception
	{
		public RenderFeatureException(string message) : base(message) { }
	}

	public abstract class BaseRenderFeature : IRenderFeature
	{
		private readonly object pSync = new();

		private IRenderer? pRenderer;

		private RenderFeatureState pCurrentState = RenderFeatureState.None;
		private bool pMustDispose = false;
		private bool pDisposed = false;

		public abstract bool IsDirty { get; protected set; }

		public bool IsDisposed
		{
			get
			{
				bool disposed = false;
				lock (pSync)
					disposed = pDisposed || pMustDispose;
				return disposed;
			}
		}
		
		public void Dispose()
		{
			lock(pSync)
			{
				if (pCurrentState == RenderFeatureState.None)
					OnDispose();
				else
					pMustDispose = true;
			}

			GC.SuppressFinalize(this);
		}

		private bool HandleDispose()
		{
			if(pMustDispose)
			{
				OnDispose();
				return true;
			}
			return false;
		}
		private bool IsDisposing()
		{
			bool disposed = false;
			lock (pSync)
				disposed = pDisposed;
			return disposed;
		}

		public IRenderFeature Setup(in RenderFeatureSetupInfo execInfo)
		{
			if (IsDisposing())
				return this;

			pRenderer = execInfo.Renderer;

			lock (pSync)
			{
				if(HandleDispose())
					return this;

				pCurrentState = RenderFeatureState.Setup;
			}
			
			OnSetup(execInfo);
			
			lock (pSync)
			{
				pCurrentState = RenderFeatureState.Compile;
				if (HandleDispose())
					return this;
			}
			return this;
		}

		public virtual IRenderFeature Compile(ICommandBuffer command)
		{
			if (IsDisposing())
				return this;

			lock (pSync)
			{
				if (HandleDispose())
					return this;

				AssertState(RenderFeatureState.Compile);
            }

			OnCompile(command);

			lock (pSync)
			{
				pCurrentState = RenderFeatureState.Execute;
				if (HandleDispose())
					return this;
			}

			return this;
		}

		public virtual IRenderFeature Execute(ICommandBuffer command)
		{
			if (IsDisposing())
				return this;

			lock (pSync)
			{
				if (HandleDispose())
					return this;

				if(pCurrentState == RenderFeatureState.None)
					pCurrentState = RenderFeatureState.Execute;
				AssertState(RenderFeatureState.Execute);
			}
			
			OnExecute(command);
			
			lock (pSync)
			{
				pCurrentState = RenderFeatureState.None;
				if (HandleDispose())
					return this;
			}
			return this;
		}

		public abstract IRenderFeature MarkAsDirty();

		protected void AssertState(RenderFeatureState expectedState)
		{
			if (pCurrentState != expectedState)
				throw new RenderFeatureException($"Invalid render feature state, state must be '{expectedState}'. State={pCurrentState}");
		}

		protected virtual void OnDispose() 
		{
			pDisposed = true;
		}
		protected abstract void OnSetup(in RenderFeatureSetupInfo setupInfo);
		protected virtual void OnCompile(ICommandBuffer command) { }
		protected abstract void OnExecute(ICommandBuffer command);

		protected virtual ITextureView? GetBackBuffer()
		{
			return pRenderer?.SwapChain?.ColorBuffer;
		}
		protected virtual ITextureView? GetDepthBuffer()
		{
			return pRenderer?.SwapChain?.DepthBuffer;
		}
	}

	public abstract class GraphicsRenderFeature : BaseRenderFeature, IGraphicsRenderFeature
	{
		public ITextureView? BackBuffer { get; set; }
		public ITextureView? DepthBuffer { get; set; }

		protected override ITextureView? GetBackBuffer()
		{
			if(BackBuffer != null)
				return BackBuffer;
			return base.GetBackBuffer();
		}
		protected override ITextureView? GetDepthBuffer()
		{
			if (DepthBuffer != null)
				return DepthBuffer;
			return base.GetDepthBuffer();
		}
	}
}
