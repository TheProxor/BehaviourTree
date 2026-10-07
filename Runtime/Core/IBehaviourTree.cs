using System.Collections.Generic;
using Swat.Game.BehaviourTree.Contexts;
using Swat.Game.BehaviourTree.Nested;
using UnityEngine;

namespace Swat.Game.BehaviourTree.Core
{
	public interface IBehaviourTree
	{
		string Name { get; }
		Object Object { get; }

		RootNode RootNode { get; }
		List<Node> Nodes { get; }
		List<NestedRoot> NestedRoots { get; }
		List<NestedCall> NestedCalls { get; }
		List<EventNode> EventNodes { get; }
		Blackboard.Blackboard Blackboard { get; }
		RuntimeTreeContext Context { get; }
		NodeState TreeState { get; }
		Vector3 ViewPosition { get; }
		Vector3 ViewScale { get; }

		NodeState UpdateTree();
		IBehaviourTree Clone();
		void Bind(RuntimeTreeContext context);
		void Dispose();
		int GetInstanceID();
		string GetFullPath();
	}
}