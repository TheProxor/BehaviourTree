using Swat.Game.BehaviourTree;
using UnityEditor;

namespace Editor.BehaviourTreeEditor
{
	public static class BehaviourTreeEditorContextMenu
	{
		// TODO: How to create context menu elements dynamically?
		// Problem: https://discussions.unity.com/t/genericmenu-used-as-context-inside-a-menuitem/583173
		//[MenuItem("CONTEXT/BehaviourTree/Apply Template")]
		public static void ApplyTemplate(MenuCommand command)
		{
			var contextMenu = new GenericMenu();
			var behaviourTree = command.context as BehaviourTreeMono;

			// foreach (BehaviourTreeTemplateType type in Enum.GetValues(typeof(BehaviourTreeTemplateType)))
			// {
			// 	if (type == BehaviourTreeTemplateType.None)
			// 		continue;
			//
			// 	contextMenu.AddItem(new GUIContent(type.ToString()), true, () => Debug.Log(type));
			// }

			contextMenu.ShowAsContext();
		}
	}
}