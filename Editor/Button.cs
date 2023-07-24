using System;
using System.Collections.Generic;
using System.Reflection;
using DGames.Essentials.Attributes;
using UnityEditor;
using UnityEngine;

namespace DGames.Essentials.Editor
{
    public  class Button
    {
        private readonly string _displayName;

        private readonly MethodInfo _method;


        protected Button(MethodInfo method, ButtonAttribute buttonAttribute)
        {
            _displayName = string.IsNullOrEmpty(buttonAttribute.Name)
                ? ObjectNames.NicifyVariableName(method.Name)
                : buttonAttribute.Name;

            _method = method;
        }

        public void Draw(IEnumerable<object> targets)
        {
            if (!GUILayout.Button(_displayName)) return;
            foreach (var target in targets)
            {
                _method?.Invoke(target, null);
            }
        }

        public static Button Create(MethodInfo method, ButtonAttribute buttonAttribute)
        {
            var parameters = method.GetParameters();
            if (parameters.Length == 0)
            {
                return new Button(method, buttonAttribute);
            }

            throw new Exception();
        }

     
    }
}