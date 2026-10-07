using Editor.BehaviourTreeEditor.Serialization;
using Swat.Game.BehaviourTree.Core;
using UnityEngine.UIElements;

namespace Editor.BehaviourTreeEditor
{
	public class TreeViewTab : Tab
	{
		public readonly SerializedBehaviourTree Serializer;
		public readonly BehaviourTreeView TreeView;
		public readonly IBehaviourTree Tree;
		public readonly bool IsRuntimeTab;

		public readonly int InstanceID;


		public TreeViewTab(IBehaviourTree tree, StyleSheet styleSheet, string tabName) : base(tabName)
		{
			Tree = tree;
			IsRuntimeTab = tree.Name.Contains("Clone");
			name = tabName;
			closeable = true;
			Serializer = new SerializedBehaviourTree(tree);
			InstanceID = tree.GetInstanceID();

			TreeView = new BehaviourTreeView();
			TreeView.styleSheets.Add(styleSheet);
			TreeView.PopulateView(Serializer);
			TreeView.OnNodeSelected += OnNodeSelected;
			
			Add(TreeView);
			
			closed += OnClosed;

			tabHeader.RegisterCallback<MouseDownEvent>(OnMouseDownEvent, TrickleDown.TrickleDown);
		}

		public void Close()
		{
			closed -= OnClosed;
			
			tabHeader.UnregisterCallback<MouseDownEvent>(OnMouseDownEvent, TrickleDown.TrickleDown);
			
			RemoveFromHierarchy();
		}

		private void OnMouseDownEvent(MouseDownEvent evt)
		{
			// Close tab on middle mouse click
			if (evt.button == (int)MouseButton.MiddleMouse)
			{
				evt.StopPropagation();
				Close();
				OnClosed(this);
			}
		}

		private void OnClosed(Tab tab) 
		{
			BehaviourTreeEditorWindow.Instance.OnTabClosed(tab);
		}

		private void OnNodeSelected(NodeView node)
		{
			var window = BehaviourTreeEditorWindow.Instance;
			window.InspectNode(Serializer, node);
		}
	}
}