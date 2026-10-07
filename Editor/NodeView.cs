using System;
using System.Collections.Generic;
using Editor.BehaviourTreeEditor.Configuration;
using Editor.BehaviourTreeEditor.Serialization;
using Swat.Game.BehaviourTree;
using Swat.Game.BehaviourTree.Composites;
using Swat.Game.BehaviourTree.Nested;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Node = Swat.Game.BehaviourTree.Node;


namespace Editor.BehaviourTreeEditor
{
	public class NodeView : UnityEditor.Experimental.GraphView.Node
	{
		public BehaviourTreeView TreeView;
		public Node Node;
		public Port Input;
		public Port Output;

		public NodeView NodeParent
		{
			get
			{
				using (IEnumerator<Edge> iter = Input.connections.GetEnumerator())
				{
					iter.MoveNext();
					return iter.Current?.output.node as NodeView;
				}
			}
		}

		public List<NodeView> NodeChildren
		{
			get
			{
				// This is untested and may not work. Possibly output should be input.
				var children = new List<NodeView>();

				foreach (Edge edge in Output.connections)
				{
					var child = edge.output.node as NodeView;

					if (child != null)
						children.Add(child);
				}

				return children;
			}
		}

		private readonly NodeInfoDrawer infoDrawer;


		public NodeView(Node node, VisualTreeAsset nodeXml, BehaviourTreeView treeView) : base(
			AssetDatabase.GetAssetPath(nodeXml))
		{
			infoDrawer = new NodeInfoDrawer(this);

			TreeView = treeView;
			capabilities &= ~Capabilities.Snappable; // Disable node snapping
			Node = node;
			title = node.GetType().Name;
			viewDataKey = node.Guid;

			style.left = node.Position.x;
			style.top = node.Position.y;

			CreateInputPorts();
			CreateOutputPorts();
			SetupClasses();
			SetupDataBinding();

			//this.AddManipulator(new DoubleClickNode());
			TreeView = treeView;
		}


		public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
		{
			base.BuildContextualMenu(evt);
			// evt.menu.AppendAction("Create Subtree...", CreateSubtree);
			//
			// if (Node is SubTree subtree)
			//     evt.menu.AppendAction("Expand Subtree...", ExpandSubtree);
		}


		public void SetupDataBinding()
		{
			SerializedBehaviourTree serializer = TreeView.serializer;
			SerializedProperty nodeProp = serializer.FindNode(Node);

			if (nodeProp == null)
				return;

			infoDrawer?.DrawInfoLabel(nodeProp);

			SerializedProperty descriptionProp = nodeProp.FindPropertyRelative("Description");
			var descriptionLabel = this.Q<Label>("description");
			SetDescriptionFieldVisible(descriptionLabel, descriptionProp);

			descriptionLabel.TrackPropertyValue(descriptionProp,
				property => { SetDescriptionFieldVisible(descriptionLabel, property); });

			descriptionLabel.BindProperty(descriptionProp);

			if (Node is ConditionNode)
			{
				var icon = this.Q<VisualElement>("icon");

				SerializedProperty enabledProperty = nodeProp.FindPropertyRelative(nameof(ConditionNode.Enabled));
				icon.TrackPropertyValue(enabledProperty, UpdateConditionNodeClasses);
				
				SerializedProperty invertProperty = nodeProp.FindPropertyRelative(nameof(ConditionNode.Invert));
				icon.TrackPropertyValue(invertProperty, UpdateConditionNodeClasses);
			}
		}


		public override void SetPosition(Rect newPos)
		{
			BehaviourTreeProjectSettings projectSettings = BehaviourTreeEditorWindow.Instance.settings;

			newPos.x = EditorUtility.SnapTo(newPos.x, projectSettings.gridSnapSizeX);
			newPos.y = EditorUtility.SnapTo(newPos.y, projectSettings.gridSnapSizeY);

			base.SetPosition(newPos);

			SerializedBehaviourTree serializer = TreeView.serializer;
			var position = new Vector2(newPos.xMin, newPos.yMin);
			serializer.SetNodePosition(Node, position);
		}


		public override void OnSelected()
		{
			base.OnSelected();
			TreeView.OnNodeSelected?.Invoke(this);
		}


		public void SortChildren()
		{
			if (Node is CompositeNode composite)
				composite.Children.Sort(SortByHorizontalPosition);
		}


		public void UpdateState(Dictionary<string, NodeState> tickResults)
		{
			if (!Application.isPlaying)
				return;

			bool hasTickResult = tickResults.TryGetValue(Node.Guid, out NodeState tickResult);

			if (Node.ResetFlag)
			{
				ApplyInactiveNodeStateStyle();
				Node.ResetFlag = false;

				if (hasTickResult)
					tickResults.Remove(Node.Guid);

				return;
			}

			if (hasTickResult)
				ApplyActiveNodeStateStyle(tickResult);
			else
				ApplyInactiveNodeStateStyle();
		}


		public void ApplyInactiveNodeStateStyle()
		{
			style.borderLeftWidth = 0;
			style.borderRightWidth = 0;
			style.borderTopWidth = 0;
			style.borderBottomWidth = 0;
			style.opacity = 0.5f;
		}


		private void ExpandSubtree(DropdownMenuAction action)
		{
			TreeView.ExpandSubtree(this);
		}


		private void CreateSubtree(DropdownMenuAction action)
		{
			TreeView.CreateSubTree(this);
		}


		private void SetDescriptionFieldVisible(Label label, SerializedProperty property)
		{
			label.style.display = string.IsNullOrEmpty(property.stringValue) 
				? DisplayStyle.None 
				: DisplayStyle.Flex;
		}


