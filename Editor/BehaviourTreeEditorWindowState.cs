using System;
using System.Collections.Generic;
using Editor.BehaviourTreeEditor.Configuration;
using Swat.Game.BehaviourTree.Core;

namespace Editor.BehaviourTreeEditor
{
	[Serializable]
	public class BehaviourTreeEditorWindowState
	{
		public List<UnityEngine.Object> activeTabs = new();
		public int activeTabIndex = -1;


		public void Restore(BehaviourTreeEditorWindow window)
		{
			activeTabs.RemoveAll(tab => tab == null || tab is not IBehaviourTree);
			activeTabIndex = activeTabs.Count == 0
				? -1
				: Math.Clamp(activeTabIndex, 0, activeTabs.Count - 1);

			if (activeTabs.Count == 0)
			{
				window.overlayView.Show();
			}
			else
			{
				int previousActiveTabIndex = activeTabIndex;

				foreach (UnityEngine.Object treeObj in activeTabs)
				{
					var tree = treeObj as IBehaviourTree;
					window.NewTab(tree, true);
				}

				window.tabView.selectedTabIndex = previousActiveTabIndex;
			}
		}


		public void TabOpened(TreeViewTab newTab)
		{
			if (newTab.IsRuntimeTab)
				return;

			if (activeTabs.Contains(newTab.Serializer.tree as UnityEngine.Object))
				return;
			
			activeTabs.Add(newTab.Serializer.tree as UnityEngine.Object);
			Save();
		}

		public void TabChanged(int tabIndex)
		{
			activeTabIndex = tabIndex;
			Save();
		}


		public void TabClosed(TreeViewTab tab)
		{
			activeTabs.Remove(tab.Serializer.tree as UnityEngine.Object);
			Save();
		}

		public void CloseAllTabs()
		{
			activeTabs.RemoveAll(_ => true);
		}


		private void Save()
		{
			var window = BehaviourTreeEditorWindow.Instance;
			BehaviourTreeProjectSettings settings = window.settings;
			UnityEditor.EditorUtility.SetDirty(settings);
		}
	}
}