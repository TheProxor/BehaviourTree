using System;
using UnityEngine;

namespace Swat.Game.BehaviourTree
{
	[UnityEngine.Scripting.APIUpdating.MovedFrom(true, sourceAssembly: "Game")]
	[Serializable]
	public abstract class DecoratorNode : Node
	{
		[SerializeReference] [HideInInspector] public Node Child;

		protected override void OnStart()
		{
		}

		protected override void OnStop()
		{
		}
	}
}