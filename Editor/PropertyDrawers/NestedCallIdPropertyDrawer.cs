using Editor.BehaviourTreeEditor.Utils;
using Swat.Game.BehaviourTree.Nested;
using UnityEditor;
using UnityEngine.UIElements;

namespace Editor.BehaviourTreeEditor.PropertyDrawers
{
	[CustomPropertyDrawer(typeof(NestedId<NestedCall>))]
	public class NestedCallIdPropertyDrawer : PropertyDrawer
	{
		public override VisualElement CreatePropertyGUI(SerializedProperty property)
		{
			SerializedProperty idProperty = property.FindPropertyRelative("Id");

			var dropdown = new PopupField<string>();
			dropdown.label = "Nested Root ID";
			dropdown.formatListItemCallback = FormatItem;
			dropdown.formatSelectedValueCallback = FormatItem;
			dropdown.value = idProperty.stringValue;

			var tree = BtUtilsEditor.GetBehaviourTree(property);

			dropdown.RegisterCallback<MouseEnterEvent>(_ =>
			{
				dropdown.choices.Clear();

				foreach (NestedRoot nestedRoot in tree.NestedRoots) dropdown.choices.Add(nestedRoot.NestedId.Id);
			});

			dropdown.RegisterCallback<ChangeEvent<string>>(evt =>
			{
				idProperty.stringValue = evt.newValue;
				property.serializedObject.ApplyModifiedProperties();
			});

			return dropdown;
		}


		private string FormatItem(string item)
		{
			return item ?? "(null)";
		}
	}
}