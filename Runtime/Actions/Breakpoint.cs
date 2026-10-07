using System;
using UnityEngine;

namespace Swat.Game.BehaviourTree.Actions
{
	[UnityEngine.Scripting.APIUpdating.MovedFrom(true, sourceAssembly: "Game")]
	[Serializable]
	public class Breakpoint : ActionNode
	{
		protected override void OnStart()
		{
			Debug.Log("Trigging Breakpoint");
			Debug.Break();
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