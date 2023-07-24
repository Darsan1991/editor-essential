using System;
using UnityEngine;

namespace DGames.Essentials.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class FooterLogo : Attribute
    {
        public Color Color { get; }
        public string LogoName { get; } = "BY DARSAN";
        private static readonly Color _defaultColor = new Color(42f / 255, 42f / 255, 42f / 255, 1f);

        public FooterLogo()
        {
            Color = _defaultColor;
        }
        // ReSharper disable once TooManyDependencies
        public FooterLogo(float r,float g,float b,float a)
        {
            Color = new Color(r, g, b, a);
        }
    }
}