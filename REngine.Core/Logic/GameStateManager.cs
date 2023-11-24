using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using REngine.Core.Mathematics;
using REngine.Core.Reflection;
using REngine.Core.Threading;

namespace REngine.Core.Logic
{
	public interface IGameState
	{
		public string Name { get; }
		public void OnStart();
		public void OnUpdate();
		public void OnExit();
	}
	public sealed class GameStateManager : IDisposable
	{
		private class StateTransaction
		{
			public IGameState? From { get; set; }
			public IGameState? To { get; set; }

			public bool NeedsTransaction => !(From == null && To == null);
		}

		private readonly object pSync = new();
		private readonly IServiceProvider pServiceProvider;
		private readonly Dictionary<ulong, IGameState> pGameStates = new();
		private readonly EngineEvents pEngineEvents;
		private readonly IExecutionPipeline pExecutionPipeline;
		private readonly StateTransaction pStateTransaction = new();

		private IGameState? pState;
		private bool pDisposed;
		public GameStateManager(
			IServiceProvider serviceProvider,
			IExecutionPipeline executionPipeline,
			EngineEvents engineEvents)
		{
			pEngineEvents = engineEvents;
			pServiceProvider = serviceProvider;
			pExecutionPipeline = executionPipeline;
			engineEvents.OnBeforeStop += HandleEngineStop;
			engineEvents.OnStart += HandleEngineStart;
		}

		public void Dispose()
		{
			if(pDisposed) 
				return;

			pState?.OnExit();
			pGameStates.Clear();

			pDisposed = true;

		}
		private void HandleEngineStop(object? sender, EventArgs e)
		{
			pEngineEvents.OnBeforeStop -= HandleEngineStop;
			Dispose();
		}

		private void HandleEngineStart(object? sender, EventArgs e)
		{
			pExecutionPipeline.AddEvent(DefaultEvents.UpdateBeginId, ExecuteBeginUpdate);
			pExecutionPipeline.AddEvent(DefaultEvents.UpdateId, ExecuteUpdateAction);
		}

		private void ExecuteBeginUpdate(IExecutionPipeline executionPipeline)
		{
			lock (pSync)
			{
				if (!pStateTransaction.NeedsTransaction)
					return;

				pStateTransaction.From?.OnExit();
				pStateTransaction.To?.OnStart();
				pState = pStateTransaction.To;

				pStateTransaction.From = pStateTransaction.To = null;
			}
		}

		private void ExecuteUpdateAction(IExecutionPipeline executionPipeline)
		{
			lock(pSync)
				pState?.OnUpdate();
		}


		public GameStateManager RegisterState<T>() where T : IGameState
		{
			IGameState? state = ActivatorExtended.CreateInstance<T>(pServiceProvider);
			if (state is null)
				throw new NullReferenceException($"Could not possible to create {nameof(T)}");

			pGameStates.Add(Hash.Digest(state.Name), state);
			return this;
		}

		public GameStateManager ClearStates()
		{
			foreach (var pair in pGameStates)
				pair.Value.OnExit();
			pGameStates.Clear();
			return this;
		}

		public GameStateManager SetState(string stateName)
		{
			return SetState(Hash.Digest(stateName));
		}

		public GameStateManager SetState(ulong stateId)
		{
			lock (pSync)
			{
				pStateTransaction.From = pState;
				pStateTransaction.To = pGameStates[stateId];
			}
			return this;
		}

		public GameStateManager Restart()
		{
			IGameState? state;
			lock (pSync)
				state = pState;

			if (state is null)
				return this;
			SetState(state.Name);
			return this;
		}
		public GameStateManager Stop()
		{
			lock (pSync)
			{
				pState?.OnExit();
				pState = null;
			}

			return this;
		}
	}
}
