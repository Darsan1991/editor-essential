using System;
using System.Collections.Generic;
using System.Linq;
using DGames.Essentials.Unity;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;
using PopupWindow = UnityEditor.PopupWindow;

namespace DGames.Essentials.Editor
{
    public sealed partial class BookMarksWindow : EditorWindow
    {
        private static readonly List<BookMarksWindow> _currentWindows = new();
        private VisualElement _contentElement;

        private GUIStyle _titleStyle;
        private Vector2 _scrollPosition;
        private bool _enableScroll;
        private bool _needToRefresh;

        private readonly List<Action> _callAtEndOfDrawing = new();

        [MenuItem("Window/BookMarks &#b")]
        // ReSharper disable once TooManyArguments
        public static void Open()
        {
            var window = GetWindow<BookMarksWindow>();
            MakeReadyAndShow(window);
        }

        private static void MakeReadyAndShow(BookMarksWindow window)
        {
            window.minSize = new Vector2(window.minSize.x, 10);
            window.titleContent = new GUIContent("BookMarks",
                EditorGUIUtility.IconContent("d_Favorite").image
            );

            window.Show();
            window.Refresh();
        }

        [MenuItem("Window/Add Tab/BookMarks")]
        // ReSharper disable once TooManyArguments
        public static void Create()
        {
            var window = CreateInstance<BookMarksWindow>();
            MakeReadyAndShow(window);
        }

        private void OnEnable()
        {
            EditorSceneManager.sceneOpened += EditorSceneManagerOnSceneOpened;
            SceneManager.sceneLoaded += EditorSceneManagerOnSceneLoaded;
            _currentWindows.Add(this);
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
            _currentWindows.Remove(this);
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

        private void HandleShortCuts()
        {
            if (Event.current is { keyCode: KeyCode.C, command: true, shift: true, type: EventType.KeyDown })
            {
                Event.current.Use();
            }
        }

        public void Refresh()
        {
            if (_contentElement == null)
            {
                return;
            }

            _contentElement.Clear();
            // UpdateSceneEditorInfos();

            if (!BookMarkInfos.Default.HasSectionFor(SceneManager.GetActiveScene()))
            {
                BookMarkInfos.Default.AddSectionFor(SceneManager.GetActiveScene());
            }

            var current = BookMarkInfos.Default.Sections.OfType<BookMarkInfos.SceneSection>().First(s =>
                s.SceneID == AssetDatabase.GUIDFromAssetPath(SceneManager.GetActiveScene().path).ToString());

            var sections = new[] { current }.Concat(BookMarkInfos.Default.Sections).ToArray();

            _contentElement.Add(new IMGUIContainer(() => { OnContentElementGUI(sections); }));
        }

        private void OnContentElementGUI(BookMarkInfos.Section[] sections)
        {
            var scroll = _enableScroll;
            if (scroll)
                _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);
            DrawContent(sections);
            if (scroll)
                EditorGUILayout.EndScrollView();
            HandleShortCuts();
        }

        private void OnGUI()
        {
            foreach (var action in _callAtEndOfDrawing)
            {
                action();
            }

            _callAtEndOfDrawing.Clear();

            if (_needToRefresh)
            {
                _needToRefresh = false;
                Refresh();
            }
        }

        private void DrawContent(IReadOnlyList<BookMarkInfos.Section> sections)
        {
            _titleStyle ??= new GUIStyle(EditorStyles.label) { alignment = TextAnchor.MiddleCenter };
            // _titleStyle ??= EditorStyles.label;
            if (!sections.Any())
                return;
            DrawSceneSectionGUI(sections[0], true);
            EditorGUILayout.Space(30);

            for (var i = 1; i < sections.Count; i++)
            {
                var section = sections[i];
                if (!section.Any() && section is not BookMarkInfos.AssetSection)
                {
                    continue;
                }

                DrawSectionWithTitle(section);
            }
        }

