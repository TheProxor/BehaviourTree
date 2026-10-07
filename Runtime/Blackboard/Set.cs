using System;

namespace Swat.Game.BehaviourTree.Blackboard
{
	[UnityEngine.Scripting.APIUpdating.MovedFrom(true, sourceAssembly: "Game")]
	[Serializable]
	public class Set : ActionNode
	{
		public BlackboardKeyValuePair Pair;

		protected override void OnStart()
		{
		}

		protected override void OnStop()
		{
		}


		protected override NodeState OnUpdate()
		{
			Pair.WriteValue();

			return NodeState.Success;
		}
	}
}