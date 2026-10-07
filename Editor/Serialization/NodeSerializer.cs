using System.Linq;
using Editor.BehaviourTreeEditor.Utils.Serialization;
using Newtonsoft.Json;
using Swat.Game.BehaviourTree;

namespace Editor.BehaviourTreeEditor.Serialization
{
	public static class NodeSerializer
	{
		// (!) TypeNameHandling must be None. It's required for custom converter
		private static JsonSerializerSettings NodeSettings =>
			new()
			{
				TypeNameHandling = TypeNameHandling.None,
				ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
				Converters = 
				{ 
					new PolymorphicConverter(), 
					new PolymorphicArrayConverter() 
				},
				ContractResolver = new UniversalContractResolver()
			};
		
		
		public static string Serialize(Node node)
		{
			var serializedNode = JsonConvert.SerializeObject(node, Formatting.Indented, NodeSettings);

			return serializedNode;
		}
		
		public static string SerializeRange(Node[] nodes)
		{
			var serializedNode = JsonConvert.SerializeObject(nodes, Formatting.Indented, NodeSettings);

			return serializedNode;
		}
		
		public static Node Deserialize(string serializedNode)
		{
			// (!) Need to call with <object> type to use custom converter. If call without type, custom converter won't be called ReadJson method
			var deserializeObject = JsonConvert.DeserializeObject<object>(serializedNode, NodeSettings);

			if(deserializeObject is Node)
				return deserializeObject as Node;

			return null;
		}
		
		public static Node[] DeserializeRange(string serializedRange)
		{
			// (!) Need to call with <object> type to use custom converter. If call without type, custom converter won't be called ReadJson method
			var deserializeObject = JsonConvert.DeserializeObject<object[]>(serializedRange, NodeSettings);

			return deserializeObject?.Cast<Node>().ToArray();
		}
	}
}
