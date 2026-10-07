using Swat.Game.BehaviourTree;
using Swat.Game.BehaviourTree.Core;
using Swat.Game.BehaviourTree.Utils;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Editor.BehaviourTreeEditor
{
	public class DoubleClickSelection : MouseManipulator
	{
		private readonly double doubleClickDuration = 0.3;
		private double time;


		public DoubleClickSelection()
		{
			time = EditorApplication.timeSinceStartup;
		}


		protected override void RegisterCallbacksOnTarget()
		{
			target.RegisterCallback<MouseDownEvent>(OnMouseDown);
		}


		protected override void UnregisterCallbacksFromTarget()
		{
			target.UnregisterCallback<MouseDownEvent>(OnMouseDown);
		}


		private void OnMouseDown(MouseDownEvent evt)
		{
			var graphView = target as BehaviourTreeView;
			var clickedElement = evt.target as NodeView;
			
			if (graphView == null)
				return;
			
			if (clickedElement == null)
			{
				var ve = evt.target as VisualElement;
				clickedElement = ve.GetFirstAncestorOfType<NodeView>();

				if (clickedElement == null)
					return;
			}

			double duration = EditorApplication.timeSinceStartup - time;

			if (duration < doubleClickDuration)
			{
				if (clickedElement.Node is SubTree subTree)
				{
					OpenSubtree(subTree);
				}
				else
				{
					SelectChildren(evt, clickedElement);
				}
			}

			time = EditorApplication.timeSinceStartup;
		}


		private void SelectChildren(MouseDownEvent evt, NodeView clickedNodeView)
		{
			var graphView = target as BehaviourTreeView;

			if (graphView == null)
				return;

			if (!CanStopManipulation(evt))
				return;

			// Add children to selection so the root element can be moved
			BtUtils.Traverse(clickedNodeView.Node, node =>
			{
				NodeView view = graphView.FindNodeView(node);
				graphView.AddToSelection(view);
			});
		}

		private void OpenSubtree(SubTree subTree)
		{
			IBehaviourTree treeToFocus = subTree.TreeAsset;

			if (Application.isPlaying)
				treeToFocus = subTree.RuntimeTree;

			if (treeToFocus != null)
				BehaviourTreeEditorWindow.Instance.NewTab(treeToFocus, true);
		}
	}
}