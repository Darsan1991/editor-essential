using System;

namespace DGames.Essentials.Attributes
{
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class ButtonAttribute : Attribute
    {
        /// <summary> Custom name of a button or <c>null</c> if not set. </summary>
        public  string Name { get; }

        public ButtonAttribute() { }
        public ButtonAttribute(string name) => Name = name;

    }


  
}



