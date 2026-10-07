using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Editor.BehaviourTreeEditor.Blackboard;
using Editor.BehaviourTreeEditor.Configuration;
using Editor.BehaviourTreeEditor.Serialization;
using Editor.BehaviourTreeEditor.Templates;
using Editor.BehaviourTreeEditor.Utils;
using Editor.BehaviourTreeEditor.Utils.Extensions;
using Swat.Game.BehaviourTree;
using Swat.Game.BehaviourTree.Core;
using Unity.Profiling;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.SceneManagement;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;


namespace Editor.BehaviourTreeEditor
{
	public class BehaviourTreeEditorWindow : EditorWindow
	{
		public static BehaviourTreeEditorWindow Instance;

		[SerializeField] public PendingScriptCreate pendingScriptCreate = new();

		public BehaviourTreeProjectSettings settings;
		public VisualTreeAsset behaviourTreeXml;
		public VisualTreeAsset nodeXml;
		public StyleSheet behaviourTreeStyle;
		public TextAsset scriptTemplateActionNode;
		public TextAsset scriptTemplateConditionNode;
		public TextAsset scriptTemplateCompositeNode;
		public TextAsset scriptTemplateDecoratorNode;


		public InspectorView inspectorView;
		public BlackboardView blackboardView;
		public OverlayView overlayView;
		public ToolbarMenu toolbarMenu;
		public Label versionLabel;
		public DialogWindow.NewScriptDialogView newScriptDialog;
		public DialogWindow.NewTemplateDialogView newTemplateDialog;
		public TabView tabView;
		public BehaviourTreeEditorWindowState windowState;

		public Vector2 LastTabViewMouseWindowPosition;
		
		private int _lastSceneHash;


		public BehaviourTreeView CurrentTreeView =>
			tabView.activeTab is TreeViewTab activeTab ? activeTab.TreeView : null;

		public IBehaviourTree CurrentTree =>
			tabView.activeTab is TreeViewTab activeTab ? activeTab.Serializer.tree : null;

		public SerializedBehaviourTree CurrentSerializer =>
			tabView.activeTab is TreeViewTab activeTab ? activeTab.Serializer : null;


		private static readonly ProfilerMarker editorUpdate = new("BehaviourTree.EditorUpdate");


		[MenuItem("BehaviourTree/Open", false, 0)]
		public static void Open()
		{
			OpenWindow();
		}
		
		[MenuItem("BehaviourTree/Find Active", false, 100)]
		public static void FindActive()
		{
			FindTree(bt => bt.TreeState == NodeState.Running);
		}


		[MenuItem("BehaviourTree/Find All", false, 101)]
		public static void FindAll()
		{
			FindTree(_ => true);
		}


		public static BehaviourTreeEditorWindow OpenWindow()
		{
			var wnd = GetWindow<BehaviourTreeEditorWindow>();
			wnd.titleContent = new GUIContent("BehaviourTreeEditor");
			wnd.minSize = new Vector2(800, 600);
			return wnd;
		}
		
		public static BehaviourTreeEditorWindow OpenWindow(IBehaviourTree tree)
		{
			var wnd = OpenWindow();
			wnd.NewTab(tree, true);
			return wnd;
		}


		[OnOpenAsset]
		public static bool OnOpenAsset(int instanceId, int line)
		{
			if (Selection.activeObject is BehaviourTreeMono)
			{
				OpenWindow(Selection.activeObject as BehaviourTreeMono);
				return true;
			}

			return false;
		}


		private void LoadDefaultResources()
		{
			var folder = Path.GetDirectoryName(AssetDatabase.GetAssetPath(MonoScript.FromScriptableObject(this))).Replace('\\', '/');
			behaviourTreeXml ??= AssetDatabase.LoadAssetAtPath<VisualTreeAsset>($"{folder}/UIBuilder/BehaviourTreeEditor.uxml");
			nodeXml ??= AssetDatabase.LoadAssetAtPath<VisualTreeAsset>($"{folder}/UIBuilder/NodeView.uxml");
			behaviourTreeStyle ??= AssetDatabase.LoadAssetAtPath<StyleSheet>($"{folder}/UIBuilder/BehaviourTreeEditorStyle.uss");
			scriptTemplateActionNode ??= AssetDatabase.LoadAssetAtPath<TextAsset>($"{folder}/ScriptTemplates/ActionNodeTemplate.txt");
			scriptTemplateConditionNode ??= AssetDatabase.LoadAssetAtPath<TextAsset>($"{folder}/ScriptTemplates/ConditionNodeTemplate.txt");
			scriptTemplateCompositeNode ??= AssetDatabase.LoadAssetAtPath<TextAsset>($"{folder}/ScriptTemplates/CompositeNodeTemplate.txt");
			scriptTemplateDecoratorNode ??= AssetDatabase.LoadAssetAtPath<TextAsset>($"{folder}/ScriptTemplates/DecoratorNodeTemplate.txt");
		}

