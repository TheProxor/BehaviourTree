using System;
using UnityEngine.UIElements;

namespace Editor.BehaviourTreeEditor.DialogWindow.Core
{
	public abstract class DialogView : VisualElement
	{
		protected Label _titleLabel;
		protected TextField _textField;
		protected Button _confirmButton;
		private VisualElement _background;

		public void Open()
		{
			_titleLabel = this.Q<Label>("Title");
			_textField = this.Q<TextField>("TextField");
			_confirmButton = this.Q<Button>();
			_background = this.Q<VisualElement>("Background");
			
			_confirmButton.clicked -= OnConfirm;
			_confirmButton.clicked += OnConfirm;
			_confirmButton.SetEnabled(true);

			_background.RegisterCallback<PointerDownEvent>(e =>
			{
				e.StopImmediatePropagation();
				Close();
			});
			
			style.visibility = Visibility.Visible;
		}

		public void Close()
		{
			style.visibility = Visibility.Hidden;
		}

		protected abstract void OnConfirm();
	}
}