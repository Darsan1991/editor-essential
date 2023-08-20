using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using DGames.Essentials.Attributes;
using DGames.Essentials.EditorHelpers;
using UnityEditor;
using UnityEngine;
using MessageType = UnityEditor.MessageType;
using Object = UnityEngine.Object;

namespace DGames.Essentials.Editor
{
    [CustomMenuEditor(typeof(ObjectTypeMenuItem), true)]
    public class ObjectTypeMenuItemEditor : BaseMenuItemEditor
    {
        private string _path;
        private string _fileName;
        private readonly List<Type> _types;
        public ObjectTypeMenuItem MenuItem => (ObjectTypeMenuItem)Item;

        private Object _tempObject;
        private Editor _tempObjectEditor;
        private int _selectedType;
        private DashboardMessageAttribute _dashboardMessageAttribute;
        public string FullPath => Application.dataPath + _path;

        public override void OnInspectorGUI()
        {
            EditorGUILayout.LabelField(GetTitleWithDashes("CREATE NEW"));
            EditorGUILayout.Space();
            
            if (_dashboardMessageAttribute != null)
            {
                EditorGUI.HelpBox(EditorGUILayout.GetControlRect(GUILayout.MaxHeight(25),GUILayout.ExpandWidth(true)),_dashboardMessageAttribute.Message, MessageType.Info);
                EditorGUILayout.Space();
            }
            
            _path = string.IsNullOrEmpty(_path) ? GetDefaultPath() : _path;
            EditorGUILayout.Space();
            EditorGUI.BeginChangeCheck();
            _selectedType = EditorGUILayout.Popup(_selectedType, _types.Select(t => t.Name).ToArray());
            if (EditorGUI.EndChangeCheck())
            {
                ReCreateTempObject();
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField(GetTitleWithDashes(""));
            if (_tempObjectEditor)
                _tempObjectEditor.OnInspectorGUI();
            EditorGUILayout.LabelField(GetTitleWithDashes(""));

            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            _path = EditorGUILayout.TextField(_path,GUILayout.ExpandWidth(true));
            if (GUILayout.Button(new GUIContent(EditorGUIUtility.IconContent("d_FolderOpened Icon")), GUILayout.MaxWidth(30),GUILayout.MaxHeight(EditorGUIUtility.singleLineHeight)))
            {
                var path = EditorUtility.OpenFolderPanel("Select", FullPath, "");
                var paths = path.Split(Path.DirectorySeparatorChar).ToList();
                var index = paths.IndexOf("Assets");

                if (index >= 0)
                    _path = Path.DirectorySeparatorChar +
                            string.Join(Path.DirectorySeparatorChar, paths.Skip(index + 1));

                Debug.Log(FullPath + ":" + string.Join(',', paths));
            }
            GUILayout.Space(5);
            _fileName = EditorGUILayout.TextField(_fileName,GUILayout.ExpandWidth(true));

            EditorGUI.BeginDisabledGroup(string.IsNullOrEmpty(_fileName));

            if (GUILayout.Button("Create", GUILayout.MaxWidth(60)))
            {
                CreateAsset();
            }

            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndHorizontal();
        }

        public static string GetTitleWithDashes(string title, int countPerSide = 70)
        {
            var dashes = string.Join("", Enumerable.Repeat("-", countPerSide));

            return $"{dashes}{title}{dashes}";
        }

        private void CreateAsset()
        {
            _tempObject.name = _fileName;
            var folderPath = "Assets" + _path;
            var path = folderPath + Path.DirectorySeparatorChar + _fileName + ".asset";

            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }
            
            if (_tempObject is ICreatingScriptable creatingScriptable)
            {
                creatingScriptable.Creating = false;
            }

            AssetDatabase.CreateAsset(_tempObject, path);
            _tempObject = null;
            AssetDatabase.SaveAssets();
            DestroyTempObjectEditor();
            CreateNewTempObject();
            _fileName = "";
            MenuItem.ContentViewer.Refresh();
        }

        public override void OnEnter(IWindow window)
        {
            base.OnEnter(window);
            _dashboardMessageAttribute = MenuItem.Type.GetCustomAttribute<DashboardMessageAttribute>();
            CreateNewTempObject();
        }

        private void ReCreateTempObject()
        {
            if (_tempObject) Object.DestroyImmediate(_tempObject);
            DestroyTempObjectEditor();

            CreateNewTempObject();
        }

        private void CreateNewTempObject()
        {
            if (!_types.Any())
                return;
            _tempObject = ScriptableObject.CreateInstance(_types[_selectedType]);
            if (_tempObject is ICreatingScriptable creatingScriptable)
            {
                creatingScriptable.Creating = true;
            }
            _tempObjectEditor = (Editor)UnityEditor.Editor.CreateEditor(_tempObject);
        }

        public override void OnExit()
        {
            base.OnExit();
            if (_tempObject)
            {
                Object.DestroyImmediate(_tempObject);
            }

            DestroyTempObjectEditor();
            Debug.Log(nameof(OnExit));
        }

        private void DestroyTempObjectEditor()
        {
            if (_tempObjectEditor)
                Object.DestroyImmediate(_tempObjectEditor);
        }

        private string GetDefaultPath()
        {
            var menuItems = MenuItem.Children.OfType<ObjectMenuItem>().ToArray();

            var path = "/";
            if (menuItems.Any())
            {
                var assetPath = AssetDatabase.GetAssetPath(menuItems.First().Object);
                var paths = assetPath.Split(Path.DirectorySeparatorChar).ToList();
                var index = paths.IndexOf("Assets");
                if (index >= 0)
                    path = Path.DirectorySeparatorChar +
                           string.Join(Path.DirectorySeparatorChar, paths.Skip(index + 1).SkipLast(1));
            }

            return path;
        }


        public static IEnumerable<Type> GetAllValidClasses(Type type)
        {
            foreach (var t in AppDomain.CurrentDomain.GetAssemblies()
                         .Select(a => a.GetTypes())
                         .SelectMany(t => t)
                         .Where(t =>
                             (!t.IsAbstract && !t.IsGenericType && type.IsAssignableFrom(t))))
            {
                yield return t;
            }
        }

        public ObjectTypeMenuItemEditor(ObjectTypeMenuItem item) : base(item)
        {
            _types = GetAllValidClasses(item.Type).ToList();
        }
    }
}