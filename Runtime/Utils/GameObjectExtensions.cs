using System.Text;
using UnityEngine;

namespace Swat.Game.BehaviourTree.Utils
{
	public static partial class Extensions
	{
		public static string GetFullPath(this GameObject go)
		{
			if (go == null)
				return "null";

			StringBuilder path = new StringBuilder(go.name);
			Transform current = go.transform.parent;

			while (current != null)
			{
				path.Insert(0, current.name + "/");
				current = current.parent;
			}

			return path.ToString();
		}
	}
}