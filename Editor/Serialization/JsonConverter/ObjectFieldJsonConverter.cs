using System;
using Newtonsoft.Json;

namespace Editor.BehaviourTreeEditor.Serialization
{
	public class ObjectFieldJsonConverter : JsonConverter<UnityEngine.Object>
	{
		public override void WriteJson(JsonWriter writer, UnityEngine.Object value, JsonSerializer serializer)
		{
			writer.WriteValue(value.GetInstanceID());
		}

		public override UnityEngine.Object ReadJson(JsonReader reader, Type objectType, UnityEngine.Object existingValue, bool hasExistingValue, JsonSerializer serializer)
		{
			// Skip references from scene
			return null;
		}
	}
}