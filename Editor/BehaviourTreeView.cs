using System;
using System.Collections.Generic;
using System.Linq;
using Editor.BehaviourTreeEditor.Game;
using Editor.BehaviourTreeEditor.Serialization;
using Editor.BehaviourTreeEditor.Utils;
using Editor.BehaviourTreeEditor.Utils.Extensions;
using Swat.Game.BehaviourTree;
using Swat.Game.BehaviourTree.Utils;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using Node = Swat.Game.BehaviourTree.Node;


namespace Editor.BehaviourTreeEditor
{
	[UxmlElement]
	public partial class BehaviourTreeView : GraphView
	{
		public Action<NodeView> OnNodeSelected;

		public SerializedBehaviourTree serializer;

		protected override bool canCopySelection => true;

		protected override bool canCutSelection => true;

		protected override bool canPaste => true;

		protected override bool canDuplicateSelection => true;

		protected override bool canDeleteSelection => true;

		private bool dontUpdateModel;


		public BehaviourTreeView()
		{
			Insert(0, new GridBackground());

			this.AddManipulator(new ContentZoomer());
			this.AddManipulator(new ContentDragger());
			this.AddManipulator(new HierarchySelector());
			this.AddManipulator(new DoubleClickSelection());
			this.AddManipulator(new SelectionDragger());
			this.AddManipulator(new RectangleSelector());
			
			// Perform Copy
			serializeGraphElements = items =>
			{
				var window = BehaviourTreeEditorWindow.Instance;
				int currentTreeId = window.CurrentTree.GetInstanceID();
				
				var copyPasteData = new CopyPasteData(currentTreeId, items);

				return JsonUtility.ToJson(copyPasteData);
			};

			// Perform Paste
			unserializeAndPaste = (operationName, data) =>
			{
				var window = BehaviourTreeEditorWindow.Instance;

				var targetView = window.CurrentTreeView;
				int currentTreeId = window.CurrentTree.GetInstanceID();

				var copyPasteData = JsonUtility.FromJson<CopyPasteData>(data);
				var nodesToCopy = new List<NodeView>();
				
				Vector2 graphMousePosition = window.WindowMouseToGraphPosition(window.LastTabViewMouseWindowPosition);
				Vector2 posOffset = graphMousePosition - copyPasteData.selectionCenter;

				bool isTheSameTree = copyPasteData.treeId == currentTreeId;
				bool isAllNodesExistsInTree = false;

				// Check if all nodes exists in target tree
				if (isTheSameTree)
				{
					foreach (string nodeGuid in copyPasteData.nodeGuids)
					{
						NodeView nodeView = FindNodeView(nodeGuid);
						nodesToCopy.Add(nodeView);
					}

					isAllNodesExistsInTree = nodesToCopy.All(n => n != null);
				}

				if (isTheSameTree && isAllNodesExistsInTree)
				{
					PasteFast(targetView, nodesToCopy.ToArray(), posOffset);
				}
				else
				{
					Paste(targetView, copyPasteData.nodesJson, posOffset);
				}
			};

			// Enable copy paste always?
			canPasteSerializedData = data => { return true; };

			viewTransformChanged += OnViewTransformChanged;
		}


		public NodeView FindNodeView(Node node)
		{
			if (node == null)
				return null;

			return GetNodeByGuid(node.Guid) as NodeView;
		}


		public NodeView FindNodeView(string guid)
		{
			return GetNodeByGuid(guid) as NodeView;
		}


		public void ClearView()
		{
			graphViewChanged -= OnGraphViewChanged;
			DeleteElements(graphElements.ToList());
			graphViewChanged += OnGraphViewChanged;
		}


		public void PopulateView(SerializedBehaviourTree tree)
		{
			serializer = tree;

			ClearView();

			Debug.Assert(serializer.tree.RootNode != null);

			BtUtils.SanitizeNodeReferences(serializer.tree);
			serializer.serializedObject.Update();
			serializer.serializedObject.ApplyModifiedPropertiesWithoutUndo();

			// Creates node view
			serializer.tree.Nodes.ForEach(n => CreateNodeView(n));

			// Create edges
			serializer.tree.Nodes.ForEach(n =>
			{
				List<Node> children = BtUtils.GetChildren(n);

				children.ForEach(c =>
				{
					if (c == null)
						return;

					NodeView parentView = FindNodeView(n);
					NodeView childView = FindNodeView(c);

					if (parentView == null)
					{
						Debug.LogWarning($"Skipping edge: parent view not found for '{n.GetType().Name}'");
						return;
					}

					if (childView == null)
					{
						Debug.LogWarning(
							$"Skipping edge: child view not found for parent '{parentView.Node.GetType().Name}'");
						return;
					}

					CreateEdgeView(parentView, childView);
				});
			});

			// Set view
			contentViewContainer.transform.position = serializer.tree.ViewPosition;
			contentViewContainer.transform.scale = serializer.tree.ViewScale;
			
			// Add Root node to class list
			if (serializer.tree is BehaviourTreeMono)
			{
				var isTreeCycled = GameSpecifics.IsTreeCycled(serializer.tree);
				var root = FindNodeView(serializer.tree.RootNode);

				root.AddToClassList(isTreeCycled ? "cycled" : "once");
			}
		}


