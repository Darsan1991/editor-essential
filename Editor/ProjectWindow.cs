using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DGames.Essentials.Unity;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using UnityEngine.EventSystems;
using Object = UnityEngine.Object;

namespace DGames.Essentials.Editor
{
    public class ProjectWindow : EditorWindow
    {
        private string _folder;
        private SimpleTreeView _tree;
        [SerializeField] private TreeViewState _treeState;
        private Vector2 _scrollPosition;

        private readonly List<AssetItem> _assets = new();
        private readonly List<AssetItemEventHandler> _handlers = new();


        private readonly List<Action> _callAtEndOfDrawing = new();
        private AssetItemDrawer _gridDrawer;

        private AssetItem _renamingItem;

        private string _rootDirectory = "Assets/Resources";
        private string _currentDirectory = "Assets/Resources";
        private readonly List<ToolBarFolderItem> _toolBarFolderItems = new();
        private bool _editing;
        private string _searchText;
        private bool _onlyAtRoot;
        private readonly List<string> _rootFolders = new();
        private int _frameToBecomeInteractable;


        [MenuItem("MyGames/Filtered Project Window &3")]
        public static void Open()
        {
            var window = GetWindow<ProjectWindow>();
            window.minSize = new Vector2(400, 300);
            window.Show();
        }

        private void OnEnable()
        {
            _treeState = new TreeViewState();
            _tree = new SimpleTreeView(_treeState);
            Debug.Log(_tree);
            _handlers.Clear();
            _handlers.Add(new SelectionEventHandler(this));
            _handlers.Add(new OpenEventHandler(this));
            _handlers.Add(new RenameEventHandler(this));
            _handlers.Add(new DragInvokeEventHandler(this));
            _handlers.Add(new ItemFolderDragAcceptEventHandler(this));
            _handlers.Add(new ImportDragAcceptEventHandler(this));
            _gridDrawer = new GridLayoutAssetItemDrawer();
            _gridDrawer.Repaint = Repaint;
            EditorApplication.projectChanged += EditorApplicationOnProjectChanged;
            Selection.selectionChanged += SelectionChanged;
            Refresh();
        }

        private void SelectionChanged()
        {
            Repaint();
        }


        public void OpenDirectory(string path)
        {
            _currentDirectory = path;
            Refresh();
            AssetDatabase.OpenAsset(AssetDatabase.LoadMainAssetAtPath(path));
        }
        

        private void OnDisable()
        {
            EditorApplication.projectChanged -= EditorApplicationOnProjectChanged;
            Selection.selectionChanged -= SelectionChanged;
            if (_gridDrawer is IDisposable disposable)
            {
                disposable.Dispose();
            }

            _gridDrawer = null;
        }

        private void EditorApplicationOnProjectChanged()
        {
            Refresh();
        }

        private void Refresh()
        {
            RefreshAssets();
            RefreshFolderToolBar();
        }

        private void RefreshFolderToolBar()
        {
            _toolBarFolderItems.Clear();
            var directories = new[] { _rootDirectory.Split(Path.DirectorySeparatorChar).Last() }
                .Concat(_currentDirectory.Replace(_rootDirectory, "")
                    .Split(Path.DirectorySeparatorChar).Where(d => !string.IsNullOrEmpty(d))).ToArray();

            Debug.Log(string.Join(",", directories));
            var currentSubDirectory = "";

            var currentDirectoryRoot =
                _rootFolders.FirstOrDefault(f => _currentDirectory.Contains(f)) ?? _rootDirectory;
            Debug.Log(currentDirectoryRoot);
            for (var i = 0; i < directories.Length; i++)
            {
                // var rect = EditorGUILayout.GetControlRect(false,EditorGUIUtility.singleLineHeight,GUILayout.Width(EditorStyles.label.CalcSize(new GUIContent(directories[i])).x+20));
                currentSubDirectory +=
                    i switch
                    {
                        0 => "",
                        1 => directories[i],
                        _ => Path.DirectorySeparatorChar + directories[i]
                    };

                var targetDirectory = _rootDirectory + (!string.IsNullOrEmpty(currentSubDirectory)
                    ? Path.DirectorySeparatorChar + currentSubDirectory
                    : "");

                if (i == 0 || i > currentDirectoryRoot.Split(Path.DirectorySeparatorChar).Length - 2)
                    _toolBarFolderItems.Add(new ToolBarFolderItem(i == 0 ? "Root" : directories[i], targetDirectory));
            }
        }