		public void CreateGUI()
		{
			Instance = this;
			LoadDefaultResources();
			settings = BehaviourTreeProjectSettings.GetOrCreateSettings();
			windowState = settings.windowState;

			// Each editor window contains a root VisualElement object
			VisualElement root = rootVisualElement;

			// Import UXML
			VisualTreeAsset visualTree = behaviourTreeXml;
			visualTree.CloneTree(root);

			// A stylesheet can be added to a VisualElement.
			// The style will be applied to the VisualElement and all of its children.
			StyleSheet styleSheet = behaviourTreeStyle;
			root.styleSheets.Add(styleSheet);

			// Main treeview
			inspectorView = root.Q<InspectorView>();
			blackboardView = root.Q<BlackboardView>();
			toolbarMenu = root.Q<ToolbarMenu>();
			overlayView = root.Q<OverlayView>("OverlayView");
			newScriptDialog = root.Q<DialogWindow.NewScriptDialogView>("NewScriptDialogView");
			newTemplateDialog = root.Q<DialogWindow.NewTemplateDialogView>("NewTemplateDialogView");
			tabView = root.Q<TabView>();
			tabView.activeTabChanged -= OnTabChanged;
			tabView.activeTabChanged += OnTabChanged;

			versionLabel = root.Q<Label>("Version");

			// Capture mouse position
			tabView.contentContainer.RegisterCallback<MouseMoveEvent>(CaptureMousePosition);
			tabView.contentContainer.RegisterCallback<MouseEnterEvent>(CaptureMousePosition);
			tabView.contentContainer.RegisterCallback<MouseLeaveEvent>(CaptureMousePosition);
			
			// TabView context menu
			var headerContainer = tabView.Q(TabView.headerContainerClassName);
			headerContainer.AddManipulator(new ContextualMenuManipulator(evt =>
			{
				evt.menu.AppendAction("Close all tabs", _ => CloseAllTabs());
			}));

			headerContainer.RegisterCallback<MouseDownEvent>(evt => 
			{
				// Disable default selection on right click
				if (evt.button == (int)MouseButton.RightMouse)
					evt.StopPropagation();
				
			}, TrickleDown.TrickleDown);

			// Toolbar assets menu
			toolbarMenu.RegisterCallback<MouseEnterEvent>(evt =>
			{
				// Refresh the menu options just before it's opened (on mouse enter)
				toolbarMenu.menu.MenuItems().Clear();
				toolbarMenu.menu.AppendAction("Open", _ => overlayView.Show());
				toolbarMenu.menu.AppendSeparator();
				toolbarMenu.menu.AppendAction("Templates provider", _ => TemplatesProviderWindow.ShowWindow());
				toolbarMenu.menu.AppendSeparator();
				toolbarMenu.menu.AppendAction("Save selected to template", _ => EditorUtility.OpenNewTemplateDialog(CurrentTreeView.selection));
				
				List<string> behaviourTrees = EditorUtility.GetAssetPaths<BehaviourTreeMono>();

				behaviourTrees.ForEach(path =>
				{
					string fileName = Path.GetFileName(path);

					toolbarMenu.menu.AppendAction($"{fileName}", a =>
					{
						var tree = AssetDatabase.LoadAssetAtPath<BehaviourTreeMono>(path);
						NewTab(tree, true);
					});
				});

				if (EditorApplication.isPlaying)
				{
					toolbarMenu.menu.AppendSeparator();

					Object[] behaviourTreeInstances = Resources.FindObjectsOfTypeAll(typeof(BehaviourTreeMono));

					foreach (Object instance in behaviourTreeInstances)
					{
						var behaviourTreeInstance = instance as BehaviourTreeMono;
						GameObject gameObject = behaviourTreeInstance.gameObject;

						if (behaviourTreeInstance != null && gameObject.scene != null && gameObject.scene.name != null)
							toolbarMenu.menu.AppendAction($"{gameObject.name} [{behaviourTreeInstance.name}]", a =>
							{
								NewTab(behaviourTreeInstance, true);
								Selection.activeObject = gameObject;
							});
					}
				}

				// toolbarMenu.menu.AppendSeparator();
				// toolbarMenu.menu.AppendAction("New Tree...", (a) => OnToolbarNewAsset());
			});

			// Version label
			EditorUtility.PackageManifest packageManifest = EditorUtility.GetPackageManifest();

			if (packageManifest != null)
				versionLabel.text = $"v {packageManifest.version}";

			// Overlay view
			overlayView.OnTreeSelected -= t => NewTab(t, true);
			overlayView.OnTreeSelected += t => NewTab(t, true);

			// New Script Dialog
			newScriptDialog.Close();
			newTemplateDialog.Close();

			// Restore window state between compilations
			windowState.Restore(this);

			// Create new node for any scripts just created coming back from a compile.
			if (pendingScriptCreate != null && pendingScriptCreate.pendingCreate)
				CreatePendingScriptNode();
			
			// Record active scene
			_lastSceneHash = SceneManager.GetActiveScene().GetHashCode();
		}

