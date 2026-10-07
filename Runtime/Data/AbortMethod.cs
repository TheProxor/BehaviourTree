namespace Swat.Game.BehaviourTree.Data
{
	public enum AbortMethod : byte
	{
		None = 0,
		Self = 1,
		LowerPriority = 2,
		Both = 3
	}
}