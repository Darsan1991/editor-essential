using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace DGames.Essentials.Editor
{
    public abstract class BaseMenuItem : IMenuItem
    {
        private readonly List<ITreeItem> _children = new();
        public ITreeItem Parent { get; set; }
        public IEnumerable<ITreeItem> Children => _children;
        public abstract string Name { get; }
        public string FullName => string.Join("", Enumerable.Range(0, ParentCount).Select(_ => "      ")) + Name;

        public int Order { get; set; }

        public void AddChildren(ITreeItem item)
        {
            var orderedItems = _children.Append(item).Cast<IMenuItem>().Order().ToArray();
            _children.Clear();
            _children.AddRange(orderedItems);

            item.Parent = this;
        }

        public IEnumerator<ITreeItem> GetEnumerator()
        {
            return new[] { this }.Concat(_children.SelectMany(c => c)).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public int ParentCount
        {
            get
            {
                var parent = Parent;
                var count = 0;
                while (parent is { })
                {
                    count++;
                    parent = parent.Parent;
                }

                return count;
            }
        }

        public IContentViewer ContentViewer { get; set; }
    }

    public class ObjectMenuItem : BaseMenuItem
    {
        private readonly Option[] _options;
        private readonly string _name;
        public override string Name => string.IsNullOrEmpty(_name) ? Object.name : _name;

        public Object Object { get; }
        public IEnumerable<Option> Options => _options;

        public ObjectMenuItem(Object obj,IEnumerable<Option> options=null,string name=null)
        {
            Object = obj;
            _options = (options ?? Array.Empty<Option>()).ToArray();
            _name =  name;
        }

        // ReSharper disable once TooManyArguments
        public static ObjectMenuItem CreateTreeBased(Object obj, string arrayPath,IEnumerable<Option> options=null,FieldInfo fieldInfo = null,string rootName=null)
        {
            fieldInfo ??= obj.GetType().GetField(arrayPath, BindingFlags.Instance | BindingFlags.Public
                | BindingFlags.NonPublic);
            var objectMenuItem = new ObjectMenuItem(obj,options,name:rootName);

            if (fieldInfo == null)
            {
                Debug.LogError("Field Not Found:"+arrayPath);
                return objectMenuItem;
            }
            
            foreach (Object item in (IEnumerable)fieldInfo!.GetValue(obj))
            {
                if(item!=null)
                    objectMenuItem.AddChildren(CreateTreeBased(item,arrayPath,new [] { Option.Rename,Option.Delete },fieldInfo));
            }

            return objectMenuItem;
        }
        
        public enum Option
        {
            Duplicate,Delete,
            Rename
        }
    }
    
    
    public class SerializablePropertyMenuItem : BaseSerializablePropertyMenuItem
    {
        private readonly string _name;

        public override string Name =>
            string.IsNullOrEmpty(_name) ? SerializedObject.FindRelativePropertyAd(PropertyPath).displayName : _name;

        public string PropertyPath { get; }

        public SerializablePropertyMenuItem(SerializedObject so,string propertyPath,string name=null) : base(so)
        {
            PropertyPath = propertyPath;
            // Debug.Log($"So:{SerializedObject} Property:{SerializedObject.FindRelativePropertyAd(PropertyPath)} Path:{PropertyPath}");
            _name = name;
        }
    }
    
    public class EmptyTreeItem : IMenuItem
    {
        private readonly List<ITreeItem> _children = new();

        public string FullName { get; } = "";
        public ITreeItem Parent { get; set; }
        public IEnumerable<ITreeItem> Children => _children;

        public void AddChildren(ITreeItem item)
        {
            _children.Add(item);
        }

        public IEnumerator<ITreeItem> GetEnumerator()
        {
            return new List<ITreeItem> { this }.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public int Order { get; set; }
        public IContentViewer ContentViewer { get; set; }
    }
    
    public class SerializablePropertyListCreateNewMenuItem : BaseSerializablePropertyMenuItem
    {
        public override string Name =>
            "Create New";

        public string PropertyPath { get; }

        public SerializablePropertyListCreateNewMenuItem(SerializedObject so,string propertyPath) : base(so)
        {
            PropertyPath = propertyPath;
        }
    }

    public static class MenuItemExtensions
    {
        public static IEnumerable<IMenuItem> Order(this IEnumerable<IMenuItem> items)
        {
            var pendingItems = new List<IMenuItem>();
            foreach (var menuItem in items)
            {
                if (pendingItems.Any() && menuItem is EmptyTreeItem)
                {
                    foreach (var item in pendingItems.OrderBy(i => i.Order)) yield return item;
                    yield return menuItem;
                    pendingItems.Clear();
                }
                
                if(menuItem is not EmptyTreeItem)
                    pendingItems.Add(menuItem);
            }
            
            foreach (var item in pendingItems.OrderBy(i => i.Order)) yield return item; 
        }
    }
    
    public abstract class BaseSerializablePropertyMenuItem : BaseMenuItem,IDisposable
    {
        protected readonly Dictionary<SerializedObject, List<BaseSerializablePropertyMenuItem>> locks = new();
        
        public SerializedObject SerializedObject { get; }

        protected BaseSerializablePropertyMenuItem(SerializedObject so)
        {
            SerializedObject = so;
            if (!locks.ContainsKey(SerializedObject))
            {
                locks.Add(SerializedObject,new List<BaseSerializablePropertyMenuItem>());
            }
            locks[SerializedObject].Add(this);
        }

        public virtual void Dispose()
        {
            locks[SerializedObject].Remove(this);
            if (!locks[SerializedObject].Any())
            {
                locks.Remove(SerializedObject);
                SerializedObject.Dispose();
            }
        }
    }
    
    public class ObjectTypeMenuItem : BaseMenuItem
    {
        public override string Name { get; }

        public Type Type { get; }

        public ObjectTypeMenuItem(Type type,string name,params ObjectMenuItem[] children)
        {
            Type = type;
            Name = string.IsNullOrEmpty(name) ? type.Name : name;
            foreach (var menuItem in children)
            {
                AddChildren(menuItem);
            }
        }
    }
    
    public class GroupObjectMenuItem : BaseMenuItem
    {
        public override string Name { get; }


        public  GroupObjectMenuItem(string name)
        {
            Name = name;
        }
    }
}