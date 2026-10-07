using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Editor.BehaviourTreeEditor.NodeInfoDrawers
{
	public class NestedTreeInfoDrawer : INodeInfoDrawer
	{
		public bool UseForChildren => true;


		public void Draw(SerializedProperty target, Label container)
		{
			SerializedProperty idProperty = target.FindPropertyRelative("NestedId.Id");
			container.BindProperty(idProperty);
		}
	}
}