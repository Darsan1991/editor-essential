using System.Linq;
using DGames.Essentials.Attributes;
using UnityEditor;
using UnityEngine;

namespace DGames.Essentials.Editor
{
    [CustomLayoutGroupEditor(typeof(BoxGroupLayout))]
    // ReSharper disable once UnusedType.Global
    public class BoxLayoutGroupEditor : LayoutGroupEditor
    {
        private bool _expand;
        private GUIStyle _titleStyle;
        private bool _allowFold = true;
        private int _style;
        private bool _drawBox = true;
        private string _title;
        private bool _defaultExpand = true;

        public bool HaveTitle => !string.IsNullOrEmpty(_title);

        // ReSharper disable once TooManyDependencies
        public BoxLayoutGroupEditor(EditorParams editorParams, LayoutGroupParams layoutGroupParams) : base(editorParams, layoutGroupParams)
        {
            _expand = !HaveTitle || _defaultExpand || !_allowFold;
        }

        private void CacheIfNeeded()
        {
            if (_titleStyle == null)
            {
                var style = EditorStyles.label;
                _titleStyle = new GUIStyle(style) { alignment = TextAnchor.MiddleCenter };
            }
        }

        protected override void ProcessAttribute(LayoutAttribute attribute)
        {
            base.ProcessAttribute(attribute);
            var box = (BoxGroupLayout)attribute;
            _allowFold = box.AllowFold && _allowFold;
            _style = box.Style != box.DefaultStyle ? box.Style : _style;
            _drawBox = box.DrawBox && _drawBox;
            _defaultExpand = box.DefaultExpand && _defaultExpand;
            _title = !string.IsNullOrEmpty(box.DisplayTitle) ? box.DisplayTitle : _title;

        }


        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            CacheIfNeeded();

            EditorGUILayout.BeginVertical(_drawBox && HaveTitle ? GUI.skin.box : GUIStyle.none);

            if (HaveTitle)
                DrawTitleSection();

            if (_expand)
            {
                EditorGUILayout.BeginVertical(GUI.skin.box);
                EditorGUI.indentLevel++;
                foreach (var layoutElement in Elements)
                {
                    DrawLayoutElement(layoutElement);
                }

                EditorGUI.indentLevel--;
                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawTitleSection()
        {
            if (_style == 0)
                DrawSectionTitleStyle();
            else
                DrawDefaultFoldTitle();
        }

        private void DrawDefaultFoldTitle()
        {
            if (_allowFold)
            {
                EditorGUI.indentLevel++;

                _expand = EditorGUILayout.Foldout(_expand, _title, EditorStyles.foldoutHeader);
                EditorGUI.indentLevel--;
            }
            else
            {
                EditorGUILayout.LabelField(_title, EditorStyles.boldLabel);
            }
        }

        private void DrawSectionTitleStyle()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUI.indentLevel++;
            if (_allowFold)
            {
                _expand = EditorGUI.Foldout(EditorGUILayout.GetControlRect(GUILayout.MaxWidth(25)), _expand, "");
            }

            EditorGUI.indentLevel--;
            EditorGUILayout.LabelField(GetTitleWithDashes(_title.ToUpper()), _titleStyle);
            EditorGUILayout.EndHorizontal();
        }


        public static string GetTitleWithDashes(string title, int countPerSide = 70)
        {
            var dashes = string.Join("", Enumerable.Repeat("-", countPerSide));

            return $"{dashes}{title}{dashes}";
        }

        private void DrawLayoutElement(LayoutElement layoutElement)
        {
            foreach (var item in layoutElement.Items)
            {
                if (item is string field)
                {
                    EditorGUILayout.PropertyField(SerializedObject.FindProperty(field), true);
                }
                else if (item is LayoutGroupElement g)
                {
                    groupVsEditors[g].OnInspectorGUI();
                }
            }
        }
    }
}