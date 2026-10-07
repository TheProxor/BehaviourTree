using System;
using System.Collections.Generic;
using Editor.BehaviourTreeEditor.Configuration;
using Editor.BehaviourTreeEditor.Templates;
using Editor.BehaviourTreeEditor.Utils;
using Editor.BehaviourTreeEditor.Utils.Extensions;
using Swat.Game.BehaviourTree;
using Swat.Game.BehaviourTree.Actions;
using Swat.Game.BehaviourTree.Blackboard;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using Node = Swat.Game.BehaviourTree.Node;

namespace Editor.BehaviourTreeEditor
{
	public class CreateNodeWindow : ScriptableObject, ISearchWindowProvider
	{
		private Texture2D icon;
		private BehaviourTreeView treeView;
		private NodeView source;
		private bool isSourceParent;
		private EditorUtility.ScriptTemplate[] scriptFileAssets;


		public static void Show(Vector2 mousePosition, NodeView source, bool isSourceParent = false)
		{
			Vector2 screenPoint = GUIUtility.GUIToScreenPoint(mousePosition);
			var searchWindowProvider = CreateInstance<CreateNodeWindow>();
			searchWindowProvider.Initialise(BehaviourTreeEditorWindow.Instance.CurrentTreeView, source, isSourceParent);
			var windowContext = new SearchWindowContext(screenPoint, 240, 320);
			SearchWindow.Open(windowContext, searchWindowProvider);
		}


		public void Initialise(BehaviourTreeView treeView, NodeView source, bool isSourceParent)
		{
			this.treeView = treeView;
			this.source = source;
			this.isSourceParent = isSourceParent;

			icon = new Texture2D(1, 1);
			icon.SetPixel(0, 0, new Color(0, 0, 0, 0));
			icon.Apply();

			scriptFileAssets = new EditorUtility.ScriptTemplate[]
			{
				new()
					{ templateFile = GetScriptTemplate(0), defaultFileName = "NewActionNode", subFolder = "Actions" },
				new()
				{
					templateFile = GetScriptTemplate(1), defaultFileName = "NewConditionNode", subFolder = "Conditions"
				},
				new()
				{
					templateFile = GetScriptTemplate(2), defaultFileName = "NewCompositeNode", subFolder = "Composites"
				},
				new()
				{
					templateFile = GetScriptTemplate(3), defaultFileName = "NewDecoratorNode", subFolder = "Decorators"
				}
			};
		}


