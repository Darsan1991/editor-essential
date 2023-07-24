using System.Linq;
using DGames.Essentials.Attributes;
using DGames.Essentials.EditorHelpers;
using UnityEditor;
using UnityEngine;

namespace DGames.Essentials.Editor
{
    [CustomPropertyDrawer(typeof(NoFoldAttribute))]
    public class NoFoldDrawer : PropertyDrawer
    {
        public NoFoldAttribute Attribute => (NoFoldAttribute)attribute;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property, label) - EditorGUIUtility.singleLineHeight +
                   (Attribute.HasColor ? EditorGUIUtility.standardVerticalSpacing * 5f : 0);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // base.OnGUI(position, property, label);
            var noFoldAttribute = (NoFoldAttribute)attribute;
            var exceptList = noFoldAttribute.Excepts.ToList();
            property.isExpanded = true;
            position.height = GetPropertyHeight(property, label);

            //
            // bool IsArrayElement(SerializedProperty property,out string arrayPath)
            // {
            //     var list = property.propertyPath.Split('.').ToList();
            //     var isArray = list.Count >= 2 && list[^2] == "Array";
            //     arrayPath = isArray ? string.Join('.', list.Take(list.Count - 2)) : null;
            //     return isArray;
            // }
            //
            // Debug.Log($"IS Parent Array: {IsArrayElement(property,out _)} : {property.propertyPath}");
            //
            // if (IsArrayElement(property,out var path))
            // {
            //     return;
            // }

            position = DrawBackgroundColorBoxIfNeeded(position,property);
            // property = property.Copy();
            var parentDepth = property.depth;
            if (property.NextVisible(true) && parentDepth < property.depth)
            {
                do
                {
                    if (exceptList.Contains(property.name))
                        continue;
                    position = DrawProperty(position, property);
                } while (property.NextVisible(false) && parentDepth < property.depth);
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

        private Rect DrawBackgroundColorBoxIfNeeded(Rect position,SerializedProperty property)
        {
            if (!Attribute.HasColor) return position;
            position.height -= EditorGUIUtility.standardVerticalSpacing * 2;
            EditorGUI.DrawRect(position, Attribute.Color);
            var haveFold = property.HaveFold();
            var foldWidth =  (EditorGUIUtility.standardVerticalSpacing * 7);
            position.position += Vector2.up * (EditorGUIUtility.standardVerticalSpacing * 2);
            position.position += Vector2.right * (EditorGUIUtility.standardVerticalSpacing * 2 + (haveFold ? foldWidth:0));
            position.width -= EditorGUIUtility.standardVerticalSpacing * 4 + (haveFold ? foldWidth:0);

            return position;
        }
    }
}