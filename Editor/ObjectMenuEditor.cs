using System;
using System.Linq;
using System.Reflection;
using DGames.Essentials.Attributes;
using UnityEditor;
using UnityEngine;
using MessageType = UnityEditor.MessageType;
using Object = UnityEngine.Object;

namespace DGames.Essentials.Editor
{
    [CustomMenuEditor(typeof(ObjectMenuItem), true)]
    public class ObjectMenuEditor : BaseMenuItemEditor
    {
        protected UnityEditor.Editor editor;
        private bool _isRenaming;
        private string _renameText;
        private DashboardMessageAttribute _dashboardMessageAttribute;
        private bool _showPopUp;

        public Editor Editor => editor as Editor;

        public ObjectMenuItem MenuItem => (ObjectMenuItem)Item;

        public override void OnInspectorGUI()
        {
            if (!editor) return;
            if (!MenuItem.Object)
                return;

            DrawDashboardMessageIfCan();
            // EditorGUI.BeginChangeCheck();
            base.OnInspectorGUI();
            


            if (editor)
                editor.OnInspectorGUI();

            MenuItem.ContentViewer.RefreshItem(MenuItem);

            if (_showPopUp)
            {
                _showPopUp = false;
                ShowPopup();
            }
        }

        private void DrawDashboardMessageIfCan()
        {
            if (_dashboardMessageAttribute is { ForInstance: true })
            {
                EditorGUI.HelpBox(EditorGUILayout.GetControlRect(GUILayout.MaxHeight(25), GUILayout.ExpandWidth(true)),
                    _dashboardMessageAttribute.Message, MessageType.Info);
                EditorGUILayout.Space();
            }
        }

        private void DoOnDrawOnTopGUI()
        {
            if (!editor) return;
            if (!MenuItem.Object)
                return;

            EditorGUILayout.BeginHorizontal();
            DrawRenameGroup();

            if (!_isRenaming)
            {
                EditorGUI.BeginDisabledGroup(true);
#pragma warning disable CS0618
                EditorGUILayout.ObjectField(MenuItem.Object, typeof(Object));
#pragma warning restore CS0618
                EditorGUI.EndDisabledGroup();
            }

            if (GUILayout.Button("Select",GUILayout.MaxWidth(50)))
            {
                Selection.activeObject = MenuItem.Object;
            }
            _showPopUp = false;
            if (MenuItem.Options.Any())
            {
                _showPopUp = GUILayout.Button(new GUIContent(EditorGUIUtility.IconContent("SettingsIcon")),
                    GUILayout.MaxWidth(20));
            }

            EditorGUILayout.EndHorizontal();
            var lastColor = GUI.color;
            GUI.color = new Color(GUI.color.r, GUI.color.g, GUI.color.b, 0.3f);
            EditorGUILayout.LabelField(GetTitleWithDashes("",130));
            GUI.color = lastColor;
            // EditorGUILayout.Space();
        }
        
        public static string GetTitleWithDashes(string title, int countPerSide = 70)
        {
            var dashes = string.Join("", Enumerable.Repeat("-", countPerSide));

            return $"{dashes}{title}{dashes}";
        }

        private void DrawRenameGroup()
        {
            if (!_isRenaming) return;
            _renameText = EditorGUILayout.TextField(_renameText);
       

            EditorGUI.BeginDisabledGroup(string.IsNullOrEmpty(_renameText) || _renameText == MenuItem.Object.name);
            if (GUILayout.Button("Rename", GUILayout.MaxWidth(60)))
            {
                MenuItem.Object.name = _renameText;
                _isRenaming = false;
                _renameText = "";
                MenuItem.ContentViewer.RefreshItem(MenuItem);
            }

            EditorGUI.EndDisabledGroup();
            
            if (GUILayout.Button("Cancel", GUILayout.MaxWidth(60)))
            {
                _renameText = "";
                _isRenaming = false;
            }
        }

        private void ShowPopup()
        {
            var menu = new GenericMenu();
            if (MenuItem.Options.Contains(ObjectMenuItem.Option.Rename))
                menu.AddItem(new GUIContent($"Rename"), false, OnClickRename);
            if (MenuItem.Options.Contains(ObjectMenuItem.Option.Duplicate))
                menu.AddItem(new GUIContent($"Duplicate"), false, OnClickDuplicate);

            if (MenuItem.Options.Contains(ObjectMenuItem.Option.Delete))
                menu.AddItem(new GUIContent($"Delete"), false, OnClickDelete);

            menu.ShowAsContext();
        }

        private void OnClickRename()
        {
            _isRenaming = true;
            _renameText = MenuItem.Object.name;
        }

        private void OnClickDuplicate()
        {
            var path = AssetDatabase.GetAssetPath(MenuItem.Object);
            AssetDatabase.CopyAsset(path, $"{path[..path.IndexOf(".asset", StringComparison.Ordinal)]}_1.asset");
            AssetDatabase.SaveAssets();
            MenuItem.ContentViewer.Refresh();
        }

        private void OnClickDelete()
        {
            var asset = AssetDatabase.LoadMainAssetAtPath(AssetDatabase.GetAssetPath(MenuItem.Object));
            if (asset == MenuItem.Object)
            {
                AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(MenuItem.Object));
                AssetDatabase.SaveAssets();
            }
            else
            {
                AssetDatabase.RemoveObjectFromAsset(MenuItem.Object);
                AssetDatabase.SaveAssetIfDirty(asset);
                Object.DestroyImmediate(MenuItem.Object);
                AssetDatabase.Refresh();
            }

            MenuItem.ContentViewer.Refresh();
        }


        public ObjectMenuEditor(ObjectMenuItem item) : base(item)
        {
        }

        public override void OnEnter(IWindow window)
        {
            base.OnEnter(window);
            editor = UnityEditor.Editor.CreateEditor(MenuItem.Object);
            Editor.DoDrawOnTop = DoOnDrawOnTopGUI;
            if (editor is IWindowContent content)
            {
                content.NotifyChanged += ContentOnNotifyChanged;
            }

            _dashboardMessageAttribute = editor.target.GetType().GetCustomAttribute<DashboardMessageAttribute>();
        }

     

        private void ContentOnNotifyChanged(Editor e)
        {
            MenuItem.ContentViewer.Refresh();
        }

        public override void OnExit()
        {
            base.OnExit();
            Object.DestroyImmediate(editor);
            editor = null;
        }
    }
}