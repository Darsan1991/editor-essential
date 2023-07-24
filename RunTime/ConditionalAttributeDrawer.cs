using System;
using DGames.Essentials.Attributes;
using UnityEditor;
using UnityEngine;

namespace DGames.Essentials.Editor
{
    [CustomPropertyDrawer(typeof(BaseConditionAttribute), true)]
    public class ConditionalAttributeDrawer : PropertyDrawer
    {
        private ConditionAttribute Attribute => _attribute ??= attribute as ConditionAttribute;

        private ConditionAttribute _attribute;

        private ConditionGUIDrawer _drawer;

        public ConditionGUIDrawer Drawer => _drawer ??= GetDrawer(Attribute);


        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return Drawer.GetPropertyHeight(property, label);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            Drawer.OnGUI(position, property, label);
        }
        

        private ConditionGUIDrawer GetDrawer(BaseConditionAttribute att)
        {
            switch (att.Type)
            {
                case ConditionType.Show:
                    return new ShowConditionGUIDrawer(att);
                case ConditionType.Enable:
                    return new EnableConditionGUIDrawer(att);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    public abstract class ConditionGUIDrawer
    {
        public BaseConditionAttribute Attribute { get; }

        protected ConditionGUIDrawer(BaseConditionAttribute attribute)
        {
            Attribute = attribute;
        }

        public abstract float GetPropertyHeight(SerializedProperty property, GUIContent label);

        public abstract void OnGUI(Rect position, SerializedProperty property, GUIContent label);
    }

    public class EnableConditionGUIDrawer : ConditionGUIDrawer
    {
        public EnableConditionGUIDrawer(BaseConditionAttribute attribute) : base(attribute)
        {
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property,label);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginDisabledGroup(!Attribute.IsConditionSatisfy(property));
            EditorGUI.PropertyField(position, property, label!= GUIContent.none ? new GUIContent(property.displayName) : GUIContent.none, true);
            EditorGUI.EndDisabledGroup();
        }
    }

    public class ShowConditionGUIDrawer : ConditionGUIDrawer
    {

        public ShowConditionGUIDrawer(BaseConditionAttribute attribute) : base(attribute)
        {
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return Attribute.IsConditionSatisfy(property) ? EditorGUI.GetPropertyHeight(property) : 0;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (Attribute.IsConditionSatisfy(property))
                EditorGUI.PropertyField(position, property, label!= GUIContent.none ? new GUIContent(property.displayName) : GUIContent.none, true);
        }
    }
}