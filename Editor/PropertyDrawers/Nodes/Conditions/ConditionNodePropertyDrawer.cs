using Swat.Game.BehaviourTree;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Editor.BehaviourTreeEditor.PropertyDrawers
{
	public abstract class ConditionNodePropertyDrawer : AbortableNodePropertyDrawer
	{
		protected VisualElement CreateInvertField(SerializedProperty relativeProperty)
		{
			SerializedProperty invertProp = relativeProperty.FindPropertyRelative(nameof(ConditionNode.Invert));

			var invertField = new Toggle("Invert");
			invertField.BindProperty(invertProp);

			return invertField;
		}

		protected VisualElement CreateConditionTypeField(SerializedProperty relativeProperty)
		{
			SerializedProperty conditionTypeProp =
				relativeProperty.FindPropertyRelative(nameof(ConditionNode.ConditionType));

			var conditionTypeField = new EnumField("Condition Type");
			conditionTypeField.BindProperty(conditionTypeProp);

			return conditionTypeField;
		}
	}
}