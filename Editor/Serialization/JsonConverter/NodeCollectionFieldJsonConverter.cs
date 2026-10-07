using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Swat.Game.BehaviourTree;

namespace Editor.BehaviourTreeEditor.Serialization
{
	public class NodeCollectionFieldJsonConverter : JsonConverter<IEnumerable<Node>>
	{
		public override void WriteJson(JsonWriter writer, IEnumerable<Node> value, JsonSerializer serializer)
		{
			var simpleSerializer = JsonSerializer.Create();
			var guids = value.Select(n => n.Guid).ToArray();
			
			simpleSerializer.Serialize(writer, guids);
		}

		public override IEnumerable<Node> ReadJson(JsonReader reader, Type objectType, IEnumerable<Node> existingValue, bool hasExistingValue, JsonSerializer serializer)
		{
			var proxyNodes = new List<Node>();
			var guids = serializer.Deserialize(reader);
			var jArray = guids as JArray;

			foreach (var jToken in jArray)
			{
				string guid = jToken.Value<string>();
				var proxyNode = new NodeProxy();
				proxyNode.Guid = guid;
				
				proxyNodes.Add(proxyNode);
			}
			
			return proxyNodes;
		}
	}
}