using System;

namespace Swat.Game.BehaviourTree
{
	[UnityEngine.Scripting.APIUpdating.MovedFrom(true, sourceAssembly: "Game")]
	[Serializable]
	public abstract class ActionNode : Node
	{
		protected override void OnStart()
		{
		}

		protected override void OnStop()
		{
		}
	}
}