using System;
using UnityEngine;

namespace DGames.Essentials.Editor
{
    public class TypeToTypeAttribute : Attribute
    {
        public TypeToTypeAttribute(Type type)
        {
            if (type == null)
                Debug.LogError("Failed to load  type");
            Type = type;
            ForChildren = false;
        }

        public TypeToTypeAttribute(Type type, bool forChildren)
        {
            if (type == null)
                Debug.LogError("Failed to load type");
            Type = type;
            ForChildren = forChildren;
        }

        public Type Type { get; }
        public bool ForChildren { get; }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class CustomMenuEditor : TypeToTypeAttribute
    {
        public CustomMenuEditor(Type type) : base(type)
        {
        }

      
        public CustomMenuEditor(Type type, bool forChildren) : base(type, forChildren)
        {
        }
        // public bool IsFallback { get; set; }
    }
}