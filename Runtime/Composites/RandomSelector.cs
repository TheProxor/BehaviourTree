using System;
using Random = UnityEngine.Random;


namespace Swat.Game.BehaviourTree.Composites
{
	[UnityEngine.Scripting.APIUpdating.MovedFrom(true, sourceAssembly: "Game")]
	[Serializable]
	public class RandomSelector : CompositeNode
	{
		protected int current;


		protected override void OnStart()
		{
			current = Random.Range(0, Children.Count);
		}


		protected override void OnStop()
		{
		}


		protected override NodeState OnUpdate()
		{
			Node child = Children[current];
			return child.UpdateNode();
		}
	}
}