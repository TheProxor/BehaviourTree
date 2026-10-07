using System;
using UnityEngine;
using Random = UnityEngine.Random;


namespace Swat.Game.BehaviourTree.Decorators
{
	[UnityEngine.Scripting.APIUpdating.MovedFrom(true, sourceAssembly: "Game")]
	[Serializable]
	public class Chance : DecoratorNode
	{
		[Range(0, 1)] public float Value;

		private bool isSuccess;

		protected override void OnStart()
		{
			isSuccess = Random.value < Value;
		}

		protected override NodeState OnUpdate()
		{
			return isSuccess
				? Child.UpdateNode()
				: NodeState.Failure;
		}
	}
}