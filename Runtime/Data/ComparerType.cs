using UnityEngine;

namespace Swat.Game.BehaviourTree.Data
{
	public enum ComparerType : byte
	{
		Equal = 1,
		Greater = 2,
		Less = 3,
		NotEqual = 4,
		GEqual = 5,
		LEqual = 6
	}
	
	public static class ComparerTypeExtensions
	{
		public static bool IsMet(this ComparerType comparer, float leftOperand, float rightOperand)
		{
			bool result = comparer switch
			{
				ComparerType.Equal => Mathf.Approximately(leftOperand, rightOperand),
				ComparerType.Greater => leftOperand > rightOperand,
				ComparerType.Less => leftOperand < rightOperand,
				ComparerType.NotEqual => !Mathf.Approximately(leftOperand, rightOperand),
				ComparerType.GEqual => leftOperand >= rightOperand,
				ComparerType.LEqual => leftOperand <= rightOperand,
				_ => false
			};

			return result;
		}
		
		public static bool IsMet(this ComparerType comparer, int leftOperand, int rightOperand)
		{
			bool result = comparer switch
			{
				ComparerType.Equal => leftOperand == rightOperand,
				ComparerType.Greater => leftOperand > rightOperand,
				ComparerType.Less => leftOperand < rightOperand,
				ComparerType.NotEqual => leftOperand == rightOperand,
				ComparerType.GEqual => leftOperand >= rightOperand,
				ComparerType.LEqual => leftOperand <= rightOperand,
				_ => false
			};

			return result;
		}

		public static string ToSymbol(this ComparerType comparer)
		{
			string symbol = comparer switch
			{
				ComparerType.Equal => "==",
				ComparerType.NotEqual => "!=",
				ComparerType.Greater => ">",
				ComparerType.Less => "<",
				ComparerType.GEqual => ">=",
				ComparerType.LEqual => "<=",
				_ => "?"
			};

			return symbol;
		}
	}
}