        private void RefreshAssets()
        {
            var directory = Application.dataPath
                            + Path.DirectorySeparatorChar + string.Join(Path.DirectorySeparatorChar,
                                _currentDirectory.Split(Path.DirectorySeparatorChar).Skip(1));
            var assetPaths = Directory
                .GetFiles(directory
                    // + Path.DirectorySeparatorChar + "ads-manager"
                    , "*.*",
                    string.IsNullOrEmpty(_searchText) ? SearchOption.TopDirectoryOnly : SearchOption.AllDirectories)
                .Concat(Directory.GetDirectories(directory, "*",
                    string.IsNullOrEmpty(_searchText) ? SearchOption.TopDirectoryOnly : SearchOption.AllDirectories))
                .Where(p =>
                    !p.Contains(".meta") && !p.Split(Path.DirectorySeparatorChar).Last().StartsWith("."))
                .Select(p => GetRightPartOfPath(p, "Assets"))
                .Where(p => !string.IsNullOrEmpty(AssetDatabase.AssetPathToGUID(p)))
                .Concat(AssetDatabase.GetSubFolders(_currentDirectory)).Distinct().OrderBy(p => p).ToArray();
            var searchPaths = _currentDirectory == _rootDirectory || !_onlyAtRoot
                ? AssetDatabase.FindAssets(_searchText, new[] { _currentDirectory })
                    .Select(AssetDatabase.GUIDToAssetPath).ToList()
                : assetPaths.ToList();
            // searchPaths.ForEach(p => Debug.Log("Search:" + p));
            // assetPaths.ToList().ForEach(p => Debug.Log("Assets:" + p));


            // Debug.Log(nameof(RefreshAssets));
            foreach (var assetItem in _assets.Where(a => a.IsCreating))
            {
                DestroyImmediate(assetItem.Object);
            }

            _assets.Clear();
            _assets.AddRange(assetPaths.Where(p => searchPaths.Contains(p)).Select(p => new AssetItem(p)));
            if (_currentDirectory == _rootDirectory)
            {
                _rootFolders.Clear();
                _rootFolders.AddRange(_assets.Where(a => a.Object is DefaultAsset).Select(a => a.Path));
                _rootFolders.ForEach(Debug.Log);
            }

            _frameToBecomeInteractable = 6;
        }

        private void OnGUI()
        {
            EditorGUILayout.BeginHorizontal(GUILayout.ExpandHeight(true));
            var sideBarWidth =
                0;
            // DrawTreeView();
            //
            EditorGUILayout.BeginVertical(GUI.skin.box, GUILayout.ExpandHeight(true));
            GUILayout.Space(-5);
            DrawContentToolBar();
            if (_editing)
                DrawEditingGroup();

            var assetAndRects = OnGUIAssetItems(EditorGUIUtility.currentViewWidth - sideBarWidth).ToArray();
            //
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();


            if (_frameToBecomeInteractable <= 0)
            {
                _handlers.ForEach(h=>h.OnGUI(assetAndRects));
                // HandleDeSelection();
                // HandleDragging(assetAndRects);
                // HandleRenameEvent();
                HandleCreatingEvent();

                if (Event.current is { command: true, keyCode: KeyCode.C, type: EventType.KeyDown })
                {
                    InstantiateItem<BookMarkInfos>();
                    Event.current.Use();
                    Repaint();
                }
            }
            else
            {
                _frameToBecomeInteractable--;
            }
            
            HandleCallAtEndOfDrawing();
            
            Debug.Log(string.Join(',',DragAndDrop.paths));
            Debug.Log(string.Join(',',DragAndDrop.objectReferences.Select(o=>o.name)));

        }

