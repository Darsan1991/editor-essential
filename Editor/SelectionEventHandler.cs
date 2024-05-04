using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace DGames.Essentials.Editor
{
    public class SelectionEventHandler:AssetItemEventHandler
    {
        public SelectionEventHandler(ProjectWindow window) : base(window)
        {
        }

        public override void OnGUI(IEnumerable<(AssetItem, AssetItemDrawer.ItemWithLabel[])> assetAndRects)
        {
            if (Event.current is { type: EventType.MouseUp, clickCount: 1 })
            {
                HandleClick(assetAndRects);
            }

            if (Selection.objects.Any() && Event.current is { type: EventType.MouseUp })
            {
                Selection.objects = Array.Empty<Object>();
            }
        }

        private void HandleClick(IEnumerable<(AssetItem, AssetItemDrawer.ItemWithLabel[])> assetAndRects)
        {
            foreach (var (item, rects) in assetAndRects)
            {
                for (var i = 0; i < rects.Length; i++)
                {
                    if (!rects[i].TotalRect.Contains(Event.current.mousePosition)) continue;
                        
                    ItemClicked(item, i == 0 ? item.Object : item.SubAssets.ElementAt(i - 1));
                    Event.current.Use();
                }
            }
        }

        private void ItemClicked(AssetItem item, Object itemObject)
        {
            if (Event.current is
                {
                    command: true
                })
                Selection.objects = Selection.objects.Append(itemObject).ToArray();
            else
            {
                Selection.activeObject = itemObject;
            }
        }
    }
}