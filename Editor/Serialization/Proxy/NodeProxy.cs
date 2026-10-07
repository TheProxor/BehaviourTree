using System;
using Swat.Game.BehaviourTree;

namespace Editor.BehaviourTreeEditor.Serialization
{
	[Serializable]
	public class NodeProxy : Node
	{
		protected override void OnStart() { }

		protected override void OnStop() { }

		protected override NodeState OnUpdate() => NodeState.Disabled;
	}
}