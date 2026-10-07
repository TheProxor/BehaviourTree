using System.Collections.Generic;
using UnityEngine;

namespace Swat.Game.BehaviourTree.Contexts
{
    public class RuntimeTreeContext
    {
        public Dictionary<string, NodeState> TickResults { get; } = new();
        public bool IsCycled { get; set; }
        public Transform Transform { get; private set; }

        public void AssignTransform(Transform transform) => Transform = transform;
    }
}
