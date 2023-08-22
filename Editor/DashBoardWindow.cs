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
        public string SelectedPath
        {
            get => EditorPrefs.GetString($"{nameof(DashBoardWindow)}:{nameof(SelectedPath)}","General");
            private set
            {
                if (SelectedPath == value)
                    return;
                EditorPrefs.SetString($"{nameof(DashBoardWindow)}:{nameof(SelectedPath)}",value);
            }
        }

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
            topBar.Add(new IMGUIContainer(() => { OnGUITopBar(toolBarDrawer); }));
            
            Refresh();
        }

        private void OnGUITopBar(RepeatedToolBarDrawer toolBarDrawer)
        {
            EditorGUILayout.BeginVertical(GUI.skin.box);
            OnGUITopToolBar();
            EditorGUILayout.Space();
            var lastPath = SelectedPath;
            SelectedPath = toolBarDrawer.Draw(SelectedPath);


            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.EndVertical();

            if (!string.IsNullOrEmpty(_tabMessage))
            {
                OnGUITabMessage();
            }

            if (lastPath != SelectedPath)
                OnTabSelectionChanged();
        }

        private void OnGUITopToolBar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Refresh", EditorStyles.toolbarButton, GUILayout.MaxWidth(70)))
            {
                Refresh();
            }

            EditorGUILayout.EndHorizontal();
        }

        private void OnGUITabMessage()
        {
            EditorGUILayout.BeginVertical(GUILayout.MaxHeight(10));
            EditorGUI.HelpBox(EditorGUILayout.GetControlRect(GUILayout.MaxHeight(35), GUILayout.ExpandWidth(true)),
                _tabMessage, MessageType.Info);
            // EditorGUILayout.HelpBox(_tabMessage,MessageType.Info,true);
            EditorGUILayout.EndVertical();
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
            if (!(_dashboardProvider?.ContainsTab(SelectedPath)??true))
            {
                Debug.Log("Tab Not Contained:"+SelectedPath);
                return;
            }
            _dashboardProvider?.RefreshTab(SelectedPath);
            // _dashboardProvider?.RefreshTab("General");
            _tabMessage = _dashboardProvider?.GetTabMessage(SelectedPath);
            base.Refresh();
        }

        protected override IEnumerable<IMenuItem> GetMenuItems()
        {
            // yield break;
            return DashBoardInfoProvider.GetInfos().FirstOrDefault(i => i.Item1 == SelectedPath).Item2 ?? Array.Empty<IMenuItem>();
        }
    }

    public interface IWindowContent
    {
        event Action<Editor> NotifyChanged;
    }
}