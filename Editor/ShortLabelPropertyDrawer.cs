using DGames.Essentials.Attributes;
using UnityEditor;
using UnityEngine;

namespace DGames.Essentials.Editor
{
    [CustomPropertyDrawer(typeof(ShortLabelAttribute), true)]
    public class ShortLabelPropertyDrawer : PropertyDrawer
    {
        private GUIStyle _labelStyle;
        public ShortLabelAttribute Attribute => (ShortLabelAttribute)attribute;


        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property, label,property.isExpanded);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            CacheIfNeeded();
            if (Attribute.LettersCount > 0 && label != GUIContent.none && property.propertyType.IsBuildInSerializableField())
            {
                position.height = EditorGUIUtility.singleLineHeight;
                var content = new GUIContent(
                    property.displayName[..Mathf.Min(property.displayName.Length, Attribute.LettersCount)] + ":" +
                    (property.displayName.Length < Attribute.LettersCount ? " " : ""),
                    property.displayName + " - " + property.tooltip);
                var size = _labelStyle.CalcSize(content);
                var lastLabelWidth = EditorGUIUtility.labelWidth;
                EditorGUI.LabelField(position, content);
                position.position += Vector2.right * (size.x + 2);
                position.width -= size.x;
                EditorGUI.PropertyField(position, property, GUIContent.none,true);
                EditorGUIUtility.labelWidth = lastLabelWidth;
            }
            else
                EditorGUI.PropertyField(position, property, label,true);
        }

        private void CacheIfNeeded()
        {
            _labelStyle ??= new GUIStyle(EditorStyles.label);
        }
    }
}