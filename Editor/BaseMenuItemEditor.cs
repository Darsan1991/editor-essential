using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;

namespace DGames.Essentials.Editor
{
    [CustomMenuEditor(typeof(BaseMenuItem), true)]
    public partial class BaseMenuItemEditor
    {
        public IMenuItem Item { get; }
        public IWindow Window { get; private set; }

        public virtual void OnInspectorGUI()
        {
        }
        public BaseMenuItemEditor(IMenuItem item)
        {
            Item = item;
        }

        public virtual void OnEnter(IWindow window)
        {
            Window = window;
        }

        public virtual void OnExit()
        {
        }
    }

    public class MapTypeToTypeCache<T> where T : TypeToTypeAttribute
    {
        public Type AttributeType { get; } = typeof(T);
        private Dictionary<Type, Type> _typeToTypes;

        private void CacheTypesIfNeeded()
        {
            if (_typeToTypes != null)
            {
                return;
            }

            _typeToTypes = new Dictionary<Type, Type>();

            foreach (var type in TypeCache.GetTypesWithAttribute(AttributeType))
            {
                var attribute = (T)type.GetCustomAttribute(AttributeType);
                if (!_typeToTypes.ContainsKey(attribute.Type))
                    _typeToTypes.Add(attribute.Type, type);
                else
                    _typeToTypes[attribute.Type] = type;

                if (attribute.ForChildren)
                {
                    CacheToChildren(attribute, type);
                }
            }
        }

        private void CacheToChildren(T attribute, Type type)
        {
            foreach (var childType in TypeCache.GetTypesDerivedFrom(attribute.Type))
            {
                if (!_typeToTypes.ContainsKey(childType) ||
                    !type.IsAssignableFrom(_typeToTypes[childType]))
                {
                    if (!_typeToTypes.ContainsKey(childType))
                        _typeToTypes.Add(childType, type);
                    else
                        _typeToTypes[childType] = type;
                }
            }
        }

        public Type Get(Type type)
        {
            CacheTypesIfNeeded();
            return _typeToTypes.ContainsKey(type) ? _typeToTypes[type] : null;
        }

        public bool Has(Type type)
        {
            CacheTypesIfNeeded();
            return _typeToTypes.ContainsKey(type);
        }
    }

    public partial class BaseMenuItemEditor
    {
        private static readonly MapTypeToTypeCache<CustomMenuEditor> _editorCache = new();

        public static BaseMenuItemEditor CreateEditor(IMenuItem menuItem)
        {
            return _editorCache.Has(menuItem.GetType())
                ? (BaseMenuItemEditor)Activator.CreateInstance(_editorCache.Get(menuItem.GetType()), menuItem)
                : null;
        }
    }
}