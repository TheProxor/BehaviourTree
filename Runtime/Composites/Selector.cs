using System;
using System.Collections.Generic;
using Swat.Game.BehaviourTree.Data;
using Swat.Game.BehaviourTree.Nested;

namespace Swat.Game.BehaviourTree.Composites
{
	[UnityEngine.Scripting.APIUpdating.MovedFrom(true, sourceAssembly: "Game")]
	[Serializable]
	public class Selector : CompositeNode
	{
		private readonly Dictionary<ConditionNode, int> _conditionIndexMap = new();
		private int _current;

		public override void Construct()
		{
			_conditionIndexMap.Clear();

			for (var i = 0; i < Children.Count; i++)
			{
				switch (Children[i])
				{
					case ConditionNode condition:
					{
						_conditionIndexMap.Add(condition, i);
						break;
					}

					case NestedCall nestedCall when nestedCall.CombineSelectorsInNestedTree:
					{
						if (nestedCall.NestedRoot.Child is Selector selectorToCombine)
						{
							foreach (Node selectorChild in selectorToCombine.Children)
							{
								if (selectorChild is not ConditionNode conditionNode)
									continue;

								_conditionIndexMap.Add(conditionNode, i);
							}
						}
						break;
					}
				}
			}
		}


		protected override void OnStart()
		{
			_current = 0;

			foreach (ConditionNode condition in _conditionIndexMap.Keys)
				switch (condition.ConditionType)
				{
					case ConditionNode.ConditionMethod.OnValueChange:
						condition.OnValueChangedEvent += TrySwitchToCondition;
						break;

					case ConditionNode.ConditionMethod.OnResultChange:
						condition.OnResultChangedEvent += TrySwitchToCondition;
						break;
				}
		}


		protected override void OnStop()
		{
			foreach (ConditionNode condition in _conditionIndexMap.Keys)
				switch (condition.ConditionType)
				{
					case ConditionNode.ConditionMethod.OnValueChange:
						condition.OnValueChangedEvent -= TrySwitchToCondition;
						break;

					case ConditionNode.ConditionMethod.OnResultChange:
						condition.OnResultChangedEvent -= TrySwitchToCondition;
						break;
				}
		}


		protected override NodeState OnUpdate()
		{
			// Update current node
			for (int i = _current; i < Children.Count; ++i)
			{
				_current = i;
				Node child = Children[_current];

				// if (child is ConditionNode condition && !condition.IsTrue())
				//     continue;

				switch (child.UpdateNode())
				{
					case NodeState.Running:
						return NodeState.Running;

					case NodeState.Success:
						return NodeState.Success;

					case NodeState.Failure:
					{
						// if (child is AbortableNode { AbortType: AbortMethod.Both or AbortMethod.LowerPriority })
						//     return NodeState.Failure;
						//
						continue;
					}
				}
			}

			return NodeState.Failure;
		}


		private void TrySwitchToCondition(ConditionNode condition)
		{
			int index = _conditionIndexMap[condition];

			if (index >= _current)
				return;

			//bool isConditionTrue = condition.IsTrue();
			Node currentNode = Children[_current];

			switch (condition.AbortType)
			{
				case AbortMethod.None:
					return;

				case AbortMethod.Self:
					condition.AbortChildren();
					break;

				case AbortMethod.LowerPriority:
					currentNode.AbortChildren();
					break;

				case AbortMethod.Both:
					condition.AbortChildren();
					currentNode.AbortChildren();
					break;
			}

			_current = index;

			foreach (Node node in Children)
				node.SetResetFlagChildren();
		}
	}
}