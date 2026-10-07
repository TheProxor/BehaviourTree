using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Swat.Game.BehaviourTree.Blackboard;
using Swat.Game.BehaviourTree.Contexts;
using Swat.Game.BehaviourTree.Nested;
using Swat.Game.BehaviourTree.Utils;
using UnityEngine;
using ConditionKey = Swat.Game.BehaviourTree.Blackboard.ConditionKey;


namespace Swat.Game.BehaviourTree
{
	[UnityEngine.Scripting.APIUpdating.MovedFrom(true, sourceAssembly: "Game")]
	[Serializable]
	public abstract class Node
	{
		[TextArea] public string Description;
		[HideInInspector] public bool DrawGizmos;

		[HideInInspector] public string Guid = System.Guid.NewGuid().ToString();
		[HideInInspector] public Vector2 Position;
		
		[NonSerialized] public NodeState State;
		[NonSerialized] public bool Started;
		[NonSerialized] public RuntimeTreeContext Context;
		[NonSerialized] public Blackboard.Blackboard Blackboard;
		[NonSerialized] public bool ResetFlag;


		/// <summary>
		///     Called when the node binds
		/// </summary>
		public virtual void Construct()
		{
		}

		/// <summary>
		///     Called every tree cycle start
		/// </summary>
		public virtual void OnInit()
		{
		}

		/// <summary>
		///     Called when tree destroyed
		/// </summary>
		public virtual void OnDispose()
		{
		}


		public virtual void OnDrawGizmos(Transform transform)
		{
		}
		
		public virtual void OnValidate()
		{
		}


		public NodeState UpdateNode()
		{
			if (!Started)
			{
				OnStart();
				Started = true;
				State = NodeState.Running;
			}

			State = OnUpdate();
			
			Context.TickResults[Guid] = State;
			
			if (State != NodeState.Running)
			{
				OnStop();
				Started = false;
			}

			return State;
		}


		public void AbortChildren()
		{
			Traverse(node =>
			{
				node.Started = false;
				node.Abort();
				node.OnStop();
			});
		}


		public void StopChildren()
		{
			Traverse(node =>
			{
				node.Started = false;
				node.OnStop();
			});
		}


		public void ResetChildren()
		{
			Traverse(node =>
			{
				node.Started = false;
				node.State = NodeState.Disabled;
			});
		}


		public void SetResetFlagChildren()
		{
			Traverse(node => { node.ResetFlag = true; });
		}


		public virtual Node Clone()
		{
			var clone = MemberwiseClone() as Node;

			// Deep copy arrays and collections using reflection
			BtUtils.DeepCopyCollections(this, clone);

			// Only clone this node. Child references will be cleared
			if (clone is DecoratorNode decorator && decorator.Child != null)
				decorator.Child = null;

			if (clone is EventNode eventNode && eventNode.Child != null)
				eventNode.Child = null;

			if (clone is NestedRoot nestedTree && nestedTree.Child != null)
				nestedTree.Child = null;

			if (clone is NestedCall nestedCall && nestedCall.NestedRoot != null)
				nestedCall.NestedRoot = null;

			if (clone is RootNode rootNode && rootNode.Child != null)
				rootNode.Child = null;

			if (clone is CompositeNode composite)
				composite.Children = new List<Node>();

			if (clone is ConditionKey conditionKey)
				conditionKey.Pair = null;

			if (clone is Set set)
				set.Pair = null;

			return clone;
		}

		public virtual bool ShouldDrawGizmos()
		{
			return DrawGizmos;
		}
		
		/// <summary>
		///     Called when node starts
		/// </summary>
		protected abstract void OnStart();

		/// <summary>
		///     Called when node stops (Success, Failure)
		/// </summary>
		protected abstract void OnStop();

		/// <summary>
		///     Called every frame
		/// </summary>
		/// <returns></returns>
		protected abstract NodeState OnUpdate();


		protected virtual void Abort()
		{
		}


		protected void Traverse(Action<Node> visiter)
		{
			BtUtils.Traverse(this, visiter);
		}


		protected virtual void Log(string message)
		{
			Debug.Log($"[{GetType()}]{message}");
		}
	}
}