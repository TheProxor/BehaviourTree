using System;
using System.Collections.Generic;
using UnityEngine;

namespace Swat.Game.BehaviourTree
{
	[UnityEngine.Scripting.APIUpdating.MovedFrom(true, sourceAssembly: "Game")]
	[Serializable]
	public abstract class CompositeNode : Node
	{
		[HideInInspector] [SerializeReference] public List<Node> Children = new();
	}
}