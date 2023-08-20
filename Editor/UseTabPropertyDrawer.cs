using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DGames.Essentials.Attributes;
using DGames.Essentials.EditorHelpers;
using UnityEditor;
using UnityEngine;

namespace DGames.Essentials.Editor
{
    [CustomPropertyDrawer(typeof(UseTabAttribute))]
    public class UseTabPropertyDrawer : PropertyDrawer
    {
        private readonly Dictionary<string, string> _fieldVsTab = new();
        private readonly List<string> _otherFields = new();
        private readonly Dictionary<string, Color> _tabVsColor = new();
        private Rect _tabPosition;
        private Rect _otherFieldsPosition;


        public UseTabAttribute Attribute => (UseTabAttribute)attribute;
        public bool HasTabs => _fieldVsTab.Any();

        private int _selected;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return !property.isExpanded ? EditorGUIUtility.singleLineHeight :  HasTabs ? GetHeightWithIfHasTabs(property,label) : base.GetPropertyHeight(property, label);
        }

        private float GetHeightWithIfHasTabs(SerializedProperty property,GUIContent label)
        {
            var otherFieldHeights =
                _otherFields.Select(p => EditorGUI.GetPropertyHeight(property.FindPropertyRelative(p))).Sum();
            var tabElementHeight = GetCurrentTabElementHeight(property);
            return otherFieldHeights + 1*EditorGUIUtility.singleLineHeight * 1 + tabElementHeight +
                   EditorGUIUtility.standardVerticalSpacing * 4 + 
                   (HasLabel(label)?1:0)*EditorGUIUtility.singleLineHeight  
                   +(HaveOtherFields()?1*EditorGUIUtility.singleLineHeight : 0)
                   ;
        }

        private float GetCurrentTabElementHeight(SerializedProperty property)
        {
            return GetFieldsForTab(GetSelectedValue(property)).Select(p => EditorGUI.GetPropertyHeight(property.FindPropertyRelative(p)))
                .Sum();
        }

        private bool HaveOtherFields() => _otherFields.Any();


        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            CacheTabsIfNeeded(property);

            position.height = GetPropertyHeight(property, label);

            var hasLabel = HasLabel(label);
            if (hasLabel)
            {
                Debug.Log(label.text);
                position.height = EditorGUIUtility.singleLineHeight;
                property.isExpanded = EditorGUI.Foldout(position,property.isExpanded,label);
            }

            if(!property.isExpanded)
                return;
            

            position.height = GetPropertyHeight(property, label);
            EditorGUI.BeginProperty(position, label, property);
            _otherFieldsPosition = new Rect(position);
            
            _otherFieldsPosition.y += GetCurrentTabElementHeight(property) + 1 * EditorGUIUtility.singleLineHeight +
                                      EditorGUIUtility.standardVerticalSpacing * 2;

            _otherFieldsPosition.height = EditorGUIUtility.singleLineHeight;
            if(_otherFields.Any())
                EditorGUI.LabelField(_otherFieldsPosition,GetTitleWithDashes("",100));

            _otherFieldsPosition.y += EditorGUIUtility.singleLineHeight;

            _tabPosition = new Rect(position)
            {
                height = EditorGUIUtility.singleLineHeight
            };
            _tabPosition.y += hasLabel ?  EditorGUIUtility.singleLineHeight : 0;
            // Debug.Log(_tabPosition.y);

            var selectedTabProperty = property.FindPropertyRelative("_selectedTab");

            if (selectedTabProperty != null)
            {
                EditorGUI.BeginChangeCheck();
                
                selectedTabProperty.intValue = GUI.Toolbar(_tabPosition, selectedTabProperty.intValue,
                    new[] { "None" }.Concat(_fieldVsTab.Values.Distinct()).Concat(new []{"All"}).Select(l=>new GUIContent(""+l, image:null)).ToArray());
                if (EditorGUI.EndChangeCheck())
                {
                    Event.current.Use();
                }
                _selected = selectedTabProperty.intValue;
            }
            else
            {
                _selected = GUI.Toolbar(_tabPosition, _selected,
                    new[] { "None" }.Concat(_fieldVsTab.Values.Distinct()).Concat(new []{"All"}).ToArray());
            }

            _tabPosition.y += EditorGUIUtility.singleLineHeight * 1;

            _tabPosition.height = GetCurrentTabElementHeight(property) + 4 * EditorGUIUtility.standardVerticalSpacing;

            if(_selected>0)
                _tabPosition = DrawBackgroundColorBoxIfNeeded(_tabPosition, property);
            _tabPosition.height = EditorGUIUtility.singleLineHeight;

