using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public abstract partial class SimpleFoldAttribute : PropertyAttribute
{
    protected SimpleFoldAttribute()
    {
    }
}

#if UNITY_EDITOR
public abstract partial class SimpleFoldAttribute
{
    public abstract float GetContentHeight(SerializedProperty property, GUIContent label);

    public abstract void OnGUI(Rect position, SerializedProperty property, GUIContent label);

    public virtual bool IsShow(SerializedProperty property)
    {
        return property.propertyType != SerializedPropertyType.ObjectReference || property.objectReferenceValue != null;
    }
}
#endif

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(SimpleFoldAttribute),true)]
public class SimpleFoldDrawer : PropertyDrawer
{
    public SimpleFoldAttribute Attribute => (SimpleFoldAttribute)attribute;

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUI.GetPropertyHeight(property, label) + (Attribute.IsShow(property) && IsExpanded(property)
            ? Attribute.GetContentHeight(property, label)+EditorGUIUtility.standardVerticalSpacing
            : 0);
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        position.height = EditorGUIUtility.singleLineHeight;

        if (!Attribute.IsShow(property))
        {
            EditorGUI.PropertyField(position, property, label, true);
            return;
        }

        SetExpanded(property, EditorGUI.Foldout(position, IsExpanded(property), GUIContent.none));
        position.x += 15;
        position.width -= 15;
        EditorGUI.PropertyField(position, property, label);


        if (IsExpanded(property))
        {
            
            position.y += position.height + EditorGUIUtility.standardVerticalSpacing;
            position.height = Attribute.GetContentHeight(property, label);
            Attribute.OnGUI(position, property, label);
        }
    }

    private bool IsExpanded(SerializedProperty property) => property.isExpanded;
    private void SetExpanded(SerializedProperty property, bool expanded) => property.isExpanded = expanded;
}

#endif