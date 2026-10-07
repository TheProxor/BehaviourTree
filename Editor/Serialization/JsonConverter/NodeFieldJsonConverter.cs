using System;
using Newtonsoft.Json;
using Swat.Game.BehaviourTree;

namespace Editor.BehaviourTreeEditor.Serialization
{
	public class NodeFieldJsonConverter : JsonConverter<Node>
	{
		public override void WriteJson(JsonWriter writer, Node value, JsonSerializer serializer)
		{
			writer.WriteValue(value.Guid);
		}

		public override Node ReadJson(JsonReader reader, Type objectType, Node existingValue, bool hasExistingValue, JsonSerializer serializer)
		{
			if(reader.TokenType == JsonToken.Null || reader.Value == null) 
				return null;
			
			var guid = reader.Value;
			var proxyNode = new NodeProxy();

			proxyNode.Guid = guid as string;
			
			return proxyNode;
		}
	}
}