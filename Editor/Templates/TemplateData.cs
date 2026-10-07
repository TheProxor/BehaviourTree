using UnityEngine;

namespace Editor.BehaviourTreeEditor.Templates
{
	public struct TemplateData
	{
		public string Guid { get; private set; }
		public string Path { get; private set; }
		public string Name { get; private set; }
		public TextAsset Json { get; private set; }

		public TemplateData(string guid, string path, string name, TextAsset json)
		{
			Guid = guid;
			Path = path;
			Name = name;
			Json = json;
		}
	}
}