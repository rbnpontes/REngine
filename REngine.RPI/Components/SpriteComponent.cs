using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using REngine.Core;
using REngine.Core.DependencyInjection;
using REngine.Core.IO;
using REngine.Core.Resources;
using REngine.Core.Serialization;
using REngine.Core.Storage;
using REngine.Core.Threading;
using REngine.Core.WorldManagement;

namespace REngine.RPI.Components
{
	public abstract class BaseSpriteComponent<T> : Component
	{
		private readonly IExecutionPipeline pExecutionPipeline;
		private readonly Action<IExecutionPipeline> pBeginUpdate;
		private readonly Transform2DSystem pTransformSystem;

		private Transform2DSnapshot pLastSnapshot = new();
		private Transform2D? pTransform;
		[SerializationIgnore]
		public Transform2D Transform
		{
			get
			{
				if (Owner is null)
					throw new NullReferenceException($"{nameof(T)} must be attached to an Entity");
				if (pTransform is not null && !pTransform.IsDisposed) 
					return pTransform;

				pTransform = Owner.GetComponent<Transform2D>();
				if (pTransform is not null) 
					return pTransform;
				
				pTransform = pTransformSystem.CreateTransform();
				Owner.AddComponent(pTransform);
				return pTransform;
			}
		}
		
		internal BaseSpriteComponent(IServiceProvider provider)
		{
			pExecutionPipeline = provider.Get<IExecutionPipeline>();
			pTransformSystem = provider.Get<Transform2DSystem>();
			pBeginUpdate = BeginUpdate;
		}

		private void BeginUpdate(IExecutionPipeline pipeline)
		{
			if (IsDisposed)
				return;
			Transform.GetSnapshot(out var currSnapshot);
			if (!pLastSnapshot.Equals(currSnapshot))
			{
				pLastSnapshot = currSnapshot;
				OnChangeTransform();
			}
			
			OnUpdate();
		}

		public override void OnSetup()
		{
			BindEvents();
		}

		protected abstract void OnUpdate();
		protected abstract void OnChangeTransform();
		protected override void OnDispose()
		{
			pExecutionPipeline.RemoveEvent(DefaultEvents.UpdateBeginId, pBeginUpdate);
		}
		protected override void OnChangeVisibility(bool value)
		{
			BindEvents();
		}

		private void BindEvents()
		{
			if (Enabled)
				pExecutionPipeline.AddEvent(DefaultEvents.UpdateBeginId, pBeginUpdate);
			else
				pExecutionPipeline.RemoveEvent(DefaultEvents.UpdateBeginId, pBeginUpdate);
		}
	}
}
