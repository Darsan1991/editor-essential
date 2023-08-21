using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace DGames.Essentials.Editor
{
    public abstract class AssetsWindow : EditorWindow
    {
        protected string filter  = "t:Scene";

        protected bool isSelectOnClick;
        private VisualElement _contentElement;


        
        // ReSharper disable once TooManyArguments
        public static void Open<T>(string filter,string title,bool isSelectOnClick = false,string icon="") where T:AssetsWindow
        {
            var assetsWindow = GetWindow<T>();
            assetsWindow.minSize = new Vector2(assetsWindow.minSize.x,10);
            assetsWindow.titleContent = new GUIContent(title,string.IsNullOrWhiteSpace(icon)?null: EditorGUIUtility.IconContent(icon).image);
            
            assetsWindow.isSelectOnClick = isSelectOnClick;
            assetsWindow.filter = filter;
            assetsWindow.Refresh();
            assetsWindow.Show();
        }


        private void CreateGUI()
        {
            rootVisualElement.Add(new IMGUIContainer(() =>
            {
                EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Refresh", EditorStyles.toolbarButton,GUILayout.MaxWidth(70)))
                {
                    Refresh();
                }
               
                EditorGUILayout.EndHorizontal();
            })
               
            );
            _contentElement = new VisualElement(){
                style = { marginTop = 10,marginLeft = 10,marginRight = 10}
            };
            rootVisualElement.Add(_contentElement);
            Refresh();
        }

        public void Refresh()
        {
            _contentElement.Clear();
            var searchAssets = FindAssets().ToList();
            _contentElement.Add(new IMGUIContainer(() =>
            {
                var assets = searchAssets.ToList();
                if (assets.Any(a=>a==null))
                {
                    Refresh();
                    return;
                }
                while (assets.Count>0)
                {
                    var count = DrawHorizontal(assets,item=>item.name,GUI.skin.box,OnDrawItem,Mathf.Max(EditorGUIUtility.currentViewWidth - 20,40));
                    assets = assets.Skip(count).ToList();
                    if(count==0)
                        break;
                }
                
            }));
        }

        protected virtual IEnumerable<Object> FindAssets()
        {
            return AssetDatabase.FindAssets(filter).Select(AssetDatabase.GUIDToAssetPath)
                .Select(AssetDatabase.LoadMainAssetAtPath);
        }

        // ReSharper disable once TooManyArguments
        private int DrawHorizontal<T>(IEnumerable<T> items,Func<T,string> contentSelector,GUIStyle skin,Action<T,float> onDrawItem,float availableWidth,float extraWidth = 4)
        {
            EditorGUILayout.BeginHorizontal();
            var leftWidth = availableWidth;
            var count = 0;
            foreach (var item in items)
            {
                var width = skin.CalcSize(new GUIContent(contentSelector(item))).x + extraWidth;
                if (leftWidth >= width)
                {
                    count++;
                    leftWidth -= width;
                    onDrawItem(item, width);
                }
                else
                    break;
            }
            EditorGUILayout.EndHorizontal();
            return count;
        }

        private  void OnDrawItem(UnityEngine.Object item, float width)
        {
            if (GUILayout.Button(item.name, GUILayout.MaxWidth(width)
                ))
            {
                if (isSelectOnClick)
                    Selection.activeObject = item;
                else
                    AssetDatabase.OpenAsset(item);
            }
        }
    }
}