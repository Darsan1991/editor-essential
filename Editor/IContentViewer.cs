namespace DGames.Essentials.Editor
{
    public interface IContentViewer
    {
        void Refresh();
        void RefreshItem(IMenuItem item);
        void SelectItem(IMenuItem item);
    }
}