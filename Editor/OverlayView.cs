using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Editor.BehaviourTreeEditor.Utils;
using Swat.Game.BehaviourTree;
using Swat.Game.BehaviourTree.Core;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace Editor.BehaviourTreeEditor
{
	[UxmlElement]
	public partial class OverlayView : VisualElement
	{
		private const string SceneModeButtonText = "Scene";
		private const string ProjectModeButtonText = "Project";
		
		public Action<IBehaviourTree> OnTreeSelected;

		private readonly string NameColumn = "Name";
		private readonly string PathColumn = "Path";

		private Button createButton;
		private Button modeButton;
		private VisualElement listViewContainer;
		private MultiColumnListView projectListView;
		
		private List<IBehaviourTree> treesOnScene;
		private List<IBehaviourTree> treesInProject;

		private bool _isProjectMode;
		private List<IBehaviourTree> TargetTrees => _isProjectMode ? treesInProject : treesOnScene;
		
		public void Show()
		{
			// Hidden in UIBuilder while editing...
			style.display = DisplayStyle.Flex;
			style.visibility = Visibility.Visible;

			// Configure fields
			createButton = this.Q<Button>("CreateButton");
			modeButton = this.Q<Button>("ModeButton");
			listViewContainer = this.Q<VisualElement>("ListViewContainer");

			// Find all behaviour trees
			treesOnScene = Object
				.FindObjectsByType<BehaviourTreeMono>(FindObjectsInactive.Include, FindObjectsSortMode.None)
				.Cast<IBehaviourTree>()
				.ToList();
			
			treesInProject = BtUtilsEditor
				.FindAllScriptableObjects<BehaviourTree>()
				.Cast<IBehaviourTree>()
				.ToList();

			// Configure create asset button
			createButton.clicked -= OnCreateAsset;
			createButton.clicked += OnCreateAsset;

			// Configure mode button
			modeButton.clicked -= OnModeChanged;
			modeButton.clicked += OnModeChanged;
			
			projectListView = CreateListView();
			listViewContainer.Clear();
			listViewContainer.Add(projectListView);
			projectListView.selectionChanged += OnSelectionChanged;

			RefreshModeButtonText();
		}


		public void Hide()
		{
			style.visibility = Visibility.Hidden;
		}


		public string ToMenuFormat(string one)
		{
			// Using the slash creates submenus...
			return one.Replace("/", "|");
		}


		public string ToAssetFormat(string one)
		{
			// Using the slash creates submenus...
			return one.Replace("|", "/");
		}


		private Column CreateColumn(string name)
		{
			var column = new Column();
			column.name = name;
			column.title = name;
			column.width = 100.0f;
			column.stretchable = true;
			return column;
		}


		private MultiColumnListView CreateListView()
		{
			var listView = new MultiColumnListView();
			listView.showBorder = true;
			listView.showAlternatingRowBackgrounds = AlternatingRowBackground.ContentOnly;
			listView.fixedItemHeight = 30.0f;
			listView.showBoundCollectionSize = false;
			listView.showAddRemoveFooter = false;
			listView.reorderable = false;
			listView.itemsSource = TargetTrees;

			listView.columns.Add(CreateColumn(NameColumn));
			listView.columns.Add(CreateColumn(PathColumn));

			listView.columns[NameColumn].makeCell = () => new Label();
			listView.columns[PathColumn].makeCell = () => new Label();

			listView.columns[NameColumn].bindCell = BindName;
			listView.columns[PathColumn].bindCell = BindPath;

			return listView;
		}


		private void BindName(VisualElement element, int index)
		{
			var label = element as Label;
			label.style.unityTextAlign = TextAnchor.MiddleLeft;
			string fileName = TargetTrees[index].Name;
			label.text = fileName;
		}


		private void BindPath(VisualElement element, int index)
		{
			var label = element as Label;
			label.style.unityTextAlign = TextAnchor.MiddleLeft;
			label.text = TargetTrees[index].GetFullPath();
		}

		private void OnModeChanged()
		{
			_isProjectMode = !_isProjectMode;
			RefreshModeButtonText();
			
			projectListView.itemsSource = TargetTrees;
		}

		private void RefreshModeButtonText()
		{
			modeButton.text = _isProjectMode ? ProjectModeButtonText : SceneModeButtonText;
		}


		private void OnSelectionChanged(IEnumerable<object> obj)
		{
			OnOpenAsset();
		}


		private void OnOpenAsset()
		{
			var tree = TargetTrees[projectListView.selectedIndex];

			if (tree == null)
				return;
			
			TreeSelected(tree);
			style.visibility = Visibility.Hidden;
		}


		private void OnCreateAsset()
		{
			BehaviourTree tree = EditorUtility.CreateNewTree();

			if (tree)
			{
				TreeSelected(tree);
				style.visibility = Visibility.Hidden;
			}
		}


		private void TreeSelected(IBehaviourTree tree)
		{
			OnTreeSelected.Invoke(tree);
		}
	}
}