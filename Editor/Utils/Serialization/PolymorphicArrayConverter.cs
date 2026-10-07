using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Editor.BehaviourTreeEditor.Utils.Serialization
{
	public class PolymorphicArrayConverter : JsonConverter
{
    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        if (value == null)
        {
            writer.WriteNull();
            return;
        }

        var array = (IEnumerable)value;
        writer.WriteStartArray();
        
        foreach (var item in array)
        {
            if (item == null)
            {
                writer.WriteNull();
                continue;
            }

            writer.WriteStartObject();
            writer.WritePropertyName("$type");
            writer.WriteValue(item.GetType().AssemblyQualifiedName);
            writer.WritePropertyName("$data");
            serializer.Serialize(writer, item);
            writer.WriteEndObject();
        }
        
        writer.WriteEndArray();
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        if (reader.TokenType == JsonToken.Null)
            return null;

        var elementType = objectType.GetElementType() ?? objectType.GenericTypeArguments[0];
        var listType = typeof(List<>).MakeGenericType(elementType);
        var list = (IList)Activator.CreateInstance(listType);

        JArray array = JArray.Load(reader);
        foreach (JObject item in array)
        {
            if (item == null || item.Type == JTokenType.Null)
            {
                list.Add(null);
                continue;
            }

            string typeName = item["$type"]?.ToString();
            if (string.IsNullOrEmpty(typeName))
                throw new JsonSerializationException("Type information is missing in array element");

            Type actualType = SerializedTypeResolver.Resolve(typeName);
            if (actualType == null)
                throw new JsonSerializationException($"Type '{typeName}' not found");

            var itemValue = item["$data"].ToObject(actualType, serializer);
            list.Add(itemValue);
        }

        if (objectType.IsArray)
            return ToArray(list, elementType);
        
        return list;
    }

    private static Array ToArray(IList list, Type elementType)
    {
        var array = Array.CreateInstance(elementType, list.Count);
        list.CopyTo(array, 0);
        return array;
    }

    public override bool CanConvert(Type objectType)
    {
        return objectType.IsArray || 
              (objectType.IsGenericType && 
               objectType.GetGenericTypeDefinition() == typeof(IEnumerable<>));
    }
}
}