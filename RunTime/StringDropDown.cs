using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#if UNITY_EDITOR
using System.Reflection;
using UnityEditor;
#endif

namespace DGames.Essentials.Attributes
{
    public sealed class StringDropDown : PropertyAttribute
    {
        private readonly Type[] _types;

        public IEnumerable<Type> Types => _types;

        public StringDropDown(params Type[] types)
        {
            _types = types;
        }
    }
    
    
    #if UNITY_EDITOR
[CustomPropertyDrawer(typeof(StringDropDown))]
    public class StringDropDownPropertyDrawer : PropertyDrawer
    {
        public StringDropDown Attribute => (StringDropDown)attribute;
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return base.GetPropertyHeight(property, label);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label,property);
            position.width -= 20;
            EditorGUI.PropertyField(position, property,
                label != GUIContent.none ? new GUIContent(property.displayName) : GUIContent.none);

            position.x += position.width;
            position.width = 20;
            
            if (EditorGUI.DropdownButton(position, EditorGUIUtility.IconContent("SettingsIcon"), FocusType.Passive))
            {
                ShowPopup(property);
            }
            
            EditorGUI.EndProperty();
        }
        
        
        private void ShowPopup(SerializedProperty property)
        {
            var menu = new GenericMenu();
            foreach (var type in Attribute.Types)
            {
                foreach (var field in type
                             .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
                             .Where(info => info.IsPublic))
                {
                    menu.AddItem(new GUIContent($"{type.Name}/{field.Name}"),false, () =>
                    {
                        property.stringValue = (string)field.GetValue(null);
                        property.serializedObject.ApplyModifiedProperties();
                    });
                }
            }


            menu.ShowAsContext();
        }
    }
    
    #endif
    
}