        private void HandleCreatingEvent()
        {
            if (_assets.Any(a => a.IsCreating) && Event.current is { type: EventType.KeyUp, keyCode: KeyCode.Return }
                    or { type: EventType.MouseDown })
            {
                foreach (var assetItem in _assets.Where(a => a.IsCreating).ToArray())
                {
                    assetItem.IsCreating = false;

                    assetItem.RenamingText = GetCorrectedFileName(assetItem.RenamingText, assetItem.Object.GetType());
                    
                    var path = $"{_currentDirectory}{Path.DirectorySeparatorChar}{assetItem.RenamingText}.{GetExtension(assetItem.Object.GetType())}";
                    assetItem.Path = path;
                    
                    
                    AssetDatabase.CreateAsset(assetItem.Object,
                        path);
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                    Selection.activeObject = assetItem.Object;
                }
            }
        }

        private void InstantiateItem<T>()
        {
            var fileName = GetCorrectedFileName(typeof(T).Name,typeof(T));
            if (typeof(ScriptableObject).IsAssignableFrom(typeof(T)))
            {
                var instance = CreateInstance(typeof(T));
                instance.name = fileName;
                Selection.activeObject = instance;
                _assets.Add(new AssetItem($"")
                    { Object = instance, IsCreating = true, RenamingText = instance.name });
                var assetItems = _assets.ToArray();
                _assets.Clear();
                _assets.AddRange(assetItems.OrderBy(a=>a.Name));
            }
        }

        private string GetCorrectedFileName(string baseName,Type type)
        {

            baseName = string.IsNullOrEmpty(baseName) ? type.Name : baseName;
            
            for (var i = 0; i < 100000; i++)
            {
                var currentName = i == 0 ? baseName : $"{baseName}_{i}";
                if (AssetDatabase.LoadMainAssetAtPath(
                        $"{_currentDirectory}{Path.DirectorySeparatorChar}{currentName}.{GetExtension(type)}") ==
                    null)
                {
                    return currentName;
                }
            }
            return "";
        }

        private static string GetExtension(Type type)
        {
            if (typeof(ScriptableObject).IsAssignableFrom(type))
            {
                return "asset";
            }

            if (typeof(GameObject).IsAssignableFrom(type))
            {
                return "prefab";
            }


            return ".asset";
        }

        private void DrawTreeView()
        {
            foreach (var treeViewItem in _tree.GetRows())
            {
                treeViewItem.icon = _tree.IsExpanded(treeViewItem.id)
                    ? (Texture2D)EditorGUIUtility.IconContent("d_FolderOpened Icon").image
                    : (Texture2D)EditorGUIUtility.IconContent("d_Folder Icon").image;
            }


            // Mathf.Min(EditorGUIUtility.currentViewWidth / 3, 200);
            // _tree.OnGUI(EditorGUILayout.GetControlRect(GUILayout.Width(sideBarWidth),
            //     GUILayout.ExpandHeight(true)));
        }

        private void HandleDragging((AssetItem, AssetItemDrawer.ItemWithLabel[])[] assetAndRects)
        {
            // HandleDragStart(assetAndRects);
            // HandleDragUpdate(assetAndRects);
            // HandleDragPerform();
        }

        private void HandleDragStart(IReadOnlyList<(AssetItem, AssetItemDrawer.ItemWithLabel[])> assetAndRects)
        {
            if (Event.current is { type: EventType.MouseDrag })
            {
                foreach (var assetAndRect in assetAndRects)
                {
                    Debug.Log($"{assetAndRect.Item1.Name}:{string.Join("/", assetAndRect.Item2)}");
                }

                var item = assetAndRects.FirstOrDefault(g =>
                    g.Item2.Any(r => r.TotalRect.Contains(Event.current.mousePosition)));
                Debug.Log(item.Item1?.Name + ":" + Event.current.mousePosition);
                var rectIndex = item.Item1 != null
                    ? item.Item2.ToList().FindIndex(r => r.TotalRect.Contains(Event.current.mousePosition))
                    : -1;
                var obj = rectIndex < 0 ? null :
                    rectIndex == 0 ? item.Item1!.Object : item.Item1!.SubAssets.ElementAt(rectIndex - 1);
                if (item.Item1 != null && obj != null)
                {
                    DragAndDrop.PrepareStartDrag();
                    DragAndDrop.objectReferences =
                        Selection.objects.Any(o => o == obj) ? Selection.objects : new[] { obj };
                    DragAndDrop.StartDrag(DragAndDrop.objectReferences.Length > 1
                        ? "<Multiple>"
                        : $"{DragAndDrop.objectReferences.FirstOrDefault()?.name}({DragAndDrop.objectReferences.FirstOrDefault()?.GetType().Name})");
                    Event.current.Use();
                }
            }
        }