		private void CaptureMousePosition<T>(T evt) where T : IMouseEvent
		{
			LastTabViewMouseWindowPosition = tabView.contentContainer.LocalToWorld(evt.localMousePosition);
		}

		private void CloseAllTabs()
		{
			var tabs = tabView.Query<TreeViewTab>();
			tabs.ForEach(t => t.Close());

			windowState.CloseAllTabs();
		}


		public void OnTabClosed(Tab tab)
		{
			var treeTab = tab as TreeViewTab;

			if (!treeTab.IsRuntimeTab)
				windowState.TabClosed(treeTab);

			CheckTheSameTabNames(treeTab.name);
		}


		public void NewTab(IBehaviourTree newTree, bool focus)
		{
			// Check for existing tab
			var tabs = tabView.Query<TreeViewTab>();
			var treeViewTabs = tabs.ToList();
			var existingTab = treeViewTabs.FirstOrDefault(t => t.InstanceID == newTree.GetInstanceID()); 
			var tabName = newTree.Name;

			if (existingTab != null)
			{
				// Focus existing tab
				if (focus)
					tabView.activeTab = existingTab;

				return;
			}
			
			// Otherwise create a new tab
			var newTab = new TreeViewTab(newTree, behaviourTreeStyle, tabName);
			tabView.Add(newTab);
			
			CheckTheSameTabNames(tabName);

			// Record active tab
			windowState.TabOpened(newTab);

			// And Focus
			if (focus)
			{
				tabView.activeTab = newTab;
				overlayView?.Hide();
			}
		}
		
		private void CheckTheSameTabNames(string changedTabName)
		{
			var tabs = tabView.Query<TreeViewTab>();
			var sameNameTabs = tabs.Where(t => string.Equals(t.name, changedTabName)).ToList();

			if (sameNameTabs.Count > 1)
			{
				foreach (var tab in sameNameTabs)
					tab.label = tab.Tree.GetFullPath();
			}
			else
			{
				foreach (var tab in sameNameTabs)
					tab.label = tab.name;
			}
		}


		public void InspectNode(SerializedBehaviourTree serializer, NodeView node)
		{
			inspectorView.UpdateSelection(serializer, node);
		}

		public Vector2 ScreenMouseToGraphPosition(Vector2 screenMousePosition)
		{
			return WindowMouseToGraphPosition(screenMousePosition - this.position.position);
		}

		public Vector2 WindowMouseToGraphPosition(Vector2 screenMousePosition)
		{
			Vector2 windowMousePosition = rootVisualElement
				.ChangeCoordinatesTo(rootVisualElement.parent, screenMousePosition);

			Vector2 graphMousePosition = CurrentTreeView.contentViewContainer
				.WorldToLocal(windowMousePosition);

			return graphMousePosition;
		}
		
		private static void FindTree(Func<BehaviourTreeMono, bool> predicate)
		{
			Object[] runningTrees = FindObjectsByType<BehaviourTreeMono>(FindObjectsSortMode.None)
				.Where(predicate)
				.Select(behaviourTree => behaviourTree.gameObject)
				.Cast<Object>()
				.ToArray();

			if (runningTrees.Length == 0)
			{
				Debug.Log("No running trees found on scene");
				return;
			}

			if (runningTrees.Length == 1)
			{
				var bt = ((GameObject)runningTrees.First())
					.GetComponent<BehaviourTreeMono>();

				OpenWindow(bt);
			}

			Selection.objects = runningTrees;
		}


		private void OnTabChanged(Tab previous, Tab current)
		{
			var newTab = current as TreeViewTab;
			inspectorView.Clear();
			blackboardView?.Bind(newTab.Serializer);

			if (!newTab.IsRuntimeTab)
				windowState.TabChanged(tabView.selectedTabIndex);
			
			Selection.objects = new Object[] { newTab.Tree.Object };
		}


