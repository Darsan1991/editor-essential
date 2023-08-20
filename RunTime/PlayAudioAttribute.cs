using DGames.Essentials.EditorHelpers;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif


namespace DGames.Essentials.Attributes
{
    public class PlayAudioAttribute : ButtonField
    {
        public PlayAudioAttribute() : base("", 25, "d_PlayButton On")
        {
            
        }

#if UNITY_EDITOR


        public override void OnClick(SerializedProperty serializedProperty)
        {
            if (serializedProperty.objectReferenceValue is AudioSource audioSource)
            {
                audioSource.Play();
            }
            else if (serializedProperty.objectReferenceValue is AudioClip clip)
            {
                AudioEditorUtils.PlayClip(clip);
            }

        }


        public override bool NeedShow(SerializedProperty property)
        {
            // Debug.Log(property.type);
            return property.propertyType == SerializedPropertyType.ObjectReference && (property.type.Contains(nameof(AudioClip)) || property.type.Contains(nameof(AudioSource)));
        }

        public override bool NeedDisable(SerializedProperty property)
        {
            return property.propertyType == SerializedPropertyType.ObjectReference && property.objectReferenceValue == null;
        }

#endif
    }
    
    public abstract class ButtonField : PropertyAttribute
    {
        public string Name { get; }
        public string Icon { get; }
        public float Width { get; }

        public Direction ButtonDirection { get; }
        
        

        protected ButtonField(string name, float width = -1, string icon = "", Direction direction = Direction.Right)
        {
            Name = name;
            Icon = icon;
            Width = width;
            ButtonDirection = direction;
        }

#if UNITY_EDITOR
        public abstract void OnClick(SerializedProperty serializedProperty);


        public virtual bool NeedShow(SerializedProperty property)
        {
            return true;
        }

        public virtual bool NeedDisable(SerializedProperty property)
        {
            return false;
        }
#endif

        public enum Direction
        {
            Right,
            Bottom
        }
    }
}