using System;

namespace Swat.Game.BehaviourTree
{
	[UnityEngine.Scripting.APIUpdating.MovedFrom(true, sourceAssembly: "Game")]
	[Serializable]
	public abstract class ConditionNode : AbortableNode
	{
		public bool Enabled = true;

		public event Action<ConditionNode> OnValueChangedEvent;
		public event Action<ConditionNode> OnResultChangedEvent;
		
		public bool Invert;
		public ConditionMethod ConditionType;

		private bool lastResult;


		public abstract bool IsTrue();

		protected override void OnStart()
		{
			lastResult = IsTrue();
		}


		protected override void OnStop()
		{
		}


		protected override NodeState OnUpdate()
		{
			bool isTrue = lastResult;
			isTrue ^= Invert;
			isTrue |= !Enabled;

			return isTrue
				? Child.UpdateNode()
				: AbortProcess();
		}


		protected virtual void OnValueChanged()
		{
			OnValueChangedEvent?.Invoke(this);

			bool isTrue = IsTrue();

			if (lastResult != isTrue)
			{
				lastResult = isTrue;
				OnResultChangedEvent?.Invoke(this);
			}
		}


		public enum ConditionMethod : byte
		{
			CheckOnce = 0,
			OnResultChange = 1,
			OnValueChange = 2
		}
	}
}