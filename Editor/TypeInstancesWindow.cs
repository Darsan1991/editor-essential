using System;
using System.Collections.Generic;
using System.Linq;
using DGames.Essentials.Unity;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace DGames.Essentials.Editor
{
    public sealed partial class TypeInstancesWindow : EditorWindow
    {
        private static readonly List<TypeInstancesWindow> _instances = new();
        private VisualElement _contentElement;

       [SerializeField] private string _typeName = "";
        [SerializeField]private string _assemblyName = "";
        private bool _editing;
        private bool _needToRefresh;

        [MenuItem("Window/Type Instances/Default")]
        public static void Open()
        {
            var window = GetWindow<TypeInstancesWindow>();
            MakeReadyAndShow(window);
        }
        
        public static void Open(string type,string assembly,string icon=null)
        {
            var window = _instances.FirstOrDefault(i => i._typeName == type && i._assemblyName == assembly);
            

            if (!window)
            {
                window = CreateWindow<TypeInstancesWindow>();
               MakeReadyAndShow(window,type,assembly,icon);
                
            }
            else
            {
                MakeReadyAndShow(window);
            }
        }

        // ReSharper disable once TooManyArguments
        private static void MakeReadyAndShow(TypeInstancesWindow window,string typeName=null,string assembly=null,string icon=null)
        {
            window.minSize = new Vector2(window.minSize.x, 10);
            window.titleContent = new GUIContent("Type Instances",
                EditorGUIUtility.IconContent(string.IsNullOrEmpty(icon)? "d_FilterByType" : icon).image
            );
            window._assemblyName = string.IsNullOrEmpty(assembly) ? window._assemblyName : assembly;
            window._typeName = string.IsNullOrEmpty(typeName) ? window._typeName : typeName;
            
            window.Show();
            window.Focus();
            window.Refresh();
        }


        private void OnEnable()
        {
            _instances.Add(this);
        }

        private void OnDisable()
        {
            _instances.Remove(this);
        }

        private Type GetTargetType()
        {
            
            return string.IsNullOrEmpty(_typeName)||string.IsNullOrEmpty(_assemblyName) ? null : Type.GetType($"{_typeName}, {_assemblyName}",true);
        }
        
        private void CreateGUI()
        {
            rootVisualElement.Add(new IMGUIContainer(() =>
                {
                    DrawToolBar();

                    if (_editing)
                    {
                        DrawEditingGUI();
                    }
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

        private void OnGUI()
        {
            if (_needToRefresh)
            {
                _needToRefresh = false;
                Refresh();
            }
        }

        private void DrawToolBar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            GUILayout.FlexibleSpace();
            _editing = GUILayout.Toggle(_editing, "Edit", EditorStyles.toolbarButton,
                GUILayout.MaxWidth(EditorStyles.toolbarButton.CalcSize(new GUIContent("Refresh")).x + 10));
            if (GUILayout.Button("Refresh", EditorStyles.toolbarButton,
                    GUILayout.MaxWidth(EditorStyles.toolbarButton.CalcSize(new GUIContent("Refresh")).x + 10)))
            {
                Refresh();
            }

            EditorGUILayout.EndHorizontal();
        }

        private void DrawEditingGUI()
        {
            EditorGUILayout.BeginHorizontal();
            var lastLabelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 40;
            _typeName = EditorGUILayout.TextField("Type", _typeName);
            EditorGUIUtility.labelWidth = 65;

            _assemblyName = EditorGUILayout.TextField("Assembly", _assemblyName);
            if (GUILayout.Button("Done", GUILayout.Width(50)))
            {
                _editing = false;
                Refresh();
            }

            EditorGUIUtility.labelWidth = lastLabelWidth;
            EditorGUILayout.EndHorizontal();
        }

        public void Refresh()
        {
            if (_contentElement == null)
            {
                return;
            }

            _contentElement.Clear();
            // UpdateSceneEditorInfos();
            var targetType = GetTargetType();


            var components = targetType != null ? FindObjectsOfType(targetType,true).OfType<Component>().ToArray() : Array.Empty<Component>();
            titleContent = targetType != null ? new GUIContent(targetType.Name + $"s - {components.Length}", titleContent.image) : titleContent;

            _contentElement.Add(new IMGUIContainer(() =>
            {
                if (components.Any(c=>!c))
                {
                    MarkForRefresh();
                    return;
                }
                OnContentElementGUI(components);
            }));
        }
        
        public void MarkForRefresh()
        {
            _needToRefresh = true;
        }

        
        private void OnContentElementGUI(Component[] components)
        {
           DrawItems(components);
        }
        
        private void DrawItems(IReadOnlyList<Component> components)
        {
            
            var currentWidths = components
                .Select(info => GUI.skin.box.CalcSize(new GUIContent(info.name)).x + 4 + 16).ToList();
            var startIndex = 0;
            while (currentWidths.Count > 0)
            {
                var currentStartIndex = startIndex;
                var count = DrawHorizontal(currentWidths, EditorGUIUtility.currentViewWidth - 20, index =>
                {
                    var component = components[currentStartIndex+index];
                    DrawItem(component);
                });
                currentWidths = currentWidths.Skip(count).ToList();
                startIndex += count;

                if (count == 0)
                    break;
            }
        }
        
        private void DrawItem(Component component)
        {
            if (GUILayout.Button(component.name,
                    GUILayout.Width(GUI.skin.box.CalcSize(new GUIContent(component.name)).x + 4),
                    GUILayout.Height(EditorGUIUtility.singleLineHeight)))
            {
                Selection.activeObject = component;

                if (Event.current is { alt: true })
                {
                    SceneView.FrameLastActiveSceneView();
                }
                
            }
            
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

    public partial class TypeInstancesWindow
    {
        [MenuItem("Window/Type Instances/New")]
        public static void OpenNew()
        {
            var window = CreateInstance<TypeInstancesWindow>();
            window._editing = true;
            MakeReadyAndShow(window);
        }
        
        [MenuItem("Window/Type Instances/Panels")]
        public static void OpenPanels()
        {
            Open("DGames.Essentials.UI.Panel","com.dgames.uiessentials","d_winbtn_win_max_a");
        }

        [MenuItem("Window/Type Instances/Images")]
        public static void OpenImages()
        {
            Open("UnityEngine.UI.Image", "UnityEngine.UI", "d_Image Icon");
        }


        [MenuItem("Window/Type Instances/Texts")]
        public static void OpenTexts()
        {
            Open("UnityEngine.UI.Text","UnityEngine.UI","d_Text Icon");
        }
    }
    
 
// Adds a mesh collider to each game object that contains collider in its name
    public class Example : AssetPostprocessor
    {
        void OnPostprocessPrefab(GameObject g)
        {
            foreach (var uid in g.GetComponentsInChildren<UID>())
            {
                Object.DestroyImmediate(uid,true);
            }
        }

    }
}