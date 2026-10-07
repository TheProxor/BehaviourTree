using System;
using System.Collections.Generic;
using Swat.Game.BehaviourTree.Contexts;
using Swat.Game.BehaviourTree.Core;
using Swat.Game.BehaviourTree.Nested;
using Swat.Game.BehaviourTree.Utils;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Swat.Game.BehaviourTree
{
	public class BehaviourTreeMono : MonoBehaviour, IBehaviourTree
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

		private Dictionary<string, NestedRoot> _nestedTreesMap;
		private bool _isPaused = true;

		#region Properties
		public string Name => name;
		public Object Object => gameObject;

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

		public void Dispose()
		{
			BtUtils.Traverse(RootNode, node => { node.OnDispose(); });
		}

		public string GetFullPath()
		{
			return gameObject.GetFullPath();
		}

		public NodeState UpdateTree()
		{
			if (_isPaused)
				return NodeState.Running;

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
			BehaviourTreeMono tree = Instantiate(this);
			return tree;
		}


		public void Bind(RuntimeTreeContext context)
		{
			Context = context;

			_nestedTreesMap = new Dictionary<string, NestedRoot>();
			
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


		public void Pause()
		{
			_isPaused = true;
		}


		public void Run()
		{
			_isPaused = false;
		}


		public void Abort()
		{
			RootNode.AbortChildren();
			RootNode.ResetChildren();
			RootNode.SetResetFlagChildren();
		}

		public void Move(BehaviourTreeMono other)
		{
			RootNode = other.RootNode;
			Nodes = other.Nodes;
			NestedRoots = other.NestedRoots;
			NestedCalls = other.NestedCalls;
			EventNodes = other.EventNodes;

			Blackboard = other.Blackboard;
			Context = other.Context;
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


		private void OnDrawGizmos()
		{
			foreach (Node node in Nodes)
				if (node.ShouldDrawGizmos())
					node.OnDrawGizmos(transform);
		}
	}
}