using System;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Editor.BehaviourTreeEditor.Utils.Serialization
{
	public class PolymorphicConverter : JsonConverter
	{
		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			var tempSerializer = CreateTemporarySerializer(serializer);

			writer.WriteStartObject();
			writer.WritePropertyName("$type");
			writer.WriteValue(value.GetType().AssemblyQualifiedName);
			writer.WritePropertyName("$data");
			tempSerializer.Serialize(writer, value);
			writer.WriteEndObject();
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			JObject jo = JObject.Load(reader);

			// Get type information
			string typeName = jo["$type"]?.ToString();
			if (string.IsNullOrEmpty(typeName))
				throw new JsonSerializationException("Type information is missing");

			Type actualType = SerializedTypeResolver.Resolve(typeName);
			if (actualType == null)
				throw new JsonSerializationException($"Type '{typeName}' not found");

			var tempSerializer = CreateTemporarySerializer(serializer);

			// Deserialize data into a specific type
			using (var dataReader = jo["$data"].CreateReader())
			{
				return tempSerializer.Deserialize(dataReader, actualType);
			}
		}

		public override bool CanConvert(Type objectType)
		{
			// (!) This converter should handle all types
			return true;
		}

		private JsonSerializer CreateTemporarySerializer(JsonSerializer mainSerializer)
		{
			// Creating a temporary serializer to avoid infinite recursion
			var tempSerializer = JsonSerializer.Create(new JsonSerializerSettings
			{
				ReferenceLoopHandling = mainSerializer.ReferenceLoopHandling,
				NullValueHandling = mainSerializer.NullValueHandling,
				ContractResolver = mainSerializer.ContractResolver,
				Converters = mainSerializer.Converters
					.Where(c => c is not PolymorphicConverter)
					.ToList()
			});

			return tempSerializer;
		}
	}
}