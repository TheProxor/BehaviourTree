using System;
using UnityEngine;
using Random = UnityEngine.Random;


namespace Swat.Game.BehaviourTree.Actions
{
	[UnityEngine.Scripting.APIUpdating.MovedFrom(true, sourceAssembly: "Game")]
	[Serializable]
	public class RandomFailure : ActionNode
	{
		[Range(0, 1)] public float chanceOfFailure = 0.5f;

		protected override void OnStart()
		{
		}

		protected override void OnStop()
		{
		}


		protected override NodeState OnUpdate()
		{
			float value = Random.value;

			if (value > chanceOfFailure)
				return NodeState.Failure;

			return NodeState.Success;
		}
	}
}