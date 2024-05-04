using System;
using System.Collections.Generic;

namespace DGames.Essentials.Editor
{
    public interface IToolButtons
    {
        IEnumerable<ButtonArgs> ToolButtons { get; }
    }
    
    public struct ButtonArgs
    {
        public string name;
        public Action action;
    }
}