        private void DrawSectionWithTitle(BookMarkInfos.Section section)
        {
            EditorGUILayout.LabelField(
                GetTitleWithDashes(section.Name), _titleStyle);
            EditorGUILayout.Space();
            DrawSceneSectionGUI(section, section is BookMarkInfos.AssetSection);
            EditorGUILayout.Space();
            EditorGUILayout.Space();
        }

        public static string GetTitleWithDashes(string title, int countPerSide = 70)
        {
            var dashes = string.Join("", Enumerable.Repeat("-", countPerSide));

            return $"{dashes}{title}{dashes}";
        }

        private void DrawSceneSectionGUI(BookMarkInfos.Section section, bool allowAdd)
        {
            DrawItems(section, allowAdd);
        }

        // ReSharper disable once MethodTooLong
        private void DrawItems(BookMarkInfos.Section section, bool allowAdd)
        {
            const string addTitle = "      +      ";
            var bookMarkInfos = section.ToArray();
            var currentWidths = bookMarkInfos
                .Select(info => GUI.skin.box.CalcSize(new GUIContent(info.Name)).x + 4 + 16)
                .Concat(allowAdd
                    ? new[] { GUI.skin.box.CalcSize(new GUIContent(addTitle)).x + 2 * 4 }
                    : Array.Empty<float>()).ToList();
            var startIndex = 0;
            while (currentWidths.Count > 0)
            {
                var currentStartIndex = startIndex;
                var count = DrawHorizontal(currentWidths, EditorGUIUtility.currentViewWidth - 20, index =>
                {
                    var actualIndex = currentStartIndex + index;
                    if (bookMarkInfos.Length <= actualIndex)
                    {
                        DrawAddTitleItem(section, addTitle);
                    }
                    else
                    {
                        var bookMarkInfo = bookMarkInfos[actualIndex];
                        DrawItem(bookMarkInfo);
                    }
                });
                currentWidths = currentWidths.Skip(count).ToList();
                startIndex += count;

                if (count == 0)
                    break;
            }
        }

        private void DrawAddTitleItem(BookMarkInfos.Section section, string addTitle)
        {
            EditorGUI.BeginDisabledGroup(!(Selection.activeObject && section.CanAdd(Selection.activeObject)));
            if (GUILayout.Button(addTitle,
                    GUILayout.Width(GUI.skin.box.CalcSize(new GUIContent(addTitle)).x + 4),
                    GUILayout.Height(EditorGUIUtility.singleLineHeight)))
            {
                OnClickAdd(section);
            }

            EditorGUI.EndDisabledGroup();
        }

        private static void OnClickAdd(BookMarkInfos.Section section)
        {
            BookMarkInfos.Default.AddInSection(Selection.activeObject, section);
            EditorApplication.RepaintHierarchyWindow();
        }

        private void DrawItem(BookMarkInfos.BookMarkInfo bookMarkInfo)
        {
            if (GUILayout.Button(bookMarkInfo.Name,
                    GUILayout.Width(GUI.skin.box.CalcSize(new GUIContent(bookMarkInfo.Name)).x + 4),
                    GUILayout.Height(EditorGUIUtility.singleLineHeight)))
            {
                if (Event.current.button == 0)
                {
                    OnClickStateButton(bookMarkInfo);
                }
                else
                {
                    ShowStateOptionsPopUp(bookMarkInfo);
                }
            }


            GUILayout.Space(-5);
            if (GUILayout.Button(EditorGUIUtility.IconContent("SettingsIcon"), GUILayout.Width(20),
                    GUILayout.Height(EditorGUIUtility.singleLineHeight)))
            {
                ShowStateOptionsPopUp(bookMarkInfo);
            }
        }
        
        public static void MenuShowUtility(Rect rect,Vector2 windowSize)
        {
            var window = GetWindow<EditorInputDialog>(true);
            window.ShowAsDropDown(rect,windowSize);
        }

