using Swat.Game.BehaviourTree.Contexts;
using UnityEngine;

namespace Swat.Game.BehaviourTree
{
    /// <summary>Runs a private instance of a tree asset without game-specific services.</summary>
    [AddComponentMenu("Behaviour Tree/Behaviour Tree Runner")]
    public class BehaviourTreeRunner : MonoBehaviour
    {
        [SerializeField] private BehaviourTree treeAsset;
        [SerializeField] private bool loop;

        public BehaviourTree TreeAsset { get => treeAsset; set => treeAsset = value; }
        public BehaviourTree RuntimeTree { get; private set; }

        protected virtual RuntimeTreeContext CreateContext()
        {
            var context = new RuntimeTreeContext { IsCycled = loop };
            context.AssignTransform(transform);
            return context;
        }

        private void Start()
        {
            if (treeAsset == null)
            {
                Debug.LogWarning("Assign a BehaviourTree asset to the runner.", this);
                enabled = false;
                return;
            }

            RuntimeTree = (BehaviourTree)treeAsset.Clone();
            RuntimeTree.Bind(CreateContext());
        }

        private void Update() => RuntimeTree?.UpdateTree();

        private void OnDestroy()
        {
            if (RuntimeTree == null) return;
            RuntimeTree.RootNode?.AbortChildren();
            foreach (var node in RuntimeTree.EventNodes) node?.AbortChildren();
            foreach (var node in RuntimeTree.NestedRoots) node?.AbortChildren();
            RuntimeTree.Dispose();
            Destroy(RuntimeTree);
            RuntimeTree = null;
        }
    }
}
