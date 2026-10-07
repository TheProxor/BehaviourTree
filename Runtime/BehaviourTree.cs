using System;
using System.Collections.Generic;
using Swat.Game.BehaviourTree.Contexts;
using Swat.Game.BehaviourTree.Core;
using Swat.Game.BehaviourTree.Nested;
using Swat.Game.BehaviourTree.Utils;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using Object = UnityEngine.Object;

namespace Swat.Game.BehaviourTree
{
	[CreateAssetMenu(menuName = "BehaviourTree/Create", fileName = "BehaviourTree")]
	public class BehaviourTree : ScriptableObject, IBehaviourTree
	{
		// TODO: get rid of code duplication
		[SerializeReference] public RootNode RootNode;
		[SerializeReference] public List<Node> Nodes = new();
		[SerializeReference] public List<NestedRoot> NestedRoots = new();
		[SerializeReference] public List<NestedCall> NestedCalls = new();
		[SerializeReference] public List<EventNode> EventNodes = new();

		public NodeState TreeState = NodeState.Disabled;

		public Blackboard.Blackboard Blackboard = new();
		public RuntimeTreeContext Context;

		[HideInInspector] public Vector3 viewPosition = new(600, 300);
		[HideInInspector] public Vector3 viewScale = Vector3.one;

		private readonly Dictionary<string, NestedRoot> _nestedTreesMap = new();
		
		#region Properties
		public string Name => name;
		public Object Object => this;

		RootNode IBehaviourTree.RootNode => RootNode;
		List<Node> IBehaviourTree.Nodes => Nodes;
		List<NestedRoot> IBehaviourTree.NestedRoots => NestedRoots;
		List<NestedCall> IBehaviourTree.NestedCalls => NestedCalls;
		List<EventNode> IBehaviourTree.EventNodes => EventNodes;
		Blackboard.Blackboard IBehaviourTree.Blackboard => Blackboard;
		RuntimeTreeContext IBehaviourTree.Context => Context;
		NodeState IBehaviourTree.TreeState => TreeState;
		Vector3 IBehaviourTree.ViewPosition => viewPosition;
		Vector3 IBehaviourTree.ViewScale => viewScale;
		
		#endregion
		
		public BehaviourTree()
		{
			RootNode = new RootNode();
			Nodes.Add(RootNode);
		}

		public void Dispose()
		{
			BtUtils.Traverse(RootNode, node => { node.OnDispose(); });
		}

		public string GetFullPath()
		{
			#if UNITY_EDITOR
			return AssetDatabase.GetAssetPath(this);
			#else
			return string.Empty;
			#endif
		}

		public NodeState UpdateTree()
		{
			// Update Events
			foreach (EventNode eventNode in EventNodes)
				switch (eventNode.State)
				{
					case NodeState.Running:
					{
						eventNode.UpdateNode();
						break;
					}

					case NodeState.Disabled:
					{
						BtUtils.Traverse(eventNode, node =>
						{
							node.Started = false;
							node.OnInit();
						});

						eventNode.State = NodeState.Running;
						break;
					}
				}

			// Update Root
			switch (RootNode.State)
			{
				case NodeState.Running:
				{
					TreeState = RootNode.UpdateNode();
					break;
				}

				case NodeState.Disabled:
				case NodeState.Success when Context.IsCycled:
				case NodeState.Failure when Context.IsCycled:
				{
					BtUtils.Traverse(RootNode, node =>
					{
						node.Started = false;
						node.OnInit();
					});

					Context.TickResults.Clear();
					TreeState = NodeState.Running;
					RootNode.State = NodeState.Running;
					break;
				}
			}

			return TreeState;
		}


		public IBehaviourTree Clone()
		{
			BehaviourTree tree = Instantiate(this);
			return tree;
		}


		public void Bind(RuntimeTreeContext context)
		{
			Context = context;

			Action<Node> onBind = node =>
			{
				node.Context = context;
				node.Blackboard = Blackboard;
				node.Construct();
			};

			// Bind Nested Trees
			foreach (NestedRoot nestedRoot in NestedRoots)
				_nestedTreesMap.Add(nestedRoot.NestedId.Id, nestedRoot);
			
			foreach (NestedCall nestedCall in NestedCalls)
				nestedCall.NestedRoot = _nestedTreesMap[nestedCall.NestedId.Id];

			foreach (NestedRoot nestedRoot in NestedRoots)
				BtUtils.Traverse(nestedRoot, onBind);
			
			// Bind Events
			foreach (EventNode eventNode in EventNodes)
				BtUtils.Traverse(eventNode, onBind);

			BtUtils.Traverse(RootNode, onBind);
		}
		
		private void OnValidate()
		{
			if (RootNode == null)
			{
				RootNode = new RootNode();
				Nodes.Add(RootNode);
			}

			BtUtils.SanitizeNodeReferences(this);

			BtUtils.Traverse(RootNode, node =>
			{
				if (node is SubTree subTree)
					subTree.ValidateTree();
				
				node.OnValidate();
			});
		}
		
		public void DrawGizmos(Transform transform)
		{
			foreach (Node node in Nodes)
				if (node.ShouldDrawGizmos())
					node.OnDrawGizmos(transform);
		}
	}
}