using UnityEditor;
using UnityEngine.UIElements;

namespace Editor.BehaviourTreeEditor.NodeInfoDrawers
{
	public interface INodeInfoDrawer
	{
		bool UseForChildren { get; }
		void Draw(SerializedProperty target, Label container);
	}
}