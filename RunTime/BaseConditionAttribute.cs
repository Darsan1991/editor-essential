using System;
using UnityEditor;
using UnityEngine;

namespace DGames.Essentials.Attributes
{
    [AttributeUsage(AttributeTargets.Field)]

    public abstract class BaseConditionAttribute : PropertyAttribute
    {
        public ConditionType Type { get; }

        protected BaseConditionAttribute(ConditionType type)
        {
            Type = type;
        }
        
#if UNITY_EDITOR

        public abstract bool IsConditionSatisfy(SerializedProperty property);
#endif
    }
    
    public enum ConditionType
    {
        Show,Enable
    }
}