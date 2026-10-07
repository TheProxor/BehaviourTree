using System;
using UnityEngine;

namespace Swat.Game.BehaviourTree.Blackboard
{
	// This is a special type to represent a key binding, with a value.
	// The BlackboardKeyValuePairPropertyDrawer takes care of updating the rendered value field
	// when the key type changes.
	[UnityEngine.Scripting.APIUpdating.MovedFrom(true, sourceAssembly: "Game")]
	[Serializable]
	public class BlackboardKeyValuePair
	{
		[SerializeReference] public BlackboardKey Key;

		[SerializeReference] public BlackboardKey Value;

		public BlackboardKeyValuePair(BlackboardKey key, BlackboardKey value)
		{
			Key = key;
			Value = value;
		}


		public void WriteValue()
		{
			if (Key != null && Value != null)
				Key.CopyFrom(Value);
		}
	}
}