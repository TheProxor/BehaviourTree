using System;
using Swat.Game.BehaviourTree.Blackboard;
using UnityEngine;

namespace Swat.Game.BehaviourTree
{
	[UnityEngine.Scripting.APIUpdating.MovedFrom(true, sourceAssembly: "Game")]
	[Serializable]
	public class NodeProperty
	{
		[SerializeReference] public BlackboardKey Reference;
	}


	[UnityEngine.Scripting.APIUpdating.MovedFrom(true, sourceAssembly: "Game")]
	[Serializable]
	public class NodeProperty<T> : NodeProperty
	{
		public T defaultValue;

		public T Value
		{
			set
			{
				if (typedKey != null)
					typedKey.SetValue(value);
				else
					defaultValue = value;
			}
			get
			{
				if (typedKey != null)
					return typedKey.Value;
				return defaultValue;
			}
		}

		private BlackboardKey<T> _typedKey;

		private BlackboardKey<T> typedKey
		{
			get
			{
				if (_typedKey == null && Reference != null)
					_typedKey = Reference as BlackboardKey<T>;

				return _typedKey;
			}
		}
	}
}