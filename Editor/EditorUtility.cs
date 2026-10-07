using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Swat.Game.BehaviourTree;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Editor.BehaviourTreeEditor
{
	public static class EditorUtility
	{
		public static BehaviourTree CreateNewTree()
		{
			var settings = BehaviourTreeEditorWindow.Instance.settings;

			string savePath =
				UnityEditor.EditorUtility.SaveFilePanel("Create New", settings.newTreePath, "New Behavior Tree",
					"asset");

			if (string.IsNullOrEmpty(savePath)) return null;

			string assetName = Path.GetFileNameWithoutExtension(savePath);
			string folder = Path.GetDirectoryName(savePath);
			folder = folder.Substring(folder.IndexOf("Assets"));


			string path = Path.Join(folder, $"{assetName}.asset");

			if (File.Exists(path))
			{
				Debug.LogError($"Failed to create behaviour tree asset: Path already exists:{assetName}");
				return null;
			}

			BehaviourTree tree = ScriptableObject.CreateInstance<BehaviourTree>();
			tree.name = assetName;
			AssetDatabase.CreateAsset(tree, path);
			AssetDatabase.SaveAssets();
			EditorGUIUtility.PingObject(tree);
			return tree;
		}


		public static void CreateNewScript(ScriptTemplate scriptTemplate, NodeView source, bool isSourceParent,
			Vector2 position)
		{
			BehaviourTreeEditorWindow.Instance.newScriptDialog.CreateScript(scriptTemplate, source, isSourceParent,
				position);
		}
		
		public static void OpenNewTemplateDialog(IReadOnlyList<ISelectable> selection)
		{
			BehaviourTreeEditorWindow.Instance.newTemplateDialog.OpenWithSelection(selection);
		}


		public static List<T> LoadAssets<T>() where T : Object
		{
			string[] assetIds = AssetDatabase.FindAssets($"t:{typeof(T).Name}");
			List<T> assets = new List<T>();

			foreach (var assetId in assetIds)
			{
				string path = AssetDatabase.GUIDToAssetPath(assetId);
				T asset = AssetDatabase.LoadAssetAtPath<T>(path);
				assets.Add(asset);
			}

			return assets;
		}


		public static List<string> GetAssetPaths<T>() where T : Object
		{
			string[] assetIds = AssetDatabase.FindAssets($"t:{typeof(T).Name}");
			List<string> paths = new List<string>();

			foreach (var assetId in assetIds)
			{
				string path = AssetDatabase.GUIDToAssetPath(assetId);
				paths.Add(path);
			}

			return paths;
		}


		public static PackageManifest GetPackageManifest()
		{
			// Loop through all package.json files in the project and find this one..  
			string[] packageJsons = AssetDatabase.FindAssets("package");
			string[] packagePaths = packageJsons.Select(AssetDatabase.GUIDToAssetPath).ToArray();

			foreach (var path in packagePaths)
			{
				var asset = AssetDatabase.LoadAssetAtPath<TextAsset>(path);

				if (asset)
					try
					{
						PackageManifest manifest = JsonUtility.FromJson<PackageManifest>(asset.text);

						if (manifest.name == "com.thekiwicoder.behaviourtreeditor") return manifest;
					}
					catch
					{
						// Ignore if the manifest file failed to parse 
					}
			}

			return null;
		}


		public static float SnapTo(float value, int nearestInteger)
		{
			return Mathf.RoundToInt(value / nearestInteger) * nearestInteger;
		}


		public static TextAsset GetNodeScriptPath(NodeView nodeView)
		{
			var nodeName = nodeView.Node.GetType().Name;
			var assetGuids = AssetDatabase.FindAssets($"t:TextAsset {nodeName}");

			for (int i = 0; i < assetGuids.Length; ++i)
			{
				var path = AssetDatabase.GUIDToAssetPath(assetGuids[i]);
				var filename = Path.GetFileName(path);

				if (filename == $"{nodeName}.cs")
				{
					var script = AssetDatabase.LoadAssetAtPath<TextAsset>(path);
					return script;
				}
			}

			return null;
		}


		public static void OpenScriptInEditor(NodeView nodeView)
		{
			var script = GetNodeScriptPath(nodeView);

			if (script)
				// Open script in the editor:
				AssetDatabase.OpenAsset(script);
		}
		
		public struct ScriptTemplate
		{
			public TextAsset templateFile;
			public string defaultFileName;
			public string subFolder;
		}


		[Serializable]
		public class PackageManifest
		{
			public string name;
			public string version;
		}
	}
}