using System.Collections.Generic;
using System.Linq;
using Editor.BehaviourTreeEditor.Configuration;
using Editor.BehaviourTreeEditor.DialogWindow.Core;
using Editor.BehaviourTreeEditor.Templates;
using Swat.Game.BehaviourTree;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Editor.BehaviourTreeEditor.DialogWindow
{
	[UxmlElement]
	public partial class NewTemplateDialogView : DialogView
	{
		private const int MaxTemplateNameLength = 50;
		
		private IReadOnlyList<ISelectable> _selection;
		private Label _inputInfo;

		public void OpenWithSelection(IReadOnlyList<ISelectable> selection)
		{
			_selection = selection;
			_inputInfo = this.Q<Label>("InputInfo");
			
			Open();

			_inputInfo.style.display = DisplayStyle.None;
		}

		protected override void OnConfirm()
		{
			var enteredName = _textField.value;

			if (TemplatesProvider.HasTemplate(enteredName))
			{
				SetInputInfo("This name already exists.", false);
				return;
			}
			
			if (string.IsNullOrEmpty(enteredName))
			{
				SetInputInfo("Name cannot be empty.", false);
				return;
			}

			if (enteredName.Length > MaxTemplateNameLength)
			{
				SetInputInfo("Name is too long.", false);
				return;
			}
			
			var nodes = _selection
				.Where(s => s is NodeView)
				.Cast<NodeView>()
				.Select(nv => nv.Node)
				.Where(n => n is not RootNode)
				.ToArray();

			if (nodes.Length == 0)
			{
				SetInputInfo("No nodes selected.", false);
				return;
			}

			SetInputInfo("Saving template...", true);

			_confirmButton.SetEnabled(false);
			
			EditorApplication.delayCall +=
				() =>
				{
					TemplatesProvider.SaveTemplate(enteredName, nodes);
					Close();
				};
		}

		private void SetInputInfo(string text, bool isValid)
		{
			_inputInfo.style.display = DisplayStyle.Flex;
			_inputInfo.text = text;
			_inputInfo.style.color = isValid ? new Color(0, 1f, 0.4f) : new Color(1, 0.2f, 0.2f);
		}
	}
}