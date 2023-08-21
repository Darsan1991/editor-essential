using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace DGames.Essentials.Editor
{
    public class RepeatedToolBarDrawer
    {
        private readonly List<Tab> _tabs = new();

        private readonly Dictionary<Tab, string> _selectedPathsCache = new();

        public RepeatedToolBarDrawer(params string[] paths)
        {
            foreach (var path in paths)
            {
                ProcessPathToPopulateTabs(path);
            }
        }

        private void ProcessPathToPopulateTabs(string path)
        {
            var tabs = path.Split('/');

            var tab = _tabs.Find(t => t.title == tabs.First());
            if (tab == null)
            {
                tab = new Tab { title = tabs.First() };
                _tabs.Add(tab);
            }

            foreach (var tabTitle in tabs.Skip(1))
            {
                var newTab = tab.children.Find(t => t.title == tabTitle);
                if (newTab == null)
                {
                    newTab = new Tab { title = tabTitle };
                    tab.children.Add(newTab);
                }

                tab = newTab;
            }
        }

        public string Draw(string path)
        {
            path = string.IsNullOrEmpty(path) ? _tabs.First().title : path;

            var tabs = ToTabs(path).ToArray();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.Space(0.1f);
            var index = GUILayout.Toolbar(_tabs.IndexOf(tabs.First()), _tabs.Select(t => t.title).ToArray(),GUILayout.MaxWidth(EditorGUIUtility.currentViewWidth*0.8f));
            EditorGUILayout.Space(0.1f);
            EditorGUILayout.EndHorizontal();
            var newPath = _tabs[index].title;

            
            if (_tabs[index].children.Any())
                newPath += "/" + DrawNextedTabs(_tabs[index], tabs.Skip(1).ToList());
            // Debug.Log($"{path}:{_tabs[index].title}:{newPath}");

            return newPath;
        }

        private void CacheSelectedPathForTab(Tab tab, string newPath)
        {
            // if (!_selectedPathsCache.ContainsKey(tab) || _selectedPathsCache[tab] != newPath)
            //     Debug.Log($"Tab:{tab.title} Path:{newPath}");

            if (!_selectedPathsCache.ContainsKey(tab))
            {
                _selectedPathsCache.Add(tab, newPath);
            }
            else
            {
                _selectedPathsCache[tab] = newPath;
            }
        }

        private string DrawNextedTabs(Tab parentTab, IReadOnlyList<Tab> nextedTabs, int depth = 1)
        {
            if (!nextedTabs.Any() && GetLastSelectedNextedTabs(parentTab).Any() ||
                nextedTabs.Any() && !parentTab.children.Contains(nextedTabs.First()))
            {
                Debug.Log("Parent Changed:"+string.Join(",",GetLastSelectedNextedTabs(parentTab).Select(t=>t.title)));
                return DrawNextedTabs(parentTab, GetLastSelectedNextedTabs(parentTab).ToList());
            }

            var index = nextedTabs.Any() ? parentTab.children.IndexOf(nextedTabs.First()) : 0;

            var selected = DrawNextedTabsGUI(parentTab, depth, index);

            var path = parentTab.children[selected].title;
            if (parentTab.children[index].children.Any())
                path += "/" + DrawNextedTabs(parentTab.children[index], nextedTabs.Skip(1).ToList(), depth + 1);

            CacheSelectedPathForTab(parentTab, path);

            return path;
        }

        private static int DrawNextedTabsGUI(Tab parentTab, int depth, int index)
        {
            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            var space = (depth) * 0.5f;
            EditorGUILayout.Space(space);
            var selected = GUILayout.Toolbar(index,
                parentTab.children.Select(t => t.title).ToArray());
            EditorGUILayout.Space(space);
            EditorGUILayout.EndHorizontal();
            return selected;
        }

        private IEnumerable<Tab> GetLastSelectedNextedTabs(Tab parentTab)
        {
            var path = _selectedPathsCache.GetValueOrDefault(parentTab, "");

            // Debug.Log(path +" "+ string.Join(',',tabs.Select(t=>t?.title)));
            return string.IsNullOrEmpty(path) ? Array.Empty<Tab>() : ToTabs(path, parentTab);
        }


        private IEnumerable<Tab> ToTabs(string path)
        {
            var tabTitles = path.Split('/');

            var tab = _tabs.Find(t => t.title == tabTitles.FirstOrDefault());

            yield return tab;

            foreach (var title in tabTitles.Skip(1))
            {
                tab = tab.children.Find(t => t.title == title);
                yield return tab;
            }
        }

        private IEnumerable<Tab> ToTabs(string path, Tab relativeTab)
        {
            var tabTitles = path.Split('/');

            var tab = relativeTab.children.Find(t => t.title == tabTitles.FirstOrDefault());

            yield return tab;

            foreach (var title in tabTitles.Skip(1))
            {
                tab = tab.children.Find(t => t.title == title);
                yield return tab;
            }
        }


        public bool IsParent(Tab tab, Tab child)
        {
            return tab.children.Contains(child) || tab.children.Any(c => IsParent(c, child));
        }

        public class Tab
        {
            public string title;
            public readonly List<Tab> children = new();
        }
    }
}