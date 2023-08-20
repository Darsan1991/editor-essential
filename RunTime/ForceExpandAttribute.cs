using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace DGames.Essentials.Attributes
{
    public sealed class ForceExpandAttribute : PropertyAttribute
    {
        public ForceExpandAttribute()
        {
            
        }
    }
    
    #if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(ForceExpandAttribute))]
    public class ForceExpandDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property,label);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            property.isExpanded = true;
            EditorGUI.PropertyField(position, property, label,true);
        }
    }
    #endif
}