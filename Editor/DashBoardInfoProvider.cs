using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DGames.Essentials.Attributes;
using DGames.Essentials.Infos;
using UnityEngine;
using Object = UnityEngine.Object;

namespace DGames.Essentials.Editor
{
    // ReSharper disable once HollowTypeName
    public class DashBoardInfoProvider
    {
        private readonly Dictionary<string, List<IMenuItem>> _tabVsMenuItems = new();
        private readonly Dictionary<string, string> _tabVsMessages = new();

        private readonly List<(Type, BaseDashboardItemAttribute)> _classWithAttributes = new();

        public DashBoardInfoProvider()
        {
            Refresh();
        }

        public void Refresh()
        {
            _tabVsMenuItems.Clear();
            foreach (var (menuItem, path, tabPath) in GetItems())
            {
                AddItem(menuItem, string.IsNullOrEmpty(tabPath) ? "General" : tabPath, path);
            }
            CacheTabMessages();
        }

        public bool ContainsTab(string tabPath) => _tabVsMenuItems.ContainsKey(tabPath);

        public void RefreshTab(string tabPath)
        {
            _tabVsMenuItems.Remove(tabPath);
            foreach (var (menuItem, path, tPath) in GetItems(tabPath))
            {
                AddItem(menuItem, string.IsNullOrEmpty(tPath) ? "General" : tPath, path);
            }
        }

        private void CacheTabMessages()
        {
            _tabVsMessages.Clear();
            foreach (var (path,attribute) in GetAllClassesOfBaseDashboardAttribute()
                         .Select(tuple=>(tuple.Item2.TabPath ,tuple.Item1.GetCustomAttribute<DashboardTabMessageAttribute>()))
                         .Where(tuple => tuple.Item2!=null))
            {
                _tabVsMessages.TryAdd(path, attribute.Message);
            }
        }


        public IEnumerable<(string, IEnumerable<IMenuItem>)> GetInfos() => _tabVsMenuItems.Where(p=> !DashboardControlInfos.Default || DashboardControlInfos.Default.BlackList.All(t=>!p.Key.Contains(t)))
            .Select(tabVsMenuItem => (tabVsMenuItem.Key, tabVsMenuItem.Value.Order()))
            .Select(pair => ((string, IEnumerable<IMenuItem>))pair);

        private IEnumerable<(IMenuItem, string, string)> GetItems(string tabPath = null) =>
            DashboardAttributeProcessor
                .ProcessAll(
                    GetAllClassesOfBaseDashboardAttribute()
                        .Where(c => string.IsNullOrEmpty(tabPath) || c.Item2.TabPath == tabPath ||
                                    tabPath == "General" && string.IsNullOrEmpty(c.Item2.TabPath))
                        .Select(c => c.Item1)
                        .ToArray())
                .Select(d => (d.MenuItem, ParentPath: d.Path, d.TabPath));

        public string GetTabMessage(string tabPath)
        {
            return _tabVsMessages.GetValueOrDefault(tabPath, null);
        }

        private void AddItem(IMenuItem menuItem, string tabPath, string path)
        {
            if (path.Any())
            {
                var parent = ParentMenuItem(menuItem, tabPath, path);
                parent.AddChildren(menuItem);
            }
            else
            {
                AddTabToTabVsMenuItemDictIfNotAlready(tabPath);
                _tabVsMenuItems[tabPath].Add(menuItem);
            }
        }

        private void AddTabToTabVsMenuItemDictIfNotAlready(string tabPath)
        {
            if (!_tabVsMenuItems.ContainsKey(tabPath))
                _tabVsMenuItems.Add(tabPath, new List<IMenuItem>());
        }


        private IMenuItem ParentMenuItem(IMenuItem menu, string tabPath, string path)
        {
            var items = path.Split('/').ToList();
            AddTabToTabVsMenuItemDictIfNotAlready(tabPath);
            var menuItems = _tabVsMenuItems[tabPath];

            IMenuItem groupObjectMenuItem;

            if (menuItems.All(i => i.FullName.Trim() != items.First()))
            {
                groupObjectMenuItem = new GroupObjectMenuItem(items.First());
                menuItems.Add(groupObjectMenuItem);
            }
            else
            {
                groupObjectMenuItem = menuItems.First(i => i.FullName.Trim() == items.First());
            }

            foreach (var name in items.Skip(1))
            {
                var newGroupItem = new GroupObjectMenuItem(name);
                groupObjectMenuItem.AddChildren(groupObjectMenuItem);
                groupObjectMenuItem = newGroupItem;
            }

            return groupObjectMenuItem;
        }

        private IEnumerable<(Type, BaseDashboardItemAttribute)> GetAllClassesOfBaseDashboardAttribute()
        {
            if (!_classWithAttributes.Any())
            {
                var types = GetAllClassesWithAttribute(typeof(BaseDashboardItemAttribute));
                _classWithAttributes.AddRange(
                    types.Select(t => (t, t.GetCustomAttribute<BaseDashboardItemAttribute>())));
            }

            return _classWithAttributes;
        }

        public static IEnumerable<Type> GetAllClassesWithAttribute(Type type)
        {
            return AppDomain.CurrentDomain.GetAssemblies()
                .Select(a => a.GetTypes())
                .SelectMany(t => t).Where(t => t.IsClass && t.GetCustomAttribute(type) != null);
        }
    }

    public struct ObjectInfo
    {
        public Object obj;
        public string path;
        public string tabPath;
    }

    // ReSharper disable once HollowTypeName

    // ReSharper disable once HollowTypeName


    // ReSharper disable once HollowTypeName
}