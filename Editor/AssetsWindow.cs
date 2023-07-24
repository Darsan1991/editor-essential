using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace DGames.Essentials.Editor
{
    public abstract class AssetsWindow : EditorWindow
    {
        protected string filter  = "t:Scene";

        protected bool isSelectOnClick;
        private VisualElement _contentElement;


        
        public static void Open<T>(string filter,string title,bool isSelectOnClick = false) where T:AssetsWindow
        {
            var assetsWindow = GetWindow<T>();
            assetsWindow.minSize = new Vector2(assetsWindow.minSize.x,10);
            assetsWindow.titleContent = new GUIContent(title);
            assetsWindow.isSelectOnClick = isSelectOnClick;
            assetsWindow.filter = filter;
            assetsWindow.Refresh();
            assetsWindow.Show();
        }


        private void CreateGUI()
        {
            rootVisualElement.Add(new IMGUIContainer(() =>
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Refresh", GUILayout.MaxWidth(70)))
                {
                    Refresh();
                }
               
                EditorGUILayout.EndHorizontal();
            }));
            _contentElement = new VisualElement();
            rootVisualElement.Add(_contentElement);
            Refresh();
        }

        public void Refresh()
        {
            _contentElement.Clear();
            var searchAssets = AssetDatabase.FindAssets(filter).Select(AssetDatabase.GUIDToAssetPath)
                .Select(AssetDatabase.LoadMainAssetAtPath).ToList();
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
                    var count = DrawHorizontal(assets,item=>item.name,GUI.skin.box,OnDrawItem);
                    assets = assets.Skip(count).ToList();
                }
            }));
        }

        // ReSharper disable once TooManyArguments
        private int DrawHorizontal<T>(IEnumerable<T> items,Func<T,string> contentSelector,GUIStyle skin,Action<T,float> onDrawItem,float extraWidth = 4)
        {
            EditorGUILayout.BeginHorizontal();
            var leftWidth = EditorGUIUtility.currentViewWidth;
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