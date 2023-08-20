using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace DGames.Essentials.Attributes
{
    public sealed class HideFieldAttribute : PropertyAttribute
    {
     

        // ReSharper disable once TooManyDependencies
        public HideFieldAttribute()
        {
         
        }
        

    }
    
    #if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(HideFieldAttribute))]
    public class HideFieldPropertyDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return 0;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            
        }
    }
    #endif
}