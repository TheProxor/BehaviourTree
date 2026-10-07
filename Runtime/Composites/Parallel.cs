using System;
using System.Collections.Generic;

namespace Swat.Game.BehaviourTree.Composites
{
	[UnityEngine.Scripting.APIUpdating.MovedFrom(true, sourceAssembly: "Game")]
	[Serializable]
	public class Parallel : CompositeNode
	{
		public FinishMode FinishType = FinishMode.None;
		public bool AbortWhenAnyFailure = true;

		private readonly List<NodeState> childrenLeftToExecute = new();


		protected override void OnStart()
		{
			childrenLeftToExecute.Clear();

			Children.ForEach(a => { childrenLeftToExecute.Add(NodeState.Running); });
		}


		protected override void OnStop()
		{
		}


		protected override NodeState OnUpdate()
		{
			bool stillRunning = false;

			for (int i = 0; i < childrenLeftToExecute.Count; ++i)
			{
				if (childrenLeftToExecute[i] != NodeState.Running
					&& childrenLeftToExecute[i] != NodeState.Failure)
					continue;

				NodeState status = Children[i].UpdateNode();

				if (status == NodeState.Failure && AbortWhenAnyFailure)
				{
					AbortRunningChildren();
					return NodeState.Failure;
				}

				if (status == NodeState.Running)
					stillRunning = true;

				childrenLeftToExecute[i] = status;
			}

			switch (FinishType)
			{
				case FinishMode.Immediate:
				{
					Node mainNode = Children[0];

					if (mainNode.State == NodeState.Success)
					{
						AbortRunningChildren();
						return NodeState.Success;
					}

					break;
				}
			}

			return stillRunning ? NodeState.Running : NodeState.Success;
		}


		private void AbortRunningChildren()
		{
			for (int i = 0; i < childrenLeftToExecute.Count; ++i)
			{
				if (childrenLeftToExecute[i] == NodeState.Running)
					Children[i].AbortChildren();
			}
		}


		public enum FinishMode : byte
		{
			None = 0,
			Immediate = 1
		}
	}
}