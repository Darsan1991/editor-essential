using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DGames.Essentials.Attributes;
using UnityEditor;

namespace DGames.Essentials.Editor
{
    public class LayoutDrawer
    {
        public SerializedObject SerializedObject { get; }
        private readonly List<LayoutGroupEditor> _editors = new();
        private readonly HashSet<string> _fields = new();

        public LayoutDrawer(SerializedObject serializedObject)
        {
            SerializedObject = serializedObject;
            var fieldInfos = GetAllFieldInfos(serializedObject.targetObject.GetType());
            foreach (var name in fieldInfos.Select(f => f.Name))
            {
                _fields.Add(name);
            }

            var fieldVsAttributes = fieldInfos.Select(
                f => (f, f.GetCustomAttributes<LayoutAttribute>(true).ToArray())).ToArray();

            CreateEditors(serializedObject, fieldVsAttributes);
        }

        private static FieldInfo[] GetAllFieldInfos(Type type)
        {
            return type.GetAllSerializedFields()
                .Where(f => f.GetCustomAttributes<LayoutAttribute>(true).Any()).ToArray();
        }

        private void CreateEditors(SerializedObject serializedObject, (FieldInfo f, LayoutAttribute[])[] fieldVsAttributes)
        {
            var itemInfos = fieldVsAttributes.Select(tuple => new LayoutTreeProvider.ItemInfo
            {
                Item = tuple.f.Name,
                Infos = tuple.Item2.Select(a => new LayoutInfo
                {
                    GroupPath = a.GroupPath,
                    Path = a.Path,
                    LayoutType = a.GetType()
                }).ToArray()
            });

            var layoutTreeProvider = new LayoutTreeProvider(itemInfos);
            _editors.AddRange(layoutTreeProvider.Groups.Select(g => LayoutGroupEditor.CreateEditor(
                new LayoutGroupEditor.EditorParams
                {
                    SerializedObject = serializedObject,
                    RootFieldVsAttributes = fieldVsAttributes.ToDictionary(p => p.f.Name, p => p.Item2)
                }, new LayoutGroupEditor.LayoutGroupParams
                {
                    Depth = 0,
                    GroupElement = g
                }
            )));
        }

        public bool IsChild(string fieldName)
        {
            return _fields.Contains(fieldName);
        }

        public void OnEnable()
        {
            _editors.ForEach(e => e.OnEnable());
        }

        public void OnDisable()
        {
            _editors.ForEach(e => e.OnDisable());
        }

        public void OnInspectorGUI()
        {
            _editors.ForEach(e => e.OnInspectorGUI());
        }
    }
}