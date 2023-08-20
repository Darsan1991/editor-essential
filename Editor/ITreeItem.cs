using System.Collections.Generic;

namespace DGames.Essentials.Editor
{
    public interface ITreeItem<T> where T : ITreeItem<T>
    {
        T Parent { get; set; }
        IEnumerable<T> Children { get; }
        void AddChildren(ITreeItem<T> item);
    }

    public interface ITreeItem : IEnumerable<ITreeItem>
    {
        ITreeItem Parent { get; set; }
        IEnumerable<ITreeItem> Children { get; }

        void AddChildren(ITreeItem item);
    }
}