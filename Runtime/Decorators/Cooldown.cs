using System;
using UnityEngine;
using Random = UnityEngine.Random;
using Time = UnityEngine.Time;


namespace Swat.Game.BehaviourTree.Decorators
{
	[UnityEngine.Scripting.APIUpdating.MovedFrom(true, sourceAssembly: "Game")]
	[Serializable]
	public class Cooldown : AbortableNode
	{
		public Vector2 CooldownMinMax = new Vector2(4444f, 4444f);
		public LaunchMethod CooldownLaunchMethod = LaunchMethod.OnSuccess;
		
		private float _startCooldownTime;
		private float _cooldownTime;

		protected override NodeState OnUpdate()
		{
			if (Time.time - _startCooldownTime >= _cooldownTime)
			{
				NodeState childState = Child.UpdateNode();

				if (childState == NodeState.Success && CooldownLaunchMethod == LaunchMethod.OnSuccess)
					LaunchCooldown();

				return childState;
			}

			return AbortProcess();
		}

		protected override void OnStop()
		{
			if(CooldownLaunchMethod == LaunchMethod.Always) 
				LaunchCooldown();
		}

		private void LaunchCooldown()
		{
			_startCooldownTime = Time.time;
			_cooldownTime = Random.Range(CooldownMinMax.x, CooldownMinMax.y);
		}

		public enum LaunchMethod : byte
		{
			Always = 0,
			OnSuccess = 1
		}
	}
}