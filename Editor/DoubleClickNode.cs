using Swat.Game.BehaviourTree;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Application = UnityEngine.Device.Application;


namespace Editor.BehaviourTreeEditor
{
	public class DoubleClickNode : MouseManipulator
	{
		private readonly double doubleClickDuration = 0.4;
		private double time;


		public DoubleClickNode()
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
			if (!CanStopManipulation(evt))
				return;

			var clickedElement = evt.target as NodeView;

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
				OnDoubleClick(evt, clickedElement);
				evt.StopImmediatePropagation();
			}

			time = EditorApplication.timeSinceStartup;
		}


		private void OpenScriptForNode(NodeView clickedElement)
		{
			TextAsset script = EditorUtility.GetNodeScriptPath(clickedElement);

			if (script)
			{
				// Open script in the editor:
				AssetDatabase.OpenAsset(script);

				// Remove the node from selection to prevent dragging it around when returning to the editor.
				BehaviourTreeEditorWindow.Instance.CurrentTreeView.RemoveFromSelection(clickedElement);
			}
		}


		private void OnDoubleClick(MouseDownEvent evt, NodeView clickedElement)
		{
			OpenScriptForNode(clickedElement);
		}
	}
}