using System;
using System.Linq;
using UnityEngine;

namespace DGames.Essentials.Attributes
{

    public abstract class LayoutAttribute : Attribute
    {
        private string _fullPath;
        private string _fullGroupPath;
        public string GroupMessage { get; }
        public string Path { get; }
        public string GroupPath { get; }

        public string FullPath => _fullPath;
        public string FullGroupPath => _fullGroupPath;

        private void CacheFullPaths()
        {
            var pathSegments = Path.Split('/');
            var groupPathSegments = GroupPath.Split('/');
            _fullPath = string.Join("/",pathSegments.Select((p, i) => new []{groupPathSegments[i],pathSegments[i]}).SelectMany(ps=>ps));

            _fullGroupPath = string.Join("/",
                new[] { groupPathSegments[0] }.Concat(groupPathSegments.Skip(1)
                    .Select((p, i) => new[] { pathSegments[i], groupPathSegments[i + 1] }).SelectMany(ps => ps)));
        }

        // public abstract Type LayoutType { get; }

        protected LayoutAttribute(string path,string groupPath,string groupMessage=null)
        {
            GroupMessage = groupMessage;
            CorrectedPath(path, groupPath, out var correctedPath, out var correctedGroupPath);
            Path = correctedPath;
            GroupPath = correctedGroupPath;
            CacheFullPaths();
        }

        // ReSharper disable once TooManyArguments
        private void CorrectedPath(string path, string groupPath, out string correctedPath, out string correctedGroupPath)
        {
            
            var paths = path.Split("/").Where(s=>!string.IsNullOrEmpty(s)).ToList();
            if (!paths.Any())
            {
                paths.Add("__Default__");
            }
            var groupPaths = groupPath.Split("/").Where(s=>!string.IsNullOrEmpty(s)).ToList();
            if (paths.Count != groupPaths.Count)
            {
                var selected = paths.Count < groupPaths.Count ? paths : groupPaths;
                selected.AddRange(Enumerable.Range(0, Mathf.Abs(paths.Count - groupPaths.Count)).Select(_ => "__Default__"));
            }

            correctedPath = string.Join("/", paths);
            correctedGroupPath = string.Join("/", groupPaths);
        }
    }

    [AttributeUsage(AttributeTargets.Field,AllowMultiple = true)]
    public class HorizontalLayout : LayoutAttribute
    {
        public HorizontalLayout(string path="", string groupPath="",string groupMessage=null) : base(string.IsNullOrEmpty(path) ? Guid.NewGuid().ToString() : path, groupPath,groupMessage)
        {
        }
    }
    
    [AttributeUsage(AttributeTargets.Field)]
    public class BoxGroupLayout : LayoutAttribute
    {
        public string DisplayTitle { get; }
        public bool AllowFold { get; }
        public bool DrawBox { get; }
        public int Style { get; }

        public int DefaultStyle { get; } = 0;
        public bool DefaultExpand { get; }

        // ReSharper disable once TooManyDependencies
        public BoxGroupLayout(string displayTitle="",string path="", string groupPath="",bool allowFold=true,bool drawBox = true,int style=0,bool defaultExpand=true,string groupMessage=null) : base(string.IsNullOrEmpty(path) ? Guid.NewGuid().ToString() : path, groupPath,groupMessage)
        {
            DisplayTitle = displayTitle;
            AllowFold = allowFold;
            DrawBox = drawBox;
            Style = style;
            DefaultExpand = defaultExpand;
        }
    }
    


    [AttributeUsage(AttributeTargets.Field)]
    public class Tab : LayoutAttribute
    {
        public int? Order { get; }
        public bool IsDefault { get; }
        public bool AllowAll { get; }
        public bool AllowNone { get; }

        public Color Color { get; }
        
        public bool HasColor { get; }



        private static readonly Color _defaultColor = new Color(42f / 255, 42f / 255, 42f / 255, 0.5f);


        // ReSharper disable once TooManyDependencies
        public Tab(string path,string groupPath="", int order = 10000000, bool isDefault = false,bool allowAll = false,bool allowNone=false,string groupMessage=null):base(path,groupPath,groupMessage)
        {
            // Debug.Log($"{Path} , {GroupPath}");
            Order = order;
            IsDefault = isDefault;
            AllowAll = allowAll;
            AllowNone = allowNone;
            Color = _defaultColor;
            HasColor = false;
        }
        
        // ReSharper disable once UnusedMember.Global
        // ReSharper disable once TooManyDependencies
        public Tab(string path,float r,float g,float b,float a,string groupPath="",int order = 10000000,bool isDefault = false,bool allowAll = false,bool allowNone=false,string groupMessage=null):base(path,groupPath,groupMessage)
        {
            Color = new Color(r, g, b, a);
            HasColor = true;
            Order = order;
            IsDefault = isDefault;
            AllowAll = allowAll;
            AllowNone = allowNone;
        }

    }
    
    public class UseTabAttribute : PropertyAttribute
    {
        public string[] HideFields { get; }

        public UseTabAttribute(params string[] hideFields)
        {
            HideFields = hideFields ?? Array.Empty<string>();
        }
    }
}