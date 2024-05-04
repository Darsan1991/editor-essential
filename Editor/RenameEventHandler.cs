using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace DGames.Essentials.Editor
{
    public class RenameEventHandler : AssetItemEventHandler
    {
        public RenameEventHandler(ProjectWindow window) : base(window)
        {
        }

        public override void OnGUI(IEnumerable<(AssetItem, AssetItemDrawer.ItemWithLabel[])> assetWithRectangles)
        {
            var array = assetWithRectangles.ToArray();
            if (Selection.objects.Length is > 1 or 0)
            {
                return;
            }
            
            var assets = array.Select(a=>a.Item1).ToArray();
            if (assets.Any(a=>a.IsCreating))
            {
                return;
            }
            var renamingItem = assets.FirstOrDefault(a=>a.IsRenaming);

            if ( renamingItem == null && Event.current is {keyCode: KeyCode.Return,type: EventType.KeyDown} && Selection.objects.Length == 1 && assets.Any(a => a.Object == Selection.activeObject))
            {
                Event.current.Use();
                
            }
            if (renamingItem == null && (Event.current is { type: EventType.MouseDown }))
            {
                HandleLabelClick(array);
            }
            if ( renamingItem == null && Event.current is {keyCode: KeyCode.Return,type: EventType.KeyUp} && Selection.objects.Length == 1 && assets.Any(a => a.Object == Selection.activeObject))
            {
                MarkAsRenaming(assets.First(a=>a.Object == Selection.activeObject));
                Event.current.Use();
            }
            else if (renamingItem != null && (Event.current is { type: EventType.MouseDown } or
                         { type: EventType.KeyUp, keyCode: KeyCode.Return }))
            {
                PerformRename(renamingItem);
                Event.current.Use();
            }
            
            if (renamingItem != null && Event.current is
                    { type: EventType.KeyDown, keyCode: KeyCode.Escape })
            {
                PerformRename(renamingItem);
                Event.current.Use();
            }
        }
        
        private void HandleLabelClick(IEnumerable<(AssetItem, AssetItemDrawer.ItemWithLabel[])> assetAndRects)
        {
            foreach (var (item, rects) in assetAndRects.Where(item=>item.Item1.Object == Selection.activeObject))
            {
                if (!rects.First().Label.Contains(Event.current.mousePosition)) continue;
                        
                MarkAsRenaming(item);
                Event.current.Use();
            }
        }

        private static void MarkAsRenaming(AssetItem renamingItem)
        {
            var assetItem = renamingItem;
            assetItem.IsRenaming = true;
        }

        private static void PerformRename(AssetItem renamingItem)
        {
            AssetDatabase.RenameAsset(renamingItem.Path, renamingItem.RenamingText);
            renamingItem.IsRenaming = false;
        }
    }
}