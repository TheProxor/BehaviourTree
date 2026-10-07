using System;
using Swat.Game.BehaviourTree.Utils;
using UnityEngine;
using UnityEngine.Serialization;

namespace Swat.Game.BehaviourTree.Nested
{
	[UnityEngine.Scripting.APIUpdating.MovedFrom(true, sourceAssembly: "Game")]
	[Serializable]
	public class NestedCall : NestedTree
	{
		[SerializeReference] [HideInInspector] public NestedRoot NestedRoot;

		public NestedId<NestedCall> NestedId;
		[FormerlySerializedAs("CombineSelectors")] public bool CombineSelectorsInNestedTree = true;

		protected override NodeState OnUpdate()
		{
			if (NestedRoot.State == NodeState.Disabled)
			{
				BtUtils.Traverse(NestedRoot, node =>
				{
					node.OnInit();
					node.State = NodeState.Running;
				});

				return NodeState.Running;
			}

			switch (NestedRoot.UpdateNode(this))
			{
				case NodeState.Running:
					return NodeState.Running;

				case NodeState.Failure:
					NestedRoot.ResetChildren();
					NestedRoot.SetResetFlagChildren();
					return NodeState.Failure;

				case NodeState.Success:
					NestedRoot.ResetChildren();
					NestedRoot.SetResetFlagChildren();
					return NodeState.Success;

				default:
					return NodeState.Failure;
			}
		}
	}
}