		public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
		{
			return ports.ToList()
				.Where(endPort =>
					endPort.direction != startPort.direction && endPort.node != startPort.node)
				.ToList();
		}


		public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
		{
			switch (evt.target)
			{
				case BehaviourTreeView:
					CreateNodeWindow.Show(evt.mousePosition, null);
					break;
				
				case NodeView:
				case Edge:
					evt.menu.AppendAction("Save template", _ => EditorUtility.OpenNewTemplateDialog(selection));
					break;
			}
		}


		public NodeView CreateNode(Type type, Vector2 position, NodeView parentView)
		{
			serializer.BeginBatch();

			// Update model
			Node node = serializer.CreateNode(type, position);

			if (parentView != null)
				serializer.AddChild(parentView.Node, node);

			// Update View
			NodeView nodeView = CreateNodeView(node);

			if (parentView != null)
				AddChild(parentView, nodeView);

			serializer.EndBatch();

			return nodeView;
		}


		public NodeView CreateNodeWithChild(Type type, Vector2 position, NodeView childView)
		{
			serializer.BeginBatch();

			// Update Model
			Node node = serializer.CreateNode(type, position);

			// Delete the childs previous parent
			foreach (Edge connection in childView.Input.connections)
			{
				var childParent = connection.output.node as NodeView;
				serializer.RemoveChild(childParent.Node, childView.Node);
			}

			// Add as child of new node.
			serializer.AddChild(node, childView.Node);

			// Update View
			NodeView nodeView = CreateNodeView(node);

			if (nodeView != null)
				AddChild(nodeView, childView);

			serializer.EndBatch();
			return nodeView;
		}


		public NodeView CreateNodeView(Node node)
		{
			var nodeView = new NodeView(node, BehaviourTreeEditorWindow.Instance.nodeXml, this);
			AddElement(nodeView);
			return nodeView;
		}


		public void AddChild(NodeView parentView, NodeView childView)
		{
			// Delete Previous output connections
			if (parentView.Output.capacity == Port.Capacity.Single)
				RemoveElements(parentView.Output.connections);

			// Delete previous child's parent
			RemoveElements(childView.Input.connections);

			CreateEdgeView(parentView, childView);
		}


		public void RemoveElements(IEnumerable<GraphElement> elementsToRemove)
		{
			dontUpdateModel = true;
			DeleteElements(
				elementsToRemove); // Just need to delete the ui elements without causing a graphChangedEvent here.
			dontUpdateModel = false;
		}


		public void UpdateNodeStates()
		{
			if (serializer == null)
				return;

			if (serializer.tree == null)
				return;

			if (serializer.tree.Context == null)
				return;

			Dictionary<string, NodeState> tickResults = serializer.tree.Context.TickResults;

			if (tickResults != null)
				nodes.ForEach(n =>
				{
					var view = n as NodeView;
					view.UpdateState(tickResults);
				});
		}


		public void ResetNodeStates()
		{
			nodes.ForEach(n =>
			{
				var view = n as NodeView;
				view.ApplyInactiveNodeStateStyle();
			});
		}


		public void SelectNode(NodeView nodeView)
		{
			ClearSelection();

			if (nodeView != null)
				AddToSelection(nodeView);
		}


		public void SelectNode(Node node)
		{
			NodeView nodeView = FindNodeView(node);
			SelectNode(nodeView);
		}


		public void InspectNode(Node node)
		{
			NodeView nodeView = FindNodeView(node);
			OnNodeSelected(nodeView);
		}


		public void ExpandSubtree(NodeView nodeView)
		{
			Node subtreeParent = nodeView.NodeParent.Node;
			var subtree = nodeView.Node as SubTree;
			BehaviourTree tree = subtree.TreeAsset;

			serializer.BeginBatch();
			serializer.RemoveChild(subtreeParent, subtree);
			serializer.DeleteTree(subtree);
			serializer.CloneTree(tree.RootNode.Child, subtreeParent, subtree.Position);
			serializer.EndBatch();
			PopulateView(serializer);
		}


