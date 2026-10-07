using System.IO;
using Editor.BehaviourTreeEditor.DialogWindow.Core;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Editor.BehaviourTreeEditor.DialogWindow
{
	[UxmlElement]
	public partial class NewScriptDialogView : DialogView
	{
		private EditorUtility.ScriptTemplate _scriptTemplate;
		private NodeView _source;
		private bool _isSourceParent;
		private Vector2 _nodePosition;

		public void CreateScript(EditorUtility.ScriptTemplate scriptTemplate, NodeView source, bool isSourceParent,
			Vector2 position)
		{
			_scriptTemplate = scriptTemplate;
			_source = source;
			_isSourceParent = isSourceParent;
			_nodePosition = position;

			Open();

			_titleLabel.text = $"New {scriptTemplate.subFolder.TrimEnd('s')} Script";
			_textField.focusable = true;

			RegisterCallback<PointerEnterEvent>(e => { _textField[0].Focus(); });

			_textField.RegisterCallback<KeyDownEvent>(e =>
			{
				if (e.keyCode == KeyCode.Return || e.keyCode == KeyCode.KeypadEnter)
					OnConfirm();
			});

			_confirmButton.clicked -= OnConfirm;
			_confirmButton.clicked += OnConfirm;
		}

		protected override void OnConfirm()
		{
			string scriptName = _textField.text;

			var newNodePath = $"{BehaviourTreeEditorWindow.Instance.settings.newNodePath}";

			if (AssetDatabase.IsValidFolder(newNodePath))
			{
				string destinationFolder = Path.Combine(newNodePath, _scriptTemplate.subFolder);
				string destinationPath = Path.Combine(destinationFolder, $"{scriptName}.cs");

				Directory.CreateDirectory(destinationFolder);

				DirectoryInfo parentPath = Directory.GetParent(Application.dataPath);

				string templateString = _scriptTemplate.templateFile.text;
				templateString = templateString.Replace("#SCRIPTNAME#", scriptName);
				string scriptPath = Path.Combine(parentPath.ToString(), destinationPath);

				if (!File.Exists(scriptPath))
				{
					File.WriteAllText(scriptPath, templateString);

					// TODO: There must be a better way to survive domain reloads after script compiling than this
					BehaviourTreeEditorWindow.Instance.pendingScriptCreate.pendingCreate = true;
					BehaviourTreeEditorWindow.Instance.pendingScriptCreate.scriptName = scriptName;
					BehaviourTreeEditorWindow.Instance.pendingScriptCreate.nodePosition = _nodePosition;

					if (_source != null)
					{
						BehaviourTreeEditorWindow.Instance.pendingScriptCreate.sourceGuid = _source.Node.Guid;
						BehaviourTreeEditorWindow.Instance.pendingScriptCreate.isSourceParent = _isSourceParent;
					}

					AssetDatabase.Refresh();
					_confirmButton.SetEnabled(false);
					EditorApplication.delayCall += WaitForCompilation;
				}
				else
				{
					Debug.LogError($"Script with that name already exists:{scriptPath}");
					Close();
				}
			}
			else
			{
				Debug.LogError(
					$"Invalid folder path:{newNodePath}. Check the project configuration settings 'newNodePath' is configured to a valid folder");
			}
		}


		private void WaitForCompilation()
		{
			if (EditorApplication.isCompiling)
			{
				EditorApplication.delayCall += WaitForCompilation;
				return;
			}

			_confirmButton.SetEnabled(true);
			Close();
		}
	}
}