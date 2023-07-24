using System;
using DGames.Essentials.Attributes;
using UnityEditor;
using UnityEngine;
using MessageType = UnityEditor.MessageType;

namespace DGames.Essentials.Editor
{
    [CustomPropertyDrawer(typeof(HelpBoxAttribute))]
    public class HelpBoxPropertyDrawer : PropertyDrawer
    {
        public HelpBoxAttribute Attribute => (HelpBoxAttribute)attribute;


        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property, label) +
                   (Attribute.ConditionSatisfy(property) ? GetBoxHeight() : 0);
        }

        private float GetBoxHeight()
        {
            var helpBoxAttribute = Attribute;
            // ReSharper disable once StringLiteralTypo
            var helpBoxStyle = (GUI.skin != null) ? GUI.skin.GetStyle("helpbox") : null;
            return helpBoxStyle == null
                ? 30
                : Mathf.Max(20f,
                    helpBoxStyle.CalcHeight(new GUIContent(helpBoxAttribute.Message),
                        EditorGUIUtility.currentViewWidth) + 4);
        }


        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (Attribute.ConditionSatisfy(property))
            {
                position.height = GetBoxHeight();
                EditorGUI.HelpBox(position, Attribute.Message, (MessageType)(Attribute.Type));
                position.height = GetPropertyHeight(property, label) - position.height;
                position.position += Vector2.up * GetBoxHeight();
            }

            EditorGUI.PropertyField(position, property, label!= GUIContent.none ? new GUIContent(property.displayName) : GUIContent.none, true);
        }
    }

    [CustomPropertyDrawer(typeof(RequiredAttribute))]
    public class RequiredPropertyDrawer : PropertyDrawer
    {
        public RequiredAttribute Attribute => (RequiredAttribute)attribute;


        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property, label) +
                   (!IsPropertySupported(property) || IsMissing(property) ? 25 : 0);
        }


        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (!IsPropertySupported(property))
            {
                DrawUnsupported(position, property);
                return;
            }

            if (IsMissing(property))
            {
                position.height = 20;
                EditorGUI.HelpBox(position, Attribute.Message, MessageType.Error);
                position.height = GetPropertyHeight(property, label) - position.height;
                position.position += Vector2.up * 25;
            }
            
            
            // Debug.Log($"Label:{label==GUIContent.none} Content:{label?.text}");

            EditorGUI.PropertyField(position, property,
                label != GUIContent.none
                    ? new GUIContent(property.displayName)
                    : GUIContent.none, true);
        }

        private static void DrawUnsupported(Rect position, SerializedProperty property)
        {
            position.height = 20;
            EditorGUI.HelpBox(position, "Required Only Support for String,Objects", MessageType.Warning);
            position.position += Vector2.up * 25;
            EditorGUI.PropertyField(position, property);
        }

        private bool IsPropertySupported(SerializedProperty property)
        {
            return property.propertyType is SerializedPropertyType.String or SerializedPropertyType.ObjectReference;
        }

        private bool IsMissing(SerializedProperty property)
        {
            return property.propertyType switch
            {
                SerializedPropertyType.String => string.IsNullOrEmpty(property.stringValue),
                SerializedPropertyType.ObjectReference => property.objectReferenceValue == null,
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }
}