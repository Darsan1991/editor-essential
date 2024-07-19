using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace DGames.Essentials.Base
{
    public class SerializeInterfaceField : PropertyAttribute
    {
            
        public static IEnumerable<FieldInfo> GetAllSerializedInterfaceFields(Type type)
        {
            foreach (var fieldInfo in type.GetFields(BindingFlags.Instance | BindingFlags.Public
                                                                           | BindingFlags.NonPublic).Where(f =>
                         f.GetCustomAttribute<SerializeInterfaceField>() != null))
            {
                yield return fieldInfo;
            }
            
            var fields = new List<FieldInfo>();
            while (type.BaseType != null && type.BaseType != typeof(MonoBehaviour) &&
                   type.BaseType != typeof(ScriptableObject))
            {
                type = type.BaseType;
                fields.AddRange(type.GetFields(BindingFlags.Instance
                                               | BindingFlags.NonPublic).Where(f => f.IsPrivate).Where(f =>
                    f.GetCustomAttribute<SerializeInterfaceField>() != null));
            }
            
            fields.Reverse();
            foreach (var fieldInfo in fields) yield return fieldInfo;
        }
    }
}