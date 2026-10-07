using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace Editor.BehaviourTreeEditor.Serialization
{
	public class ObjectCollectionFieldJsonConverter : JsonConverter<IEnumerable<UnityEngine.Object>>
	{
		public override void WriteJson(JsonWriter writer, IEnumerable<UnityEngine.Object> value, JsonSerializer serializer)
		{
			var simpleSerializer = JsonSerializer.Create();
			var guids = value.Select(o => o.GetInstanceID()).ToArray();

			simpleSerializer.Serialize(writer, guids);
		}

		public override IEnumerable<UnityEngine.Object> ReadJson(JsonReader reader, Type objectType, IEnumerable<UnityEngine.Object> existingValue, bool hasExistingValue, JsonSerializer serializer)
		{
			// Skip references from scene
			return null;
		}
	}
}