namespace Swat.Game.BehaviourTree.Decorators
{
	public class Succeed : DecoratorNode
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

			if (state == NodeState.Failure)
				return NodeState.Success;

			return state;
		}
	}
}