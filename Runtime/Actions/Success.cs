using System;

namespace Swat.Game.BehaviourTree.Actions
{
	[UnityEngine.Scripting.APIUpdating.MovedFrom(true, sourceAssembly: "Game")]
	[Serializable]
	public class Success : ActionNode
	{
		protected override void OnStart()
		{
		}

		protected override void OnStop()
		{
		}


		protected override NodeState OnUpdate()
		{
			return NodeState.Success;
		}
	}
}