using System;
using UnityEngine;

namespace Swat.Game.BehaviourTree
{
	[UnityEngine.Scripting.APIUpdating.MovedFrom(true, sourceAssembly: "Game")]
	[Serializable]
	public class RootNode : Node
	{
		[SerializeReference] [HideInInspector] public Node Child;

		protected override void OnStart()
		{
		}

		protected override void OnStop()
		{
		}


		protected override NodeState OnUpdate()
		{
			return Child?.UpdateNode() ?? NodeState.Failure;
		}
	}
}