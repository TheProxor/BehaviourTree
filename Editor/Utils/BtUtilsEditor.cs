using System.Collections.Generic;
using System.Linq;
using Swat.Game.BehaviourTree.Core;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using Node = Swat.Game.BehaviourTree.Node;

namespace Editor.BehaviourTreeEditor.Utils
{
	public static class BtUtilsEditor
	{
		public static Node[] SelectNodesArray(IEnumerable<NodeView> nodeViews)
		{
			return SelectNodes(nodeViews)
				.ToArray();
		}

		public static Node[] SelectNodesArray(IEnumerable<GraphElement> graphElements)
		{
			var nodesView = SelectNodeViewsArray(graphElements);

			return SelectNodesArray(nodesView);
		}

		public static NodeView[] SelectNodeViewsArray(IEnumerable<GraphElement> graphElements)
		{
			return SelectNodeViews(graphElements)
				.ToArray();
		}

		public static IEnumerable<NodeView> SelectNodeViews(IEnumerable<GraphElement> graphElements)
		{
			return graphElements
				.Select(e => e as NodeView)
				.Where(e => e != null);
		}

		public static IEnumerable<Node> SelectNodes(IEnumerable<NodeView> nodeViews)
		{
			return nodeViews
				.Select(nv => nv.Node);
		}

		public static Vector2 CalculateCenterOfMass(Node[] nodes)
		{
			Vector2 min = new Vector2(float.MaxValue, float.MaxValue);
			Vector2 max = new Vector2(float.MinValue, float.MinValue);

			foreach (var node in nodes)
			{
				min.x = Mathf.Min(min.x, node.Position.x);
				min.y = Mathf.Min(min.y, node.Position.y);
				max.x = Mathf.Max(max.x, node.Position.x);
				max.y = Mathf.Max(max.y, node.Position.y);
			}

			Vector2 center = (min + max) * 0.5f;
			return center;
		}

		public static IBehaviourTree GetBehaviourTree(SerializedProperty property)
		{
			if (property.serializedObject.targetObject is IBehaviourTree tree)
				return tree;

			Debug.LogError("Could not find behaviour tree this is referencing");
			return null;
		}

		public static T[] FindAllScriptableObjects<T>() where T : ScriptableObject
		{
			var results = new List<T>();
			var guids = AssetDatabase.FindAssets($"t:{typeof(T).Name}");

			foreach (string guid in guids)
			{
				string assetPath = AssetDatabase.GUIDToAssetPath(guid);
				T asset = AssetDatabase.LoadAssetAtPath<T>(assetPath);
				
				if (asset != null)
					results.Add(asset);
			}

			return results.ToArray();
		}
	}
}