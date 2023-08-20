using System;

namespace DGames.Essentials.Editor
{
    public interface IWindow
    {
        public event Action<string, object> EventEmitted;
    }
}