using System;
using Swat.Game.BehaviourTree;
using Swat.Game.BehaviourTree.Blackboard;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Editor.BehaviourTreeEditor.PropertyDrawers
{
	[CustomPropertyDrawer(typeof(NodeProperty<>))]
	public class GenericNodePropertyPropertyDrawer : PropertyDrawer
	{
		public override VisualElement CreatePropertyGUI(SerializedProperty property)
		{
			var tree = property.serializedObject.targetObject as BehaviourTreeMono;

			Type[] genericTypes = fieldInfo.FieldType.GenericTypeArguments;
			Type propertyType = genericTypes[0];

			SerializedProperty reference = property.FindPropertyRelative("Reference");

			var label = new Label();
			label.AddToClassList("unity-base-field__label");
			label.AddToClassList("unity-property-field__label");
			label.AddToClassList("unity-property-field");
			label.text = property.displayName;

			var defaultValueField = new PropertyField();
			defaultValueField.label = "";
			defaultValueField.style.flexGrow = 1.0f;
			defaultValueField.bindingPath = nameof(NodeProperty<int>.defaultValue);

			var dropdown = new PopupField<BlackboardKey>();
			dropdown.label = "";
			dropdown.formatListItemCallback = FormatItem;
			dropdown.formatSelectedValueCallback = FormatSelectedItem;
			dropdown.value = reference.managedReferenceValue as BlackboardKey;
			dropdown.tooltip = "Bind value to a BlackboardKey";
			dropdown.style.flexGrow = 1.0f;

			dropdown.RegisterCallback<MouseEnterEvent>(evt =>
			{
				dropdown.choices.Clear();

				foreach (BlackboardKey key in tree.Blackboard.Keys)
					if (propertyType.IsAssignableFrom(key.UnderlyingType))
						dropdown.choices.Add(key);

				dropdown.choices.Add(null);

				dropdown.choices.Sort((left, right) =>
				{
					if (left == null)
						return -1;

					if (right == null)
						return 1;

					return left.Name.CompareTo(right.Name);
				});
			});

			dropdown.RegisterCallback<ChangeEvent<BlackboardKey>>(evt =>
			{
				BlackboardKey newKey = evt.newValue;
				reference.managedReferenceValue = newKey;
				BehaviourTreeEditorWindow.Instance.CurrentSerializer.ApplyChanges();

				if (evt.newValue == null)
				{
					defaultValueField.style.display = DisplayStyle.Flex;
					dropdown.style.flexGrow = 0.0f;
				}
				else
				{
					defaultValueField.style.display = DisplayStyle.None;
					dropdown.style.flexGrow = 1.0f;
				}
			});

			defaultValueField.style.display = dropdown.value == null ? DisplayStyle.Flex : DisplayStyle.None;
			dropdown.style.flexGrow = dropdown.value == null ? 0.0f : 1.0f;

			var container = new VisualElement();
			container.AddToClassList("unity-base-field");
			container.AddToClassList("node-property-field");
			container.style.flexDirection = FlexDirection.Row;
			container.Add(label);
			container.Add(defaultValueField);
			container.Add(dropdown);

			return container;
		}


		private string FormatItem(BlackboardKey item)
		{
			return item == null
				? "[Inline]"
				: item.Name;
		}


		private string FormatSelectedItem(BlackboardKey item)
		{
			return item == null
				? ""
				: item.Name;
		}
	}


	[CustomPropertyDrawer(typeof(NodeProperty))]
	public class NodePropertyPropertyDrawer : PropertyDrawer
	{
		public override VisualElement CreatePropertyGUI(SerializedProperty property)
		{
			var tree = property.serializedObject.targetObject as BehaviourTreeMono;

			SerializedProperty reference = property.FindPropertyRelative("Reference");

			var dropdown = new PopupField<BlackboardKey>();
			dropdown.label = property.displayName;
			dropdown.formatListItemCallback = FormatItem;
			dropdown.formatSelectedValueCallback = FormatItem;
			dropdown.value = reference.managedReferenceValue as BlackboardKey;

			dropdown.RegisterCallback<MouseEnterEvent>(evt =>
			{
				dropdown.choices.Clear();

				foreach (BlackboardKey key in tree.Blackboard.Keys)
					dropdown.choices.Add(key);

				dropdown.choices.Sort((left, right) => { return left.Name.CompareTo(right.Name); });
			});

			dropdown.RegisterCallback<ChangeEvent<BlackboardKey>>(evt =>
			{
				BlackboardKey newKey = evt.newValue;
				reference.managedReferenceValue = newKey;
				BehaviourTreeEditorWindow.Instance.CurrentSerializer.ApplyChanges();
			});

			return dropdown;
		}


		private string FormatItem(BlackboardKey item)
		{
			return item == null
				? "(null)"
				: item.Name;
		}
	}
}