using System;
using UnityEngine;

#if UNITY_EDITOR
using System.Collections;
using UnityEditor;
using DGames.Essentials.EditorHelpers;
#endif

namespace DGames.Essentials.Attributes
{
	[AttributeUsage(AttributeTargets.Field)]

	public class ConditionAttribute : BaseConditionAttribute
	{
		private readonly string _propertyToCheck;
		private readonly object _compareValue;
		private readonly CompareOperator _op;

		public ConditionAttribute(string propertyToCheck, object compareValue = null,ConditionType type = ConditionType.Show,CompareOperator op = CompareOperator.Equal):base(type)
		{
			_propertyToCheck = propertyToCheck;
			_compareValue = compareValue;
			_op = op;
		}

#if UNITY_EDITOR

		public override bool IsConditionSatisfy(SerializedProperty property)
		{
			var conditionProperty = property.FindRelativePropertyAd(_propertyToCheck);
			if (conditionProperty == null) return true;

			var valueType = conditionProperty.GetValueType();
			var value = conditionProperty.ToObjectValue(valueType);
			
			return IsCompareSatisfy(value,_compareValue,_op);
		}

		public static bool IsCompareSatisfy(object first, object second, CompareOperator op)
		{
			if( first.GetType() != second.GetType())
			{
				return true;
			}

			return op switch
			{
				CompareOperator.Equal => Comparer.Default.Compare(first, second) == 0,
				CompareOperator.GreaterThan => Comparer.Default.Compare(first, second) > 0,
				CompareOperator.LessThan => Comparer.Default.Compare(first, second) < 0,
				CompareOperator.GreaterThanOrEqual => Comparer.Default.Compare(first, second) >= 0,
				CompareOperator.LessThanOrEqual => Comparer.Default.Compare(first, second) <= 0,
				_ => throw new ArgumentOutOfRangeException(nameof(op), op, null)
			};
		}
#endif
	}
	
	public enum CompareOperator
	{
		Equal,GreaterThan,LessThan,GreaterThanOrEqual,LessThanOrEqual
	}
}