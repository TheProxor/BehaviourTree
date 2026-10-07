using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Editor.BehaviourTreeEditor.PropertyDrawers
{
	public abstract class NodeBasePropertyDrawer : PropertyDrawer
	{
		public override VisualElement CreatePropertyGUI(SerializedProperty property)
		{
			var container = new VisualElement();

			SerializedProperty enabledProp = property.FindPropertyRelative("Enabled");
			SerializedProperty descriptionProp = property.FindPropertyRelative("Description");
			SerializedProperty drawGizmosProp = property.FindPropertyRelative("DrawGizmos");

			if (enabledProp != null)
			{
				var enabledField = new PropertyField(enabledProp);
				enabledField.BindProperty(enabledProp);
				container.Add(enabledField);
			}

			var descriptionField = new PropertyField(descriptionProp);
			descriptionField.style.marginTop = 5f;
			descriptionField.BindProperty(descriptionProp);

			var drawGizmosField = new Toggle("Draw Gizmos");
			drawGizmosField.style.marginBottom = 15f;
			drawGizmosField.BindProperty(drawGizmosProp);
			
			container.Add(descriptionField);
			//container.Add(drawGizmosField);

			return container;
		}
	}
}