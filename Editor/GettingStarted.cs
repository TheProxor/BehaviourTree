using Swat.Game.BehaviourTree;
using Swat.Game.BehaviourTree.Actions;
using UnityEditor;
using UnityEngine;

namespace Editor.BehaviourTreeEditor
{
    public static class GettingStarted
    {
        [MenuItem("BehaviourTree/Create Example Tree")]
        public static void CreateExample()
        {
            var path = UnityEditor.EditorUtility.SaveFilePanelInProject("Create example tree", "ExampleTree", "asset", "Choose where to save the example.");
            if (string.IsNullOrEmpty(path)) return;
            var tree = ScriptableObject.CreateInstance<BehaviourTree>();
            var log = new Log { Description = "Hello from Behaviour Tree", Position = new Vector2(0, 200) };
            tree.RootNode.Child = log;
            tree.Nodes.Add(log);
            AssetDatabase.CreateAsset(tree, path);
            AssetDatabase.SaveAssets();
            Selection.activeObject = tree;
            BehaviourTreeEditorWindow.OpenWindow(tree);
        }
    }
}
