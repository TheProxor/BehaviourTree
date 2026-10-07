using System;
using Editor.BehaviourTreeEditor.Serialization;
using Swat.Game.BehaviourTree;
using Swat.Game.BehaviourTree.Blackboard;
using Swat.Game.BehaviourTree.Core;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Editor.BehaviourTreeEditor.Blackboard
{
	[UxmlElement]
	public partial class BlackboardView : VisualElement
	{
		private SerializedBehaviourTree behaviourTree;

		private ListView listView;
		private TextField newKeyTextField;
		private PopupField<Type> newKeyTypeField;

		private Button createButton;


		public void ClearView()
		{
			behaviourTree = null;
			listView?.Unbind();
		}


		internal void Bind(SerializedBehaviourTree behaviourTree)
		{
			this.behaviourTree = behaviourTree;

			listView = this.Q<ListView>("ListView_Keys");
			listView.bindingPath = behaviourTree.BlackboardKeys.propertyPath;

			newKeyTextField = this.Q<TextField>("TextField_KeyName");
			var popupContainer = this.Q<VisualElement>("PopupField_Placeholder");

			createButton = this.Q<Button>("Button_KeyCreate");

			// ListView
			listView.Bind(behaviourTree.serializedObject);
			listView.RegisterCallback<KeyDownEvent>(e =>
			{
				if (e.keyCode == KeyCode.Delete)
					if (listView.selectedItem is SerializedProperty key)
						BehaviourTreeEditorWindow.Instance.CurrentSerializer.DeleteBlackboardKey(key.displayName);
			});

			// NewKeyTypeField
			newKeyTypeField = new PopupField<Type>();
			newKeyTypeField.label = "Type";
			newKeyTypeField.formatListItemCallback = FormatItem;
			newKeyTypeField.formatSelectedValueCallback = FormatItem;

			TypeCache.TypeCollection types = TypeCache.GetTypesDerivedFrom<BlackboardKey>();

			foreach (Type type in types)
			{
				if (type.IsGenericType)
					continue;

				newKeyTypeField.choices.Add(type);
				newKeyTypeField.value ??= type;
			}

			popupContainer.Clear();
			popupContainer.Add(newKeyTypeField);

			// TextField
			newKeyTextField.RegisterCallback<ChangeEvent<string>>(_ => { ValidateButton(); });

			// Button
			createButton.clicked -= CreateNewKey;
			createButton.clicked += CreateNewKey;

			ValidateButton();
		}


		private string FormatItem(Type arg)
		{
			return arg == null
				? "(null)"
				: arg.Name.Replace("Key", "");
		}


		private void ValidateButton()
		{
			// Disable the create button if trying to create a non-unique key
			bool isValidKeyText = ValidateKeyText(newKeyTextField.text);
			createButton.SetEnabled(isValidKeyText);
		}


		private bool ValidateKeyText(string text)
		{
			if (string.IsNullOrEmpty(text))
				return false;

			var tree = behaviourTree.Blackboard.serializedObject.targetObject as IBehaviourTree;
			bool keyExists = tree.Blackboard.Find(newKeyTextField.text) != null;
			return !keyExists;
		}


		private void CreateNewKey()
		{
			Type newKeyType = newKeyTypeField.value;

			if (newKeyType != null)
				behaviourTree.CreateBlackboardKey(newKeyTextField.text, newKeyType);

			ValidateButton();
		}
	}
}