using System.Collections.Generic;
using System.Linq;
using DGames.Essentials.Attributes;
using UnityEditor;
using UnityEngine;

namespace DGames.Essentials.Editor
{
    [CustomLayoutGroupEditor(typeof(Tab))]
    // ReSharper disable once UnusedType.Global
    public class TabLayoutGroupEditor : LayoutGroupEditor
    {
        private readonly List<string> _tabs = new();
        private readonly Dictionary<string, Color> _tabVsColors = new();
        private int _selected;
        private string _defaultTab;
        private bool _allowAll;
        private bool _allowNone;


        public string SelectedTab => _tabs[_selected];

        // ReSharper disable once TooManyDependencies
        public TabLayoutGroupEditor(EditorParams editorParams,LayoutGroupParams groupParams) : base(editorParams,groupParams)
        {
            foreach (var layoutElement in Elements)
            {
                _tabs.Add(layoutElement.Name);
            }

            HandleAddAllAndNone();
            HandleDefaultTab();
        }

        private void HandleDefaultTab()
        {
            _defaultTab = string.IsNullOrEmpty(_defaultTab) ? _tabs.First() : _defaultTab;
            _selected = Mathf.Max(_tabs.IndexOf(_defaultTab),0);
        }

        private void HandleAddAllAndNone()
        {
            if (_allowAll)
            {
                _tabs.Add("All");
            }

            if (_allowNone)
                _tabs.Insert(0, "None");
        }

        protected override void ProcessAttribute(LayoutAttribute attribute)
        {
            base.ProcessAttribute(attribute);
            var tab = (Tab)attribute;
            var tabName = tab.Path.Split("/").Last();
            if (tab.IsDefault)
            {
                _defaultTab = tabName;
            }

            if (tab.HasColor)
            {
                if (!_tabVsColors.ContainsKey(tabName))
                {
                    _tabVsColors.Add(tabName, tab.Color);
                }
            }

            _allowAll = _allowAll || tab.AllowAll;
            _allowNone = _allowNone || tab.AllowNone;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            DrawTab();
        }

        private void DrawTab()
        {
            var lastColor = GUI.color;
            GUI.color = _tabVsColors.GetValueOrDefault(SelectedTab, lastColor);
            EditorGUILayout.BeginVertical(GUI.skin.box);
            GUI.color = lastColor;
            DrawToolBar();
            EditorGUI.indentLevel++;
            DrawTabContent();
            EditorGUI.indentLevel--;
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
        }

        private void DrawTabContent()
        {
            foreach (var item in GetItemsForTab(SelectedTab))
            {
                if (item is string field)
                {
                    EditorGUILayout.PropertyField(SerializedObject.FindProperty(field), true,
                        GUILayout.ExpandWidth(true));
                }
                else if (item is LayoutGroupElement groupElement)
                {
                    groupVsEditors[groupElement].OnInspectorGUI();
                }
            }
        }

        private void DrawToolBar()
        {
            GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
            GUILayout.Space(20 * Depth);
            _selected = GUILayout.Toolbar(_selected,
                _tabs.ToArray(), GUILayout.ExpandWidth(true));
            GUILayout.Space(20 * Depth);
            GUILayout.EndHorizontal();
        }

        private IEnumerable<object> GetItemsForTab(string tab) => Elements.Where(e=> tab == "All" || e.Name == tab ).SelectMany(l=>l.Items);
    }
}