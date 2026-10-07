using System;
using Swat.Game.BehaviourTree.Core;

namespace Editor.BehaviourTreeEditor.Game
{
    public static class GameSpecifics
    {
        public static Func<IBehaviourTree, bool> CycledTreeCondition { get; set; }
        public static string[] BlackboardDefaultBoolKeysNames { get; set; } = Array.Empty<string>();
        public static bool IsTreeCycled(IBehaviourTree tree) =>
            CycledTreeCondition?.Invoke(tree) ?? tree.Context?.IsCycled ?? false;
    }
}
