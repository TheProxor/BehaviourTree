using System;
using Swat.Game.BehaviourTree.Data;

namespace Swat.Game.BehaviourTree.Composites
{
	[UnityEngine.Scripting.APIUpdating.MovedFrom(true, sourceAssembly: "Game")]
	[Serializable]
	public class Sequencer : CompositeNode
	{
		public bool AbortOnAnyChildFailure;
		
		protected int current;

		protected override void OnStart()
		{
			current = 0;
		}

		protected override void OnStop()
		{
		}

		protected override NodeState OnUpdate()
		{
			for (int i = current; i < Children.Count; ++i)
			{
				current = i;
				Node child = Children[current];

				switch (child.UpdateNode())
				{
					case NodeState.Running:
						return NodeState.Running;

					case NodeState.Failure:
					{
						if (child is AbortableNode { AbortType: AbortMethod.Both or AbortMethod.LowerPriority })
							return NodeState.Failure;

						if(AbortOnAnyChildFailure)
							return NodeState.Failure;
						
						continue;
					}

					case NodeState.Success:
						continue;
				}
			}

			return NodeState.Success;
		}
	}
}