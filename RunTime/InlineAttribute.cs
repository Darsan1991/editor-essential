using System;
using UnityEditor;
using UnityEngine;

namespace DGames.Essentials.Attributes
{
    public sealed class InlineAttribute : PropertyAttribute
    {
        public float MinWidth { get; }
        public bool DrawBox { get; }
        public Color Color { get; }


        private static readonly Color _defaultColor = new Color(42f / 255, 42f / 255, 42f / 255, 0.5f);


        public InlineAttribute(float minWidth=400f,bool drawBox = false)
        {
            MinWidth = minWidth;    
            DrawBox = drawBox;
            Color =  _defaultColor;
        }
        
        // ReSharper disable once TooManyDependencies
        public InlineAttribute(float r,float g,float b,float a=1f,float minWidth=500f)
        {
            MinWidth = minWidth;    
            DrawBox = true;
            Color = new Color(r, g, b,a);
        }
   
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class DashboardMessageAttribute : Attribute
    {
        public string Message { get; }
        public bool ForInstance { get; }

        public DashboardMessageAttribute(string message,bool forInstance=true)
        {
            Message = message;
            ForInstance = forInstance;
        }
    }
    
    [AttributeUsage(AttributeTargets.Class)]
    public class ObjectMessageAttribute : Attribute
    {
        public string PropertyPath { get; }

        public ObjectMessageAttribute(string propertyPath)
        {
            PropertyPath = propertyPath;
        }
    }
    
    [AttributeUsage(AttributeTargets.Class)]
    public class TypeMessageAttribute : Attribute
    {
        public string Message { get; }

        public TypeMessageAttribute(string message)
        {
            Message = message;
        }
    }
    
    [AttributeUsage(AttributeTargets.Class)]
    public class HideScriptField : Attribute
    {
        public HideScriptField()
        {
        }
    }
    
    [AttributeUsage(AttributeTargets.Class)]
    public class DashboardTabMessageAttribute : Attribute
    {
        public string Message { get; }

        public DashboardTabMessageAttribute(string message)
        {
            Message = message;
        }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public abstract class BaseDashboardItemAttribute : Attribute
    {
        public string Path { get; }
        public string TabPath { get; }
        public int Order { get; }
        public string DisplayName { get; }

        // ReSharper disable once TooManyDependencies
        protected BaseDashboardItemAttribute(string path,string tabPath,int order,string displayName = null)
        {
            Path = path;
            TabPath = tabPath;
            Order = order;
            DisplayName = displayName;
        }
    }
    

    public class DashboardResourceItemAttribute : BaseDashboardItemAttribute
    {
        public string FileName { get; }
        public string SubFolderPath { get; }

        // ReSharper disable once TooManyDependencies
        public DashboardResourceItemAttribute(string fileName="",string path="", string tabPath="",string subFolderPath="",int order = 0,string displayName=null) : base(path, tabPath,order,displayName)
        {
            FileName = fileName;
            SubFolderPath = subFolderPath;
        }
    }
    
    public class DashboardResourceListAttribute : BaseDashboardItemAttribute
    {
        public string PropertyName { get; }
        public string Name { get; }
        public string SubFolderPath { get; }

        // ReSharper disable once TooManyDependencies
        public DashboardResourceListAttribute(string propertyName, string name="",string path="", string tabPath="",string subFolderPath="",int order=0,string displayName=null) : base(path, tabPath,order,displayName)
        {
            PropertyName = propertyName;
            Name = name;
            SubFolderPath = subFolderPath;
        }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class TreeBasedResourceItem : Attribute
    {
        public string ChildrenName { get; }

        public TreeBasedResourceItem(string childrenName)
        {
            ChildrenName = childrenName;
        }
    }
    
    public class DashboardTypeAttribute : BaseDashboardItemAttribute
    {
        // ReSharper disable once TooManyDependencies
        public DashboardTypeAttribute(string path="", string tabPath="",int order=0,string displayName=null) : base(path, tabPath,order,displayName)
        {
        }
    }

    
}
