using System.Globalization;
using Swat.Game.BehaviourTree.Decorators;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Editor.BehaviourTreeEditor.NodeInfoDrawers
{
	public class WaitInfoDrawer : INodeInfoDrawer
	{
		public bool UseForChildren => false;


		public void Draw(SerializedProperty target, Label container)
		{
			SerializedProperty durationProp = target.FindPropertyRelative(nameof(Wait.DurationMinMax));

			OnPropertyChanged(durationProp, container);

			container.TrackPropertyValue(durationProp, prop => OnPropertyChanged(prop, container));
		}


		private void OnPropertyChanged(SerializedProperty property, Label container)
		{
			Vector2 vectorValue = property.vector2Value;

			container.text = Mathf.Approximately(vectorValue.x, vectorValue.y)
				? $"{vectorValue.x.ToString(CultureInfo.CurrentCulture)} sec"
				: string.Format($"{vectorValue.x} - {vectorValue.y} sec", CultureInfo.CurrentCulture);
		}
	}
}