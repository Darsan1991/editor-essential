using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DGames.Essentials.Attributes;

namespace DGames.Essentials.Editor
{
    // ReSharper disable once HollowTypeName
    public class CustomDashboardAttributeProcessor : CustomAttributeProcessor
    {
        public CustomDashboardAttributeProcessor(Type type) : base(type)
        {
        }

        public CustomDashboardAttributeProcessor(Type type, bool forChildren) : base(type, forChildren)
        {
        }
    }

    // ReSharper disable once HollowTypeName
    public abstract partial class DashboardAttributeProcessor
    {
        private static readonly MapTypeToTypeCache<CustomDashboardAttributeProcessor> _attributeProcessor = new();

        public static DashboardAttributeProcessor GetProcessorForAttribute(Type dashboardAttribute)
        {
            return _attributeProcessor.Has(dashboardAttribute)
                ? (DashboardAttributeProcessor)Activator.CreateInstance(_attributeProcessor.Get(dashboardAttribute))
                : null;
        }

        public static IEnumerable<MenuInfo> ProcessAll(params Type[] types)
        {
            return from type in types
                from attribute in type.GetCustomAttributes<BaseDashboardItemAttribute>()
                let processor = GetProcessorForAttribute(attribute.GetType())
                from menuInfo in processor.Process(type)
                select menuInfo;
        }
    }

    // ReSharper disable once HollowTypeName
    public abstract partial class DashboardAttributeProcessor
    {
        public abstract Type TargetType { get; }
        public abstract IEnumerable<MenuInfo> Process(params Type[] types);


        public struct MenuInfo
        {
            public string TabPath { get; set; }
            public IMenuItem MenuItem { get; set; }
            public string Path { get; set; }
        }
    }
}