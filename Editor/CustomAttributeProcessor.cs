using System;

namespace DGames.Essentials.Editor
{
    // ReSharper disable once HollowTypeName
    public class CustomAttributeProcessor:TypeToTypeAttribute
    {
        public CustomAttributeProcessor(Type type) : base(type)
        {
        }

        public CustomAttributeProcessor(Type type, bool forChildren) : base(type, forChildren)
        {
        }
    }
}