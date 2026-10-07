using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Swat.Game.BehaviourTree.Core;
using Swat.Game.BehaviourTree.Nested;
using UnityEngine;
using Object = System.Object;

namespace Swat.Game.BehaviourTree.Utils
{
	public static class BtUtils
	{
		private static readonly Stack<Stack<Node>> _stackPool = new Stack<Stack<Node>>(8);

		private static Stack<Node> RentStack()
		{
			return _stackPool.Count > 0 ? _stackPool.Pop() : new Stack<Node>(64);
		}

		private static void ReturnStack(Stack<Node> stack)
		{
			stack.Clear();
			_stackPool.Push(stack);
		}
		
		public static List<Node> GetChildren(Node parent)
		{
			var children = new List<Node>();

			if (parent is DecoratorNode decorator && decorator.Child != null)
				children.Add(decorator.Child);

			if (parent is EventNode eventNode && eventNode.Child != null)
				children.Add(eventNode.Child);

			if (parent is NestedRoot nestedRoot && nestedRoot.Child != null)
				children.Add(nestedRoot.Child);

			if (parent is RootNode rootNode && rootNode.Child != null)
				children.Add(rootNode.Child);

			if (parent is CompositeNode composite)
				return composite.Children;

			return children;
		}

		public static void SanitizeNodeReferences(IBehaviourTree tree)
		{
			if (tree == null)
				return;

			var validNodes = new HashSet<Node>(tree.Nodes);
			tree.Nodes.RemoveAll(node => node == null);

			void SanitizeSubtree(Node root)
			{
				if (root == null)
					return;

				Traverse(root, SanitizeParentReferences);
			}

			void SanitizeParentReferences(Node node)
			{
				switch (node)
				{
					case CompositeNode composite:
						composite.Children.RemoveAll(child => child == null || !validNodes.Contains(child));
						break;

					case DecoratorNode decorator when decorator.Child != null && !validNodes.Contains(decorator.Child):
						decorator.Child = null;
						break;

					case RootNode root when root.Child != null && !validNodes.Contains(root.Child):
						root.Child = null;
						break;

					case EventNode eventNode when eventNode.Child != null && !validNodes.Contains(eventNode.Child):
						eventNode.Child = null;
						break;

					case NestedRoot nestedRoot when nestedRoot.Child != null && !validNodes.Contains(nestedRoot.Child):
						nestedRoot.Child = null;
						break;

					case NestedCall nestedCall when nestedCall.NestedRoot != null && !validNodes.Contains(nestedCall.NestedRoot):
						nestedCall.NestedRoot = null;
						break;
				}
			}

			SanitizeSubtree(tree.RootNode);

			foreach (EventNode eventNode in tree.EventNodes)
				SanitizeSubtree(eventNode);

			foreach (NestedRoot nestedRoot in tree.NestedRoots)
				SanitizeSubtree(nestedRoot);
		}

		public static void DetachFromParents(IBehaviourTree tree, Node node)
		{
			if (tree == null || node == null)
				return;

			void DetachInSubtree(Node root)
			{
				if (root == null)
					return;

				Traverse(root, parent => RemoveChildReference(parent, node));
			}

			DetachInSubtree(tree.RootNode);

			foreach (EventNode eventNode in tree.EventNodes)
				DetachInSubtree(eventNode);

			foreach (NestedRoot nestedRoot in tree.NestedRoots)
				DetachInSubtree(nestedRoot);
		}

		private static void RemoveChildReference(Node parent, Node child)
		{
			switch (parent)
			{
				case CompositeNode composite:
					composite.Children.Remove(child);
					break;

				case DecoratorNode decorator when decorator.Child == child:
					decorator.Child = null;
					break;

				case RootNode root when root.Child == child:
					root.Child = null;
					break;

				case EventNode eventNode when eventNode.Child == child:
					eventNode.Child = null;
					break;

				case NestedRoot nestedRoot when nestedRoot.Child == child:
					nestedRoot.Child = null;
					break;

				case NestedCall nestedCall when nestedCall.NestedRoot == child:
					nestedCall.NestedRoot = null;
					break;
			}
		}

		public static BehaviourTreeMono MoveBehaviourTree(GameObject to, BehaviourTreeMono from)
		{
			var newBt = (to.GetComponent<BehaviourTreeMono>() ?? to.AddComponent<BehaviourTreeMono>());
			newBt.Move(from);
			return newBt;
		}
		
		public static BehaviourTreeMono CopyBehaviourTree(GameObject to, BehaviourTreeMono from)
		{
			var tempBt = UnityEngine.Object.Instantiate(from);
			var newBt = MoveBehaviourTree(to, tempBt);
			UnityEngine.Object.Destroy(tempBt.gameObject);
			return newBt;
		}
		
