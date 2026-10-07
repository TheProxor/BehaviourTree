using Editor.BehaviourTreeEditor.Serialization;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Editor.BehaviourTreeEditor
{
	[UxmlElement]
	public partial class InspectorView : VisualElement
	{
		internal void UpdateSelection(SerializedBehaviourTree serializer, NodeView nodeView)
		{
			Clear();

			if (nodeView == null)
				return;

			SerializedProperty nodeProperty = serializer.FindNode(nodeView.Node);

			if (nodeProperty == null)
				return;

			// Auto-expand the property
			nodeProperty.isExpanded = true;

			// Property field
			var field = new PropertyField();
			field.label = nodeProperty.managedReferenceValue.GetType().ToString();
			field.BindProperty(nodeProperty);

			Add(field);
		}
	}
}