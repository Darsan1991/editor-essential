using DGames.Essentials.Attributes;
using DGames.Essentials.EditorHelpers;
using UnityEditor;
using UnityEngine;

namespace DGames.Essentials.Editor
{
    [CustomPropertyDrawer(typeof(Box))]
    public class BoxPropertyDrawer : PropertyDrawer
    {
        public Box Attribute => (Box)attribute;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property, label)  +
                   (Attribute.HasColor ? EditorGUIUtility.standardVerticalSpacing * 5f:0) ;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            position.height = GetPropertyHeight(property, label);
            position = DrawBackgroundColorBoxIfNeeded(position,property);
            // Debug.Log($"{property.name}: {property.propertyType}");
            EditorGUI.PropertyField(position, property, label!= GUIContent.none ? new GUIContent(property.displayName) : GUIContent.none, true);
        }


        private Rect DrawBackgroundColorBoxIfNeeded(Rect position,SerializedProperty property)
        {
            if (!Attribute.HasColor) return position;
            position.height -= EditorGUIUtility.standardVerticalSpacing * 2;
            EditorGUI.DrawRect(position, Attribute.Color);
            var haveFold = property.HaveFold() && !Attribute.ForceRemoveSpaceForFold;
            var foldWidth =  (EditorGUIUtility.standardVerticalSpacing * 7);
            position.position += Vector2.up * (EditorGUIUtility.standardVerticalSpacing * 2);
            position.position += Vector2.right * (EditorGUIUtility.standardVerticalSpacing * 2 + (haveFold ? foldWidth:0));
            position.width -= EditorGUIUtility.standardVerticalSpacing * 4 + (haveFold ? foldWidth:0);

            return position;
        }
    }
}