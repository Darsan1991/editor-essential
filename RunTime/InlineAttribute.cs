using UnityEngine;

namespace DGames.Essentials.Attributes
{
    public sealed class InlineAttribute : PropertyAttribute
    {
        public float MinWidth { get; }
        public bool DrawBox { get; }
        public Color Color { get; }


        private static readonly Color _defaultColor = new Color(42f / 255, 42f / 255, 42f / 255, 0.5f);


        public InlineAttribute(float minWidth=400f,bool drawBox = false)
        {
            MinWidth = minWidth;    
            DrawBox = drawBox;
            Color =  _defaultColor;
        }
        
        public InlineAttribute(float r,float g,float b,float a=1f,float minWidth=500f)
        {
            MinWidth = minWidth;    
            DrawBox = true;
            Color = new Color(r, g, b,a);
        }
    }
}