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
		private bool pDisposed;

		public abstract bool IsDirty { get; protected set; }

		public bool IsDisposed
		{
			get
			{
				bool disposed;
				lock (pSync)
					disposed = pDisposed;
				return disposed;
			}
		}
		
		public void Dispose()
		{
			if (IsDisposed)
				return;
			lock(pSync)
			{
				if (pCurrentState != RenderFeatureState.None)
					throw new InvalidOperationException("Can´t dispose Render Feature that is on execution. You must remove from Renderer first before dispose");
			}
			
			OnDispose();
			GC.SuppressFinalize(this);
		}
		
		public IRenderFeature Setup(in RenderFeatureSetupInfo execInfo)
		{
			if (IsDisposed)
				return this;
			
			lock (pSync)
				pCurrentState = RenderFeatureState.Setup;
			pRenderer = execInfo.Renderer;
			OnSetup(execInfo);

			lock (pSync)
				pCurrentState = RenderFeatureState.None;
			return this;
		}

		public virtual IRenderFeature Compile(ICommandBuffer command)
		{
			if (IsDisposed)
				return this;
			lock (pSync)
				pCurrentState = RenderFeatureState.Compile;
			OnCompile(command);

			lock (pSync)
				pCurrentState = RenderFeatureState.None;
			return this;
		}

		public virtual IRenderFeature Execute(ICommandBuffer command)
		{
			if (IsDisposed)
				return this;
			
			lock (pSync)
				pCurrentState = RenderFeatureState.Execute;
			
			OnExecute(command);

			lock (pSync)
				pCurrentState = RenderFeatureState.None;
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
