using System;
using System.Collections.Generic;
using Editor.BehaviourTreeEditor.NodeInfoDrawers;
using Swat.Game.BehaviourTree;
using Swat.Game.BehaviourTree.Decorators;
using UnityEditor;
using UnityEngine.UIElements;

namespace Editor.BehaviourTreeEditor
{
	public class NodeInfoDrawer
	{
		private static readonly Dictionary<Type, INodeInfoDrawer> customDrawers = new()
		{
			[typeof(NestedTree)] = new NestedTreeInfoDrawer(),
			[typeof(Loop)] = new LoopInfoDrawer(),
			[typeof(Wait)] = new WaitInfoDrawer(),
			[typeof(Chance)] = new ChanceInfoDrawer(),
			[typeof(Cooldown)] = new CooldownInfoDrawer()
		};

		public static void Register<T>(INodeInfoDrawer drawer) where T : Node => customDrawers[typeof(T)] = drawer;

		private readonly NodeView nodeView;


		public NodeInfoDrawer(NodeView nodeView)
		{
			this.nodeView = nodeView;
		}


		public void DrawInfoLabel(SerializedProperty targetNodeProperty)
		{
			var container = nodeView.Q<Label>("info");

			if (container == null)
				return;

			var drawer = GetDrawer();

			drawer?.Draw(targetNodeProperty, container);
		}


		private INodeInfoDrawer GetDrawer()
		{
			foreach ((Type type, INodeInfoDrawer drawer) in customDrawers)
			{
				Type curType = nodeView.Node.GetType();

				if (curType == type)
					return drawer;

				if (drawer.UseForChildren && curType.IsSubclassOf(type))
					return drawer;
			}

			return null;
		}
	}
}