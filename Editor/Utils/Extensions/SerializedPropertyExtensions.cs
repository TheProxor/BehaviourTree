using UnityEditor;

namespace Editor.BehaviourTreeEditor.Utils.Extensions
{
	public static partial class Extensions
	{
		public static SerializedProperty GetParentProperty(this SerializedProperty property)
		{
			string path = property.propertyPath;
			if (path.EndsWith("]"))
			{
				int index = path.LastIndexOf('.');
				if (index >= 0) path = path.Substring(0, index);
			}

			string parentPath = path.Contains(".") ? path.Substring(0, path.LastIndexOf('.')) : "";

			if (string.IsNullOrEmpty(parentPath)) return null;

			return property.serializedObject.FindProperty(parentPath);
		}

		public static SerializedProperty GetRootParent(this SerializedProperty property)
		{
			string path = property.propertyPath;
			if (string.IsNullOrEmpty(path) || !path.Contains("."))
				return null;

			string rootPath = path.Split('.')[0];
			return property.serializedObject.FindProperty(rootPath);
		}


		public static bool IsArrayElement(this SerializedProperty property)
		{
			return property.propertyPath.Contains(".Array.data[");
		}

		public static int GetArrayIndex(this SerializedProperty property)
		{
			if (!property.IsArrayElement())
				return -1;

			string path = property.propertyPath;
			int start = path.LastIndexOf("[") + 1;
			int end = path.LastIndexOf("]");
			string indexStr = path.Substring(start, end - start);
			return int.Parse(indexStr);
		}

		public static SerializedProperty GetContainingArray(this SerializedProperty property)
		{
			if (!property.IsArrayElement())
				return null;

			string path = property.propertyPath;
			int arrayEnd = path.LastIndexOf(".Array.data[");
			string arrayPath = path.Substring(0, arrayEnd);
			return property.serializedObject.FindProperty(arrayPath);
		}
	}
}