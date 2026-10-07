using Swat.Game.BehaviourTree.Decorators;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Editor.BehaviourTreeEditor.NodeInfoDrawers
{
	public class LoopInfoDrawer : INodeInfoDrawer
	{
		public bool UseForChildren => false;


		public void Draw(SerializedProperty target, Label container)
		{
			SerializedProperty loopCountProp = target.FindPropertyRelative(nameof(Loop.LoopCount));

			OnPropertyChanged(loopCountProp, container);

			container.TrackPropertyValue(loopCountProp, prop => OnPropertyChanged(prop, container));
		}


		private void OnPropertyChanged(SerializedProperty property, Label container)
		{
			int intValue = property.intValue;

			container.text = intValue < 1 ? "Endless" : intValue.ToString();
		}
	}
}