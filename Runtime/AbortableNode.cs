using System;
using Swat.Game.BehaviourTree.Data;

namespace Swat.Game.BehaviourTree
{
	[UnityEngine.Scripting.APIUpdating.MovedFrom(true, sourceAssembly: "Game")]
	[Serializable]
	public abstract class AbortableNode : DecoratorNode
	{
		public AbortMethod AbortType = AbortMethod.None;


		protected NodeState AbortProcess()
		{
			switch (AbortType)
			{
				case AbortMethod.Self:
				case AbortMethod.Both:
				{
					Child.AbortChildren();
					return NodeState.Failure;
				}

				default:
					return NodeState.Failure;
			}
		}
	}
}