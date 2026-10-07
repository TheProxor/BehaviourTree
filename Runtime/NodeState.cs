namespace Swat.Game.BehaviourTree
{
	public enum NodeState : byte
	{
		Disabled = 0,
		Running = 1,
		Failure = 2,
		Success = 3
	}
}