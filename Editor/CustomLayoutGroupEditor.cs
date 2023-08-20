using System;

namespace DGames.Essentials.Editor
{
    public class CustomLayoutGroupEditor : TypeToTypeAttribute
    {
        public CustomLayoutGroupEditor(Type type) : base(type)
        {
        }

        public CustomLayoutGroupEditor(Type type, bool forChildren) : base(type, forChildren)
        {
        }
    }
}