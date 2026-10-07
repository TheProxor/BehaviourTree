using System;
using System.Collections.Generic;
using System.IO;
using Editor.BehaviourTreeEditor.Serialization;
using Swat.Game.BehaviourTree;
using UnityEditor;
using UnityEngine;

namespace Editor.BehaviourTreeEditor.Templates
{
	public static class TemplatesProvider
	{
		private const string TemplateExtension = ".json";
		private const string FolderPath = "Assets/BehaviourTree/Templates";

		public static void SaveTemplate(string templateName, Node[] nodes)
		{
			Directory.CreateDirectory(FolderPath);
			var filePath = GetFilePath(templateName);

			var serializedTemplate = NodeSerializer.SerializeRange(nodes);

			File.WriteAllText(filePath, serializedTemplate);
			AssetDatabase.Refresh();
		}
		
		public static Node[] LoadTemplate(string templateName)
		{
			var textAsset = FindTemplate(templateName);
			
			return LoadTemplate(textAsset);
		}
		
		public static Node[] LoadTemplate(TextAsset templateJson)
		{
			var serializedTemplate = templateJson.text;

			return NodeSerializer.DeserializeRange(serializedTemplate);
		}

		public static string RenameTemplate(string oldPath, string newFileName)
		{
			string directory = Path.GetDirectoryName(oldPath);

			string fullFileName = Path.GetFileName(oldPath); // name.ext
			string extension = Path.GetExtension(fullFileName); // .ext

			string newPath = Path.Combine(directory, newFileName + extension);
			
			return AssetDatabase.MoveAsset(oldPath, newPath);
		}

		public static void DeleteTemplate(string templateName)
		{
			var filePath = GetFilePath(templateName);

			if (!File.Exists(filePath))
				return;
			
			AssetDatabase.DeleteAsset(filePath);
			AssetDatabase.Refresh();
		}

		public static void DeleteTemplateAtPath(string filePath)
		{
			var templateName = GetFileNameWithoutExtension(filePath);
			DeleteTemplate(templateName);
		}

		public static TextAsset FindTemplate(string templateName)
		{
			var allTemplates = FindAllTemplates();

			foreach (var data in allTemplates)
			{
				if (data.Name.Equals(templateName, StringComparison.OrdinalIgnoreCase))
					return data.Json;
			}

			return null;
		}

		public static TemplateData[] FindAllTemplates()
		{
			if (!AssetDatabase.IsValidFolder(FolderPath)) return Array.Empty<TemplateData>();
			var guids = AssetDatabase.FindAssets("t:TextAsset", new[] { FolderPath });
			var data = new List<TemplateData>();
			
			foreach (string guid in guids)
			{
				var path = AssetDatabase.GUIDToAssetPath(guid);

				if (!path.EndsWith($"{TemplateExtension}", StringComparison.OrdinalIgnoreCase))
					continue;

				var name = GetFileNameWithoutExtension(path);
				var json = AssetDatabase.LoadAssetAtPath<TextAsset>(path);
				
				data.Add(new TemplateData(guid, path, name, json));
			}

			return data.ToArray();
		}

		public static string GetFileNameWithoutExtension(string path)
		{
			string fileName = Path.GetFileName(path); // name.json
			return Path.GetFileNameWithoutExtension(fileName); // name
		}

		public static bool HasTemplate(string templateName)
		{
			var filePath = GetFilePath(templateName);
			return File.Exists(filePath);
		}

		private static string GetFilePath(string templateName)
		{
			return $"{FolderPath}/{templateName}{TemplateExtension}";
		}
	}
}
