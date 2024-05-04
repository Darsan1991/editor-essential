using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace DGames.Essentials.Editor
{
    public class OpenEventHandler : AssetItemEventHandler
    {
        public OpenEventHandler(ProjectWindow window) : base(window)
        {
        }

        public override void OnGUI(IEnumerable<(AssetItem, AssetItemDrawer.ItemWithLabel[])> assetWithRectangles)
        {
            if (Selection.objects.Length is > 1 or 0)
            {
                return;
            }

            if (Event.current is { type: EventType.MouseDown, clickCount: 2 })
            {
                HandleDoubleClick(assetWithRectangles);
            }
        }
        private void HandleDoubleClick(IEnumerable<(AssetItem, AssetItemDrawer.ItemWithLabel[])> assetAndRects)
        {
            foreach (var (item, rects) in assetAndRects)
            {
                for (var i = 0; i < rects.Length; i++)
                {
                    if (!rects[i].TotalRect.Contains(Event.current.mousePosition)) continue;
                        
                    OpenAsset(i == 0 ? item.Object : item.SubAssets.ElementAt(i - 1));
                    Event.current.Use();
                }
            }
        }
            
        public void OpenAsset(Object obj)
        {
            if (obj is DefaultAsset)
            {
                Window.OpenDirectory(AssetDatabase.GetAssetPath(obj));
            }
            else
            {
                AssetDatabase.OpenAsset(obj);
            }
        }
    }
}