		private void SetSubTreeLabel(Label label, SerializedProperty property)
		{
			if (property.objectReferenceValue == null)
			{
				label.text = "SubTree";
			}
			else
			{
				var treeAsset = property.objectReferenceValue as BehaviourTreeMono;
				label.text = treeAsset.name;
			}
		}


		private void UpdateConditionNodeClasses(SerializedProperty property)
		{
			ConditionNode conditionNode = Node as ConditionNode;
			bool isInverted = conditionNode.Invert;
			bool isEnabled = conditionNode.Enabled;

			RemoveFromClassList("invert");
			RemoveFromClassList("disabled");
			
			if(!isEnabled)
				AddToClassList("disabled");
			
			else if (isInverted) 
				AddToClassList("invert");
		}


		private void SetupClasses()
		{
			switch (Node)
			{
				case ActionNode:
				{
					AddToClassList("action");

					if (Node is SubTree)
						AddToClassList("subtree");
					break;
				}

				case CompositeNode:
				{
					AddToClassList("composite");

					if (Node is Sequencer)
						AddToClassList("sequencer");
					else if (Node is Selector)
						AddToClassList("selector");
					else if (Node is Parallel)
						AddToClassList("parallel");
					break;
				}

				case DecoratorNode:
				{
					AddToClassList("decorator");

					if (Node is ConditionNode conditionNode)
					{
						AddToClassList("condition");

						if (conditionNode.Invert)
							AddToClassList("invert");
					
						if (!conditionNode.Enabled)
							AddToClassList("disabled");
					}

					break;
				}

				case EventNode:
					AddToClassList("event");
					break;

				case RootNode:
					AddToClassList("root");
					break;

				case NestedTree:
					AddToClassList("nested");
					break;
			}
		}


		private void CreateInputPorts()
		{
			switch (Node)
			{
				case ActionNode:
				case CompositeNode:
				case DecoratorNode:
				case NestedCall:
					Input = new NodePort(Direction.Input, Port.Capacity.Single);
					break;

				case EventNode or RootNode or NestedRoot:
					break;
			}

			if (Input == null)
				return;
			
			Input.portName = "";
			Input.style.flexDirection = FlexDirection.Column;
			Input.style.width = 25;
			
			Input.UnregisterCallback<MouseEnterEvent>(OnInputPortMouseEnter);
			Input.RegisterCallback<MouseEnterEvent>(OnInputPortMouseEnter);
			
			Input.UnregisterCallback<MouseLeaveEvent>(OnPortMouseLeave);
			Input.RegisterCallback<MouseLeaveEvent>(OnPortMouseLeave);
			
			inputContainer.Add(Input);
		}


		private void CreateOutputPorts()
		{
			switch (Node)
			{
				case CompositeNode:
					Output = new NodePort(Direction.Output, Port.Capacity.Multi);
					break;

				case DecoratorNode:
				case EventNode:
				case NestedRoot:
				case RootNode:
					Output = new NodePort(Direction.Output, Port.Capacity.Single);
					break;

				case ActionNode or NestedCall:
					break;
			}

			if (Output == null)
				return;
			
			Output.portName = "";
			Output.style.flexDirection = FlexDirection.ColumnReverse;
			Output.style.width = 25;

			Output.UnregisterCallback<MouseEnterEvent>(OnOutputPortMouseEnter);
			Output.RegisterCallback<MouseEnterEvent>(OnOutputPortMouseEnter);
			
			Output.UnregisterCallback<MouseLeaveEvent>(OnPortMouseLeave);
			Output.RegisterCallback<MouseLeaveEvent>(OnPortMouseLeave);
			
			outputContainer.Add(Output);
		}


		private int SortByHorizontalPosition(Node left, Node right)
		{
			return left.Position.x < right.Position.x ? -1 : 1;
		}

		private void OnPortMouseLeave(MouseLeaveEvent evt)
		{
			if (evt.target is not Port port)
				return;

			port.style.backgroundColor = new Color(0, 0, 0, 0);
		}

		private void OnInputPortMouseEnter(MouseEnterEvent evt)
		{
			if (evt.target is not Port port 
				|| evt.pressedButtons != 0)
				return;

			var parentColor = port.parent.resolvedStyle.backgroundColor;
			port.style.backgroundColor = new Color(parentColor.r * 1.4f, parentColor.g * 1.4f, parentColor.b * 1.4f, 0.7f);
		}
		

		private void OnOutputPortMouseEnter(MouseEnterEvent evt)
		{
			if (evt.target is not Port port
				|| evt.pressedButtons != 0)
				return;
			
			port.style.backgroundColor = new Color(0.8f, 0.6f, 0, 0.7f);
		}

		private void ApplyActiveNodeStateStyle(NodeState state)
		{
			style.borderLeftWidth = 5;
			style.borderRightWidth = 5;
			style.borderTopWidth = 5;
			style.borderBottomWidth = 5;
			style.opacity = 1.0f;

			switch (state)
			{
				case NodeState.Success:
					style.borderLeftColor = Color.green;
					style.borderRightColor = Color.green;
					style.borderTopColor = Color.green;
					style.borderBottomColor = Color.green;
					break;

				case NodeState.Failure:
					style.borderLeftColor = Color.red;
					style.borderRightColor = Color.red;
					style.borderTopColor = Color.red;
					style.borderBottomColor = Color.red;
					break;

				case NodeState.Running:
					style.borderLeftColor = Color.yellow;
					style.borderRightColor = Color.yellow;
					style.borderTopColor = Color.yellow;
					style.borderBottomColor = Color.yellow;
					break;
			}
		}
	}
}