using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DGames.Essentials.Attributes;
using UnityEditor;
using UnityEngine;

namespace DGames.Essentials.Editor
{
    public class TabEditorLayout
    {
        private readonly SerializedObject _so;
        private readonly List<TabGroup> _tabGroups = new();

        public TabEditorLayout(SerializedObject so)
        {
            _so = so;
            CacheTabsIfNeeded();
        }

        public bool IsChild(string field)
        {
            return _tabGroups.Any(g => g.FieldsVsTab.ContainsKey(field));
        }

        public void OnInspectorGUI()
        {
            if (!HasChildren())
            {
                return;
            }

            foreach (var group in _tabGroups.Where(g => g.FieldsVsTab.Any()).OrderBy(g => g.Order))
            {
                DrawTabGroup(group.Name);
            }
        }

        private void DrawTabGroup(string name)
        {
            var group = GetGroup(name);
            var lastColor = GUI.color;
            var tab = GetTabForSelection(name, group.Selection);
            GUI.color = group.TabVsColor.GetValueOrDefault(tab, lastColor);

            EditorGUILayout.BeginVertical(GUI.skin.box);
            GUI.color = lastColor;

            group.Selection = GUILayout.Toolbar(group.Selection,
                new[] { "None" }.Concat(group.FieldsVsTab.Values.Distinct()).Concat(new[] { "All" }).ToArray());

            EditorGUI.indentLevel++;
            foreach (var field in GetFieldsForTab(name, group.Selection))
            {
                EditorGUILayout.PropertyField(_so.FindProperty(field));
            }

            EditorGUI.indentLevel--;
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
        }

        private bool HasChildren()
        {
            return _tabGroups.Any(g => g.FieldsVsTab.Any());
        }

        private void CacheTabsIfNeeded()
        {
            if (_tabGroups.Any())
            {
                return;
            }

            if (_so.targetObject == null)
            {
                return;
            }

            _tabGroups.Add(new TabGroup("Global"));

            var type = _so.targetObject.GetType();

            var serializedFields = type.GetFields(BindingFlags.Instance | BindingFlags.Public
                                                                        | BindingFlags.NonPublic).Where(f =>
                    f.GetCustomAttribute<HideInInspector>() == null)
                .ToList();
            CacheFields(serializedFields);

            CacheInParents(type);
        }

        private void CacheInParents(Type type)
        {
            while (type.BaseType != null && type.BaseType != typeof(MonoBehaviour) &&
                   type.BaseType != typeof(ScriptableObject))
            {
                type = type.BaseType;
                var fields = type.GetFields(BindingFlags.Instance
                                            | BindingFlags.NonPublic).Where(f => f.IsPrivate).Where(f =>
                        f.GetCustomAttribute<SerializeField>() != null &&
                        f.GetCustomAttribute<HideInInspector>() == null)
                    .ToList();
                CacheFields(fields);
            }
        }

        private void CacheFields(IEnumerable<FieldInfo> serializedFields)
        {
            foreach (var info in serializedFields.Where(f => f.GetCustomAttribute<Tab>() != null))
            {
                var tab = info.GetCustomAttribute<Tab>();
                GroupGroupAndTabName(tab, out var groupName, out var tabName);
                var tabGroup = GetGroup(groupName);
                tabGroup.AddField(info.Name, tabName);
                CacheOtherTabVariables(tab, tabGroup, tabName);
            }
        }

        private static void CacheOtherTabVariables(Tab tab, TabGroup tabGroup, string tabName)
        {
            if (tab.Order < tabGroup.Order)
            {
                tabGroup.Order = tab.Order ?? tabGroup.Order;
            }

            if (tab.IsDefault)
            {
                tabGroup.Default = tabName;
            }

            if (tab.HasColor)
            {
                tabGroup.AddTabColor(tab.Path, tab.Color);
            }
        }

        private void GroupGroupAndTabName(Tab tab, out string groupName, out string tabName)
        {
            if (tab.Path.Contains('/'))
            {
                groupName = tab.Path.Split('/').First();

                var gName = groupName;
                if (_tabGroups.All(g => gName != g.Name))
                {
                    _tabGroups.Add(new TabGroup(groupName));
                }

                tabName = tab.Path[(tab.Path.IndexOf('/') + 1)..];
            }
            else
            {
                groupName = "Global";
                tabName = tab.Path;
            }
        }

        private IEnumerable<string> GetFieldsForTab(string groupName, int index)
        {
            if (index == 0) return ArraySegment<string>.Empty;

            if (index == BaseTabs(groupName).Count() + 1) return GetGroup(groupName).FieldsVsTab.Keys;

            var tab = GetTabForSelection(groupName, index);

            // Debug.Log(tab);
            return GetGroup(groupName).FieldsVsTab.Where(f => f.Value == tab).Select(p => p.Key);
        }

        private string GetTabForSelection(string groupName, int index)
        {
            if (index == 0)
                return "None";
            if (index == BaseTabs(groupName).Count() + 1)
                return "All";
            return GetGroup(groupName).FieldsVsTab.Values.Distinct().ElementAt(index - 1);
        }

        private IEnumerable<string> BaseTabs(string groupName)
        {
            return GetGroup(groupName).FieldsVsTab.Values.Distinct();
        }

        public TabGroup GetGroup(string name) => _tabGroups.Find(g => g.Name == name);

        public class TabGroup
        {
            public string Name { get; }
            private readonly Dictionary<string, string> _fieldsVsTab = new();
            private readonly Dictionary<string, Color> _tabVsColor = new();
            private int _selection = -1;

            public IReadOnlyDictionary<string, string> FieldsVsTab => _fieldsVsTab;
            public IReadOnlyDictionary<string, Color> TabVsColor => _tabVsColor;


            public int Selection
            {
                get
                {
                    if (_selection < 0)
                        _selection = string.IsNullOrEmpty(Default)
                            ? 0
                            : _fieldsVsTab.Values.Distinct().ToList().IndexOf(Default) + 1;
                    return _selection;
                }
                set => _selection = value;
            }

            public int Order { get; set; } = 10000000;
            public bool FoldOut { get; set; } = true;
            public string Default { get; set; }


            public TabGroup(string name)
            {
                Name = name;
            }

            public void AddField(string field, string tab) => _fieldsVsTab.Add(field, tab);

            public void AddTabColor(string tab, Color color) => _tabVsColor.Add(tab, color);
        }
    }

    public static class TypeExtensions
    {
        public static IEnumerable<FieldInfo> GetAllSerializedFields(this Type type)
        {
            var fields = new List<FieldInfo>();
            while (type.BaseType != null && type.BaseType != typeof(MonoBehaviour) &&
                   type.BaseType != typeof(ScriptableObject))
            {
                type = type.BaseType;
                fields.AddRange(type.GetFields(BindingFlags.Instance
                                               | BindingFlags.NonPublic).Where(f => f.IsPrivate).Where(f =>
                    f.GetCustomAttribute<SerializeField>()!=null && f.GetCustomAttribute<HideInInspector>() == null));
            }

            fields.Reverse();
            foreach (var fieldInfo in fields) yield return fieldInfo;

            foreach (var fieldInfo in type.GetFields(BindingFlags.Instance | BindingFlags.Public
                                                                           | BindingFlags.NonPublic).Where(f =>
                         f.GetCustomAttribute<HideInInspector>() == null))
            {
                yield return fieldInfo;
            }
        }
    }
}