using Swat.Game.BehaviourTree;
using Swat.Game.BehaviourTree.Core;

namespace Editor.BehaviourTreeEditor.Game.Core
{
	public interface ICycledTreeCondition
	{
		public bool IsCycled(IBehaviourTree tree);
	}
}