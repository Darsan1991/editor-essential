namespace DGames.Essentials.Editor
{
    public interface IMenuItem:ITreeItem
    {
        string FullName { get; }
        int Order { get; set; }
        IContentViewer ContentViewer { get; set; }
    }
}