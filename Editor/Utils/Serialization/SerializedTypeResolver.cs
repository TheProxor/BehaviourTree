using System;
using System.Reflection;

namespace Editor.BehaviourTreeEditor.Utils.Serialization
{
    internal static class SerializedTypeResolver
    {
        public static Type Resolve(string name)
        {
            return Type.GetType(name, assemblyName =>
            {
                if (assemblyName.Name == "Game") return typeof(Swat.Game.BehaviourTree.Node).Assembly;
                foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                    if (assembly.GetName().Name == assemblyName.Name) return assembly;
                return null;
            }, (assembly, typeName, ignoreCase) =>
            {
                var type = assembly?.GetType(typeName, false, ignoreCase);
                if (type != null) return type;
                foreach (var candidate in AppDomain.CurrentDomain.GetAssemblies())
                {
                    type = candidate.GetType(typeName, false, ignoreCase);
                    if (type != null) return type;
                }
                return null;
            }, false);
        }
    }
}
