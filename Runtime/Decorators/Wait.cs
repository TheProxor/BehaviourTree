using System;
using UnityEngine;
using Random = UnityEngine.Random;


namespace Swat.Game.BehaviourTree.Decorators
{
	[UnityEngine.Scripting.APIUpdating.MovedFrom(true, sourceAssembly: "Game")]
	[Serializable]
	public class Wait : DecoratorNode
	{
		public Vector2 DurationMinMax = new(1.0f, 1.0f);

		private float startTime;
		private float duration;


		protected override void OnStart()
		{
			startTime = Time.time;
			duration = Random.Range(DurationMinMax.x, DurationMinMax.y);
		}


		protected override void OnStop()
		{
		}


		protected override NodeState OnUpdate()
		{
			return duration > 0 && Time.time - startTime < duration
				? NodeState.Running
				: Child?.UpdateNode() ?? NodeState.Success;
		}
	}
}