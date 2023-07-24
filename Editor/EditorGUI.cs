using System;
using UnityEngine;

namespace DGames.Presets.Editor
{
    public static class EditorGUI
    {
        public static object BuildInFieldEditorGUI(Rect position, object value)
        {
            if (value is Color color)
            {
                return UnityEditor.EditorGUI.ColorField(position, color);
            }

            if (value is string str)
            {
                return UnityEditor.EditorGUI.TextField(position, str);
            }

            if (value is float f)
            {
                return UnityEditor.EditorGUI.FloatField(position, f);
            }

            if (value is int i)
            {
                return UnityEditor.EditorGUI.IntField(position, i);
            }

            if (value is bool b)
                return UnityEditor.EditorGUI.Toggle(position, b);

            if (value is AnimationClip animationClip)
                return UnityEditor.EditorGUI.ObjectField(position, animationClip, typeof(AnimationClip),false);

            if (value is AudioClip audioClip)
                return UnityEditor.EditorGUI.ObjectField(position, audioClip, typeof(AudioClip),false);
            
            if (value is Sprite sprite)
                return UnityEditor.EditorGUI.ObjectField(position, sprite, typeof(Sprite),false);

            throw new NotImplementedException();
        }
        
        
        //
        //     public static void BuildInFieldEditorGUI(Rect position, SerializedProperty property)
        //     {
        //         switch (property.propertyType)
        //         {
        //             case SerializedPropertyType.Generic:
        //
        //                 break;
        //             case SerializedPropertyType.Integer:
        //                 property.intValue = EditorGUI.IntField(position, property.intValue);
        //                 break;
        //             case SerializedPropertyType.Boolean:
        //                 property.boolValue = EditorGUI.Toggle(position, property.boolValue);
        //                 break;
        //             case SerializedPropertyType.Float:
        //                 property.floatValue = EditorGUI.FloatField(position, property.floatValue);
        //                 break;
        //             case SerializedPropertyType.String:
        //                 property.stringValue = EditorGUI.TextField(position, property.stringValue);
        //                 break;
        //             case SerializedPropertyType.Color:
        //                 property.colorValue = EditorGUI.ColorField(position, property.colorValue);
        //                 break;
        //             case SerializedPropertyType.ObjectReference:
        //                 property.objectReferenceValue = EditorGUI.ObjectField(position,
        //                     property.objectReferenceValue, property.GetValueType(), allowSceneObjects: false);
        //                 break;
        //             case SerializedPropertyType.LayerMask:
        //                 
        //                 break;
        //             case SerializedPropertyType.Enum:
        //                 break;
        //             case SerializedPropertyType.Vector2:
        //                 break;
        //             case SerializedPropertyType.Vector3:
        //                 break;
        //             case SerializedPropertyType.Vector4:
        //                 break;
        //             case SerializedPropertyType.Rect:
        //                 break;
        //             case SerializedPropertyType.ArraySize:
        //                 break;
        //             case SerializedPropertyType.Character:
        //                 break;
        //             case SerializedPropertyType.AnimationCurve:
        //                 property.animationCurveValue = EditorGUI.CurveField(position, property.animationCurveValue);
        //                 break;
        //             case SerializedPropertyType.Bounds:
        //                 break;
        //             case SerializedPropertyType.Gradient:
        //                 break;
        //             case SerializedPropertyType.Quaternion:
        //                 break;
        //             case SerializedPropertyType.ExposedReference:
        //                 break;
        //             case SerializedPropertyType.FixedBufferSize:
        //                 break;
        //             case SerializedPropertyType.Vector2Int:
        //                 break;
        //             case SerializedPropertyType.Vector3Int:
        //                 break;
        //             case SerializedPropertyType.RectInt:
        //                 break;
        //             case SerializedPropertyType.BoundsInt:
        //                 break;
        //             case SerializedPropertyType.ManagedReference:
        //                 break;
        //             case SerializedPropertyType.Hash128:
        //                 break;
        //             default:
        //                 throw new ArgumentOutOfRangeException();
        //         }
        //
        //     }
        //
        //
        // public static bool CanDrawBuildInFieldEditorGUI(Type type)
        //     {
        //         if (type == typeof(Color)) return true;
        //         if (type == typeof(string)) return true;
        //         if (type == typeof(float)) return true;
        //         if (type == typeof(bool)) return true;
        //         return type == typeof(int);
        //     }
        // }
    }
}