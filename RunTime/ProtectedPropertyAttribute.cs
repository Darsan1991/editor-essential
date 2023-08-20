using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace DGames.Essentials.Attributes
{
    public class ProtectedPropertyAttribute : PropertyAttribute
    {
        public bool Show { get; }
        public string EditPath { get; }


        public ProtectedPropertyAttribute(bool show = false, string editPath = null)
        {
            Show = show;
            EditPath = editPath;
        }
    }

#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(ProtectedPropertyAttribute))]
    public class ProtectedPropertyDrawer : PropertyDrawer
    {
        public ProtectedPropertyAttribute Attribute => (ProtectedPropertyAttribute)attribute;

        private bool _editing;


        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return ((Attribute.Show || IsEditing(property)) ? EditorGUI.GetPropertyHeight(property, GUIContent.none) : 0) +
                   EditorGUIUtility.singleLineHeight;
        }


        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var startX = position.x;
            var startWidth = position.width;
            position.height = EditorGUIUtility.singleLineHeight;
            EditorGUI.LabelField(position, property.displayName);
            position.x = position.x + position.width - 60;
            position.width = 60;

            var editing = IsEditing(property);

            if (GUI.Button(position, editing ? "Done" : "Edit"))
            {
                SetEditing(property, !editing);
                editing = !editing;
            }

            position.width = startWidth;
            position.y += position.height;
            position.height = EditorGUI.GetPropertyHeight(property, GUIContent.none);
            position.x = startX;
            if (Attribute.Show || editing)
            {
                EditorGUI.BeginDisabledGroup(!editing);
                EditorGUI.PropertyField(position, property, GUIContent.none, true);
                EditorGUI.EndDisabledGroup();
            }
        }

        private bool IsEditing(SerializedProperty property) =>
            string.IsNullOrEmpty(Attribute.EditPath) || property.serializedObject.FindProperty(Attribute.EditPath)==null
                ? _editing
                : property.serializedObject.FindProperty(Attribute.EditPath).boolValue;

        private void SetEditing(SerializedProperty property, bool editing)
        {
            if (string.IsNullOrEmpty(Attribute.EditPath) || property.serializedObject.FindProperty(Attribute.EditPath)==null)
            {
                _editing = editing;
            }
            else
            {
                property.serializedObject.FindProperty(Attribute.EditPath).boolValue = editing;
            }
        }
    }
#endif
}