		public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
		{
			var tree = new List<SearchTreeEntry>
			{
				new SearchTreeGroupEntry(new GUIContent("Create Node"))
			};

			// Action nodes can only be added as children
			if (isSourceParent || source == null)
			{
				tree.Add(new SearchTreeGroupEntry(new GUIContent("Actions")) { level = 1 });
				TypeCache.TypeCollection types = TypeCache.GetTypesDerivedFrom<ActionNode>();

				foreach (Type type in types) // Ignore condition types
				{
					if (type == typeof(StateMachineActionNode<>))
						continue;

					if (!type.IsSubclassOf(typeof(ConditionNode)) && type != typeof(SubTree))
					{
						Action invoke = () => CreateNode(type, context);
						tree.Add(new SearchTreeEntry(new GUIContent($"{type.Name}")) { level = 2, userData = invoke });
					}
				}
			}

			// Condition nodes can only be added as children
			if (isSourceParent || source == null)
			{
				tree.Add(new SearchTreeGroupEntry(new GUIContent("Conditions")) { level = 1 });
				TypeCache.TypeCollection types = TypeCache.GetTypesDerivedFrom<ConditionNode>();

				foreach (Type type in types)
				{
					if (type.IsAbstract)
						continue;

					Action invoke = () => CreateNode(type, context);
					tree.Add(new SearchTreeEntry(new GUIContent($"{type.Name}")) { level = 2, userData = invoke });
				}
			}

			{
				tree.Add(new SearchTreeGroupEntry(new GUIContent("Composites")) { level = 1 });

				{
					TypeCache.TypeCollection types = TypeCache.GetTypesDerivedFrom<CompositeNode>();

					foreach (Type type in types)
					{
						Action invoke = () => CreateNode(type, context);
						tree.Add(new SearchTreeEntry(new GUIContent($"{type.Name}")) { level = 2, userData = invoke });
					}
				}
			}

			{
				tree.Add(new SearchTreeGroupEntry(new GUIContent("Decorators")) { level = 1 });

				{
					TypeCache.TypeCollection types = TypeCache.GetTypesDerivedFrom<DecoratorNode>();

					foreach (Type type in types)
					{
						if (type == typeof(AbortableNode) || type == typeof(ConditionNode) ||
							type.IsSubclassOf(typeof(ConditionNode)))
							continue;

						Action invoke = () => CreateNode(type, context);
						tree.Add(new SearchTreeEntry(new GUIContent($"{type.Name}")) { level = 2, userData = invoke });
					}
				}
			}

			// {
			//     tree.Add(new SearchTreeGroupEntry(new GUIContent("Events")) { level = 1 });
			//
			//     {
			//         TypeCache.TypeCollection types = TypeCache.GetTypesDerivedFrom<EventNode>();
			//
			//         foreach (Type type in types)
			//         {
			//             Action invoke = () => CreateNode(type, context);
			//             tree.Add(new SearchTreeEntry(new GUIContent($"{type.Name}")) { level = 2, userData = invoke });
			//         }
			//     }
			// }

			{
				tree.Add(new SearchTreeGroupEntry(new GUIContent("Nested")) { level = 1 });

				{
					TypeCache.TypeCollection types = TypeCache.GetTypesDerivedFrom<NestedTree>();

					foreach (Type type in types)
					{
						Action invoke = () => CreateNode(type, context);
						tree.Add(new SearchTreeEntry(new GUIContent($"{type.Name}")) { level = 2, userData = invoke });
					}
				}
			}
			
			{
			    tree.Add(new SearchTreeGroupEntry(new GUIContent("Subtree")) { level = 1 });
			
			    {
			        List<string> behaviourTrees = EditorUtility.GetAssetPaths<BehaviourTree>();
			
			        behaviourTrees.ForEach(path =>
			        {
			            string fileName = System.IO.Path.GetFileName(path);
			
			            Action invoke = () =>
			            {
			                var tree = AssetDatabase.LoadAssetAtPath<BehaviourTree>(path);
			                NodeView subTreeNodeView = CreateNode(typeof(SubTree), context);
			                var subtreeNode = subTreeNodeView.Node as SubTree;
			                subtreeNode.TreeAsset = tree;
			            };
			
			            tree.Add(new SearchTreeEntry(new GUIContent($"{fileName}")) { level = 2, userData = invoke });
			        });
			    }
			}
			
			{
				tree.Add(new SearchTreeGroupEntry(new GUIContent("Template")) { level = 1 });

				{
					var allTemplates = TemplatesProvider.FindAllTemplates();

					foreach (var templateData in allTemplates)
					{
						Action invoke = () => PlaceTemplate(templateData.Json, context);
						tree.Add(new SearchTreeEntry(new GUIContent($"{templateData.Name}")) { level = 2, userData = invoke });
					}
				}
			}
			
			// {
			//     tree.Add(new SearchTreeGroupEntry(new GUIContent("New Script ...")) { level = 1 });
			//
			//     Action createActionScript = () => CreateScript(scriptFileAssets[0], context);
			//     tree.Add(new SearchTreeEntry(new GUIContent($"New Action Script")) { level = 2, userData = createActionScript });
			//
			//     Action createConditionScript = () => CreateScript(scriptFileAssets[1], context);
			//     tree.Add(new SearchTreeEntry(new GUIContent($"New Condition Script")) { level = 2, userData = createConditionScript });
			//
			//     Action createCompositeScript = () => CreateScript(scriptFileAssets[2], context);
			//     tree.Add(new SearchTreeEntry(new GUIContent($"New Composite Script")) { level = 2, userData = createCompositeScript });
			//
			//     Action createDecoratorScript = () => CreateScript(scriptFileAssets[3], context);
			//     tree.Add(new SearchTreeEntry(new GUIContent($"New Decorator Script")) { level = 2, userData = createDecoratorScript });
			// }

			// {
			//     Action invoke = () =>
			//     {
			//         BehaviourTree newTree = EditorUtility.CreateNewTree();
			//
			//         if (newTree)
			//         {
			//             NodeView subTreeNodeView = CreateNode(typeof(SubTree), context);
			//             var subtreeNode = subTreeNodeView.Node as SubTree;
			//             subtreeNode.treeAsset = newTree;
			//         }
			//     };
			//
			//     tree.Add(new SearchTreeEntry(new GUIContent("     New Subtree ...")) { level = 1, userData = invoke });
			// }

			return tree;
		}