		private void CreatePendingScriptNode()
		{
			// #TODO: Unify this with CreateNodeWindow.CreateNode

			if (CurrentTreeView == null)
				return;

			var source = CurrentTreeView.GetNodeByGuid(pendingScriptCreate.sourceGuid) as NodeView;
			var nodeType = TypeCache.GetTypesDerivedFrom<Node>().FirstOrDefault(type => !type.IsAbstract && !type.ContainsGenericParameters && type.Name == pendingScriptCreate.scriptName);

			if (nodeType != null)
			{
				NodeView createdNode;

				if (source != null)
				{
					if (pendingScriptCreate.isSourceParent)
						createdNode = CurrentTreeView.CreateNode(nodeType, pendingScriptCreate.nodePosition, source);
					else
						createdNode =
							CurrentTreeView.CreateNodeWithChild(nodeType, pendingScriptCreate.nodePosition, source);
				}
				else
				{
					createdNode = CurrentTreeView.CreateNode(nodeType, pendingScriptCreate.nodePosition, null);
				}

				CurrentTreeView.SelectNode(createdNode);
				EditorUtility.OpenScriptInEditor(createdNode);
			}

			pendingScriptCreate.Reset();
		}


		private void OnUndoRedo()
		{
			if (CurrentTree != null)
			{
				CurrentSerializer.serializedObject.Update();
				CurrentTreeView.PopulateView(CurrentSerializer);
			}
		}


		private void OnEnable()
		{
			Undo.undoRedoPerformed -= OnUndoRedo;
			Undo.undoRedoPerformed += OnUndoRedo;

			EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
			EditorApplication.playModeStateChanged += OnPlayModeStateChanged;

			EditorApplication.hierarchyChanged -= OnHierarchyChanged;
			EditorApplication.hierarchyChanged += OnHierarchyChanged;
		}


		private void OnDisable()
		{
			Undo.undoRedoPerformed -= OnUndoRedo;
			
			EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
			EditorApplication.hierarchyChanged -= OnHierarchyChanged;
		}

		private void OnHierarchyChanged()
		{
			if (SceneManager.GetActiveScene().GetHashCode() == _lastSceneHash)
				return;
			
			windowState.CloseAllTabs();
			Close();
		}

		private void OnPlayModeStateChanged(PlayModeStateChange obj)
		{
			switch (obj)
			{
				case PlayModeStateChange.EnteredEditMode:
					EditorApplication.delayCall += OnExitPlayMode;
					break;

				case PlayModeStateChange.ExitingEditMode:
					break;

				case PlayModeStateChange.EnteredPlayMode:
					EditorApplication.delayCall += OnEnterPlayMode;
					break;

				case PlayModeStateChange.ExitingPlayMode:
					inspectorView?.Clear();
					break;
			}
		}


		private void CloseRuntimeTabs()
		{
			List<TreeViewTab> tabs = tabView.Query<TreeViewTab>().ToList();

			foreach (TreeViewTab tab in tabs)
				if (tab.IsRuntimeTab)
					tab.Close();
		}


		private void OnEnterPlayMode()
		{
			OnSelectionChange();
		}


		private void OnExitPlayMode()
		{
			OnSelectionChange();
			CloseRuntimeTabs();
		}


		private void OnSelectionChange()
		{
			if (Selection.activeGameObject)
			{
				var runner = Selection.activeGameObject.GetComponent<BehaviourTreeMono>();

				if (runner != null && runner != null)
				{
					NewTab(runner, true);
				}
			}
		}


		private void ClearSelection()
		{
			inspectorView?.Clear();
			CurrentTreeView?.ClearView();
			blackboardView?.ClearView();
			overlayView?.Show();
		}


		private void OnInspectorUpdate()
		{
			if (Application.isPlaying)
			{
				editorUpdate.Begin();
				CurrentTreeView?.UpdateNodeStates();
				editorUpdate.End();
			}
		}


		private void OnToolbarNewAsset()
		{
			BehaviourTree tree = EditorUtility.CreateNewTree();

			if (tree)
				NewTab(tree, true);
		}

		[Serializable]
		public class PendingScriptCreate
		{
			public bool pendingCreate;
			public string scriptName = "";
			public string sourceGuid = "";
			public bool isSourceParent;
			public Vector2 nodePosition;


			public void Reset()
			{
				pendingCreate = false;
				scriptName = "";
				sourceGuid = "";
				isSourceParent = false;
				nodePosition = Vector2.zero;
			}
		}


		public class BehaviourTreeEditorAssetModificationProcessor : AssetModificationProcessor
		{
			private static AssetDeleteResult OnWillDeleteAsset(string path, RemoveAssetOptions opt)
			{
				if (HasOpenInstances<BehaviourTreeEditorWindow>())
				{
					var wnd = GetWindow<BehaviourTreeEditorWindow>();
					List<TreeViewTab> tabs = wnd.tabView.Query<TreeViewTab>().ToList();

					foreach (TreeViewTab tab in tabs)
						if (tab.Serializer.tree is BehaviourTree behaviourTree && AssetDatabase.GetAssetPath(behaviourTree) == path)
							tab.Close();
				}

				return AssetDeleteResult.DidNotDelete;
			}
		}
	}
}
