using System;
using UnityEngine;

namespace Swat.Game.BehaviourTree.Blackboard
{
	[UnityEngine.Scripting.APIUpdating.MovedFrom(true, sourceAssembly: "Game")]
	[Serializable]
	public class BooleanKey : BlackboardKey<bool>
	{
	}


	[UnityEngine.Scripting.APIUpdating.MovedFrom(true, sourceAssembly: "Game")]
	[Serializable]
	public class IntKey : BlackboardKey<int>
	{
		public override bool Greater(BlackboardKey other)
		{
			return Value > ((IntKey)other).Value;
		}

		public override bool Less(BlackboardKey other)
		{
			return Value < ((IntKey)other).Value;
		}
	}


	[UnityEngine.Scripting.APIUpdating.MovedFrom(true, sourceAssembly: "Game")]
	[Serializable]
	public class FloatKey : BlackboardKey<float>
	{
		public override bool Greater(BlackboardKey other)
		{
			return Value > ((FloatKey)other).Value;
		}

		public override bool Less(BlackboardKey other)
		{
			return Value < ((FloatKey)other).Value;
		}
	}


	[UnityEngine.Scripting.APIUpdating.MovedFrom(true, sourceAssembly: "Game")]
	[Serializable]
	public class DoubleKey : BlackboardKey<double>
	{
		public override bool Greater(BlackboardKey other)
		{
			return Value > ((DoubleKey)other).Value;
		}

		public override bool Less(BlackboardKey other)
		{
			return Value < ((DoubleKey)other).Value;
		}
	}


	[UnityEngine.Scripting.APIUpdating.MovedFrom(true, sourceAssembly: "Game")]
	[Serializable]
	public class StringKey : BlackboardKey<string>
	{
	}


	[UnityEngine.Scripting.APIUpdating.MovedFrom(true, sourceAssembly: "Game")]
	[Serializable]
	public class Vector2Key : BlackboardKey<Vector2>
	{
	}


	[UnityEngine.Scripting.APIUpdating.MovedFrom(true, sourceAssembly: "Game")]
	[Serializable]
	public class Vector3Key : BlackboardKey<Vector3>
	{
	}


	[UnityEngine.Scripting.APIUpdating.MovedFrom(true, sourceAssembly: "Game")]
	[Serializable]
	public class Vector4Key : BlackboardKey<Vector4>
	{
	}


	[UnityEngine.Scripting.APIUpdating.MovedFrom(true, sourceAssembly: "Game")]
	[Serializable]
	public class Vector2IntKey : BlackboardKey<Vector2Int>
	{
	}


	[UnityEngine.Scripting.APIUpdating.MovedFrom(true, sourceAssembly: "Game")]
	[Serializable]
	public class Vector3IntKey : BlackboardKey<Vector3Int>
	{
	}


	[UnityEngine.Scripting.APIUpdating.MovedFrom(true, sourceAssembly: "Game")]
	[Serializable]
	public class GradientKey : BlackboardKey<Gradient>
	{
	}


	[UnityEngine.Scripting.APIUpdating.MovedFrom(true, sourceAssembly: "Game")]
	[Serializable]
	public class ColorKey : BlackboardKey<Color>
	{
	}


	[UnityEngine.Scripting.APIUpdating.MovedFrom(true, sourceAssembly: "Game")]
	[Serializable]
	public class LayerMaskKey : BlackboardKey<LayerMask>
	{
	}


	[UnityEngine.Scripting.APIUpdating.MovedFrom(true, sourceAssembly: "Game")]
	[Serializable]
	public class CurveKey : BlackboardKey<AnimationCurve>
	{
	}


	[UnityEngine.Scripting.APIUpdating.MovedFrom(true, sourceAssembly: "Game")]
	[Serializable]
	public class BoundsKey : BlackboardKey<Bounds>
	{
	}


	[UnityEngine.Scripting.APIUpdating.MovedFrom(true, sourceAssembly: "Game")]
	[Serializable]
	public class BoundsIntKey : BlackboardKey<BoundsInt>
	{
	}


	[UnityEngine.Scripting.APIUpdating.MovedFrom(true, sourceAssembly: "Game")]
	[Serializable]
	public class GameObjectKey : BlackboardKey<GameObject>
	{
	}


	[UnityEngine.Scripting.APIUpdating.MovedFrom(true, sourceAssembly: "Game")]
	[Serializable]
	public class MaterialKey : BlackboardKey<Material>
	{
	}


	[UnityEngine.Scripting.APIUpdating.MovedFrom(true, sourceAssembly: "Game")]
	[Serializable]
	public class RigidBodyKey : BlackboardKey<Rigidbody>
	{
	}


	[UnityEngine.Scripting.APIUpdating.MovedFrom(true, sourceAssembly: "Game")]
	[Serializable]
	public class ColliderKey : BlackboardKey<Collider>
	{
	}
	
	[UnityEngine.Scripting.APIUpdating.MovedFrom(true, sourceAssembly: "Game")]
	[Serializable]
	public class TransformKey : BlackboardKey<Transform>
	{
	}
}