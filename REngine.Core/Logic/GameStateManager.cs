using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using REngine.Core.IO;
using REngine.Core.Mathematics;
using REngine.Core.Reflection;
using REngine.Core.Threading;

namespace REngine.Core.Logic
{
	public interface IGameState
	{
		public string Name { get; }
		public Task OnStart();
		public void OnUpdate();
		public Task OnExit();
	}
	public sealed class GameStateManager
	{
		private enum StateTransactionStep
		{
			Idle = 0,
			Transact,
			Busy
		}
		private class StateTransaction
		{
			public IGameState? From { get; set; }
			public IGameState? To { get; set; }
			public StateTransactionStep Step { get; set; } = StateTransactionStep.Idle;
		}

		private readonly IServiceProvider pServiceProvider;
		private readonly Dictionary<ulong, IGameState> pGameStates = new();
		private readonly IExecutionPipeline pExecutionPipeline;
		private readonly StateTransaction pStateTransaction = new();
		private readonly ConcurrentQueue<Action> pStateTransitionQueue = new();
		private readonly ILogger<GameStateManager> pLogger;
		
		private IGameState? pState;
		private bool pDisposed;
		
		public GameStateManager(
			IServiceProvider serviceProvider,
			IExecutionPipeline executionPipeline,
			EngineEvents engineEvents,
			ILoggerFactory factory)
		{
			pServiceProvider = serviceProvider;
			pExecutionPipeline = executionPipeline;
			pLogger = factory.Build<GameStateManager>();
			
			engineEvents.OnBeforeStop.Once(HandleEngineStop);
			engineEvents.OnStart.Once(HandleEngineStart);
		}

		private async Task AsyncDispose()
		{
			await EngineGlobals.MainDispatcher.Yield();
			if(pDisposed) 
				return;

			if(pState != null)
				await pState.OnExit();
			pGameStates.Clear();

			pDisposed = true;

		}
		private async Task HandleEngineStop(object sender)
		{
			await AsyncDispose();
		}

		private async Task HandleEngineStart(object sender)
		{
			await EngineGlobals.MainDispatcher.Yield();
			pExecutionPipeline.AddEvent(DefaultEvents.UpdateId, ExecuteUpdateAction);
		}

		private void ExecuteUpdateAction(IExecutionPipeline executionPipeline)
		{
			var step = pStateTransaction.Step;
			switch (step)
			{
				case StateTransactionStep.Idle:
					RunIdleStep();
					break;
				case StateTransactionStep.Transact:
					RunTransactStep();
					break;
				case StateTransactionStep.Busy:
				default:
					// Do nothing. Just wait to step change
					break;
			}

			return;

			void RunIdleStep()
			{
				if (!pStateTransitionQueue.TryDequeue(out var action))
				{
					pState?.OnUpdate();
					return;
				}
				action();
				pLogger.Info(
					$"Start Transition: From => {pStateTransaction.From?.Name ?? "No Game State"} To => {pStateTransaction.To?.Name ?? "No Game State"}");
			}

			async void RunTransactStep()
			{
				pStateTransaction.Step = StateTransactionStep.Busy;

				if (pStateTransaction.From is not null)
				{
					pLogger.Info($"Exiting '{pStateTransaction.From.Name}'");
					await pStateTransaction.From.OnExit();
				}

				if (pStateTransaction.To is not null)
				{
					pLogger.Info($"Starting '{pStateTransaction.To.Name}'");
					await pStateTransaction.To.OnStart();
				}

				pState = pStateTransaction.To;
				pStateTransaction.From = pStateTransaction.To = null;
				pStateTransaction.Step = StateTransactionStep.Idle;

				pLogger.Info("Finish Transition");
			}
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
			pStateTransitionQueue.Enqueue(() =>
			{
				pStateTransaction.From = pState;
				pStateTransaction.To = pGameStates[stateId];
				pStateTransaction.Step = StateTransactionStep.Transact;
			});
			return this;
		}

		public GameStateManager Restart()
		{
			pStateTransitionQueue.Enqueue(() =>
			{
				if (pState is null)
					return;

				SetState(pState.Name);
			});
			return this;
		}
		public GameStateManager Stop()
		{
			pStateTransitionQueue.Enqueue(() =>
			{
				pStateTransaction.From = pState;
				pStateTransaction.To = null;
				pStateTransaction.Step = StateTransactionStep.Transact;
			});

			return this;
		}

		/// <summary>
		/// State Transitions is recorded on a Queue
		/// This queue is executed at Update Loop
		/// Clearing this queue is clearing any other requested state transitions.
		/// </summary>
		public GameStateManager ClearQueue()
		{
			pStateTransitionQueue.Clear();
			return this;
		}
	}
}
