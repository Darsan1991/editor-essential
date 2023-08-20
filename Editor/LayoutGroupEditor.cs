using System;
using System.Collections.Generic;
using System.Linq;
using DGames.Essentials.Attributes;
using UnityEditor;
using UnityEngine;
using MessageType = UnityEditor.MessageType;

namespace DGames.Essentials.Editor
{
    public abstract partial class LayoutGroupEditor
    {
        private static readonly MapTypeToTypeCache<CustomLayoutGroupEditor> _typeCache = new();

        public static Type GetGroupEditor(Type type)
        {
            return _typeCache.Get(type);
        }
    }

    public abstract partial class LayoutGroupEditor
    {
        public LayoutGroupElement GroupElement { get; }
        public SerializedObject SerializedObject { get; }
        public int Depth { get; }

        public string GroupMessage { get; private set; }
        public IEnumerable<LayoutElement> Elements => GroupElement.Children;


        protected readonly IReadOnlyDictionary<string, LayoutAttribute[]> rootFieldVsAttributes;
        protected readonly Dictionary<LayoutGroupElement, LayoutGroupEditor> groupVsEditors = new();
        protected readonly Dictionary<LayoutAttribute, string> currentAttributeVsFields = new();

        // ReSharper disable once TooManyDependencies
        protected LayoutGroupEditor(EditorParams editorParams, LayoutGroupParams layoutGroupParams)
        {
            SerializedObject = editorParams.SerializedObject;
            rootFieldVsAttributes = editorParams.RootFieldVsAttributes;
            Depth = layoutGroupParams.Depth;
            GroupElement = layoutGroupParams.GroupElement;

            CreateChildEditors(editorParams);
            ProcessAttributes();
           
        }

        private void ProcessAttributes()
        {
            foreach (var (attribute, field) in rootFieldVsAttributes.Select(pair =>
                             pair.Value.Where(t => t.GetType().IsAssignableFrom((Type)GroupElement.LayoutType))
                                 .Select(attribute => (attribute, pair.Key))).SelectMany(ts => ts)
                         .Where(ts=>
                         {
                             Debug.Log($"{ts.attribute.FullGroupPath} ----- {GroupElement.Path}");
                             return ts.attribute.FullGroupPath == GroupElement.Path;
                         })
                         .Where(ts => !currentAttributeVsFields.ContainsKey(ts.attribute)))
            {
                
                currentAttributeVsFields.Add(attribute, field);
            }

            foreach (var layoutAttribute in currentAttributeVsFields.Keys)
            {
                ProcessAttribute(layoutAttribute);
            }
        }


        protected void DrawGroupMessageIfExist()
        {
            if(string.IsNullOrEmpty(GroupMessage))
                return;
            
            EditorGUILayout.BeginVertical(GUILayout.MaxHeight(10));
            EditorGUI.HelpBox(EditorGUILayout.GetControlRect(GUILayout.MaxHeight(35),GUILayout.ExpandWidth(true)),GroupMessage,MessageType.Info);
            // EditorGUILayout.HelpBox(_tabMessage,MessageType.Info,true);
            EditorGUILayout.EndVertical();
        }

        private void CreateChildEditors(EditorParams editorParams)
        {
            foreach (var layoutElement in GroupElement.Children)
            {
                var layoutGroupElements = layoutElement.Groups.ToList();
                foreach (var (g, e) in layoutGroupElements
                             .Select(g => (g, CreateEditor(editorParams, new LayoutGroupParams
                             {
                                 Depth = Depth + 1,
                                 GroupElement = g
                             }))))
                {
                    groupVsEditors.Add(g, e);
                }
            }
        }


        protected virtual void ProcessAttribute(LayoutAttribute attribute)
        {
            
            GroupMessage = string.IsNullOrEmpty(attribute.GroupMessage) ? GroupMessage : attribute.GroupMessage;
            
            if(!string.IsNullOrEmpty(attribute.GroupMessage))
                Debug.Log(attribute.GroupMessage+ ":"+attribute);
        }


        public virtual void OnEnable()
        {
            foreach (var layoutGroupEditor in groupVsEditors.Select(p => p.Value))
            {
                layoutGroupEditor.OnEnable();
            }
        }

        public virtual void OnDisable()
        {
            foreach (var layoutGroupEditor in groupVsEditors.Select(p => p.Value))
            {
                layoutGroupEditor.OnDisable();
            }
        }

        public virtual void OnInspectorGUI()
        {
            DrawGroupMessageIfExist();
        }


        protected virtual void DrawLayoutElementChildren(LayoutElement element)
        {
            foreach (var layoutGroupEditor in element.Groups.Select(g => groupVsEditors[g]))
            {
                layoutGroupEditor.OnInspectorGUI();
            }
        }

        public static LayoutGroupEditor CreateEditor(EditorParams editorParams, LayoutGroupParams layoutGroupParams) =>
            (LayoutGroupEditor)Activator.CreateInstance(GetGroupEditor((Type)layoutGroupParams.GroupElement.LayoutType),
                editorParams, layoutGroupParams);


        public struct EditorParams
        {
            public SerializedObject SerializedObject { get; set; }
            public IReadOnlyDictionary<string, LayoutAttribute[]> RootFieldVsAttributes { get; set; }
        }

        public struct LayoutGroupParams
        {
            public LayoutGroupElement GroupElement { get; set; }
            public int Depth { get; set; }
        }
    }
}