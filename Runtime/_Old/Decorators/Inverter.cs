namespace Swat.Game.BehaviourTree.Decorators
{
	public class Inverter : DecoratorNode
	{
		protected override void OnStart()
		{
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

				case NodeState.Failure:
					return NodeState.Success;

				case NodeState.Success:
					return NodeState.Failure;
			}

			return NodeState.Failure;
		}
	}
}