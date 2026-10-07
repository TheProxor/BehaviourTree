namespace Swat.Game.BehaviourTree.Composites
{
	public class Any : CompositeNode
	{
		protected override void OnStart()
		{
		}


		protected override void OnStop()
		{
		}


		protected override NodeState OnUpdate()
		{
			for (int i = 0; i < Children.Count; ++i)
			{
				Node child = Children[i];

				switch (child.UpdateNode())
				{
					case NodeState.Success:
						return NodeState.Success;

					case NodeState.Running:
					case NodeState.Failure:
						continue;
				}
			}

			return NodeState.Running;
		}
	}
}