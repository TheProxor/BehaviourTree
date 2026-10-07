using Swat.Game.BehaviourTree;
using Swat.Game.BehaviourTree.Core;
using UnityEditor;
using UnityEngine;

namespace Editor.BehaviourTreeEditor.CustomInspector
{
	[CustomEditor(typeof(BehaviourTree))]
	public class BehaviourTreeEditorInspector : UnityEditor.Editor
	{
		public override void OnInspectorGUI()
		{
			// base.OnInspectorGUI();

			if (target == null)
				return;

			var bt = target as IBehaviourTree;

			GUILayout.Space(20f);

			GuiHorizontalInfo("Tree State: ", $"{bt.TreeState}");
			GuiHorizontalInfo("Nodes Count: ", $"{bt.Nodes.Count}");

			if (GUILayout.Button("Open Graph", GUILayout.Height(40)))
			{
				BehaviourTreeEditorWindow.OpenWindow(bt);
			}
		}


		private void GuiHorizontalInfo(string leftText, string rightText)
		{
			GUILayout.BeginHorizontal();

			var textStyle = new GUIStyle
			{
				fontSize = 15,
				fontStyle = FontStyle.Bold,
				normal = new GUIStyleState(),
				alignment = TextAnchor.MiddleLeft,
				stretchWidth = false
			};

			textStyle.normal.textColor = new Color(0, 0.9f, 0.6f, 9f);

			GUILayout.Label(leftText, textStyle);

			textStyle.normal.textColor = new Color(0.9f, 0.9f, 0.6f, 9f);

			GUILayout.Label(rightText, textStyle);

			GUILayout.EndHorizontal();
		}
	}
}