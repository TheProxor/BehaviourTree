using System.Collections.Generic;
using System.Linq;
using Swat.Game.BehaviourTree;
using Swat.Game.BehaviourTree.Blackboard;
using Swat.Game.BehaviourTree.Utils;
using UnityEngine;

namespace Editor.BehaviourTreeEditor.Utils.Extensions
{
	public static partial class Extensions
	{
		public static void CloneNodes(this BehaviourTreeView treeView, Node[] nodesToClone, Vector2 posOffset)
		{
			int nodesCount = nodesToClone.Length;
			var createdNodes = new List<Node>(nodesCount);
			var oldToNewGuidMap = new Dictionary<string, string>(nodesCount);
			var newToOldNodeMap = new Dictionary<Node, Node>(nodesCount);
			var serializer = treeView.serializer;
			
			serializer.BeginBatch();
			treeView.ClearSelection();
			
			// Create Nodes From Template
			foreach (var node in nodesToClone)
			{
				var newNode = serializer.CloneNode(node, node.Position + posOffset);
				var newNodeView = treeView.CreateNodeView(newNode);
				
				var oldGuid = node.Guid;
				var newGuid = newNode.Guid;
				
				createdNodes.Add(newNode);
				oldToNewGuidMap[oldGuid] = newGuid;
				newToOldNodeMap[newNode] = node;
					
				treeView.AddToSelection(newNodeView);
			}
			
			
			foreach (var node in createdNodes)
			{
				// Create Edges
				var nodeView = treeView.FindNodeView(node.Guid);
				var oldNode = newToOldNodeMap[node];
				var children = BtUtils.GetChildren(oldNode);

				foreach (var child in children)
				{
					var needToCreateChild = oldToNewGuidMap.TryGetValue(child.Guid, out var newNodeGuid);
					
					if(!needToCreateChild)
						continue;
					
					var childView = treeView.FindNodeView(newNodeGuid);

					serializer.AddChild(nodeView.Node, childView.Node);
					treeView.AddChild(nodeView, childView);
				}
				
				// Create Blackboard Entries
				treeView.CopyBlackboardKey(oldNode, node);
			}

			serializer.EndBatch();
		}

		public static void CloneNodesFast(this BehaviourTreeView treeView, NodeView[] nodeViewsToClone, Vector2 posOffset)
		{
			var oldToNewMapping = new Dictionary<string, string>();
			var nodesToCloneGuids = nodeViewsToClone.Select(n => n.Node.Guid);
			var serializer = treeView.serializer;
			
			serializer.BeginBatch();
			treeView.ClearSelection();
			
			// Gather all edges to create
			var edgesToCreate = new List<EdgeToCreate>();

			foreach (string nodeGuid in nodesToCloneGuids)
			{
				NodeView nodeView = treeView.FindNodeView(nodeGuid);

				if (nodeView != null && nodeView.Input != null)
				{
					NodeView nodesParent = nodeView.NodeParent;

					if (nodeViewsToClone.Contains(nodesParent))
					{
						var newEdge = new EdgeToCreate();
						newEdge.parent = nodesParent;
						newEdge.child = nodeView;
						edgesToCreate.Add(newEdge);
					}
				}
			}

			// Copy all nodes
			foreach (NodeView nodeView in nodeViewsToClone)
			{
				Node newNode = serializer.CloneNode(nodeView.Node, nodeView.Node.Position + posOffset);
				NodeView newNodeView = treeView.CreateNodeView(newNode);
				treeView.AddToSelection(newNodeView);

				// Map old to new guids so edges can be cloned.
				oldToNewMapping[nodeView.Node.Guid] = newNode.Guid;

				// Copy blackboard keys
				treeView.CopyBlackboardKey(nodeView.Node, newNode);
			}

			// Copy all edges
			foreach (EdgeToCreate edge in edgesToCreate)
			{
				NodeView oldParent = edge.parent;
				NodeView oldChild = edge.child;

				// These should already have been created.
				NodeView newParent = treeView.FindNodeView(oldToNewMapping[oldParent.Node.Guid]);
				NodeView newChild = treeView.FindNodeView(oldToNewMapping[oldChild.Node.Guid]);

				serializer.AddChild(newParent.Node, newChild.Node);
				treeView.AddChild(newParent, newChild);
			}

			serializer.EndBatch();
		}
		
		public static void CopyBlackboardKey(this BehaviourTreeView treeView, Node from, Node to)
		{
			var oldBlackboardKeyValuePair = from switch
			{
				ConditionKey conditionKey => conditionKey.Pair,
				Set set => set.Pair,
				_ => null
			};

			if (oldBlackboardKeyValuePair == null)
				return;
			
			var keyNameToCopy = oldBlackboardKeyValuePair.Key.Name;
			var valueToCopy = oldBlackboardKeyValuePair.Value;
			var existsKey = treeView.serializer.GetBlackboardKey(keyNameToCopy);

			if (existsKey == null
				|| valueToCopy == null
				|| existsKey.UnderlyingType != valueToCopy.UnderlyingType)
				return;
			
			var serializedNode = treeView.serializer.FindNode(to);

			var serializedKey = serializedNode.FindPropertyRelative("Pair.Key");
			var serializedValue = serializedNode.FindPropertyRelative("Pair.Value");

			serializedKey.managedReferenceValue = existsKey;
			serializedValue.managedReferenceValue = valueToCopy;
		}


		private class EdgeToCreate
		{
			public NodeView parent;
			public NodeView child;
		}
	}
}