        private void OnClickStateButton(BookMarkInfos.BookMarkInfo bookMarkInfo)
        {
            if (_callAtEndOfDrawing.Any())
            {
                return;
            }

            _callAtEndOfDrawing.Add(bookMarkInfo.Action);
        }


        private void ShowStateOptionsPopUp(BookMarkInfos.BookMarkInfo bookMarkInfo)
        {
            var menu = new GenericMenu();


            menu.AddItem(new GUIContent($"Update"), false, () =>
            {
                OnClickUpdate(bookMarkInfo);
                EditorApplication.RepaintHierarchyWindow();
            });
            menu.AddItem(new GUIContent($"Remove"), false, () =>
            {
                OnClickDelete(bookMarkInfo);
                EditorApplication.RepaintHierarchyWindow();
            });

            menu.ShowAsContext();
        }

        private void OnClickUpdate(BookMarkInfos.BookMarkInfo bookMarkInfo)
        {
            BookMarkInfos.Default.UpdateBookMark(bookMarkInfo.Id);
        }

        private void OnClickDelete(BookMarkInfos.BookMarkInfo bookMarkInfo)
        {
            BookMarkInfos.Default.RemoveBookMark(bookMarkInfo.Id);
            MarkForRefresh();
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
    }

    public partial class BookMarksWindow
    {
        [MenuItem("Assets/BookMark")]
        public static void BookMarkAssetMenu()
        {
            BookMarkSelected();
        }

        [MenuItem("Assets/Un BookMark")]
        public static void UnBookMarkAssetMenu()
        {
            UnBookMarkSelected();
        }

        [MenuItem("GameObject/BookMark")]
        public static void BookMarkHierarchyMenu()
        {
            BookMarkSelected();
        }

        [MenuItem("GameObject/Un BookMark")]
        public static void UnBookMarkHierarchyMenu()
        {
            UnBookMarkSelected();
        }


        private static void BookMarkSelected()
        {
            BookMarkInfos.Default.AddBookMark(Selection.activeObject);
            _currentWindows.ForEach(w =>
            {
                w.MarkForRefresh();
                w.Repaint();
            });
        }

        private static void UnBookMarkSelected()
        {
            BookMarkInfos.Default.RemoveBookMark(Selection.activeObject);
            _currentWindows.ForEach(w =>
            {
                w.MarkForRefresh();
                w.Repaint();
            });
        }


        [MenuItem("Assets/Un BookMark", true)]
        private static bool UnBookMarkAssetMenuValidation()
        {
            return UnBookMarkValidation();
        }


        [MenuItem("Assets/BookMark", true)]
        private static bool BookMarkAssetMenuValidation()
        {
            return BookMarkValidation();
        }

        [MenuItem("MyGames/Toggle BookMark &b")]
        public static void ToggleBookMark()
        {
            // Debug.Log(nameof(ToggleBookMark));
            if (!Selection.activeObject)
            {
                return;
            }

            if (BookMarkValidation())
            {
                BookMarkSelected();
            }
            else if (UnBookMarkValidation())
            {
                UnBookMarkSelected();
            }

            EditorApplication.RepaintHierarchyWindow();
        }

        [MenuItem("GameObject/Un BookMark", true, priority = -15)]
        private static bool UnBookMarkHierarchyMenuValidation()
        {
            return UnBookMarkValidation();
        }


        [MenuItem("GameObject/BookMark", true, priority = -15)]
        private static bool BookMarkHierarchyMenuValidation()
        {
            return BookMarkValidation();
        }


        private static bool UnBookMarkValidation()
        {
            return BookMarkInfos.Default.ContainsBookMark(Selection.activeObject);
        }


        private static bool BookMarkValidation()
        {
            return !BookMarkInfos.Default.ContainsBookMark(Selection.activeObject);
        }


#if UNITY_EDITOR
        [InitializeOnLoadMethod]
        public static void OnInitializedLoad()
        {
            if (BookMarkInfos.Default)
                HierarchyIcons.RegisterIconProvider(BookMarkInfos.Default);
        }
#endif
    }
}