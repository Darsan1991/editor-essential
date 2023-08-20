using System.Collections.Generic;
using System.Linq;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace DGames.Essentials.Attributes
{
    public class ToggleGroupAttribute : PropertyAttribute
    {
        public string Path { get; }
        public string Name { get; }
        public bool DrawFull { get; }

        public ToggleGroupAttribute(string path,string name=null,bool drawFull = false)
        {
            Path = path;
            Name = name;
            DrawFull = drawFull;
        }
    }
    
#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(ToggleGroupAttribute))]
    public class ToggleGroupDrawer : PropertyDrawer
    {
        public ToggleGroupAttribute Attribute => (ToggleGroupAttribute)attribute;
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var toggleProperty = property.FindPropertyRelative(Attribute.Path);
            property.isExpanded = true;
            return toggleProperty.boolValue ?  EditorGUI.GetPropertyHeight(property,label) - (Attribute.DrawFull ? -EditorGUIUtility.singleLineHeight : EditorGUIUtility.singleLineHeight): EditorGUIUtility.singleLineHeight;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var toggleProperty = property.FindPropertyRelative(Attribute.Path);
            position.height = EditorGUIUtility.singleLineHeight;
            EditorGUI.PropertyField(position, toggleProperty,
                new GUIContent(string.IsNullOrEmpty(Attribute.Name) ? property.displayName : Attribute.Name));

            position.y += EditorGUIUtility.singleLineHeight;
            if (toggleProperty.boolValue)
            {
                EditorGUI.indentLevel++;
                if (Attribute.DrawFull)
                {
                    EditorGUI.PropertyField(position, property,label != null && label!=GUIContent.none && !string.IsNullOrEmpty(label.text) ? label : GUIContent.none, true);
                }
                else
                {
                    var parentDepth = property.depth;
                    if (property.NextVisible(true) && parentDepth < property.depth)
                    {
                        do
                        {
                            if (Attribute.Path == property.name)
                                continue;
                            position = DrawProperty(position, property);
                        } while (property.NextVisible(false) && parentDepth < property.depth);
                    }
                }

                EditorGUI.indentLevel--;
            }


        }
        
        private static Rect DrawProperty(Rect position, SerializedProperty property)
        {
            EditorGUI.PropertyField(position, property, true);
            position.position += Vector2.up *
                                 (EditorGUI.GetPropertyHeight(property) +
                                  EditorGUIUtility.standardVerticalSpacing);

            return position;
        }
        
        public static void DrawChildrenDefault( SerializedProperty property,
            params string[] exceptChildren)
        {
            var exceptList = exceptChildren?.ToList() ?? new List<string>();

            property = property.Copy();
            var parentDepth = property.depth;
            if (property.NextVisible(true) && parentDepth < property.depth)
            {
                do
                {
                    if (exceptList.Contains(property.name))
                        continue;

                    EditorGUILayout.PropertyField(property, true);
                } while (property.NextVisible(false) && parentDepth < property.depth);
            }
        }
    }
    
#endif
}