using System;
using UnityEngine;

namespace Swat.Game.BehaviourTree.Actions
{
	[UnityEngine.Scripting.APIUpdating.MovedFrom(true, sourceAssembly: "Game")]
	[Serializable]
	public class Log : ActionNode
	{
		protected override void OnStart()
		{
		}

		protected override void OnStop()
		{
		}


		protected override NodeState OnUpdate()
		{
			Debug.Log($"{Description}");
			return NodeState.Success;
		}
	}
}