using Swat.Game.BehaviourTree;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Editor.BehaviourTreeEditor.PropertyDrawers
{
	[CustomPropertyDrawer(typeof(SubTree))]
	public class SubTreePropertyDrawer : NodeBasePropertyDrawer
	{
		public override VisualElement CreatePropertyGUI(SerializedProperty property)
		{
			var container = base.CreatePropertyGUI(property);
			
			var subTree = property.boxedValue as SubTree;

			var treeAssetProp = property.FindPropertyRelative(nameof(SubTree.TreeAsset));
			var blackboardOverridesProp = property.FindPropertyRelative(nameof(SubTree.BlackboardOverrides));

			var treeAssetField = new PropertyField(treeAssetProp);
			treeAssetField.BindProperty(treeAssetProp);
			
			var blackboardOverridesField = new PropertyField(blackboardOverridesProp);
			blackboardOverridesField.BindProperty(blackboardOverridesProp);
			
			container.Add(treeAssetField);
			container.Add(blackboardOverridesField);

			treeAssetField.RegisterValueChangeCallback(evt =>
			{
				blackboardOverridesField.style.display = evt.changedProperty.boxedValue != null ? DisplayStyle.Flex : DisplayStyle.None;
			});
			
			BehaviourTreeEditorWindow.Instance.CurrentSerializer.ApplyChanges();
			
			return container;
		}
	}
}