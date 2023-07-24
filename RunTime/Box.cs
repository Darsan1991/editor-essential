using UnityEngine;

namespace DGames.Essentials.Attributes
{
    public sealed class Box : PropertyAttribute
    {
        public bool ForceRemoveSpaceForFold { get; }
        public bool HasColor { get; }
        public Color Color { get; }


        private static readonly Color _defaultColor = new Color(42f / 255, 42f / 255, 42f / 255, 0.5f);

        public Box(bool forceRemoveSpaceForFold = false)
        {
            ForceRemoveSpaceForFold = forceRemoveSpaceForFold;
            HasColor = true;
            Color =  _defaultColor;
        }


        // ReSharper disable once TooManyDependencies
        public Box(float r,float g,float b,float a,bool forceRemoveSpaceForFold = false)
        {
            Color = new Color(r, g, b, a);
            HasColor = true;
            ForceRemoveSpaceForFold = forceRemoveSpaceForFold;
        }
    }
}