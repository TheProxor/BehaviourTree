using System;
using UnityEngine;

namespace Swat.Game.BehaviourTree.Nested
{
	[UnityEngine.Scripting.APIUpdating.MovedFrom(true, sourceAssembly: "Game")]
	[Serializable]
	public class NestedRoot : NestedTree
	{
		[SerializeReference] [HideInInspector] public Node Child;

		public NestedId<NestedRoot> NestedId;

		private NestedCall locker;

		public override void OnInit()
		{
			locker = null;
		}


		public NodeState UpdateNode(NestedCall locker)
		{
			this.locker ??= locker;

			if (this.locker != locker)
				return NodeState.Failure;

			NodeState nextState = UpdateNode();

			if (nextState == NodeState.Running)
				return NodeState.Running;

			this.locker = null;
			return nextState;
		}


		protected override NodeState OnUpdate()
		{
			return Child?.UpdateNode() ?? NodeState.Failure;
		}
	}
}