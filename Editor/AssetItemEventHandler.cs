using System.Collections.Generic;

namespace DGames.Essentials.Editor
{
    public abstract class AssetItemEventHandler
    {
        public ProjectWindow Window { get; }

        protected AssetItemEventHandler(ProjectWindow window)
        {
            Window = window;
        }
        public abstract void OnGUI(IEnumerable<(AssetItem,AssetItemDrawer.ItemWithLabel[])> assetWithRectangles);
    }
}