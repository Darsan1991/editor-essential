
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public partial class SpriteFoldAttribute: SimpleFoldAttribute
{
    public float OffsetWidth { get; }
    public string PropertyPath { get; }
    public float MinIfPropertyPathWidth { get; }
    private readonly float _height;

    // ReSharper disable once TooManyDependencies
    public SpriteFoldAttribute(float height=-1,float offsetWidth = 20,string propertyPath=null,float minIfPropertyPathWidth=400f)
    {
        OffsetWidth = offsetWidth;
        PropertyPath = propertyPath;
        MinIfPropertyPathWidth = minIfPropertyPathWidth;
        _height = height <=0?100:height;
    }

    public override bool IsShow(SerializedProperty property)
    {
        property = string.IsNullOrEmpty(PropertyPath) ? property : property.FindPropertyRelative(PropertyPath);

        return base.IsShow(property) && property.type.Contains(nameof(Sprite)) && (string.IsNullOrEmpty(PropertyPath) || MinIfPropertyPathWidth<= EditorGUIUtility.currentViewWidth);
    }
}

#if UNITY_EDITOR
public partial class SpriteFoldAttribute 
{
    public override float GetContentHeight(SerializedProperty property, GUIContent label)
    {
        return _height;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        property = string.IsNullOrEmpty(PropertyPath) ? property : property.FindPropertyRelative(PropertyPath);
        
        var texture = ((Sprite)property.objectReferenceValue).texture;

        var aspect = (float)texture.height/texture.width;

        var height = Mathf.Min(_height, aspect * position.width);
        position.y += (position.height - height) / 2;
        position.height = height;
        var width = GetWidth(texture,height);
        position.x += (position.width - width) / 2;
        position.width = width;
        GUI.DrawTexture(position,texture);
    }

    private float GetWidth(Texture texture,float height)
    {
        return texture.width*height / texture.height;
    }
}
#endif