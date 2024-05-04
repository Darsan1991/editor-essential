using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace DGames.Essentials.Editor
{
    public class DragInvokeEventHandler : AssetItemEventHandler
    {
        public DragInvokeEventHandler(ProjectWindow window) : base(window)
        {
        }

        public override void OnGUI(IEnumerable<(AssetItem, AssetItemDrawer.ItemWithLabel[])> assetWithRectangles)
        {
            var assetAndRects = assetWithRectangles.ToArray();
            if (Event.current is { type: EventType.MouseDrag })
            {
        
                var item = assetAndRects.FirstOrDefault(g =>
                    g.Item2.Any(r => r.TotalRect.Contains(Event.current.mousePosition)));
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
    }
    
    public class ItemFolderDragAcceptEventHandler : AssetItemEventHandler
    {
        public ItemFolderDragAcceptEventHandler(ProjectWindow window) : base(window)
        {
        }

        public override void OnGUI(IEnumerable<(AssetItem, AssetItemDrawer.ItemWithLabel[])> assetWithRectangles)
        {
            var assetAndRects = assetWithRectangles.ToArray();
           HandleDragUpdate(assetAndRects);
           HandleDragPerform(assetAndRects.Select(a=>a.Item1));
        }
        
        
        private void HandleDragPerform(IEnumerable<AssetItem> assets)
        {
            if (!DragAndDrop.objectReferences.Any() || Event.current is not { type: EventType.DragPerform }) return;
            foreach (var item in assets.Where(a => a.IsHighlighting).ToArray())
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
        }

        private void HandleDragUpdate((AssetItem, AssetItemDrawer.ItemWithLabel[])[] assetAndRects)
        {
            if (!DragAndDrop.objectReferences.Any() || Event.current is not { type: EventType.DragUpdated }) return;
            foreach (var (asset, rects) in assetAndRects.Where(a => a.Item1.Object is DefaultAsset))
            {
                asset.IsHighlighting = rects.First().TotalRect.Contains(Event.current.mousePosition);
            }

            DragAndDrop.visualMode = assetAndRects.Select(a=>a.Item1).Any(a => a.IsHighlighting)
                ? DragAndDropVisualMode.Move
                : DragAndDropVisualMode.None;
        }
    }
    
    
    public class ImportDragAcceptEventHandler : AssetItemEventHandler
    {
        public ImportDragAcceptEventHandler(ProjectWindow window) : base(window)
        {
        }

        public override void OnGUI(IEnumerable<(AssetItem, AssetItemDrawer.ItemWithLabel[])> assetWithRectangles)
        {
            var assetAndRects = assetWithRectangles.ToArray();
            HandleDragUpdate(assetAndRects);
            HandleDragPerform(assetAndRects.Select(a=>a.Item1));
        }
        
        
        private void HandleDragPerform(IEnumerable<AssetItem> assets)
        {
            Debug.Log(Event.current);
            if (DragAndDrop.objectReferences.Any() || !DragAndDrop.paths.Any() || Event.current is not { type: EventType.DragPerform }) return;
            foreach (var item in assets.Where(a => a.IsHighlighting).ToArray())
            {
                // foreach (var reference in DragAndDrop.objectReferences)
                // {
                //     var assetPath = AssetDatabase.GetAssetPath(reference);
                DragAndDrop.AcceptDrag();
                //     AssetDatabase.ImportAsset(assetPath,
                //         $"{item.Path}{Path.DirectorySeparatorChar}{assetPath.Split(Path.DirectorySeparatorChar).Last()}");
                // }
                item.IsHighlighting = false;
                FileUtils.CopyFileAndFoldersToFolder(DragAndDrop.paths,Path.Combine(Application.dataPath,item.Path));
                

            }
        }

        private void HandleDragUpdate((AssetItem, AssetItemDrawer.ItemWithLabel[])[] assetAndRects)
        {
            Debug.Log(nameof(HandleDragUpdate)+":"+Event.current);

            if (DragAndDrop.objectReferences.Any() || !DragAndDrop.paths.Any() || Event.current is not { type: EventType.DragUpdated }) return;
            Debug.Log(nameof(HandleDragUpdate));
            foreach (var (asset, rects) in assetAndRects.Where(a => a.Item1.Object is DefaultAsset))
            {
                asset.IsHighlighting = rects.First().TotalRect.Contains(Event.current.mousePosition);
            }

            DragAndDrop.visualMode = assetAndRects.Select(a=>a.Item1).Any(a => a.IsHighlighting)
                ? DragAndDropVisualMode.Copy
                : DragAndDropVisualMode.None;
        }
    }

    public static class FileUtils
    {
        public static void CopyFileAndFoldersToFolder(IEnumerable<string> paths, string folder)
        {
            foreach (var path in paths)
            {
                if (Directory.Exists(path))
                {
                    CopyDirectoryTo(path,folder);
                }
                else if (File.Exists(path))
                {
                    CopyFile(path,folder);
                }
            }
        }


        public static void CopyDirectoryTo(string sourceDir, string parentFolder, bool recursive = true)
        {
            var dir = new DirectoryInfo(sourceDir);

            if (!dir.Exists)
                throw new DirectoryNotFoundException($"Source directory not found: {dir.FullName}");
            CopyDirectory(sourceDir,Path.Combine(parentFolder,dir.Name));
        }
        
        public static void CopyDirectory(string sourceDir, string destinationDir, bool recursive=true)
        {
            var dir = new DirectoryInfo(sourceDir);

            if (!dir.Exists)
                throw new DirectoryNotFoundException($"Source directory not found: {dir.FullName}");

            var dirs = dir.GetDirectories();

            Directory.CreateDirectory(destinationDir);

            foreach (FileInfo file in dir.GetFiles())
            {
                string targetFilePath = Path.Combine(destinationDir, file.Name);
                file.CopyTo(targetFilePath);
            }
            if (recursive)
            {
                foreach (DirectoryInfo subDir in dirs)
                {
                    string newDestinationDir = Path.Combine(destinationDir, subDir.Name);
                    CopyDirectory(subDir.FullName, newDestinationDir, true);
                }
            }
        }

        public static void CopyFile(string source, string destinationDir)
        {
            if (!File.Exists(source))
            {
                return;
            }

            var destinationPath = Path.Combine(destinationDir, source.Split(Path.DirectorySeparatorChar).Last());
            File.Copy(source,destinationPath);
        }
    }
}