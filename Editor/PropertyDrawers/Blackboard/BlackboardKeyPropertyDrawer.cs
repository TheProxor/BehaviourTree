using Swat.Game.BehaviourTree.Blackboard;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Editor.BehaviourTreeEditor.PropertyDrawers
{
	[CustomPropertyDrawer(typeof(BlackboardKey))]
	public class BlackboardKeyPropertyDrawer : PropertyDrawer
	{
		public override VisualElement CreatePropertyGUI(SerializedProperty property)
		{
			if (property.propertyType == SerializedPropertyType.ArraySize)
				return null;
			
			var keyName = new Label();
			var renameField = new TextField();
			var keyValue = new PropertyField();
			var container = new VisualElement();

			keyName.bindingPath = nameof(BlackboardKey.Name);
			keyName.AddToClassList("unity-base-field__label");

			renameField.style.display = DisplayStyle.None;
			renameField.bindingPath = nameof(BlackboardKey.Name);
			renameField.selectAllOnFocus = false;
			renameField.selectAllOnMouseUp = false;
			renameField.RegisterCallback<FocusOutEvent>(evt =>
			{
				keyValue.style.display = DisplayStyle.Flex;
				keyName.style.display = DisplayStyle.Flex;
				renameField.style.display = DisplayStyle.None;
			});

			keyValue.label = "";
			keyValue.style.flexGrow = 1.0f;
			keyValue.bindingPath = nameof(BlackboardKey<object>.Value);

			container.AddManipulator(new ContextualMenuManipulator(evt =>
			{
				evt.menu.AppendAction("Delete",
					x => BehaviourTreeEditorWindow.Instance.CurrentSerializer.DeleteBlackboardKey(
						property.displayName), DropdownMenuAction.AlwaysEnabled);
			}));

			container.style.flexDirection = FlexDirection.Row;
			container.Add(keyName);
			container.Add(renameField);
			container.Add(keyValue);

			keyName.RegisterCallback<MouseDownEvent>(evt =>
			{
				if (evt.clickCount < 2)
					return;
					
				renameField.value = keyName.text;
				renameField.style.display = DisplayStyle.Flex;
				renameField.RegisterCallbackOnce<GeometryChangedEvent>(_ => renameField.Focus());
						
				keyValue.style.display = DisplayStyle.None;
				keyName.style.display = DisplayStyle.None;
			});

			return container;
		}
	}
}