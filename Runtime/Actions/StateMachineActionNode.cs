using System;

namespace Swat.Game.BehaviourTree.Actions
{
	[UnityEngine.Scripting.APIUpdating.MovedFrom(true, sourceAssembly: "Game")]
	[Serializable]
	public class StateMachineActionNode<T> : ActionNode where T : Enum
	{
		protected T CurrentState { get; private set; }

		protected void SwitchState(T state)
		{
			OnStateEnd();
			CurrentState = state;
			OnStateBegin();
		}


		protected virtual void OnStateBegin()
		{
		}

		protected virtual void OnStateEnd()
		{
		}

		protected override NodeState OnUpdate()
		{
			return NodeState.Success;
		}
	}
}