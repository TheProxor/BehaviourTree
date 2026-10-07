using Swat.Game.BehaviourTree.Decorators;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Editor.BehaviourTreeEditor.NodeInfoDrawers
{
	public class ChanceInfoDrawer : INodeInfoDrawer
	{
		public bool UseForChildren => false;


		public void Draw(SerializedProperty target, Label container)
		{
			SerializedProperty chanceProp = target.FindPropertyRelative(nameof(Chance.Value));

			OnPropertyChanged(chanceProp, container);

			container.TrackPropertyValue(chanceProp, prop => OnPropertyChanged(prop, container));
		}


		private void OnPropertyChanged(SerializedProperty property, Label container)
		{
			float floatValue = property.floatValue;

			container.text = $"{Mathf.RoundToInt(floatValue * 100).ToString()}%";
		}
	}
}