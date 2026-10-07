using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Editor.BehaviourTreeEditor.Templates
{
	public class TemplatesProviderWindow : EditorWindow
	{
		private string[] _filePaths;
		private Vector2 _scrollPosition;
		private string _searchFilter = "";
		private string _renameTarget = "";
		private string _newName = "";

		[MenuItem("Window/Templates Editor")]
		public static void ShowWindow()
		{
			GetWindow<TemplatesProviderWindow>("Templates Editor");
		}

		private void OnEnable()
		{
			RefreshFileList();
		}

		private void RefreshFileList()
		{
			_filePaths = TemplatesProvider.FindAllTemplates().Select(data => data.Path).ToArray();
		}

		private void OnGUI()
		{
			GUILayout.Label("Templates Editor", EditorStyles.boldLabel);

			// Search
			EditorGUILayout.BeginHorizontal();
			_searchFilter = EditorGUILayout.TextField("Search", _searchFilter);
			if (GUILayout.Button("Refresh", GUILayout.Width(80)))
			{
				RefreshFileList();
			}

			EditorGUILayout.EndHorizontal();

			// File list
			_scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

			var filteredFiles = _filePaths
				.Where(path => 
					string.IsNullOrEmpty(_searchFilter) 
					|| TemplatesProvider.GetFileNameWithoutExtension(path).ToLower().Contains(_searchFilter.ToLower())).ToList();

			foreach (var filePath in filteredFiles)
			{
				EditorGUILayout.BeginHorizontal();

				// If this file is being renamed
				if (_renameTarget == filePath)
				{
					_newName = EditorGUILayout.TextField(_newName);

					if (GUILayout.Button("Save", GUILayout.Width(60)))
					{
						RenameFile(filePath, _newName);
						_renameTarget = "";
						_newName = "";
					}

					if (GUILayout.Button("Cancel", GUILayout.Width(60)))
					{
						_renameTarget = "";
						_newName = "";
					}
				}
				else
				{
					// Display file name without extension
					EditorGUILayout.LabelField(TemplatesProvider.GetFileNameWithoutExtension(filePath));

					// Rename Button
					if (GUILayout.Button("Rename", GUILayout.Width(60)))
					{
						_renameTarget = filePath;
						_newName = TemplatesProvider.GetFileNameWithoutExtension(filePath);
					}

					// Delete Button
					if (GUILayout.Button("Delete", GUILayout.Width(60)))
					{
						if (UnityEditor.EditorUtility.DisplayDialog("Delete file",
								$"Are you sure you want to delete {TemplatesProvider.GetFileNameWithoutExtension(filePath)}?",
								"Delete", "Cancel"))
						{
							TemplatesProvider.DeleteTemplateAtPath(filePath);
							RefreshFileList();
						}
					}
				}

				EditorGUILayout.EndHorizontal();
			}

			EditorGUILayout.EndScrollView();
		}

		private void RenameFile(string oldPath, string newFileName)
		{
			if (TemplatesProvider.HasTemplate(newFileName))
			{
				UnityEditor.EditorUtility.DisplayDialog("Error",
					"A file with this name already exists!", "OK");
				return;
			}

			var error = TemplatesProvider.RenameTemplate(oldPath, newFileName);
			
			if (string.IsNullOrEmpty(error))
			{
				RefreshFileList();
			}
			else
			{
				UnityEditor.EditorUtility.DisplayDialog("Error",
					$"Failed to rename file: {error}", "OK");
			}
		}
	}
}