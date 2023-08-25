using UnityEngine;

namespace DGames.Essentials.Editor
{
    public interface IMenuItem:ITreeItem
    {
        string FullName { get; }
        int Order { get; set; }
        Texture2D Icon { get; }
        IContentViewer ContentViewer { get; set; }
    }
}