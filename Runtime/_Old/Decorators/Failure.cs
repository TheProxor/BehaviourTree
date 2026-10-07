namespace Swat.Game.BehaviourTree.Decorators
{
	public class Failure : DecoratorNode
	{
		protected override void OnStart()
		{
		}

		protected override void OnStop()
		{
		}


		protected override NodeState OnUpdate()
		{
			NodeState state = Child.UpdateNode();

			if (state == NodeState.Success)
				return NodeState.Failure;

			return state;
		}
	}
}