            var parentDepth = property.depth;

            if (property.NextVisible(true) && parentDepth < property.depth)
            {
                do
                {
                    if (_otherFields.Contains(property.name))
                        DrawOtherItem(property);
                    else if (GetFieldsForTab(_selected).Contains(property.name))
                        DrawTabItem(property);
                } while (property.NextVisible(false) && parentDepth < property.depth);
            }

            EditorGUI.EndProperty();
        }
        
        public static string GetTitleWithDashes(string title, int countPerSide = 70)
        {
            var dashes = string.Join("", Enumerable.Repeat("-", countPerSide));

            return $"{dashes}{title}{dashes}";
        }

        private static bool HasLabel(GUIContent label)
        {
            return label != null && label != GUIContent.none && !string.IsNullOrEmpty(label.text);
        }

        private int GetSelectedValue(SerializedProperty property)
        {
            var selectedTabProperty = property.FindPropertyRelative("_selectedTab");

            return selectedTabProperty?.intValue ?? _selected;
        }

        private void DrawOtherItem(SerializedProperty property)
        {
            EditorGUI.PropertyField(_otherFieldsPosition, property,true);
            _otherFieldsPosition.y += EditorGUI.GetPropertyHeight(property);
        }

        private void DrawTabItem(SerializedProperty property)
        {
            EditorGUI.PropertyField(_tabPosition, property,true);
            _tabPosition.y += EditorGUI.GetPropertyHeight(property);
        }

        private IEnumerable<string> GetFieldsForTab(int index)
        {
            if(index==0)
                return ArraySegment<string>.Empty;
            if (index == _fieldVsTab.Values.Distinct().Count()+1)
            {
                return _fieldVsTab.Keys;
            }

            var tab = _fieldVsTab.Values.Distinct().ElementAt(index - 1);

            // Debug.Log(tab);
            return _fieldVsTab.Where(f => f.Value == tab).Select(p => p.Key);
        }


        private void CacheTabsIfNeeded(SerializedProperty property)
        {
            if (_fieldVsTab.Any())
            {
                return;
            }

            _otherFields.Clear();
            var type = property.GetValueType();

            var serializedFields = type.GetFields(BindingFlags.Instance | BindingFlags.Public
                                                                        | BindingFlags.NonPublic).Where(f =>
                (f.GetCustomAttribute<SerializeField>() != null || f.IsPublic) && f.GetCustomAttribute<HideInInspector>()==null).ToList();
            string defaultTab = null;
            foreach (var info in serializedFields.Where(f => f.GetCustomAttribute<Tab>() != null))
            {
                var tab = info.GetCustomAttribute<Tab>();
                if (tab.HasColor && !_tabVsColor.ContainsKey(tab.Path))
                {
                    _tabVsColor.Add(tab.Path,tab.Color);
                }
                if (tab.IsDefault)
                {
                    defaultTab = tab.Path;
                }
                _fieldVsTab.Add(info.Name, tab.Path);
            }
            
            Debug.Log(defaultTab);

            _selected = Mathf.Max(_fieldVsTab.Values.Distinct().ToList().IndexOf(defaultTab)+1,0);
            

            _otherFields.AddRange(serializedFields.Where(f => f.GetCustomAttribute<Tab>() == null && !Attribute.HideFields.Contains(f.Name)).Select(f => f.Name));
        }

        private Rect DrawBackgroundColorBoxIfNeeded(Rect position, SerializedProperty property)
        {
            // position.height -= EditorGUIUtility.standardVerticalSpacing * 2;
            var tab = GetSelectedTabName(property);
            EditorGUI.DrawRect(position, _tabVsColor.ContainsKey(tab) ? _tabVsColor[tab]: new Color(0.15f, 0.15f, 0.15f, 0.4f));
            var haveFold = property.HaveFold();
            var foldWidth = (EditorGUIUtility.standardVerticalSpacing * 7);
            position.position += Vector2.up * (EditorGUIUtility.standardVerticalSpacing * 1);
            position.position +=
                Vector2.right * foldWidth; //(EditorGUIUtility.standardVerticalSpacing * 2 + (!haveFold ? foldWidth:0));
            position.width -= EditorGUIUtility.standardVerticalSpacing * 4 + (haveFold ? foldWidth : 0);

            return position;
        }

        private string GetSelectedTabName(SerializedProperty property)
        {
            var selected = GetSelectedValue(property);
            return  selected == 0 ? "None"  : selected == _fieldVsTab.Values.Distinct().Count()+1 ? "All": _fieldVsTab.Values.Distinct().ElementAt(selected-1);
        }
    }
    
    
}