        private void HandleDragPerform()
        {
            if (DragAndDrop.objectReferences.Any() && Event.current is { type: EventType.DragPerform })
            {
                Debug.Log("Drag Performed");
                foreach (var item in _assets.Where(a => a.IsHighlighting).ToArray())
                {
                    foreach (var reference in DragAndDrop.objectReferences)
                    {
                        var assetPath = AssetDatabase.GetAssetPath(reference);
                        DragAndDrop.AcceptDrag();
                        AssetDatabase.MoveAsset(assetPath,
                            $"{item.Path}{Path.DirectorySeparatorChar}{assetPath.Split(Path.DirectorySeparatorChar).Last()}");
                    }

                    item.IsHighlighting = false;
                }

                if (DragAndDrop.objectReferences.Any() && _toolBarFolderItems.FirstOrDefault(b => b.Highlighting) is
                        { } folderItem)
                {
                    foreach (var reference in DragAndDrop.objectReferences)
                    {
                        var assetPath = AssetDatabase.GetAssetPath(reference);
                        DragAndDrop.AcceptDrag();
                        AssetDatabase.MoveAsset(assetPath,
                            $"{folderItem.Path}{Path.DirectorySeparatorChar}{assetPath.Split(Path.DirectorySeparatorChar).Last()}");
                    }

                    folderItem.Highlighting = false;
                }
            }
        }

        private void HandleDragUpdate((AssetItem, AssetItemDrawer.ItemWithLabel[])[] assetAndRects)
        {
            if (DragAndDrop.objectReferences.Any() && Event.current is { type: EventType.DragUpdated })
            {
                foreach (var (asset, rects) in assetAndRects.Where(a => a.Item1.Object is DefaultAsset))
                {
                    asset.IsHighlighting = rects.First().TotalRect.Contains(Event.current.mousePosition);
                    Repaint();
                }

                DragAndDrop.visualMode = _assets.Any(a => a.IsHighlighting)
                    ? DragAndDropVisualMode.Move
                    : DragAndDropVisualMode.None;

                if (DragAndDrop.visualMode == DragAndDropVisualMode.None)
                {
                    foreach (var item in _toolBarFolderItems)
                    {
                        item.Highlighting = item.LastRect.Contains(Event.current.mousePosition) &&
                                            DragAndDrop.objectReferences.Select(AssetDatabase.GetAssetPath).Any(p =>
                                            {
                                                Debug.Log(item.Path + ":" + string.Join(Path.DirectorySeparatorChar,
                                                    p.Split(Path.DirectorySeparatorChar).SkipLast(1)));
                                                return item.Path != string.Join(Path.DirectorySeparatorChar,
                                                    p.Split(Path.DirectorySeparatorChar).SkipLast(1));
                                            });
                    }

                    DragAndDrop.visualMode = _toolBarFolderItems.Any(b => b.Highlighting)
                        ? DragAndDropVisualMode.Move
                        : DragAndDropVisualMode.None;
                }
            }
        }


        private void DrawEditingGroup()
        {
            EditorGUILayout.BeginHorizontal();
            var lastLabelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 50;

            _rootDirectory = EditorGUILayout.TextField("Root", _rootDirectory, GUILayout.ExpandWidth(true));

            if (GUILayout.Button(new GUIContent(EditorGUIUtility.IconContent("d_FolderOpened Icon")),
                    GUILayout.MaxWidth(30), GUILayout.MaxHeight(EditorGUIUtility.singleLineHeight)))
            {
                var path = EditorUtility.OpenFolderPanel("Select", _currentDirectory, "");
                var paths = path.Split(Path.DirectorySeparatorChar).ToList();
                var index = paths.IndexOf("Assets");

                if (index >= 0)
                    _rootDirectory =
                        string.Join(Path.DirectorySeparatorChar, paths.Skip(index));

                Debug.Log(_currentDirectory);
            }

            GUILayout.Space(10);
            // _currentDirectory = _currentDirectory.Contains(_rootDirectory) ? _currentDirectory : _rootDirectory;
            _searchText = EditorGUILayout.TextField("Search", _searchText, GUILayout.ExpandWidth(true));

            GUILayout.Space(10);
            EditorGUIUtility.labelWidth = 70;

            // _currentDirectory = _currentDirectory.Contains(_rootDirectory) ? _currentDirectory : _rootDirectory;
            _onlyAtRoot = EditorGUILayout.Toggle("Only At Root", _onlyAtRoot, GUILayout.Width(100));


            if (GUILayout.Button("Done", GUILayout.Width(40)))
            {
                _editing = false;
                _currentDirectory = _rootDirectory;
                _callAtEndOfDrawing.Add(Refresh);
            }

            EditorGUILayout.EndHorizontal();
        }

