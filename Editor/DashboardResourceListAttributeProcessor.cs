using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DGames.Essentials.Attributes;
using UnityEditor;
using UnityEngine;

namespace DGames.Essentials.Editor
{
    [CustomDashboardAttributeProcessor(typeof(DashboardResourceListAttribute))]
    // ReSharper disable once HollowTypeName
    // ReSharper disable once UnusedType.Global
    public class DashboardResourceListAttributeProcessor : DashboardAttributeProcessor
    {
        public override Type TargetType => typeof(DashboardResourceListAttribute);

        public override IEnumerable<MenuInfo> Process(params Type[] types)
        {
            foreach (var type in types)
            {
                var attribute = type.GetCustomAttribute<DashboardResourceListAttribute>();
                var obj = string.IsNullOrEmpty(attribute.Name)
                    ? ScriptableEditorUtils.GetOrCreate(type, childrenPath: attribute.SubFolderPath)
                    : Resources.Load(attribute.Name, type);
                if (obj != null)
                {
                    var objectMenuItem = new ObjectMenuItem(obj,name:attribute.DisplayName) { Order = -1 };
                    var so = new SerializedObject(obj);

                    foreach (var menuItem in Enumerable.Range(0, so.FindProperty(attribute.PropertyName).arraySize)
                                 .Select(i =>
                                     new SerializablePropertyMenuItem(so, $"{attribute.PropertyName}.Array.data[{i}]")
                                     {
                                         Order = i
                                     }
                                 ))
                    {
                        objectMenuItem.AddChildren(menuItem);
                    }

                    yield return new MenuInfo
                    {
                        MenuItem = objectMenuItem,
                        Path = attribute.Path,
                        TabPath = attribute.TabPath,
                    };
                    //
                    //
                    //
                    // yield return new MenuInfo
                    // {
                    //     MenuItem = new EmptyTreeItem(),
                    //     Path = attribute.Path,
                    //     TabPath = attribute.TabPath,
                    // };
                    yield return new MenuInfo
                    {
                        MenuItem = new SerializablePropertyListCreateNewMenuItem(so, attribute.PropertyName)
                            { Order = 1000 },
                        Path = attribute.Path,
                        TabPath = attribute.TabPath
                    };
                }
            }
        }
    }
}