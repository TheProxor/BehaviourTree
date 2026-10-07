using Swat.Game.BehaviourTree.Blackboard;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using ConditionKey = Swat.Game.BehaviourTree.Blackboard.ConditionKey;


namespace Editor.BehaviourTreeEditor.PropertyDrawers
{
	[CustomPropertyDrawer(typeof(ConditionKey))]
	public class ConditionKeyPropertyDrawer : ConditionNodePropertyDrawer
	{
		public override VisualElement CreatePropertyGUI(SerializedProperty property)
		{
			VisualElement container = base.CreatePropertyGUI(property);

			SerializedProperty pairProp = property.FindPropertyRelative(nameof(ConditionKey.Pair));
			SerializedProperty comparerTypeProp = property.FindPropertyRelative(nameof(ConditionKey.ComparerType));

			var pairField = new PropertyField(pairProp);
			//pairField.style.marginTop = 15f;
			pairField.BindProperty(pairProp);

			var comparerTypeField = new PropertyField(comparerTypeProp);
			comparerTypeField.BindProperty(comparerTypeProp);
			comparerTypeField.style.display = DisplayStyle.None;

			var invertField = CreateInvertField(property);
			invertField.style.marginTop = 10f;

			var conditionTypeField = CreateConditionTypeField(property);

			var abortTypeField = CreateAbortTypeField(property);
			//abortTypeField.style.marginTop = 15f;

			pairField.RegisterValueChangeCallback(evt =>
			{
				comparerTypeField.style.display = evt.changedProperty.boxedValue is BlackboardKeyValuePair pair
												  && pair.Value != null
												  && pair.Value.UnderlyingType != typeof(bool)
												  && !pair.Value.UnderlyingType.IsSubclassOf(typeof(UnityEngine.Object))
					? DisplayStyle.Flex
					: DisplayStyle.None;
			});
			
			container.Add(pairField);
			container.Add(comparerTypeField);
			container.Add(invertField);
			container.Add(conditionTypeField);
			container.Add(abortTypeField);

			BehaviourTreeEditorWindow.Instance.CurrentSerializer.ApplyChanges();

			return container;
		}
	}
}