		public static void Traverse(Node root, Action<Node> visitor)
		{
			if (root == null || visitor == null)
				return;
			
			var stack = RentStack();
			try
			{
				stack.Push(root);

				while (stack.Count > 0)
				{
					var node = stack.Pop();
					visitor(node);

					AddChildrenToStack(node, stack);
				}
			}
			finally
			{
				ReturnStack(stack);
			}
		}

		private static void AddChildrenToStack(Node parent, Stack<Node> stack)
		{
			switch (parent)
			{
				case CompositeNode composite:
					var children = composite.Children;
					for (int i = children.Count - 1; i >= 0; i--)
						stack.Push(children[i]);
					break;

				case DecoratorNode decorator when decorator.Child != null:
					stack.Push(decorator.Child);
					break;

				case EventNode eventNode when eventNode.Child != null:
					stack.Push(eventNode.Child);
					break;

				case NestedCall nestedRoot when nestedRoot.NestedRoot != null:
					stack.Push(nestedRoot.NestedRoot);
					break;
				
				case NestedRoot nestedRoot when nestedRoot.Child != null:
					stack.Push(nestedRoot.Child);
					break;

				case RootNode rootNode when rootNode.Child != null:
					stack.Push(rootNode.Child);
					break;
			}
		}

		public static void DeepCopyCollections(Node source, Node target)
		{
			DeepCopyCollectionsInObject(source, target, source.GetType());
		}

		private static void DeepCopyCollectionsInObject(object source, object target, Type type)
		{
			FieldInfo[] fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

			foreach (FieldInfo field in fields)
			{
				object sourceValue = field.GetValue(source);
				if (sourceValue == null)
					continue;

				Type fieldType = field.FieldType;

				// Deep copy arrays (excluding Node arrays which will be handled separately)
				if (fieldType.IsArray && !typeof(Node).IsAssignableFrom(fieldType.GetElementType()))
				{
					Array sourceArray = (Array)sourceValue;
					Type elementType = fieldType.GetElementType();
					Array clonedArray = Array.CreateInstance(elementType, sourceArray.Length);

					if (NeedsDeepCopy(elementType))
					{
						for (int i = 0; i < sourceArray.Length; i++)
							clonedArray.SetValue(DeepCopyElement(sourceArray.GetValue(i), elementType), i);
					}
					else
					{
						Array.Copy(sourceArray, clonedArray, sourceArray.Length);
					}

					field.SetValue(target, clonedArray);
				}
				// Deep copy generic lists (excluding Node lists)
				else if (fieldType.IsGenericType &&
						 fieldType.GetGenericTypeDefinition() == typeof(List<>) &&
						 !typeof(Node).IsAssignableFrom(fieldType.GetGenericArguments()[0]))
				{
					IList sourceList = (IList)sourceValue;
					IList clonedList = (IList)Activator.CreateInstance(fieldType);
					Type elementType = fieldType.GetGenericArguments()[0];

					if (NeedsDeepCopy(elementType))
					{
						foreach (var srcElement in sourceList)
							clonedList.Add(DeepCopyElement(srcElement, elementType));
					}
					else
					{
						foreach (var item in sourceList)
							clonedList.Add(item);
					}

					field.SetValue(target, clonedList);
				}
				// Deep copy plain serializable class fields
				else if (!fieldType.IsArray && NeedsDeepCopy(fieldType) && !typeof(Node).IsAssignableFrom(fieldType))
				{
					field.SetValue(target, DeepCopyElement(sourceValue, fieldType));
				}
			}
		}

		private static object DeepCopyElement(object source, Type declaredType)
		{
			if (source == null)
				return null;

			if (TryCopyNativeBackedType(source, out object nativeCopy))
				return nativeCopy;

			object copy = Activator.CreateInstance(declaredType);
			ShallowCopyFields(source, copy, declaredType);
			DeepCopyCollectionsInObject(source, copy, declaredType);
			return copy;
		}

		private static bool TryCopyNativeBackedType(object source, out object copy)
		{
			switch (source)
			{
				case AnimationCurve curve:
					copy = new AnimationCurve(curve.keys)
					{
						preWrapMode = curve.preWrapMode,
						postWrapMode = curve.postWrapMode
					};
					return true;

				case Gradient gradient:
					copy = new Gradient
					{
						colorKeys = gradient.colorKeys,
						alphaKeys = gradient.alphaKeys,
						mode = gradient.mode
					};
					return true;

				default:
					copy = null;
					return false;
			}
		}

		// Returns true for plain serializable classes that own their own arrays/lists
		// (i.e. not primitives, strings, or Unity/Node objects that manage their own lifecycle)
		private static bool NeedsDeepCopy(Type t) =>
			t.IsClass &&
			t != typeof(string) &&
			!t.IsSubclassOf(typeof(UnityEngine.Object));

		private static void ShallowCopyFields(object source, object target, Type type)
		{
			foreach (var f in type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
			{
				if (f.FieldType == typeof(IntPtr) || f.FieldType == typeof(UIntPtr) || f.FieldType.IsPointer)
					continue;

				f.SetValue(target, f.GetValue(source));
			}
		}
	}
}