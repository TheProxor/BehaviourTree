using System;
using UnityEngine;

namespace Swat.Game.BehaviourTree.Actions
{
	[UnityEngine.Scripting.APIUpdating.MovedFrom(true, sourceAssembly: "Game")]
	[Serializable]
	public class SetGObjectActive : ActionNode
	{
		public GameObject[] Objects;
		public bool Status;
		
		protected override void OnStart()
		{
			foreach (var gameObject in Objects)
			{
				gameObject.SetActive(Status);
			}
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