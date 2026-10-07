using System;
using System.Collections.Generic;
using Editor.BehaviourTreeEditor.Blackboard;
using Swat.Game.BehaviourTree;
using Swat.Game.BehaviourTree.Blackboard;
using Swat.Game.BehaviourTree.Core;
using Swat.Game.BehaviourTree.Nested;
using Swat.Game.BehaviourTree.Utils;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Editor.BehaviourTreeEditor.Serialization
{
	// This is a helper class which wraps a serialized object for finding properties on the behaviour.
	// It's best to modify the behaviour tree via SerializedObjects and SerializedProperty interfaces
	// to keep the UI in sync, and undo/redo
	// It's a hodge podge mix of various functions that will evolve over time. It's not exhaustive by any means.
	[Serializable]
	public class SerializedBehaviourTree
	{
		// Wrapper serialized object for writing changes to the behaviour tree
		public SerializedObject serializedObject;

		public IBehaviourTree tree;

		public SerializedProperty RootNode => serializedObject.FindProperty(sPropRootNode);

		public SerializedProperty Nodes => serializedObject.FindProperty(sPropNodes);
		public SerializedProperty NestedRoots => serializedObject.FindProperty(sPropNestedRoots);
		public SerializedProperty NestedCalls => serializedObject.FindProperty(sPropNestedCalls);
		public SerializedProperty EventNodes => serializedObject.FindProperty(sPropEventNodes);

		public SerializedProperty Blackboard => serializedObject.FindProperty(sPropBlackboard);

		public SerializedProperty BlackboardKeys =>
			serializedObject.FindProperty($"{sPropBlackboard}.{sPropBlackboardKeys}");

		// Property names. These correspond to the variable names on the behaviour tree
		private const string sPropRootNode = nameof(BehaviourTreeMono.RootNode);
		private const string sPropNodes = nameof(BehaviourTreeMono.Nodes);
		private const string sPropNestedRoots = nameof(BehaviourTreeMono.NestedRoots);
		private const string sPropNestedCalls = nameof(BehaviourTreeMono.NestedCalls);
		private const string sPropEventNodes = nameof(BehaviourTreeMono.EventNodes);
		private const string sPropBlackboard = nameof(BehaviourTreeMono.Blackboard);
		private const string sPropBlackboardKeys = nameof(Swat.Game.BehaviourTree.Blackboard.Blackboard.Keys);
		private const string sPropGuid = nameof(Node.Guid);
		private const string sPropChild = nameof(DecoratorNode.Child);
		private const string sPropChildren = nameof(CompositeNode.Children);
		private const string sPropPosition = nameof(Node.Position);
		private const string sViewTransformPosition = nameof(BehaviourTreeMono.viewPosition);
		private const string sViewTransformScale = nameof(BehaviourTreeMono.viewScale);
		private bool batchMode;


		public SerializedBehaviourTree(IBehaviourTree tree)
		{
			serializedObject = new SerializedObject(tree as Object);
			this.tree = tree;

			if (BlackboardKeys.arraySize == 0)
				new BlackboardDefaultKeysCreator(this).CreateDefaultKeys();
		}
		

		public SerializedProperty FindNode(Node node)
		{
			SerializedProperty nodes = Nodes;
			
			for (var i = 0; i < nodes.arraySize; ++i)
			{
				SerializedProperty current = nodes.GetArrayElementAtIndex(i);

				if (current.FindPropertyRelative(sPropGuid).stringValue == node.Guid)
					return current;
			}

			return null;
		}


		public void SetViewTransform(Vector3 position, Vector3 scale)
		{
			serializedObject.FindProperty(sViewTransformPosition).vector3Value = position;
			serializedObject.FindProperty(sViewTransformScale).vector3Value = scale;
			serializedObject.ApplyModifiedPropertiesWithoutUndo();
		}


		public void SetNodePosition(Node node, Vector2 position)
		{
			SerializedProperty nodeProp = FindNode(node);
			nodeProp.FindPropertyRelative(sPropPosition).vector2Value = position;
			ApplyChanges();
		}


		public void RemoveNodeArrayElement(SerializedProperty array, Node node)
		{
			for (var i = 0; i < array.arraySize; ++i)
			{
				SerializedProperty current = array.GetArrayElementAtIndex(i);

				if (current.FindPropertyRelative(sPropGuid).stringValue == node.Guid)
				{
					array.DeleteArrayElementAtIndex(i);
					return;
				}
			}
		}


		public Node CloneTree(Node rootNode, Node parentNode, Vector2 position)
		{
			var oldToNewMapping = new Dictionary<Node, Node>();

			var sourceNodes = new List<Node>();

			BtUtils.Traverse(rootNode, n => { sourceNodes.Add(n); });

			// Clone Nodes
			foreach (Node node in sourceNodes)
			{
				Node newNode = CloneNode(node, node.Position - rootNode.Position + position);
				oldToNewMapping[node] = newNode;
			}

			// Clone Edges
			foreach (Node node in sourceNodes)
			{
				List<Node> children = BtUtils.GetChildren(node);
				Node newParent = oldToNewMapping[node];

				foreach (Node child in children)
				{
					Node newChild = oldToNewMapping[child];
					AddChild(newParent, newChild);
				}
			}

			// Parent subtree root to rootnode in new tree asset
			Node newSubTreeRoot = oldToNewMapping[rootNode];
			AddChild(parentNode, newSubTreeRoot);

			return newSubTreeRoot;
		}


		public Node CloneNode(Node node, Vector2 position)
		{
			Node copy = node.Clone();
			copy.Guid = GUID.Generate().ToString();
			copy.Position = position;

			SerializedProperty newNode = AppendArrayElement(Nodes);
			newNode.managedReferenceValue = copy;

			// Nested Root
			if (copy is NestedRoot nestedRootCopy)
			{
				SerializedProperty newNestedRoot = AppendArrayElement(NestedRoots);
				newNestedRoot.managedReferenceValue = nestedRootCopy;
			}
			// Nested Call
			else if (copy is NestedCall nestedCallCopy)
			{
				SerializedProperty newNestedCall = AppendArrayElement(NestedCalls);
				newNestedCall.managedReferenceValue = nestedCallCopy;
			}
			// Event Node
			else if (copy.GetType().IsSubclassOf(typeof(EventNode)))
			{
				SerializedProperty newEventNode = AppendArrayElement(EventNodes);
				newEventNode.managedReferenceValue = copy as EventNode;
			}
			
			ApplyChanges();

			return copy;
		}


		public Node CreateNode<T>(Vector2 position) where T : Node, new()
		{
			Node node = new T();
			node.Guid = GUID.Generate().ToString();
			node.Position = position;

			SerializedProperty newNode = AppendArrayElement(Nodes);
			newNode.managedReferenceValue = node;

			ApplyChanges();

			return node;
		}


		private Node CreateNodeInstance(Type type)
		{
			var node = Activator.CreateInstance(type) as Node;
			node.Guid = GUID.Generate().ToString();
			return node;
		}


		public Node CreateNode(Type type, Vector2 position)
		{
			Node child = CreateNodeInstance(type);
			child.Position = position;

			SerializedProperty newNode = AppendArrayElement(Nodes);
			newNode.managedReferenceValue = child;

			// Nested Root
			if (type == typeof(NestedRoot))
			{
				SerializedProperty newNestedRoot = AppendArrayElement(NestedRoots);
				newNestedRoot.managedReferenceValue = child as NestedRoot;
			}
			// Nested Call
			else if (type == typeof(NestedCall))
			{
				SerializedProperty newNestedCall = AppendArrayElement(NestedCalls);
				newNestedCall.managedReferenceValue = child as NestedCall;
			}
			// Event Node
			else if (type.IsSubclassOf(typeof(EventNode)))
			{
				SerializedProperty newEventNode = AppendArrayElement(EventNodes);
				newEventNode.managedReferenceValue = child as EventNode;
			}

			ApplyChanges();

			return child;
		}


		public void SetRootNode(RootNode node)
		{
			RootNode.managedReferenceValue = node;
			ApplyChanges();
		}


		public void DeleteNode(Node node)
		{
			BtUtils.DetachFromParents(tree, node);
			RemoveNodeArrayElement(Nodes, node);

			switch (node)
			{
				case NestedRoot nestedRoot:
					RemoveNodeArrayElement(NestedRoots, nestedRoot);
					break;

				case NestedCall nestedCall:
					RemoveNodeArrayElement(NestedCalls, nestedCall);
					break;

				case EventNode eventNode:
					RemoveNodeArrayElement(EventNodes, eventNode);
					break;
			}

			ApplyChanges();
		}


		public void DeleteTree(Node rootNode)
		{
			var nodesToDelete = new List<Node>();

			BtUtils.Traverse(rootNode, n => { nodesToDelete.Add(n); });

			foreach (Node node in nodesToDelete)
				DeleteNode(node);

			ApplyChanges();
		}


		public void AddChild(Node parent, Node child)
		{
			SerializedProperty parentProperty = FindNode(parent);
			SerializedProperty childNode = FindNode(child);

			if (childNode == null)
			{
				Debug.LogError("Child does not exist in the tree");
				return;
			}

			// RootNode, Decorator node
			SerializedProperty childProperty = parentProperty.FindPropertyRelative(sPropChild);

			if (childProperty != null)
			{
				childProperty.managedReferenceValue = child;
				ApplyChanges();
				return;
			}

			// Composite nodes
			SerializedProperty childrenProperty = parentProperty.FindPropertyRelative(sPropChildren);

			if (childrenProperty != null)
			{
				SerializedProperty newChild = AppendArrayElement(childrenProperty);
				newChild.managedReferenceValue = child;
				ApplyChanges();
			}
		}


		public void RemoveChild(Node parent, Node child)
		{
			SerializedProperty parentProperty = FindNode(parent);

			// RootNode, Decorator node
			SerializedProperty childProperty = parentProperty.FindPropertyRelative(sPropChild);

			if (childProperty != null)
			{
				childProperty.managedReferenceValue = null;
				ApplyChanges();
				return;
			}

			// Composite nodes
			SerializedProperty childrenProperty = parentProperty.FindPropertyRelative(sPropChildren);

			if (childrenProperty != null)
			{
				RemoveNodeArrayElement(childrenProperty, child);
				ApplyChanges();
			}
		}


		public void CreateBlackboardKey(string keyName, Type keyType, bool isUndeletable = false)
		{
			var key = BlackboardKey.CreateKey(keyType);

			if (key != null)
			{
				key.Name = keyName;
				key.IsUndeletable = isUndeletable;
				SerializedProperty keysArray = BlackboardKeys;
				keysArray.InsertArrayElementAtIndex(keysArray.arraySize);
				SerializedProperty newKey = keysArray.GetArrayElementAtIndex(keysArray.arraySize - 1);

				newKey.managedReferenceValue = key;

				ApplyChanges();
			}
			else
			{
				Debug.LogError($"Failed to create blackboard key, invalid type:{keyType}");
			}
		}

		public BlackboardKey GetBlackboardKey(string keyName)
		{
			return tree.Blackboard.Find(keyName);
		}

		public void DeleteBlackboardKey(string keyName)
		{
			SerializedProperty keysArray = BlackboardKeys;

			for (var i = 0; i < keysArray.arraySize; ++i)
			{
				SerializedProperty key = keysArray.GetArrayElementAtIndex(i);
				var itemKey = key.managedReferenceValue as BlackboardKey;

				if (itemKey.Name == keyName)
				{
					if (itemKey.IsUndeletable)
						return;

					keysArray.DeleteArrayElementAtIndex(i);
					ApplyChanges();
					return;
				}
			}
		}


		public void BeginBatch()
		{
			batchMode = true;
		}


		public void ApplyChanges()
		{
			if (!batchMode)
				serializedObject.ApplyModifiedProperties();
		}


		public void EndBatch()
		{
			batchMode = false;
			ApplyChanges();
		}


		internal void SetNodeProperty(Node node, string propertyPath, Object objectReference)
		{
			SerializedProperty serializedNode = FindNode(node);

			if (serializedNode == null)
			{
				Debug.LogError($"Node {node} does not exist in serialized object");
				return;
			}

			SerializedProperty nodeProperty = serializedNode.FindPropertyRelative(propertyPath);

			if (nodeProperty == null)
			{
				Debug.LogError($"Node Property '{propertyPath}' does not exist in {node.GetType().Name}");
				return;
			}

			if (nodeProperty.propertyType != SerializedPropertyType.ObjectReference)
			{
				Debug.LogError($"Node Property '{propertyPath}' is not an ObjectReference type");
				return;
			}

			nodeProperty.objectReferenceValue = objectReference;

			ApplyChanges();
		}


		private SerializedProperty AppendArrayElement(SerializedProperty arrayProperty)
		{
			arrayProperty.InsertArrayElementAtIndex(arrayProperty.arraySize);
			return arrayProperty.GetArrayElementAtIndex(arrayProperty.arraySize - 1);
		}
	}
}