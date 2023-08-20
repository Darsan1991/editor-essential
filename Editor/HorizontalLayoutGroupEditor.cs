using System.Collections.Generic;
using System.Linq;
using DGames.Essentials.Attributes;
using UnityEditor;
using UnityEngine;

namespace DGames.Essentials.Editor
{
    [CustomLayoutGroupEditor(typeof(HorizontalLayout))]
    // ReSharper disable once UnusedType.Global
    public class HorizontalLayoutGroupEditor : LayoutGroupEditor
    {
        // ReSharper disable once TooManyDependencies
        public HorizontalLayoutGroupEditor(EditorParams editorParams, LayoutGroupParams layoutGroupParams) : base(editorParams, layoutGroupParams)
        {
        }

        public override void OnInspectorGUI()
        {
            var layoutElements = Elements.ToList();
            var rect = EditorGUILayout.GetControlRect(false, 0);
            var width = (rect.width - (layoutElements.Count - 1) * 6) / layoutElements.Count;


            EditorGUILayout.BeginHorizontal();
            DrawContent(layoutElements, width);
            EditorGUILayout.EndHorizontal();
        }

        private void DrawContent(List<LayoutElement> layoutElements, float width)
        {
            foreach (var layoutElement in layoutElements)
            {
                EditorGUILayout.BeginVertical(GUI.skin.box, GUILayout.ExpandWidth(true));
                foreach (var item in layoutElement.Items)
                {
                    DrawItem(item, width);
                }

                //
                // // DrawLayoutElementChildren(layoutElement);
                EditorGUILayout.EndVertical();
            }
        }

        private void DrawItem(object item, float width)
        {
            if (item is string field)
            {
                EditorGUILayout.PropertyField(SerializedObject.FindProperty(field), true,
                    GUILayout.MinWidth(width), GUILayout.ExpandWidth(true));
            }
            else if (item is LayoutGroupElement g)
            {
                groupVsEditors[g].OnInspectorGUI();
            }
        }
    }
}