using System;
using System.Collections.Generic;
using Swat.Game.BehaviourTree.Blackboard;
using Swat.Game.BehaviourTree.Core;
using Swat.Game.BehaviourTree.Utils;
using UnityEngine;

namespace Swat.Game.BehaviourTree
{
	[UnityEngine.Scripting.APIUpdating.MovedFrom(true, sourceAssembly: "Game")]
	[Serializable]
	public class SubTree : ActionNode
	{
		[Tooltip("Behaviour tree asset to run as a subtree")]
		public BehaviourTree TreeAsset;

		[Tooltip("Override / set blackboard key values for sub tree")]
		public List<BlackboardKeyValuePair> BlackboardOverrides = new();
		
		private BehaviourTree _treeInstance;

		public IBehaviourTree RuntimeTree => _treeInstance;
		
		public override void Construct()
		{
			if (TreeAsset == null)
				return;
			
			_treeInstance = TreeAsset.Clone() as BehaviourTree;
			
			ApplyBlackboardOverrides();
			
			_treeInstance.Bind(Context);
		}

		protected override void OnStart()
		{
		}

		protected override void OnStop()
		{
		}

		public override void OnDispose()
		{
			if (_treeInstance == null) return;
			_treeInstance.Dispose();
			UnityEngine.Object.Destroy(_treeInstance);
			_treeInstance = null;
		}

		protected override NodeState OnUpdate()
		{
			if (_treeInstance == null)
				return NodeState.Failure;
			
			return _treeInstance.UpdateTree();
		}

		public bool ValidateTree()
		{
			if (TreeAsset == null)
				return false;

			bool isValid = !IsRecursive(TreeAsset, out var cyclePath);

			if (!isValid)
			{
				TreeAsset = null;
				Debug.LogError($"Failed to create recursive behaviour tree. Found cycle at: {cyclePath}");
			}

			return isValid;
		}

		public override void OnDrawGizmos(Transform transform)
		{
			_treeInstance?.DrawGizmos(transform);
		}

		private void ApplyBlackboardOverrides()
		{
			foreach (var pair in BlackboardOverrides)
			{
				// Find the key from the new behaviour tree instance
				var targetKey = _treeInstance.Blackboard.Find(pair.Key.Name);
				var sourceKey = pair.Value;
				
				if (targetKey != null && sourceKey != null)
					targetKey.CopyFrom(sourceKey);
			}
		}

		private bool IsRecursive(BehaviourTree tree, out string cycle)
		{
			// Check if any of the subtree nodes and their dependents form a circular reference, which will cause a stack overflow.
			var treeStack = new List<string>();
			var referencedTrees = new HashSet<IBehaviourTree>();

			bool cycleFound = false;
			string cyclePath = "";

			System.Action<Node> traverse = null;
			traverse = (node) =>
			{
				if (!cycleFound)
				{
					if (node is SubTree subtree && subtree.TreeAsset != null)
					{
						treeStack.Add(subtree.TreeAsset.name);
						if (referencedTrees.Contains(subtree.TreeAsset))
						{
							int index = 0;
							foreach (var tree in treeStack)
							{
								index++;
								if (index == treeStack.Count)
								{
									cyclePath += $"{tree}";
								}
								else
								{
									cyclePath += $"{tree} -> ";
								}
							}

							cycleFound = true;
						}
						else
						{
							referencedTrees.Add(subtree.TreeAsset);
							BtUtils.Traverse(subtree.TreeAsset.RootNode, traverse);
							referencedTrees.Remove(subtree.TreeAsset);
						}

						treeStack.RemoveAt(treeStack.Count - 1);
					}
				}
			};
			treeStack.Add(tree.name);

			referencedTrees.Add(tree);
			BtUtils.Traverse(tree.RootNode, traverse);
			referencedTrees.Remove(tree);

			treeStack.RemoveAt(treeStack.Count - 1);
			cycle = cyclePath;
			return cycleFound;
		}
	}
}
