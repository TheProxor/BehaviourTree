using System;

namespace Swat.Game.BehaviourTree.Nested
{
	[UnityEngine.Scripting.APIUpdating.MovedFrom(true, sourceAssembly: "Game")]
	[Serializable]
	public class NestedId<T> where T : NestedTree
	{
		public string Id;
	}
}