using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DGames.Essentials.Attributes;
using UnityEngine;
using Object = UnityEngine.Object;

namespace DGames.Essentials.Editor
{
    [CustomDashboardAttributeProcessor(typeof(DashboardResourceItemAttribute))]
    // ReSharper disable once HollowTypeName
    // ReSharper disable once UnusedType.Global
    public class DashboardResourceItemAttributeProcessor : DashboardAttributeProcessor
    {
        public override Type TargetType => typeof(DashboardResourceItemAttribute);

        public override IEnumerable<MenuInfo> Process(params Type[] types)
        {
            return from type in types
                let attribute =
                    type.GetCustomAttribute<DashboardResourceItemAttribute>()
                let obj = string.IsNullOrEmpty(attribute.FileName)
                    ? ScriptableEditorUtils.GetOrCreate(type, childrenPath: attribute.SubFolderPath)
                    : Resources.Load(attribute.FileName, type)
                let treeAttribute = type.GetCustomAttribute<TreeBasedResourceItem>()
                where obj != null
                select new MenuInfo()
                {
                    MenuItem = treeAttribute != null
                        ? ObjectMenuItem.CreateTreeBased(obj, treeAttribute.ChildrenName,rootName:attribute.DisplayName)
                        : new ObjectMenuItem(obj,name:attribute.DisplayName),
                    Path = attribute.Path,
                    TabPath = attribute.TabPath
                };
        }
    }

    // ReSharper disable once HollowTypeName
}