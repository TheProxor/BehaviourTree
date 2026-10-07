using System;
using System.Collections.Generic;
using UnityEngine;

namespace Swat.Game.BehaviourTree.Blackboard
{
	[UnityEngine.Scripting.APIUpdating.MovedFrom(true, sourceAssembly: "Game")]
	[Serializable]
	public abstract class BlackboardKey : ISerializationCallbackReceiver
	{
		public abstract event Action OnValueChanged;
		public string Name;
		public Type UnderlyingType;
		public string TypeName;
		public bool IsUndeletable;

		protected BlackboardKey(Type underlyingType)
		{
			UnderlyingType = underlyingType;
			TypeName = UnderlyingType.FullName;
		}


		public static BlackboardKey CreateKey(Type type)
		{
			return Activator.CreateInstance(type) as BlackboardKey;
		}


		public void OnBeforeSerialize()
		{
			TypeName = UnderlyingType.AssemblyQualifiedName;
		}


		public void OnAfterDeserialize()
		{
			UnderlyingType = Type.GetType(TypeName);
		}


		public abstract void CopyFrom(BlackboardKey key);


		public abstract bool Equals(BlackboardKey key);

		public abstract bool Greater(BlackboardKey key);

		public abstract bool Less(BlackboardKey key);
	}


	[UnityEngine.Scripting.APIUpdating.MovedFrom(true, sourceAssembly: "Game")]
	[Serializable]
	public abstract class BlackboardKey<T> : BlackboardKey
	{
		public override event Action OnValueChanged;

		public T Value;


		protected BlackboardKey() : base(typeof(T))
		{
		}


		public void SetValue(T value)
		{
			Value = value;
			OnValueChanged?.Invoke();
		}


		public override string ToString()
		{
			return $"{Name} : {Value}";
		}


		public override void CopyFrom(BlackboardKey key)
		{
			if (key.UnderlyingType == UnderlyingType)
			{
				var other = key as BlackboardKey<T>;
				Value = other.Value;
			}
		}


		public override bool Equals(BlackboardKey key)
		{
			if (key.UnderlyingType != UnderlyingType)
				return false;

			if (key is BlackboardKey<T> other)
				return EqualityComparer<T>.Default.Equals(Value, other.Value);

			return false;
		}


		public override bool Greater(BlackboardKey key)
		{
			Debug.LogError($"Greater is not supported for this BlackboardKey type {UnderlyingType}");
			return false;
		}


		public override bool Less(BlackboardKey key)
		{
			Debug.LogError($"Less is not supported for this BlackboardKey type {UnderlyingType}");
			return false;
		}
	}
}