		public void CreateSubTree(NodeView nodeView)
		{
			var window = BehaviourTreeEditorWindow.Instance;


			InspectNode(null);

			BehaviourTree tree = EditorUtility.CreateNewTree();

			if (tree)
			{
				Node subTreeRootParent = nodeView.NodeParent.Node;
				Node subTreeRoot = nodeView.Node;

				// Create new Subtree asset
				{
					Vector2 position = tree.RootNode.Position;
					position.y += BehaviourTreeEditorWindow.Instance.settings.gridSnapSizeY;

					var newTree = new SerializedBehaviourTree(tree);
					newTree.BeginBatch();
					newTree.CloneTree(subTreeRoot, tree.RootNode, position);
					newTree.EndBatch();
					window.NewTab(tree, false);
				}

				// Replace subtree with subtree node
				{
					serializer.BeginBatch();

					// Create New SubTree Node with the newly created asset
					var subTreeNode = serializer.CreateNode<SubTree>(subTreeRoot.Position) as SubTree;
					serializer.SetNodeProperty(subTreeNode, nameof(SubTree.TreeAsset), tree);

					// Unparent subtree from it's parent
					serializer.RemoveChild(subTreeRootParent, subTreeRoot);

					// Parent Subtree node to previous parent
					serializer.AddChild(subTreeRootParent, subTreeNode);

					// Delete remaining subtree
					serializer.DeleteTree(subTreeRoot);

					serializer.EndBatch();

					// Refresh view, focus new subtree node
					PopulateView(serializer);
					InspectNode(subTreeNode);
					SelectNode(subTreeNode);
				}
			}
		}


		internal void DeleteNodeView(Node n)
		{
			NodeView nodeView = FindNodeView(n);

			if (nodeView != null)
			{
				if (nodeView.Input != null)
					RemoveElements(nodeView.Input.connections);

				if (nodeView.Output != null)
					RemoveElements(nodeView.Output.connections);

				RemoveElement(nodeView);
			}
		}

		private void PasteFast(BehaviourTreeView targetView, NodeView[] nodesToCopy, Vector2 posOffset)
		{
			targetView.CloneNodesFast(nodesToCopy, posOffset);
		}

		private void Paste(BehaviourTreeView targetView, string nodesJson, Vector2 posOffset)
		{
			var nodesToCopyArray = NodeSerializer.DeserializeRange(nodesJson);
			targetView.CloneNodes(nodesToCopyArray, posOffset);
		}

		private void OnViewTransformChanged(GraphView graphView)
		{
			Vector3 position = contentViewContainer.transform.position;
			Vector3 scale = contentViewContainer.transform.scale;
			serializer.SetViewTransform(position, scale);
		}


		private GraphViewChange OnGraphViewChanged(GraphViewChange graphViewChange)
		{
			if (dontUpdateModel)
				return graphViewChange;

			var blockedDeletes = new List<GraphElement>();

			if (graphViewChange.elementsToRemove != null)
				graphViewChange.elementsToRemove.ForEach(elem =>
				{
					var nodeView = elem as NodeView;

					if (nodeView != null)
					{
						// The root node is not deletable
						if (nodeView.Node is not RootNode)
						{
							OnNodeSelected(null);
							serializer.DeleteNode(nodeView.Node);
						}
						else
						{
							blockedDeletes.Add(elem);
						}
					}

					var edge = elem as Edge;

					if (edge != null)
					{
						var parentView = edge.output.node as NodeView;
						var childView = edge.input.node as NodeView;
						serializer.RemoveChild(parentView.Node, childView.Node);
					}
				});

			if (graphViewChange.edgesToCreate != null)
				graphViewChange.edgesToCreate.ForEach(edge =>
				{
					var parentView = edge.output.node as NodeView;
					var childView = edge.input.node as NodeView;
					serializer.AddChild(parentView.Node, childView.Node);
				});

			nodes.ForEach(n =>
			{
				var view = n as NodeView;
				// Need to rebind description labels as the serialized properties will be invalidated after removing from array
				view.SetupDataBinding();
				view.SortChildren();
			});

			foreach (GraphElement elem in blockedDeletes)
				graphViewChange.elementsToRemove.Remove(elem);

			return graphViewChange;
		}


		private void CreateEdgeView(NodeView parentView, NodeView childView)
		{
			Edge edge = parentView.Output.ConnectTo(childView.Input);
			AddElement(edge);
		}


		[Serializable]
		private class CopyPasteData
		{
			public int treeId;
			public string nodesJson;
			public List<string> nodeGuids;
			public Vector2 selectionCenter;

			public CopyPasteData(int treeId, IEnumerable<GraphElement> elementsToCopy)
			{
				this.treeId = treeId;
				this.nodeGuids = new List<string>();

				var nodeViews = BtUtilsEditor
					.SelectNodeViews(elementsToCopy)
					.Where(nv => nv != null);
				
				var nodes = BtUtilsEditor
					.SelectNodes(nodeViews)
					.Where(n => n is not RootNode)
					.ToArray();
				
				this.nodesJson = NodeSerializer.SerializeRange(nodes);
				this.selectionCenter = BtUtilsEditor.CalculateCenterOfMass(nodes);
				
				foreach (var node in nodes)
					nodeGuids.Add(node.Guid);
			}
		}
	}
}