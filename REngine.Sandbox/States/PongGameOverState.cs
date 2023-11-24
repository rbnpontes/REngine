using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using REngine.Core.Logic;
using REngine.Core.WorldManagement;

namespace REngine.Sandbox.States
{
	internal class PongGameOverState : IGameState
	{
		public string Name => nameof(PongGameOverState);

		public PongGameOverState(
			EntityManager entityManager,
			GameStateManager gameStateManager)
		{

		}

		public void OnStart()
		{

		}

		public void OnUpdate()
		{
		}

		public void OnExit()
		{
		}
	}
}
