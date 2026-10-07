using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Swat.Game.BehaviourTree.Blackboard;
using UnityEngine;

namespace Editor.BehaviourTreeEditor.Serialization
{
	public class BlackboardKeyValuePairFieldJsonConverter : JsonConverter<BlackboardKeyValuePair>
	{
		public override void WriteJson(JsonWriter writer, BlackboardKeyValuePair value, JsonSerializer serializer)
		{
			serializer.ContractResolver = new UniversalContractResolver();
			serializer.Serialize(writer, value);
		}

		public override BlackboardKeyValuePair ReadJson(JsonReader reader, Type objectType, BlackboardKeyValuePair existingValue, bool hasExistingValue, JsonSerializer serializer)
		{
			//serializer.ContractResolver = new BlackboardValueContractResolver();
			
			var jo = JObject.Load(reader);

			var keyNameToken = jo.SelectToken("Key.Name");
			var valueToken = jo.SelectToken("Value.Value");
			var typeToken = jo.SelectToken("Value.UnderlyingType");

			var keyName = keyNameToken.ToString();
			var type = typeToken.ToObject<Type>();

			return CreatePairByType(type, keyName, valueToken);
		}

		private BlackboardKeyValuePair CreatePairByType(Type type, string keyName, JToken valueToken)
		{
			BlackboardKey key = null;
			BlackboardKey value = null;
			
			if (type == typeof(bool))
			{
				key = new BooleanKey { Name = keyName };
				value = new BooleanKey { Value = valueToken.ToObject<bool>() };
			}
			
			if (type == typeof(float))
			{
				key = new FloatKey { Name = keyName };
				value = new FloatKey { Value = valueToken.ToObject<float>() };
			}
			
			if (type == typeof(int))
			{
				key = new IntKey { Name = keyName };
				value = new IntKey { Value = valueToken.ToObject<int>() };
			}
			
			if (type == typeof(string))
			{
				key = new StringKey { Name = keyName };
				value = new StringKey { Value = valueToken.ToObject<string>() };
			}
			
			if (type == typeof(double))
			{
				key = new DoubleKey { Name = keyName };
				value = new DoubleKey { Value = valueToken.ToObject<double>() };
			}
			
			if (type == typeof(Vector2))
			{
				key = new Vector2Key { Name = keyName };
				value = new Vector2Key { Value = valueToken.ToObject<Vector2>() };
			}
			
			if (type == typeof(Vector3))
			{
				key = new Vector3Key { Name = keyName };
				value = new Vector3Key { Value = valueToken.ToObject<Vector3>() };
			}
			
			if (type == typeof(Vector4))
			{
				key = new Vector4Key { Name = keyName };
				value = new Vector4Key { Value = valueToken.ToObject<Vector4>() };
			}
			
			if (type == typeof(Vector2Int))
			{
				key = new Vector2IntKey { Name = keyName };
				value = new Vector2IntKey { Value = valueToken.ToObject<Vector2Int>() };
			}
			
			if (type == typeof(Vector3Int))
			{
				key = new Vector3IntKey { Name = keyName };
				value = new Vector3IntKey { Value = valueToken.ToObject<Vector3Int>() };
			}
			
			if (type == typeof(Gradient))
			{
				key = new GradientKey { Name = keyName };
				value = new GradientKey { Value = valueToken.ToObject<Gradient>() };
			}
			
			if (type == typeof(Color))
			{
				key = new ColorKey { Name = keyName };
				value = new ColorKey { Value = valueToken.ToObject<Color>() };
			}
			
			if (type == typeof(LayerMask))
			{
				key = new LayerMaskKey { Name = keyName };
				value = new LayerMaskKey { Value = valueToken.ToObject<LayerMask>() };
			}
			
			if (type == typeof(AnimationCurve))
			{
				key = new CurveKey { Name = keyName };
				value = new CurveKey { Value = valueToken.ToObject<AnimationCurve>() };
			}
			
			if (type == typeof(Bounds))
			{
				key = new BoundsKey { Name = keyName };
				value = new BoundsKey { Value = valueToken.ToObject<Bounds>() };
			}
			
			if (type == typeof(BoundsInt))
			{
				key = new BoundsIntKey { Name = keyName };
				value = new BoundsIntKey { Value = valueToken.ToObject<BoundsInt>() };
			}
			
			// TODO: Deserialize reference on scene or project
			if (type == typeof(GameObject))
			{
				key = new GameObjectKey { Name = keyName };
				value = new GameObjectKey { Value = null };
			}
			
			if (type == typeof(Material))
			{
				key = new MaterialKey { Name = keyName };
				value = new MaterialKey { Value = null };
			}
			
			if (type == typeof(Rigidbody))
			{
				key = new RigidBodyKey { Name = keyName };
				value = new RigidBodyKey { Value = null };
			}
			
			if (type == typeof(Collider))
			{
				key = new ColliderKey { Name = keyName };
				value = new ColliderKey { Value = null };
			}

			return new BlackboardKeyValuePair(key, value);
		}
	}
}