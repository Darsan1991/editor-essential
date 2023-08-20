using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace DGames.Essentials.Attributes
{
    public class TestAttribute1 : PropertyAttribute
    {
        
    }
    
    public class TestAttribute2 : PropertyAttribute
    {
        
    }
    
    
    public class TestAttribute3 : PropertyAttribute
    {
        
    }

    [CustomPropertyDrawer(typeof(TestAttribute1))]
    public class TestAttribute1Editor : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property, label);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            position.height = 1*EditorGUI.GetPropertyHeight(property, label);
            // EditorGUI.DrawRect(position,Color.red);
            EditorGUI.PropertyField(position, property, label, true);
        }
    }
    
    [CustomPropertyDrawer(typeof(TestAttribute2))]
    public class TestAttribute2Editor : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property, label)*2 ;
        }
        
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            position.height = EditorGUI.GetPropertyHeight(property, label);
            // EditorGUI.DrawRect(position,Color.red);
            EditorGUI.PropertyField(position, property, label, true);
            position.y += position.height;
            EditorGUI.PropertyField(position, property, label, true);
        }
    }
    
    [CustomPropertyDrawer(typeof(TestAttribute3))]
    public class TestAttribute3Editor : PropertyDrawer
    {
        private bool _expanded;
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property, label) *2 + (_expanded?3*EditorGUIUtility.singleLineHeight : EditorGUIUtility.singleLineHeight);
        }
        
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            
            position.height = EditorGUI.GetPropertyHeight(property, label);
            EditorGUI.DrawRect(position,Color.red);
            EditorGUI.PropertyField(position, property, label, true);
            position.y += position.height;
            EditorGUI.PropertyField(position, property, label, true);

            position.y += position.height;

            position.height = (_expanded ? 3 : 1) * EditorGUIUtility.singleLineHeight;
            EditorGUI.DrawRect(position,Color.blue);

            _expanded = EditorGUI.Foldout(position, _expanded, "Foldout");
            

        }
    }
}