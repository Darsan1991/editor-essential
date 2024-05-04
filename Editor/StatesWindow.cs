using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DGames.Essentials.Unity;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace DGames.Essentials.Editor
{
    public class StatesWindow : EditorWindow
    {
        private VisualElement _contentElement;

        private readonly List<SceneEditInfo> _editInfos = new();
        private GUIStyle _titleStyle;
        private Vector2 _scrollPosition;
        private bool _enableScroll;
        private bool _needToRefresh;

        private readonly List<Action> _callAtEndOfDrawing = new();
        private readonly List<(Action,int)> _delayActions = new();

        [MenuItem("Window/States")]
        // ReSharper disable once TooManyArguments
        public static void Open()
        {
            var assetsWindow = GetWindow<StatesWindow>();
            assetsWindow.minSize = new Vector2(assetsWindow.minSize.x, 10);
            assetsWindow.titleContent = new GUIContent("States",
                EditorGUIUtility.IconContent("AnimatorStateTransition Icon").image
            );

            assetsWindow.Show();
            assetsWindow.Refresh();
        }
        
        

        [MenuItem("Window/Add Tab/States")]
        // ReSharper disable once TooManyArguments
        public static void Create()
        {
            var assetsWindow = CreateInstance<StatesWindow>();
            assetsWindow.minSize = new Vector2(assetsWindow.minSize.x, 10);
            assetsWindow.titleContent = new GUIContent("States",
                EditorGUIUtility.IconContent("AnimatorStateTransition Icon").image
            );

            assetsWindow.Show();
        }

        private void OnEnable()
        {
            EditorSceneManager.sceneOpened += EditorSceneManagerOnSceneOpened;
            SceneManager.sceneLoaded += EditorSceneManagerOnSceneLoaded;
        }

        private void EditorSceneManagerOnSceneOpened(Scene scene, OpenSceneMode mode)
        {
            MarkForRefresh();
        }

        public void MarkForRefresh()
        {
            _needToRefresh = true;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= EditorSceneManagerOnSceneLoaded;
            EditorSceneManager.sceneOpened -= EditorSceneManagerOnSceneOpened;
        }


        private void EditorSceneManagerOnSceneLoaded(Scene arg0, LoadSceneMode arg1)
        {
            MarkForRefresh();
        }


        private void CreateGUI()
        {
            rootVisualElement.Add(new IMGUIContainer(() =>
                {
                    EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
                    GUILayout.FlexibleSpace();
                    _enableScroll = GUILayout.Toggle(_enableScroll, "Scroll", EditorStyles.toolbarButton);

                    if (GUILayout.Button("Refresh", EditorStyles.toolbarButton,
                            GUILayout.MaxWidth(EditorStyles.toolbarButton.CalcSize(new GUIContent("Refresh")).x + 10)))
                    {
                        Refresh();
                    }

                    EditorGUILayout.EndHorizontal();
                })
            );
            _contentElement = new VisualElement()
            {
                style =
                {
                    marginTop = 10, marginLeft = 10, marginRight = 10,
                }
            };
            rootVisualElement.Add(_contentElement);
            Refresh();
        }


        [MenuItem("MyGames/CaptureState &s")]
        private static void HandleShortCuts()
        {
            Selection.activeObject = null;
            
            var name = EditorInputDialog.Show( "Question", "Please enter your name", "" );

            CreateNewState(name, AssetDatabase.GUIDFromAssetPath(SceneManager.GetActiveScene().path).ToString());

        }

        public void Refresh()
        {
            if (_contentElement == null)
            {
                return;
            }

            _contentElement.Clear();
            UpdateSceneEditorInfos();

            var stateInfos = _editInfos.Select(info => StateInfos.Default.GetStates(info.Scene).ToArray()).ToArray();


            _contentElement.Add(new IMGUIContainer(() =>
            {
                var scroll = _enableScroll;
                if (scroll)
                    _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);
                DrawContent(stateInfos);
                if (scroll)
                    EditorGUILayout.EndScrollView();
                // HandleShortCuts();
            }));
        }

        private void OnGUI()
        {
            foreach (var action in _callAtEndOfDrawing)
            {
                action();
            }

            _callAtEndOfDrawing.Clear();
            
            HandleDelayAction();

            if (_needToRefresh)
            {
                _needToRefresh = false;
                Refresh();
            }
            
            
        }

        private void HandleDelayAction()
        {
            for (var i = 0; i < _delayActions.Count; i++)
            {
                _delayActions[i] = (_delayActions[i].Item1, _delayActions[i].Item2 - 1);
                // Debug.Log(_delayActions[i].Item2);
                if (_delayActions[i].Item2 <= 0)
                {
                    _delayActions[i].Item1();
                    _delayActions.RemoveAt(i);
                    --i;
                }
            }
        }

        private void UpdateSceneEditorInfos()
        {
            _editInfos.Clear();
            var scene = AssetDatabase.GUIDFromAssetPath(SceneManager.GetActiveScene().path).ToString();
            _editInfos.Add(new SceneEditInfo
            {
                Scene = scene
            });

            foreach (var statesInfo in StateInfos.Default)
            {
                _editInfos.Add(new SceneEditInfo
                {
                    Scene = statesInfo.sceneId
                });
            }
        }

        private void DrawContent(IReadOnlyList<StateInfo[]> stateInfos)
        {
            _titleStyle ??= new GUIStyle(EditorStyles.label) { alignment = TextAnchor.MiddleCenter };
            // _titleStyle ??= EditorStyles.label;
            DrawSceneSectionGUI(stateInfos[0], _editInfos[0]);
            EditorGUILayout.Space(30);

            for (var i = 1; i < _editInfos.Count; i++)
            {
                EditorGUILayout.LabelField(
                    GetTitleWithDashes(AssetDatabase.GUIDToAssetPath(_editInfos[i].Scene)
                        .Split(Path.DirectorySeparatorChar).Last().Split(".").First()), _titleStyle);
                EditorGUILayout.Space();
                DrawSceneSectionGUI(stateInfos[i], _editInfos[i]);
                EditorGUILayout.Space();
                EditorGUILayout.Space();
            }
        }

        public static string GetTitleWithDashes(string title, int countPerSide = 70)
        {
            var dashes = string.Join("", Enumerable.Repeat("-", countPerSide));

            return $"{dashes}{title}{dashes}";
        }

        private void DrawSceneSectionGUI(IReadOnlyList<StateInfo> stateInfos, SceneEditInfo scene)
        {
            DrawItems(scene, stateInfos);

            if (scene.Editing)
            {
                DrawEditingGUI(scene);
            }
        }

        private void DrawItems(SceneEditInfo sceneInfo, IReadOnlyList<StateInfo> stateInfos)
        {
            const string addTitle = "      +      ";
            var currentWidths = stateInfos.Select(info => GUI.skin.box.CalcSize(new GUIContent(info.name)).x + 4 + 16)
                .Append(GUI.skin.box.CalcSize(new GUIContent(addTitle)).x + 2 * 4).ToList();

            var startIndex = 0;
            while (currentWidths.Count > 0)
            {
                var currentStartIndex = startIndex;
                var count = DrawHorizontal(currentWidths, EditorGUIUtility.currentViewWidth - 20, index =>
                {
                    var actualIndex = currentStartIndex + index;
                    if (stateInfos.Count <= actualIndex)
                    {
                        DrawAddTitleItem(sceneInfo, addTitle);
                    }
                    else
                    {
                        var stateInfo = stateInfos[actualIndex];
                        DrawStateInfoItem(sceneInfo, stateInfo);
                    }
                });
                currentWidths = currentWidths.Skip(count).ToList();
                startIndex += count;

                if (count == 0)
                    break;
            }
        }

        private void DrawAddTitleItem(SceneEditInfo sceneInfo, string addTitle)
        {
            EditorGUI.BeginDisabledGroup(SceneManager.GetActiveScene().path !=
                                         AssetDatabase.GUIDToAssetPath(sceneInfo.Scene));
            if (GUILayout.Button(addTitle,
                    GUILayout.Width(GUI.skin.box.CalcSize(new GUIContent(addTitle)).x + 4),
                    GUILayout.Height(EditorGUIUtility.singleLineHeight)))
            {
                OnClickAdd(sceneInfo);
            }

            EditorGUI.EndDisabledGroup();
        }

        private void OnClickAdd(SceneEditInfo sceneInfo)
        {
            sceneInfo.Editing = true;
            sceneInfo.EditingStatePreviousName = null;
            Repaint();
            _delayActions.Add((() =>
            {
                EditorGUI.FocusTextInControl(sceneInfo.Scene + "Edit");
                Focus();
            },4));
        }

        private void DrawStateInfoItem(SceneEditInfo sceneInfo, StateInfo stateInfo)
        {
            if (GUILayout.Button(stateInfo.name,
                    GUILayout.Width(GUI.skin.box.CalcSize(new GUIContent(stateInfo.name)).x + 4),
                    GUILayout.Height(EditorGUIUtility.singleLineHeight)))
            {
                OnClickStateButton(sceneInfo, stateInfo);
            }

            GUILayout.Space(-5);
            if (GUILayout.Button(EditorGUIUtility.IconContent("SettingsIcon"), GUILayout.Width(20),
                    GUILayout.Height(EditorGUIUtility.singleLineHeight)))
            {
                ShowStateOptionsPopUp(sceneInfo, stateInfo);
            }
        }

        private void OnClickStateButton(SceneEditInfo sceneInfo, StateInfo stateInfo)
        {
            if (_callAtEndOfDrawing.Any())
            {
                return;
            }

            _callAtEndOfDrawing.Add(() => { RestoreSceneState(sceneInfo, stateInfo); });
        }

        private static void RestoreSceneState(SceneEditInfo sceneInfo, StateInfo stateInfo)
        {
            var path = AssetDatabase.GUIDToAssetPath(sceneInfo.Scene);
            if (SceneManager.GetActiveScene().path != path)
            {
                EditorSceneManager.SaveModifiedScenesIfUserWantsTo(new[] { SceneManager.GetActiveScene() });
                EditorSceneManager.OpenScene(path);
            }

            stateInfo.sceneInfo?.Restore(SceneView.lastActiveSceneView);

            foreach (var uid in FindObjectsOfType<UID>(true))
            {
                foreach (var goInfo in stateInfo.GameObjectInfos.Where(i => i.id == uid.ID))
                {
                    goInfo.Restore(uid.gameObject);
                }
            }

            if (!Event.current.alt && !Application.isPlaying)
                EditorSceneManager.MarkAllScenesDirty();
        }

        private void ShowStateOptionsPopUp(SceneEditInfo sceneInfo, StateInfo stateInfo)
        {
            var menu = new GenericMenu();
            menu.AddItem(new GUIContent($"Rename"), false, () => { OnPopUpClickRename(sceneInfo, stateInfo); });

            menu.AddItem(new GUIContent($"Update"), false, () => { OnPopupClickUpdate(sceneInfo, stateInfo); });

            menu.AddItem(new GUIContent($"Delete"), false, () => { OnClickDelete(sceneInfo, stateInfo); });

            menu.ShowAsContext();
        }

        private void OnClickDelete(SceneEditInfo sceneInfo, StateInfo stateInfo)
        {
            StateInfos.Default.RemoveState(sceneInfo.Scene, stateInfo.name);
            MarkForRefresh();
        }

        private void OnPopupClickUpdate(SceneEditInfo sceneInfo, StateInfo stateInfo)
        {
            stateInfo.GameObjectInfos = FindObjectsOfType<Transform>(true)
                .Select(t => GameObjectStateInfo.CreateInfo(t.gameObject)).ToArray();
            stateInfo.sceneInfo = SceneView.currentDrawingSceneView != null
                ? SceneInfo.Create(SceneView.currentDrawingSceneView)
                : SceneInfo.Create(SceneView.lastActiveSceneView);
            StateInfos.Default.UpdateState(sceneInfo.Scene, stateInfo, stateInfo.name);
            MarkForRefresh();
        }

        private void OnPopUpClickRename(SceneEditInfo sceneInfo, StateInfo stateInfo)
        {
            sceneInfo.EditingStatePreviousName = stateInfo.name;
            sceneInfo.EditingText = sceneInfo.EditingStatePreviousName;
            sceneInfo.Editing = true;
        }

        private void DrawEditingGUI(SceneEditInfo scene)
        {
            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            GUI.SetNextControlName(scene.Scene + "Edit");
            scene.EditingText = EditorGUILayout.TextField(scene.EditingText);

            if (GUILayout.Button("Cancel", GUILayout.Width(50)))
            {
                OnClickEditingGUICancel(scene);
            }

            EditorGUI.BeginDisabledGroup(string.IsNullOrEmpty(scene.EditingText));

            if (GUILayout.Button("Done", GUILayout.Width(50)))
            {
                OnClickEditingGUIDone(scene);
            }

            if (Event.current is { keyCode: KeyCode.Return, type: EventType.KeyDown or EventType.KeyUp } &&
                !string.IsNullOrEmpty(scene.EditingText))
            {
                OnClickEditingGUIDone(scene);

                Event.current.Use();
            }

            // Debug.Log(Event.current.type);
            if (Event.current is { keyCode: KeyCode.Escape, type: EventType.Ignore })
            {
                OnClickEditingGUICancel(scene);
                Event.current.Use();
            }


            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndHorizontal();
        }

        private static void OnClickEditingGUICancel(SceneEditInfo scene)
        {
            scene.Editing = false;
            scene.EditingStatePreviousName = null;
            scene.EditingText = "";
        }

        private void OnClickEditingGUIDone(SceneEditInfo scene)
        {
            if (string.IsNullOrEmpty(scene.EditingStatePreviousName))
            {
                CreateNewState(scene.EditingText,scene.Scene);
            }
            else
            {
                var stateInfo = StateInfos.Default.GetState(scene.Scene, scene.EditingStatePreviousName);
                stateInfo.name = scene.EditingText;
                StateInfos.Default.UpdateState(scene.Scene, stateInfo, scene.EditingStatePreviousName);
            }

            scene.EditingStatePreviousName = null;
            scene.EditingText = null;
            scene.Editing = false;
            MarkForRefresh();
        }

        private static void CreateNewState(string name,string scene)
        {
            var path = AssetDatabase.GUIDToAssetPath(scene);
            var stateInfo = new StateInfo
            {
                name =
                    name,
                GameObjectInfos = FindObjectsOfType<Transform>(true).Where(t => t.gameObject.scene.path == path)
                    .Select(t => GameObjectStateInfo.CreateInfo(t.gameObject)).ToArray(),
                sceneInfo = SceneInfo.Create(SceneView.lastActiveSceneView)
            };

            StateInfos.Default.AddState(scene, stateInfo);
        }


        private static int DrawHorizontal(IReadOnlyList<float> widths, float availableWidth, Action<int> onDraw)
        {
            EditorGUILayout.BeginHorizontal();
            var leftWidth = availableWidth;
            var count = 0;
            for (var i = 0; i < widths.Count; i++)
            {
                var width = widths[i];
                if (leftWidth >= width)
                {
                    count++;
                    leftWidth -= width;
                    onDraw?.Invoke(i);
                }
                else
                    break;
            }

            EditorGUILayout.EndHorizontal();
            return count;
        }

        public class SceneEditInfo
        {
            public string EditingStatePreviousName { get; set; }
            public string EditingText { get; set; }
            public bool Editing { get; set; }
            public string Scene { get; set; }
        }
    }
}