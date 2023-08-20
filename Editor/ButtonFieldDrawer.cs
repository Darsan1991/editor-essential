using System;
using DGames.Essentials.Attributes;
using UnityEditor;
using UnityEngine;

namespace DGames.Essentials.Editor
{
    [CustomPropertyDrawer(typeof(ButtonField),true)]
    public class ButtonFieldDrawer : PropertyDrawer
    {

        public ButtonField Attribute => (ButtonField)attribute;
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property, label) + (Attribute.NeedShow(property)&&Attribute.ButtonDirection == ButtonField.Direction.Bottom ? EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing : 0);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            switch (Attribute.ButtonDirection)
            {
                case ButtonField.Direction.Right:
                    OnGUIRightDirection(position, property, label);
                    break;
                case ButtonField.Direction.Bottom:
                    OnGUIDownDirection(position,property,label);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

        }

        private void OnGUIRightDirection(Rect position, SerializedProperty property, GUIContent label)
        {
            var buttonWidth = Attribute.Width > 0 ? Attribute.Width : 60;

            if (Attribute.NeedShow(property))
                position.width -= buttonWidth + 5;


            EditorGUI.PropertyField(position, property, label != GUIContent.none
                ? new GUIContent(property.displayName)
                : GUIContent.none, true);

            if (Attribute.NeedShow(property))
            {
                position.x += position.width + 5;
                position.height = EditorGUIUtility.singleLineHeight;
                position.width = buttonWidth;

                EditorGUI.BeginDisabledGroup(Attribute.NeedDisable(property));

                if (GUI.Button(position,
                        new GUIContent(Attribute.Name,
                            string.IsNullOrEmpty(Attribute.Icon) ? null : EditorGUIUtility.IconContent(Attribute.Icon).image)))
                {
                    Attribute.OnClick(property);
                }

                EditorGUI.EndDisabledGroup();
            }
        }
        
        private void OnGUIDownDirection(Rect position, SerializedProperty property, GUIContent label)
        {

            position.height = EditorGUI.GetPropertyHeight(property, label);
            
            EditorGUI.PropertyField(position, property, label != GUIContent.none
                ? new GUIContent(property.displayName)
                : GUIContent.none, true);

            position.y += position.height + EditorGUIUtility.standardVerticalSpacing;

            if (Attribute.NeedShow(property))
            {
                position.height = EditorGUIUtility.singleLineHeight;

                EditorGUI.BeginDisabledGroup(Attribute.NeedDisable(property));

                if (GUI.Button(position,
                        new GUIContent(Attribute.Name,
                            string.IsNullOrEmpty(Attribute.Icon) ? null : EditorGUIUtility.IconContent(Attribute.Icon).image)))
                {
                    Attribute.OnClick(property);
                }

                EditorGUI.EndDisabledGroup();
            }
        }
    }
}