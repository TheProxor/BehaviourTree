using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Editor.BehaviourTreeEditor.PropertyDrawers
{
	public abstract class AbortableNodePropertyDrawer : NodeBasePropertyDrawer
	{
		protected VisualElement CreateAbortTypeField(SerializedProperty relativeProperty)
		{
			SerializedProperty abortTypeProp = relativeProperty.FindPropertyRelative("AbortType");

			var abortTypeField = new EnumField("Abort Type");
			abortTypeField.BindProperty(abortTypeProp);
			return abortTypeField;
		}
	}
}