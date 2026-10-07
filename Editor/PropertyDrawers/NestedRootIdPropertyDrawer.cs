using Editor.BehaviourTreeEditor.Utils;
using Swat.Game.BehaviourTree.Nested;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using NestedRoot = Swat.Game.BehaviourTree.Nested.NestedRoot;


namespace Editor.BehaviourTreeEditor.PropertyDrawers
{
	[CustomPropertyDrawer(typeof(NestedId<NestedRoot>))]
	public class NestedRootIdPropertyDrawer : PropertyDrawer
	{
		public override VisualElement CreatePropertyGUI(SerializedProperty property)
		{
			SerializedProperty idProperty = property.FindPropertyRelative("Id");

			var container = new VisualElement();

			var stringField = new TextField();
			stringField.label = "ID";
			stringField.value = idProperty.stringValue;

			var warningField = new Label();
			warningField.style.paddingTop = 5;
			warningField.focusable = false;
			warningField.style.display = DisplayStyle.None;
			warningField.style.backgroundColor = new Color(1f, 0.2f, 0.2f);
			warningField.style.fontSize = 12;
			warningField.style.unityFontStyleAndWeight = FontStyle.Bold;
			warningField.text = "This ID already exists in the behaviour tree!";

			stringField.RegisterCallback<ChangeEvent<string>>(evt =>
			{
				idProperty.stringValue = evt.newValue;

				warningField.style.display = DisplayStyle.None;

				var tree = BtUtilsEditor.GetBehaviourTree(property);

				// Validate the same ID
				foreach (NestedRoot nestedRoot in tree.NestedRoots)
				{
					if (nestedRoot.NestedId.Id != evt.newValue)
						continue;

					warningField.style.display = DisplayStyle.Flex;
					return;
				}

				property.serializedObject.ApplyModifiedProperties();
			});

			container.Add(stringField);
			container.Add(warningField);

			return container;
		}
	}
}