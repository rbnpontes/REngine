using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using REngine.Core.DependencyInjection;
using REngine.Core.Threading;

namespace REngine.Core.WorldManagement
{
	public abstract class Behavior : Component
	{
		private readonly IExecutionPipeline pPipeline;
		private readonly IEngine pEngine;
		private readonly Action<IExecutionPipeline> pUpdateAction;
		protected Behavior(IServiceProvider provider)
		{
			pPipeline = provider.Get<IExecutionPipeline>();
			pEngine = provider.Get<IEngine>();

			pUpdateAction = Update;
			pPipeline.AddEvent(DefaultEvents.UpdateId, pUpdateAction);
		}

		protected override void OnDispose()
		{
			pPipeline.RemoveEvent(DefaultEvents.UpdateId, pUpdateAction);
		}

		private void Update(IExecutionPipeline _)
		{
			if(Owner is null || IsDisposed)
				return;

			OnUpdate((float)pEngine.DeltaTime);
		}
		protected virtual void OnUpdate(float deltaTime){}
	}
}
