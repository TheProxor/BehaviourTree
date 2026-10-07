using System;

namespace Swat.Game.BehaviourTree.Decorators
{
	[UnityEngine.Scripting.APIUpdating.MovedFrom(true, sourceAssembly: "Game")]
	[Serializable]
	public class Loop : DecoratorNode
	{
		public int LoopCount;

		private int completedLoops;


		protected override void OnStart()
		{
			completedLoops = 0;
		}


		protected override void OnStop()
		{
		}


		protected override NodeState OnUpdate()
		{
			switch (Child.UpdateNode())
			{
				case NodeState.Running:
					return NodeState.Running;

				case NodeState.Success:
				{
					completedLoops++;

					return completedLoops == LoopCount
						? NodeState.Success
						: NodeState.Running;
				}

				default:
					return NodeState.Running;
			}
		}
	}
}