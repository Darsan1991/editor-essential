using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DGames.Essentials.Attributes;
using UnityEditor;
using UnityEngine;

namespace DGames.Essentials.Editor
{
    // ReSharper disable once HollowTypeName
    [CustomDashboardAttributeProcessor(typeof(DashboardTypeAttribute))]
    public class DashboardTypeAttributeProcessor : DashboardAttributeProcessor
    {
        public override Type TargetType => typeof(DashboardTypeAttribute);

        public override IEnumerable<MenuInfo> Process(params Type[] types)
        {
            return from type in types
                let attribute =
                    type.GetCustomAttribute<DashboardTypeAttribute>()
                let objs = AssetDatabase.FindAssets($"t:{type.Name}").Select(AssetDatabase.GUIDToAssetPath)
                    .Select(AssetDatabase.LoadMainAssetAtPath)
                select new MenuInfo()
                {
                    MenuItem = new ObjectTypeMenuItem(type,string.IsNullOrEmpty(attribute.DisplayName)? type.Name:attribute.DisplayName,objs.Select(obj=>new ObjectMenuItem(obj,new []
                        {
                            ObjectMenuItem.Option.Rename,
                            ObjectMenuItem.Option.Duplicate
                        ,ObjectMenuItem.Option.Delete})).ToArray()),
                    Path = attribute.Path,
                    TabPath = attribute.TabPath
                };
        }
    }
}