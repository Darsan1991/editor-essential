using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DGames.Essentials.Attributes;
using DGames.Presets.Editor;
using UnityEditor;
using UnityEngine;
using EditorGUI = UnityEditor.EditorGUI;

namespace DGames.Essentials.Editor
{


    [CustomPropertyDrawer(typeof(InlineAttribute))]
    public partial class InLineDrawer : PropertyDrawer
    {
        public InlineAttribute Attribute => (InlineAttribute)attribute;

        private bool ConditionSatisfy => Attribute.MinWidth < EditorGUIUtility.currentViewWidth;

        private readonly Dictionary<SerializedPropertyType, float> _typeVsWidth = new();
        private readonly Dictionary<string, BaseConditionAttribute> _typeVsConditions = new();
        private readonly List<string> _complexChildren = new();
        private bool _hasCacheComplexField;
        private bool _hasCacheConditions;
        private ChildrenDrawer _childrenDrawer;

        private ChildrenDrawer CurrentChildrenDrawer => _childrenDrawer ??= new ChildrenDrawer();

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var childrenHeight = ConditionSatisfy && property.isExpanded
                ? property.Copy().GetPropertyWithDefaultChildren().Skip(1)
                    .Where(p => !p.propertyType.IsBuildInSerializableField()).Select(EditorGUI.GetPropertyHeight)
                    .Sum()
                : 0;
            return ConditionSatisfy
                ? (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing * 1)
                  + (Attribute.DrawBox ? EditorGUIUtility.standardVerticalSpacing * 5 : 0)
                  + childrenHeight
                : EditorGUI.GetPropertyHeight(property, label);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (DrawDefaultIfConditionNotSatisfy(position, property)) return;
            position = HandleDrawBox(position, property, label);
            position.height = EditorGUIUtility.singleLineHeight;

            CacheComplexField(property);
            CachePropertyConditions(property);
            CacheWidthForProperty(position, property);

            DrawPrimary(position, property);
        }

        private Rect HandleDrawBox(Rect position, SerializedProperty property, GUIContent label)
        {
            if (Attribute.DrawBox)
            {
                position.height = GetPropertyHeight(property, label);
                position = DrawBackgroundColorBox(position);
            }

            return position;
        }

        private void DrawPrimary(Rect position, SerializedProperty property)
        {
            var startWidth = position.width;
            var startPositionX = position.position.x;
            position = DrawPrimaryFold(position, property, out var expanded);
            
            CurrentChildrenDrawer.MakeReadyToDraw(position.position,20);
            
            foreach (var p in property.GetPropertyWithDefaultChildren().Where(IsVisible))
            {
                if (_complexChildren.All(c => p.name != c))
                {
                    position.width = _typeVsWidth[p.propertyType];
                    position = DrawProperty(position, p);
                   
                }
                else if (expanded)
                {
                    CurrentChildrenDrawer.Draw(new Rect(startPositionX,position.y,startWidth,position.height),p);
                }
            }
        }

        private Rect DrawPrimaryFold(Rect position, SerializedProperty property,out bool expanded)
        {
            var startWidth = position.width;
            if (_complexChildren.Count > 0)
            {
                position.width = 25;
                property.isExpanded = EditorGUI.Foldout(position, property.isExpanded, "");
                expanded = property.isExpanded;
                position.width = startWidth - position.width;
            }
            else
                expanded = false;

            return position;
        }

        private bool DrawDefaultIfConditionNotSatisfy(Rect position, SerializedProperty property)
        {
            if (ConditionSatisfy) return false;
            
            EditorGUI.PropertyField(position, property, new GUIContent(property.displayName), true);
            return true;

        }

        // ReSharper disable once TooManyArguments

        private void CacheComplexField(SerializedProperty property)
        {
            if (_hasCacheComplexField)
                return;
            property = property.Copy();
            _complexChildren.Clear();
            _complexChildren.AddRange(property.GetPropertyWithDefaultChildren().Skip(1)
                .Where(p => !p.propertyType.IsBuildInSerializableField()).Select(p => p.name));
            _hasCacheComplexField = true;
        }

        private bool IsVisible(SerializedProperty property)
        {
            return !_typeVsConditions.ContainsKey(property.name) || _typeVsConditions[property.name] == null ||
                   _typeVsConditions[property.name].IsConditionSatisfy(property) ||
                   _typeVsConditions[property.name].Type != ConditionType.Show;
        }

