using DGames.Essentials.Attributes;
using UnityEditor;
using UnityEngine;
using MessageType = UnityEditor.MessageType;

namespace DGames.Essentials.Editor
{
    [CustomPropertyDrawer(typeof(MinMaxAttribute))]
    public class MinMaxPropertyDrawer : PropertyDrawer
    {
        private GUIStyle _labelStyle;
        public MinMaxAttribute Attribute => (MinMaxAttribute)attribute;


        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return property.propertyType != SerializedPropertyType.Vector2
                ? 40 + EditorGUI.GetPropertyHeight(property, label)
                : EditorGUI.GetPropertyHeight(property, label);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.propertyType != SerializedPropertyType.Vector2)
            {
                DrawUnsupported(position, property);
                return;
            }

            position.height = EditorGUIUtility.singleLineHeight;

            DrawLabelIfCan(position, property, label);

            float x = property.vector2Value.x, y = property.vector2Value.y;

            EditorGUI.BeginChangeCheck();

            position = DrawFieldsIfCan(position, property, ref x, ref y);

            EditorGUI.MinMaxSlider(position, ref x, ref y, Attribute.Min, Attribute.MAX);

            if (EditorGUI.EndChangeCheck())
            {
                property.vector2Value = new Vector2(x, y);
            }
        }

        private static void DrawUnsupported(Rect position, SerializedProperty property)
        {
            position.height = 35;
            EditorGUI.HelpBox(position, "MinMax Only Support for Vector2", MessageType.Warning);
            position.position += Vector2.up * 40;
            EditorGUI.PropertyField(position, property);
        }

        private static void DrawLabelIfCan(Rect position, SerializedProperty property, GUIContent label)
        {
            if (label!= GUIContent.none)
            {
                EditorGUI.LabelField(position, property.displayName);
                position.position += EditorGUIUtility.labelWidth * Vector2.right;
                position.width -= EditorGUIUtility.labelWidth;
            }
        }

        // ReSharper disable once TooManyArguments
        private Rect DrawFieldsIfCan(Rect position, SerializedProperty property, ref float x, ref float y)
        {
            if (!Attribute.ShowFields) return position;

            EditorGUI.BeginDisabledGroup(Attribute.DisableField);
            var startWidth = position.width;
            var startX = position.x;
            const int textWidth = 40;
            const int space = 3;

            position.width = textWidth;

            x = Mathf.Clamp(EditorGUI.FloatField(position, property.vector2Value.x), Attribute.Min, y);
            position.x = startX + (startWidth - textWidth);
            y = Mathf.Clamp(EditorGUI.FloatField(position, property.vector2Value.y), x, Attribute.MAX);

            position = new Rect(startX + textWidth + space, position.y, startWidth - 2 * textWidth - 2 * space,
                position.height);

            EditorGUI.EndDisabledGroup();

            return position;
        }
    }
}