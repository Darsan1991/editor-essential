using System.Linq;
using DGames.Essentials.Attributes;
using UnityEditor;
using UnityEngine;

namespace DGames.Essentials.Editor
{
    [CustomPropertyDrawer(typeof(SectionTitleAttribute))]
    public class SectionTitleDecoratorDrawer : DecoratorDrawer
    {
        private GUIStyle _titleStyle;
        public SectionTitleAttribute Attribute => (SectionTitleAttribute)attribute;


        public override float GetHeight()
        {
            return EditorGUIUtility.singleLineHeight * (Attribute.Above + Attribute.Below + 1);
        }

        public override void OnGUI(Rect position)
        {
            CacheIfNeeded();

            position.position += Vector2.up * EditorGUIUtility.singleLineHeight * Attribute.Above;
            position.height = EditorGUIUtility.singleLineHeight;
            EditorGUI.LabelField(position, GetTitleWithDashes(Attribute.Title), _titleStyle);
        }

        private void CacheIfNeeded()
        {
            if (_titleStyle == null)
            {
                var style = GUI.skin.GetStyle("Label");
                _titleStyle = new GUIStyle(style) { alignment = TextAnchor.MiddleCenter };
            }
        }

        public static string GetTitleWithDashes(string title, int countPerSide = 70)
        {
            var dashes = string.Join("", Enumerable.Repeat("-", countPerSide));

            return $"{dashes}{title}{dashes}";
        }
    }
}