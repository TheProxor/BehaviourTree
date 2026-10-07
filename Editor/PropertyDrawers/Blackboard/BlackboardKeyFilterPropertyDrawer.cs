using System.Linq;
using Editor.BehaviourTreeEditor.Utils;
using Swat.Game.BehaviourTree.Blackboard;
using UnityEditor;
using UnityEngine.UIElements;

namespace Editor.BehaviourTreeEditor.PropertyDrawers
{
	[CustomPropertyDrawer(typeof(BlackboardKeyFilter<>))]
	public class BlackboardKeyFilterPropertyDrawer : PropertyDrawer
	{
		public override VisualElement CreatePropertyGUI(SerializedProperty property)
		{
			var container = new VisualElement();
			var tree = BtUtilsEditor.GetBehaviourTree(property);

			var key = property.FindPropertyRelative(nameof(BlackboardKeyFilter<object>.Key));
			var genericType = property.boxedValue.GetType().GenericTypeArguments.First();

			var dropdown = new PopupField<BlackboardKey>();
			dropdown.label = property.name;
			dropdown.formatListItemCallback = FormatItem;
			dropdown.formatSelectedValueCallback = FormatItem;
			dropdown.value = key.managedReferenceValue as BlackboardKey;

			dropdown.RegisterCallback<MouseEnterEvent>(evt =>
			{
				dropdown.choices.Clear();

				foreach (BlackboardKey key in tree.Blackboard.Keys.Where(k => k.UnderlyingType == genericType))
					dropdown.choices.Add(key);
			});

			dropdown.RegisterCallback<ChangeEvent<BlackboardKey>>(evt =>
			{
				BlackboardKey newKey = evt.newValue;
				key.managedReferenceValue = newKey;
				property.serializedObject.ApplyModifiedProperties();

				if (container.childCount > 1)
					container.RemoveAt(1);
			});

			container.Add(dropdown);

			return container;
		}

		private string FormatItem(BlackboardKey item)
		{
			return item == null
				? "(null)"
				: item.Name;
		}
	}
}