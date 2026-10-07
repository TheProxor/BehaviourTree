using System;
using Swat.Game.BehaviourTree.Data;

namespace Swat.Game.BehaviourTree.Blackboard
{
	[UnityEngine.Scripting.APIUpdating.MovedFrom(true, sourceAssembly: "Game")]
	[Serializable]
	public class ConditionKey : ConditionNode
	{
		public BlackboardKeyValuePair Pair;
		public ComparerType ComparerType = ComparerType.Equal;

		private BlackboardKey source;
		private BlackboardKey destination;


		public override void Construct()
		{
			source = Pair.Value;
			destination = Pair.Key;

			if(destination != null)
				destination.OnValueChanged += OnValueChanged;
		}


		public override void OnDispose()
		{
			if (destination != null)
				destination.OnValueChanged -= OnValueChanged;
		}


		public override bool IsTrue()
		{
			if (source == null || destination == null)
				return false;

			return ComparerType switch
			{
				ComparerType.Equal => source.Equals(destination),
				ComparerType.NotEqual => !source.Equals(destination),
				ComparerType.Less => source.Less(destination),
				ComparerType.Greater => source.Greater(destination),
				ComparerType.LEqual => source.Less(destination) || source.Equals(destination),
				ComparerType.GEqual => source.Greater(destination) || source.Equals(destination),
				_ => false
			};
		}
	}
}