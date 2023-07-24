using System;
using UnityEngine;

namespace DGames.Essentials.Attributes
{
    public sealed class NoFoldAttribute : PropertyAttribute
    {
        public string[] Excepts { get; }
        public Color Color { get; }
        public bool HasColor { get; }


        private static readonly Color _defaultColor = new Color(42f / 255, 42f / 255, 42f / 255, 0.5f);

        public NoFoldAttribute(params string[] excepts)
        {
            Excepts = excepts ?? Array.Empty<string>();
            HasColor = true;
            Color =  _defaultColor;
        }


        public NoFoldAttribute(bool noColor,params string[] excepts)
        {
            Color = noColor ? Color.clear : _defaultColor;
            HasColor = !noColor;
            Excepts = excepts ?? Array.Empty<string>();
        }
        
        public NoFoldAttribute(float r,float g,float b,float a,params string[] excepts)
        {
            Color = new Color(r, g, b, a);
            HasColor = true;
            Excepts = excepts ?? Array.Empty<string>();
        }
    }
}