        private void DrawContentToolBar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);


            DrawContentToolBarFolderItems();

            GUILayout.FlexibleSpace();

            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.LabelField(_searchText, EditorStyles.miniLabel);
            EditorGUI.EndDisabledGroup();
            _editing = GUILayout.Toggle(_editing, "Editing", EditorStyles.toolbarButton);


            EditorGUILayout.EndHorizontal();
        }

        private void DrawContentToolBarFolderItems()
        {
            foreach (var item in _toolBarFolderItems)
            {
                var rect = EditorGUILayout.GetControlRect(false, EditorGUIUtility.singleLineHeight,
                    GUILayout.Width(EditorStyles.label.CalcSize(new GUIContent(item.Name)).x + 20));

                var folderItem = item;
                var lastColor = GUI.contentColor;
                GUI.contentColor = folderItem.Highlighting ? Color.green : lastColor;
                if (GUI.Button(rect, folderItem.Name, EditorStyles.label))
                {
                    _callAtEndOfDrawing.Add(() => { OpenDirectory(folderItem.Path); });
                }

                GUI.contentColor = lastColor;
                folderItem.LastRect = rect;
                if (GUILayout.Button(">", EditorStyles.toolbarButton, GUILayout.Width(16)))
                {
                }
            }
        }

        private void HandleCallAtEndOfDrawing()
        {
            foreach (var action in _callAtEndOfDrawing)
            {
                action();
            }

            if (_callAtEndOfDrawing.Any())
            {
                _callAtEndOfDrawing.Clear();
                Repaint();
            }
        }

        private IEnumerable<(AssetItem, AssetItemDrawer.ItemWithLabel[])> OnGUIAssetItems(float width)
        {
            // EditorGUILayout.BeginVertical(GUI.skin.box, GUILayout.ExpandHeight(true));
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

            // EditorGUILayout.BeginVertical(GUI.skin.box);
Debug.Log(width);
            _gridDrawer.Width = width;
            var assetAndRects = _gridDrawer.DrawAssets(_assets)
                .Select(g=>(g.Item1,g.Item2.Select(r=>
            {
                r.Move(-_scrollPosition+ Vector2.up*(EditorGUIUtility.singleLineHeight+5));
                return r;
            }).ToArray())).ToArray();
            // EditorGUILayout.GetControlRect(GUILayout.Height(1000));
            // EditorGUILayout.EndVertical();
        Debug.Log(_scrollPosition);
            EditorGUILayout.EndScrollView();
            // Debug.Log(GUILayoutUtility.GetLastRect());
            EditorGUILayout.Space(20);
            // EditorGUILayout.EndVertical();
            return assetAndRects;
            // return ArraySegment<(AssetItem, AssetItemDrawer.ItemWithLabel[])>.Empty;
        }

        private static string GetRightPartOfPath(string path, string after)
        {
            var parts = path.Split(Path.DirectorySeparatorChar);
            int afterIndex = Array.IndexOf(parts, after);

            if (afterIndex == -1)
            {
                return null;
            }

            return string.Join(Path.DirectorySeparatorChar.ToString(),
                parts, afterIndex, parts.Length - afterIndex);
        }

        public class ToolBarFolderItem
        {
            public string Path { get; }
            public string Name { get; }
            public bool Highlighting { get; set; }
            public Rect LastRect { get; set; }

            public ToolBarFolderItem(string name, string path)
            {
                Name = name;
                Path = path;
            }
        }

        // ReSharper disable once HollowTypeName
    }
}