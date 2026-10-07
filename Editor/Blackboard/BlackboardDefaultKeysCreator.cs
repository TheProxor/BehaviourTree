using Editor.BehaviourTreeEditor.Game;
using Editor.BehaviourTreeEditor.Serialization;
using Swat.Game.BehaviourTree.Blackboard;

namespace Editor.BehaviourTreeEditor.Blackboard
{
	public class BlackboardDefaultKeysCreator
	{
		private readonly SerializedBehaviourTree serializer;


		public BlackboardDefaultKeysCreator(SerializedBehaviourTree serializer)
		{
			this.serializer = serializer;
		}


		public void CreateDefaultKeys()
		{
			foreach (var name in GameSpecifics.BlackboardDefaultBoolKeysNames)
				CreateBool(name);
		}


		private void CreateBool(string name)
		{
			serializer.CreateBlackboardKey(name, typeof(BooleanKey), true);
		}
	}
}