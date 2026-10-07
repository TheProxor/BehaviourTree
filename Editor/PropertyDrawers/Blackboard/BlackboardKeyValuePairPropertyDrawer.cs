using Editor.BehaviourTreeEditor.Utils;
using Editor.BehaviourTreeEditor.Utils.Extensions;
using Swat.Game.BehaviourTree;
using Swat.Game.BehaviourTree.Blackboard;
using Swat.Game.BehaviourTree.Core;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Editor.BehaviourTreeEditor.PropertyDrawers
{
	[CustomPropertyDrawer(typeof(BlackboardKeyValuePair))]
	public class BlackboardKeyValuePairPropertyDrawer : PropertyDrawer
	{
		public override VisualElement CreatePropertyGUI(SerializedProperty property)
		{
			var tree = IsInSubTree(property, out var subTree) 
				? subTree.TreeAsset 
				: BtUtilsEditor.GetBehaviourTree(property);

			return CreateBlackboardGUI(property, tree);
		}

		private static bool IsInSubTree(SerializedProperty property, out SubTree subTree)
		{
			var pairArrayProp = property.GetParentProperty();
			var subTreeProp = pairArrayProp.GetParentProperty();

			if (subTreeProp.propertyType == SerializedPropertyType.ManagedReference)
			{
				subTree = subTreeProp.managedReferenceValue as SubTree;
				return subTree != null;
			}

			subTree = null;
			return false;
		}

		public VisualElement CreateBlackboardGUI(SerializedProperty property, IBehaviourTree tree)
		{
			SerializedProperty first = property.FindPropertyRelative(nameof(BlackboardKeyValuePair.Key));
			SerializedProperty second = property.FindPropertyRelative(nameof(BlackboardKeyValuePair.Value));
			
			var pairContainer = new VisualElement();

			var dropdown = new PopupField<BlackboardKey>();
			dropdown.label = first.displayName;
			dropdown.formatListItemCallback = FormatItem;
			dropdown.formatSelectedValueCallback = FormatItem;
			dropdown.value = first.managedReferenceValue as BlackboardKey;


			dropdown.RegisterCallback<MouseEnterEvent>(evt =>
			{
				dropdown.choices.Clear();

				foreach (BlackboardKey key in tree.Blackboard.Keys)
					dropdown.choices.Add(key);
			});

			dropdown.RegisterCallback<ChangeEvent<BlackboardKey>>(evt =>
			{
				BlackboardKey newKey = evt.newValue;
				first.managedReferenceValue = newKey;
				property.serializedObject.ApplyModifiedProperties();

				if (pairContainer.childCount > 1)
					pairContainer.RemoveAt(1);

				if (second.managedReferenceValue == null ||
					second.managedReferenceValue.GetType() != dropdown.value.GetType())
				{
					second.managedReferenceValue = BlackboardKey.CreateKey(dropdown.value.GetType());
					second.serializedObject.ApplyModifiedProperties();
				}

				var field = new PropertyField();
				field.label = second.displayName;
				field.BindProperty(second.FindPropertyRelative(nameof(BlackboardKey<object>.Value)));
				pairContainer.Add(field);
			});

			pairContainer.Add(dropdown);

			if (dropdown.value != null)
			{
				if (second.managedReferenceValue == null ||
					first.managedReferenceValue.GetType() != second.managedReferenceValue.GetType())
				{
					second.managedReferenceValue = BlackboardKey.CreateKey(dropdown.value.GetType());
					second.serializedObject.ApplyModifiedProperties();
				}

				var field = new PropertyField();
				field.label = second.displayName;
				field.BindProperty(second.FindPropertyRelative(nameof(BlackboardKey<object>.Value)));
				pairContainer.Add(field);
			}

			return pairContainer;
		}


		private string FormatItem(BlackboardKey item)
		{
			return item == null
				? "(null)"
				: item.Name;
		}
	}
}