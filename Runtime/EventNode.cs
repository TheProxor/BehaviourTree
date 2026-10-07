using System;
using UnityEngine;

namespace Swat.Game.BehaviourTree
{
	[UnityEngine.Scripting.APIUpdating.MovedFrom(true, sourceAssembly: "Game")]
	[Serializable]
	public abstract class EventNode : Node
	{
		[SerializeReference] [HideInInspector] public Node Child;


		private bool wasInvoked;


		protected override void OnStart()
		{
			wasInvoked = false;
		}


		protected override void OnStop()
		{
		}


		protected override NodeState OnUpdate()
		{
			if (wasInvoked) return Child.UpdateNode();

			return NodeState.Running;
		}


		protected void InvokeEvent()
		{
			wasInvoked = true;
		}
	}
}