using System.Linq;
using System.Reflection;
using DGames.Essentials.Attributes;
using UnityEditor;
using UnityEngine;

namespace DGames.Essentials.Editor
{
    [CustomEditor(typeof(Object),true)]
     [CanEditMultipleObjects]
     public class Editor : UnityEditor.Editor
     {
         private ButtonsDrawer _buttonsDrawer;
         private ScriptLogo _scriptLogo;
         private FooterLogo _footerLogo;
         private GUIStyle _footerStyle;

         private void OnEnable()
         {
             _buttonsDrawer = new ButtonsDrawer(target);

             var type = target.GetType();
             _scriptLogo = type.GetCustomAttribute<ScriptLogo>();
             _footerLogo = type.GetCustomAttribute<FooterLogo>();
            
         }

         

         public override void OnInspectorGUI()
         {
             // base.OnInspectorGUI();
             serializedObject.Update();
             var serializedProperty = serializedObject.GetIterator();
             if (serializedProperty.NextVisible(true))
             {
                 
                 EditorGUI.BeginDisabledGroup(true);
                 EditorGUILayout.PropertyField(serializedProperty);
                 EditorGUI.EndDisabledGroup();


                 DrawScriptLogoIfCan();

                 while(serializedProperty.NextVisible(false))
                 {
                     EditorGUILayout.PropertyField(serializedProperty, true);
                 }

             }

             serializedObject.ApplyModifiedProperties();
             DrawOtherDefaults();
             
             DrawFooterIfCan();
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
             _buttonsDrawer.DrawButtons(targets);
         }
         
         public static string GetTitleWithSymbols(string title, int countPerSide = 70,string symbol="#")
         {
             var dashes = string.Join("", Enumerable.Repeat(symbol, countPerSide));

             return $"{dashes}{title}{dashes}";
         }
     }
}