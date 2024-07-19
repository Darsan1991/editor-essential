using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DGames.Essentials.Attributes;
using DGames.Essentials.Base;
using UnityEditor;
using UnityEngine;
using MessageType = UnityEditor.MessageType;
using Object = UnityEngine.Object;

namespace DGames.Essentials.Editor
{
   [CustomEditor(typeof(Object), true)]
    [CanEditMultipleObjects]
    public class Editor : UnityEditor.Editor
    {
        private ButtonsDrawer _buttonsDrawer;
        private ScriptLogo _scriptLogo;
        private FooterLogo _footerLogo;
        private GUIStyle _footerStyle;
        // private TabEditorLayout _tabEditorLayout;
        private ObjectMessageAttribute _objectMessage;
        private TypeMessageAttribute _typeMessage;
        private HideScriptField _hideScriptField;
        private LayoutDrawer _layoutDrawer;
        private FieldInfo[] _serializeInterfaces;

        public Action DoDrawOnTop { get; set; }

        private void OnEnable()
        {
            if (target == null || !serializedObject.targetObject)
                return;
            _buttonsDrawer = new ButtonsDrawer(target);

            var type = target.GetType();
            _scriptLogo = type.GetCustomAttribute<ScriptLogo>();
            _footerLogo = type.GetCustomAttribute<FooterLogo>();
            _objectMessage = type.GetCustomAttribute<ObjectMessageAttribute>();
            _typeMessage = type.GetCustomAttribute<TypeMessageAttribute>();
            _hideScriptField = type.GetCustomAttribute<HideScriptField>();
            _serializeInterfaces = SerializeInterfaceField.GetAllSerializedInterfaceFields(type).ToArray();
            SerializedObject so = null;
            try
            {
                so = serializedObject;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            if (so != null)
            {
                // _tabEditorLayout = new TabEditorLayout(so);
                _layoutDrawer = new LayoutDrawer(so);
                _layoutDrawer.OnEnable();
            }
        }


        public override void OnInspectorGUI()
        {
            if (target == null || !serializedObject.targetObject)
                return;
            if (_typeMessage != null && !string.IsNullOrEmpty(_typeMessage.Message))
            {
                EditorGUI.HelpBox(EditorGUILayout.GetControlRect(GUILayout.MaxHeight(25), GUILayout.ExpandWidth(true)),
                    _typeMessage.Message, MessageType.Info);
                EditorGUILayout.Space();
            }
            
            if (_objectMessage != null && !string.IsNullOrEmpty(serializedObject.FindProperty(_objectMessage.PropertyPath).stringValue))
            {
                EditorGUI.HelpBox(EditorGUILayout.GetControlRect(GUILayout.MaxHeight(25), GUILayout.ExpandWidth(true)),
                    serializedObject.FindProperty(_objectMessage.PropertyPath).stringValue, MessageType.Info);
                EditorGUILayout.Space();
            }

            DoDrawOnTop?.Invoke();
            // base.OnInspectorGUI();
            OnContentGUI();
            DrawOtherDefaults();

            DrawFooterIfCan();
        }

        protected virtual void OnContentGUI()
        {
            serializedObject.Update();
            var serializedProperty = serializedObject.GetIterator();
            if (serializedProperty.NextVisible(true))
            {
                if (_hideScriptField == null)
                {
                    EditorGUI.BeginDisabledGroup(true);
                    EditorGUILayout.PropertyField(serializedProperty);
                    EditorGUI.EndDisabledGroup();
                }
                DrawScriptLogoIfCan();

                while (serializedProperty.NextVisible(false))
                {
                    if (_layoutDrawer?.IsChild(serializedProperty.name) ?? false)
                    {
                        continue;
                    }

                    DrawPropertyField(serializedProperty);
                }
            }

            _layoutDrawer?.OnInspectorGUI();
            
            if (_serializeInterfaces != null)
            {
                EditorGUI.BeginChangeCheck();
                foreach (var fieldInfo in _serializeInterfaces)
                {
                    var fName = fieldInfo.Name.Replace("_","");
                    var value = EditorGUILayout.ObjectField(char.ToUpper(fName.First())+ fName[1..],fieldInfo.GetValue(target) as Object, fieldInfo.FieldType, true);

                    if ((Object)fieldInfo.GetValue(target) != value)
                    {
                        fieldInfo.SetValue(target, value);
                    }
                }
                // if (EditorGUI.EndChangeCheck())
                // {
                //     var property = serializedObject.FindProperty("fieldVsObjects");
                //     property.arraySize +=1 ;
                // }
            }
            
            serializedObject.ApplyModifiedProperties();
        }

        private static void DrawPropertyField(SerializedProperty serializedProperty)
        {
            EditorGUILayout.PropertyField(serializedProperty, true);
        }

        private void DrawFooterIfCan()
        {
            if (_footerLogo == null) return;

            _footerStyle ??= new GUIStyle(GUI.skin.GetStyle("Label")) { alignment = TextAnchor.MiddleCenter };
            var lastColor = GUI.color;
            GUI.color = _footerLogo.Color;

            DrawFooter();
            GUI.color = lastColor;
        }

        private void DrawFooter()
        {
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.LabelField(GetTitleWithSymbols("", 100, "-"));
            EditorGUILayout.Space();
            var topRow = GetTitleWithSymbols("", 18).ToUpper();
            EditorGUILayout.LabelField(topRow, _footerStyle, GUILayout.ExpandWidth(true));

            var nonTopRow = $"#{GetTitleWithSymbols(_footerLogo.LogoName, 34, " ")}#";
            EditorGUILayout.LabelField(GetTitleWithSymbols(nonTopRow, 40), _footerStyle, GUILayout.ExpandWidth(true));
            EditorGUILayout.LabelField(topRow, _footerStyle, GUILayout.ExpandWidth(true));


            EditorGUILayout.Space();
            EditorGUILayout.LabelField(GetTitleWithSymbols("", 100, "-"));
            EditorGUILayout.Space();
        }

        private void DrawScriptLogoIfCan()
        {
            if (_scriptLogo == null) return;
            EditorGUI.BeginDisabledGroup(true);
            var lastColor = GUI.color;
            GUI.color = Color.yellow;
            EditorGUI.LabelField(
                new Rect(60, EditorGUIUtility.singleLineHeight * 0.3f, 120, EditorGUIUtility.singleLineHeight),
                $"-  {_scriptLogo.LogoName}");
            GUI.color = lastColor;
            EditorGUI.EndDisabledGroup();
        }

        protected virtual void DrawOtherDefaults()
        {
            _buttonsDrawer?.DrawButtons(targets);
        }

        public static string GetTitleWithSymbols(string title, int countPerSide = 70, string symbol = "#")
        {
            var dashes = string.Join("", Enumerable.Repeat(symbol, countPerSide));

            return $"{dashes}{title}{dashes}";
        }

        private void OnDisable()
        {
            _layoutDrawer?.OnDisable();
        }
    }
    
}