        private void CachePropertyConditions(SerializedProperty property)
        {
            if (_hasCacheConditions)
            {
                return;
            }

            _typeVsConditions.Clear();
            property = property.Copy();

            foreach (var p in property.GetPropertyWithDefaultChildren())
            {
                var field = p.GetFieldInfo();
                _typeVsConditions.Add(p.name, field?.GetCustomAttribute<BaseConditionAttribute>());
            }

            _hasCacheConditions = true;
        }

        private void CacheWidthForProperty(Rect position, SerializedProperty property)
        {
            _typeVsWidth.Clear();

            var nonBoolPropertyWidth = CalculateNonBoolPropertyWidth(position, property);

            foreach (var type in Enum
                         .GetValues(typeof(SerializedPropertyType))
                         .Cast<SerializedPropertyType>()
                         .Where(p => p != SerializedPropertyType.Boolean))
            {
                _typeVsWidth.Add(type, nonBoolPropertyWidth);
            }

            _typeVsWidth.Add(SerializedPropertyType.Boolean, 20);
        }

        private float CalculateNonBoolPropertyWidth(Rect position, SerializedProperty property)
        {
            var propertyTypes = GetVisiblePropertiesTypesOfProperty(property, _complexChildren.ToArray()).ToList();
            var boolProperties = propertyTypes.Where(p => p == SerializedPropertyType.Boolean).ToList();
            var otherProperties = propertyTypes.Where(p => p != SerializedPropertyType.Boolean).ToList();

            var nonBoolPropertyWidth =
                (Mathf.Min(position.width, EditorGUIUtility.currentViewWidth) 
                 - (boolProperties.Count + otherProperties.Count)*(15)  - boolProperties.Count * 20)
                /
                otherProperties.Count;
            return nonBoolPropertyWidth;
        }

        private IEnumerable<SerializedPropertyType> GetVisiblePropertiesTypesOfProperty(SerializedProperty property,
            params string[] excepts)
        {
            property = property.Copy();
            var exceptList = excepts?.ToList() ?? new List<string>();



            foreach (var p in property.GetPropertyWithDefaultChildren())
            {
                if (!IsVisible(p) || exceptList.Any(c => c == p.name))
                    continue;


                yield return p.propertyType;

            }

        }

        private static Rect DrawProperty(Rect position, SerializedProperty property)
        {
            var content = new GUIContent(property.displayName.Substring(0,Mathf.Min(1,property.displayName.Length)) + "",property.displayName);
            GUI.Label(position,content);
            
            position.position += Vector2.right * 15;
            // position.width -= size.x + 2;
            EditorGUI.PropertyField(position, property, GUIContent.none);


            position.position += Vector2.right * position.width;

            return position;
        }

        private Rect DrawBackgroundColorBox(Rect position)
        {
            position.height -= EditorGUIUtility.standardVerticalSpacing * 2;
            EditorGUI.DrawRect(position, Attribute.Color);
            // GUI.skin.box.Draw(position,true,true,true,true);
            position.position += Vector2.up * (EditorGUIUtility.standardVerticalSpacing * 2);
            position.position += Vector2.right * (EditorGUIUtility.standardVerticalSpacing * 10);
            position.width -= EditorGUIUtility.standardVerticalSpacing * 11;

            return position;
        }
    }
    
    public partial class InLineDrawer{
        public class ChildrenDrawer
        {
            private Vector2 _lastChildrenPosition;
            private float _lastChildrenHeight;

            public void MakeReadyToDraw(Vector2 position,float offset)
            {
                _lastChildrenPosition = position + Vector2.right*offset;
                _lastChildrenHeight = (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing);
            }

            public void Draw(Rect position, 
                 SerializedProperty p)
            {
                position.width -=  _lastChildrenPosition.x - position.x;
                position.position = _lastChildrenPosition + (_lastChildrenHeight) * Vector2.up;
                // position.position = new Vector2(position.position.x + offset, position.position.y);
                EditorGUI.PropertyField(position, p, true);
                _lastChildrenPosition = position.position;
                _lastChildrenHeight = EditorGUI.GetPropertyHeight(p, GUIContent.none, true);

            }
        }
    }
}