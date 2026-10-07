using System;
using UnityEngine;

namespace Swat.Game.BehaviourTree.Blackboard
{
	// This type is needed to display keys of a specific type
	[UnityEngine.Scripting.APIUpdating.MovedFrom(true, sourceAssembly: "Game")]
	[Serializable]
	public class BlackboardKeyFilter<T>
	{
		[SerializeReference] public BlackboardKey<T> Key;

		public BlackboardKeyFilter() { }
		
		public BlackboardKeyFilter(BlackboardKey<T> key)
		{
			Key = key;
		}
	}
}