		public bool OnSelectEntry(SearchTreeEntry searchTreeEntry, SearchWindowContext context)
		{
			var invoke = (Action)searchTreeEntry.userData;
			invoke();
			return true;
		}


		public NodeView CreateNode(Type type, SearchWindowContext context)
		{
			BehaviourTreeEditorWindow editorWindow = BehaviourTreeEditorWindow.Instance;
			
			var graphMousePosition = editorWindow.ScreenMouseToGraphPosition(context.screenMousePosition);

			var nodeOffset = new Vector2(-75, -20);
			Vector2 nodePosition = graphMousePosition + nodeOffset;

			nodePosition.x = EditorUtility.SnapTo(nodePosition.x, editorWindow.settings.gridSnapSizeX);
			nodePosition.y = EditorUtility.SnapTo(nodePosition.y, editorWindow.settings.gridSnapSizeY);

			// #TODO: Unify this with CreatePendingScriptNode
			NodeView createdNode;

			if (source != null)
			{
				if (isSourceParent)
					createdNode = treeView.CreateNode(type, nodePosition, source);
				else
					createdNode = treeView.CreateNodeWithChild(type, nodePosition, source);
			}
			else
			{
				createdNode = treeView.CreateNode(type, nodePosition, null);
			}

			treeView.SelectNode(createdNode);
			return createdNode;
		}

		private void PlaceTemplate(TextAsset templateJson, SearchWindowContext context)
		{
			var editorWindow = BehaviourTreeEditorWindow.Instance;
			
			Node[] nodesToCreate = TemplatesProvider.LoadTemplate(templateJson);
			
			Vector2 graphMousePosition = editorWindow.ScreenMouseToGraphPosition(context.screenMousePosition);
			Vector2 selectionCenter = BtUtilsEditor.CalculateCenterOfMass(nodesToCreate);
			Vector2 posOffset = graphMousePosition - selectionCenter;

			treeView.CloneNodes(nodesToCreate, posOffset);
		}
		

		public void CreateScript(EditorUtility.ScriptTemplate scriptTemplate, SearchWindowContext context)
		{
			var editorWindow = BehaviourTreeEditorWindow.Instance;

			Vector2 windowMousePosition = editorWindow.rootVisualElement.ChangeCoordinatesTo(
				editorWindow.rootVisualElement.parent, context.screenMousePosition - editorWindow.position.position);
			Vector2 graphMousePosition =
				editorWindow.CurrentTreeView.contentViewContainer.WorldToLocal(windowMousePosition);
			var nodeOffset = new Vector2(-75, -20);
			Vector2 nodePosition = graphMousePosition + nodeOffset;

			EditorUtility.CreateNewScript(scriptTemplate, source, isSourceParent, nodePosition);
		}


		private TextAsset GetScriptTemplate(int type)
		{
			var projectSettings = BehaviourTreeProjectSettings.GetOrCreateSettings();

			switch (type)
			{
				case 0:
					if (projectSettings.scriptTemplateActionNode)
						return projectSettings.scriptTemplateActionNode;

					return BehaviourTreeEditorWindow.Instance.scriptTemplateActionNode;

				case 1:
					if (projectSettings.scriptTemplateConditionNode)
						return projectSettings.scriptTemplateConditionNode;

					return BehaviourTreeEditorWindow.Instance.scriptTemplateConditionNode;

				case 2:
					if (projectSettings.scriptTemplateCompositeNode)
						return projectSettings.scriptTemplateCompositeNode;

					return BehaviourTreeEditorWindow.Instance.scriptTemplateCompositeNode;

				case 3:
					if (projectSettings.scriptTemplateDecoratorNode)
						return projectSettings.scriptTemplateDecoratorNode;

					return BehaviourTreeEditorWindow.Instance.scriptTemplateDecoratorNode;
			}

			Debug.LogError("Unhandled script template type:" + type);
			return null;
		}
	}
}