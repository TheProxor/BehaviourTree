using System;
using System.Collections.Generic;
using System.Linq;
using Swat.Game.BehaviourTree.Composites;
using Swat.Game.BehaviourTree.Utils;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace Swat.Game.BehaviourTree
{
	// This is a WIP class for generating behaviour trees dynamically at runtime.
	public class BehaviourTreeBuilder
	{
		public BehaviourTreeMono tree;


		public BehaviourTreeBuilder(string treeName)
		{
			tree = new GameObject().AddComponent<BehaviourTreeMono>();
			tree.name = treeName;
		}


		public T CreateNode<T>(params object[] args) where T : Node
		{
			var newNode = Activator.CreateInstance(typeof(T), args) as T;
			newNode.Guid = Guid.NewGuid().ToString();
			tree.Nodes.Add(newNode);
			return newNode;
		}


		public void Selector(params Node[] nodes)
		{
			var selector = CreateNode<Selector>();
			selector.Children.AddRange(nodes);
			tree.Nodes.AddRange(nodes);
		}


#if UNITY_EDITOR
		public void Save(string path)
		{
			if (tree)
			{
				LayoutNodes();
				AssetDatabase.CreateAsset(tree, $"{path}/{tree.name}.asset");
				AssetDatabase.SaveAssets();
			}
		}
#endif


		private void LayoutNodes()
		{
			CalculatePositions(tree.RootNode, tree.RootNode.Position, 80.0f);
		}


		private float CalculatePositions(Node node, Vector2 position, float verticalSpacing)
		{
			node.Position = position;
			float currentX = position.x;

			List<Node> children = BtUtils.GetChildren(node);

			foreach (Node child in children)
			{
				currentX += CalculateTreeWidth(child) / 2.0f;
				CalculatePositions(child, new Vector2(currentX, position.y + verticalSpacing), verticalSpacing);
				currentX += CalculateTreeWidth(child) / 2.0f;
			}

			return currentX;
		}


		private float CalculateTreeWidth(Node node)
		{
			List<Node> children = BtUtils.GetChildren(node);

			if (children.Count == 0)
				return 100.0f;

			float totalWidth = children.Sum(c => CalculateTreeWidth(c));
			return totalWidth;
		}
	}
}