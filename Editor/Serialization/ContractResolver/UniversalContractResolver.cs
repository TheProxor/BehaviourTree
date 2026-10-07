using System.Collections.Generic;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Swat.Game.BehaviourTree;
using Swat.Game.BehaviourTree.Blackboard;

namespace Editor.BehaviourTreeEditor.Serialization
{
	public class UniversalContractResolver : DefaultContractResolver
	{
		protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
		{
			var property = base.CreateProperty(member, memberSerialization);

			// Child
			if (property.PropertyType == typeof(Node))
			{
				property.Converter = new NodeFieldJsonConverter();
				return property;
			}
			
			// Children
			if (typeof(IEnumerable<Node>).IsAssignableFrom(property.PropertyType))
			{
				property.Converter = new NodeCollectionFieldJsonConverter();
				return property;
			}
			
			// Object
			if (typeof(UnityEngine.Object).IsAssignableFrom(property.PropertyType))
			{
				property.Converter = new ObjectFieldJsonConverter();
				return property;
			}
			
			// Objects
			if (typeof(IEnumerable<UnityEngine.Object>).IsAssignableFrom(property.PropertyType))
			{
				property.Converter = new ObjectCollectionFieldJsonConverter();
				property.Writable = false;		// Collections in Node collection cant deserialize correctly with this converter - dont know why.
												// We dont need to deserialize references from scene, ignore this field
				return property;
			}
			
			// Blackboard
			if (property.PropertyType == typeof(BlackboardKeyValuePair))
			{
				property.Converter = new BlackboardKeyValuePairFieldJsonConverter();
				return property;
			}

			return property;
		}
	}
}