using System.Linq;
using DGames.Essentials.Attributes;
using DGames.Essentials.EditorHelpers;
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

    [CustomPropertyDrawer(typeof(SectionFoldAttribute))]
    public class SectionFoldPropertyDrawer : PropertyDrawer
    {
        private GUIStyle _titleStyle;
        public SectionFoldAttribute Attribute => (SectionFoldAttribute)attribute;


        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight * (Attribute.Above + Attribute.Below + 1) + (property.isExpanded
                ? 4 * EditorGUIUtility.standardVerticalSpacing + EditorGUI.GetPropertyHeight(property, GUIContent.none)
                : 0);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            CacheIfNeeded();

            position.position += Vector2.up * (EditorGUIUtility.singleLineHeight * Attribute.Above);
            position.height = EditorGUIUtility.singleLineHeight;
            var startWidth = position.width;
            position.width = 10;
            property.isExpanded = EditorGUI.Foldout(position, property.isExpanded, "");
            position.x += position.width;
            position.width = startWidth - position.width;
            EditorGUI.LabelField(position,
                GetTitleWithDashes(
                    (string.IsNullOrEmpty(Attribute.Title) ? property.displayName : Attribute.Title).ToUpper(), 120),
                _titleStyle);
            position.position += Vector2.up * (EditorGUIUtility.singleLineHeight * (2 + Attribute.Below));

            position.height = EditorGUI.GetPropertyHeight(property, GUIContent.none) +
                              4 * EditorGUIUtility.standardVerticalSpacing - EditorGUIUtility.singleLineHeight;


            if (property.isExpanded)
            {
                position = DrawBackgroundColorBoxIfNeeded(position, property);

                var parentDepth = property.depth;
                if (property.NextVisible(true) && parentDepth < property.depth)
                {
                    do
                    {
                        position = DrawProperty(position, property);
                    } while (property.NextVisible(false) && parentDepth < property.depth);
                }
            }
        }

        private static Rect DrawProperty(Rect position, SerializedProperty property)
        {
            EditorGUI.PropertyField(position, property, true);
            position.position += Vector2.up *
                                 (EditorGUI.GetPropertyHeight(property) +
                                  EditorGUIUtility.standardVerticalSpacing);

            return position;
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

        private Rect DrawBackgroundColorBoxIfNeeded(Rect position, SerializedProperty property)
        {
            position.height -= EditorGUIUtility.standardVerticalSpacing * 2;
            EditorGUI.DrawRect(position, new Color(42f / 255, 42f / 255, 42f / 255, 0.5f));

            var haveFold = property.HaveFold();
            var foldWidth =  (EditorGUIUtility.standardVerticalSpacing * 7);
            position.position += Vector2.up * (EditorGUIUtility.standardVerticalSpacing * 2);
            position.position += Vector2.right * (EditorGUIUtility.standardVerticalSpacing * 2 + (haveFold ? foldWidth:0));
            position.width -= EditorGUIUtility.standardVerticalSpacing * 4 + (haveFold ? foldWidth:0);
            return position;
        }
    }
}