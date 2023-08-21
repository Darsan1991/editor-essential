using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace DGames.Essentials.Editor
{
    public class DashBoardWindow : SplitSelectionWindow
    {
        private string _selectedPath = "General";
        private DashBoardInfoProvider _dashboardProvider;
        private string _tabMessage;
        private bool _needToRefresh;
        private DashBoardInfoProvider DashBoardInfoProvider => _dashboardProvider ??= new DashBoardInfoProvider();

        [MenuItem("MyGames/Dashboard &1",priority=-2)]
        public static void Open()
        {
            var window = GetWindow<DashBoardWindow>();
            window.titleContent = new GUIContent("Dashboard",EditorGUIUtility.IconContent("TextAsset Icon").image);
            window.minSize = new Vector2(600, 400);
            window.Show();
        }

        private void OnEnable()
        {
            Undo.undoRedoPerformed += UndoRedoPerformed;
        }

        private void OnDisable()
        {
            Undo.undoRedoPerformed -= UndoRedoPerformed;
        }

        private void UndoRedoPerformed()
        {
            Refresh();
        }

        public override void CreateGUI()
        {
            base.CreateGUI();

            var strings = new[] { "General" }.Concat(DashBoardInfoProvider.GetInfos().Select(s => s.Item1).Distinct()
                .Except(new[] { "General" }).OrderBy(s => s)).ToArray();
            var toolBarDrawer = new RepeatedToolBarDrawer(strings);
            _selectedPath = "General";
            topBar.Add(new IMGUIContainer(() =>
            {
                EditorGUILayout.BeginVertical(GUI.skin.box);
                EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Refresh", EditorStyles.toolbarButton,GUILayout.MaxWidth(70)))
                {
                    Refresh();
                }
               
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.Space();
                var lastPath = _selectedPath;
                _selectedPath = toolBarDrawer.Draw(_selectedPath);

               

                EditorGUILayout.Space();
                EditorGUILayout.Space();
                EditorGUILayout.EndVertical();

                if (!string.IsNullOrEmpty(_tabMessage))
                {
                    EditorGUILayout.BeginVertical(GUILayout.MaxHeight(10));
                    EditorGUI.HelpBox(EditorGUILayout.GetControlRect(GUILayout.MaxHeight(35),GUILayout.ExpandWidth(true)),_tabMessage,MessageType.Info);
                    // EditorGUILayout.HelpBox(_tabMessage,MessageType.Info,true);
                    EditorGUILayout.EndVertical();
                }
                
                if (lastPath != _selectedPath)
                    OnTabSelectionChanged();
            }));
            
            Refresh();
        }


        private void OnTabSelectionChanged()
        {
            // Debug.Log(nameof(OnTabSelectionChanged)+":"+_selectedPath);
            _needToRefresh = true;
        }

        private void OnGUI()
        {
            if(_needToRefresh)
            {
                _needToRefresh = false;
                Refresh();
            }
        }

        public override void Refresh()
        {
            if (!(_dashboardProvider?.ContainsTab(_selectedPath)??true))
            {
                Debug.Log("Tab Not Contained:"+_selectedPath);
                return;
            }
            _dashboardProvider?.RefreshTab(_selectedPath);
            // _dashboardProvider?.RefreshTab("General");
            _tabMessage = _dashboardProvider?.GetTabMessage(_selectedPath);
            base.Refresh();
        }

        protected override IEnumerable<IMenuItem> GetMenuItems()
        {
            // yield break;
            return DashBoardInfoProvider.GetInfos().FirstOrDefault(i => i.Item1 == _selectedPath).Item2 ?? Array.Empty<IMenuItem>();
        }
    }

    public interface IWindowContent
    {
        event Action<Editor> NotifyChanged;
    }
}