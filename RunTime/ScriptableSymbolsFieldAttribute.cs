using System;
using System.Collections.Generic;
using System.Linq;
using DGames.Essentials.Unity;
using UnityEngine;
using BuildTargetGroup = UnityEditor.BuildTargetGroup;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace DGames.Essentials.Attributes
{
    public class ScriptableSymbolsToggleAttribute : ScriptableSymbolsFieldAttribute
    {
        public string Symbol { get; }

        public ScriptableSymbolsToggleAttribute(string path, string symbol, Unity.BuildTargetGroup platform) : base(path, platform)
        {
            Symbol = symbol;
        }

#if UNITY_EDITOR
        public override bool IsMissing(SerializedProperty property, params SerializedProperty[] otherBool)
        {
            return Platforms.Any(p => ScriptingDefineSymbolHandler.HaveBuildSymbol(p, Symbol) !=
                                      property.boolValue);
        }

        public override void UpdateSymbols(SerializedProperty property, params SerializedProperty[] otherBool)
        {
            foreach (var platform in Platforms)
            {
                ScriptingDefineSymbolHandler.HandleScriptingSymbol(platform, property.boolValue, Symbol);
            }
        }
#endif
    }

    public class ScriptableSymbolsEnumAttribute : ScriptableSymbolsFieldAttribute
    {
        public Type EnumType { get; }
        public string SymbolPrefix { get; }

        // ReSharper disable once TooManyDependencies
        public ScriptableSymbolsEnumAttribute(string path, Type enumType, string symbolPrefix,
            Unity.BuildTargetGroup platform, params string[] otherBool) : base(path, platform, otherBool)
        {
            EnumType = enumType;
            SymbolPrefix = symbolPrefix;
        }

#if UNITY_EDITOR
        public override bool IsMissing(SerializedProperty property, params SerializedProperty[] otherBool)
        {
            var sdkType =
                property.enumValueIndex;
            var enable = otherBool.All(b => b.boolValue);
            foreach (var value in Enum.GetValues(EnumType))
            {
                if (enable &&
                    Platforms.Any(p => ScriptingDefineSymbolHandler.HaveBuildSymbol(p,
                        $"{SymbolPrefix}{value}") != ((int)value == sdkType))
                    ||
                    !enable && Platforms.Any(p =>
                        ScriptingDefineSymbolHandler.HaveBuildSymbol(p, $"{SymbolPrefix}{value}"))
                   )
                    return true;
            }

            return false;
        }


        public override void UpdateSymbols(SerializedProperty property, params SerializedProperty[] otherBool)
        {
            foreach (var value in Enum.GetValues(EnumType))
            {
                foreach (var platform in Platforms)
                {
                    ScriptingDefineSymbolHandler.HandleScriptingSymbol(platform,
                        otherBool.All(b => b.boolValue) && (int)value == property.enumValueIndex,
                        $"{SymbolPrefix}{value}");
                }
            }
        }
#endif
    }

    public abstract class ScriptableSymbolsFieldAttribute : PropertyAttribute
    {
        public string Path { get; }
        public BuildTargetGroup[] Platforms { get; }
        public string[] OtherBool { get; }


        protected ScriptableSymbolsFieldAttribute(string path, Unity.BuildTargetGroup platform, params string[] otherBool)
        {
            Path = path;

            OtherBool = otherBool?.ToArray() ?? Array.Empty<string>();
#if UNITY_EDITOR
            Platforms = GetPlatforms(platform).ToArray();
#endif
        }


#if UNITY_EDITOR
        private IEnumerable<BuildTargetGroup> GetPlatforms(Unity.BuildTargetGroup platform)
        {
            foreach (int value in Enum.GetValues(typeof(Essentials.Unity.BuildTargetGroup)))
            {
                if ((value & (int)platform) > 0)
                {
                    yield return ((Essentials.Unity.BuildTargetGroup)value).ToUnityGroup();
                }
            }
        }

        public abstract bool IsMissing(SerializedProperty fieldProperty, params SerializedProperty[] otherBool);

        public abstract void UpdateSymbols(SerializedProperty fieldProperty, params SerializedProperty[] otherBool);

#endif
    }

#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(ScriptableSymbolsFieldAttribute), true)]
    public class ScriptableSymbolsToggleDrawer : PropertyDrawer
    {
        private bool _isMissing;
        public ScriptableSymbolsFieldAttribute Attribute => (ScriptableSymbolsFieldAttribute)attribute;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property, label) + (_isMissing ? EditorGUIUtility.singleLineHeight : 0);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var fieldProperty = string.IsNullOrEmpty(Attribute.Path)
                ? property
                : property.FindPropertyRelative(Attribute.Path);
            var otherBool = Attribute.OtherBool.Select(property.FindPropertyRelative).ToArray();
            position.height = EditorGUIUtility.singleLineHeight;
            EditorGUI.BeginChangeCheck();
            EditorGUI.PropertyField(position, property, label);
            if (EditorGUI.EndChangeCheck())
            {
                Attribute.UpdateSymbols(fieldProperty, otherBool);
            }

            position.y += EditorGUI.GetPropertyHeight(property, label);
            _isMissing = Attribute.IsMissing(fieldProperty, otherBool);


            if (_isMissing && GUI.Button(position, "Fix Symbols"))
            {
                Attribute.UpdateSymbols(fieldProperty, otherBool);
            }
        }
    }

    public static class ScriptingDefineSymbolHandler
    {
        public static bool HaveBuildSymbol(BuildTargetGroup group, string symbol)
        {
            var scriptingDefineSymbolsForGroup = PlayerSettings.GetScriptingDefineSymbolsForGroup(group);
            var strings = scriptingDefineSymbolsForGroup.Split(';').ToList();

            return strings.Contains(symbol);
        }

        // ReSharper disable once FlagArgument
        public static void AddBuildSymbol(BuildTargetGroup group, string symbol)
        {
            if (HaveBuildSymbol(group, symbol))
                return;
            var scriptingDefineSymbolsForGroup = PlayerSettings.GetScriptingDefineSymbolsForGroup(group);
            var strings = scriptingDefineSymbolsForGroup.Split(';').ToList();
            strings.Add(symbol);
            var str = "";
            foreach (var s in strings)
            {
                str += s + ";";
            }

            PlayerSettings.SetScriptingDefineSymbolsForGroup(group, str);
        }

        // ReSharper disable once FlagArgument
        public static void RemoveBuildSymbol(BuildTargetGroup group, string symbol)
        {
            if (!HaveBuildSymbol(group, symbol))
                return;
            var scriptingDefineSymbolsForGroup = PlayerSettings.GetScriptingDefineSymbolsForGroup(group);
            var strings = scriptingDefineSymbolsForGroup.Split(';').ToList();
            strings.Remove(symbol);
            var str = "";
            foreach (var s in strings)
            {
                str += s + ";";
            }

            PlayerSettings.SetScriptingDefineSymbolsForGroup(group, str);
        }

        // ReSharper disable once FlagArgument
        public static void HandleScriptingSymbol(BuildTargetGroup buildTargetGroup, bool enable, string key)
        {
            var scriptingDefineSymbolsForGroup = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTargetGroup);
            var strings = scriptingDefineSymbolsForGroup.Split(';').ToList();

            if (enable)
            {
                strings.Add(key);
            }
            else
            {
                strings.Remove(key);
            }


            PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTargetGroup, string.Join(";", strings.Distinct()));
        }
    }
#endif
}

namespace DGames.Essentials.Unity
{
}