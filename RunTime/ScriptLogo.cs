using System;

namespace DGames.Essentials.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class ScriptLogo : Attribute
    {
        public string LogoName { get; } = "DGAMES";
        public ScriptLogo()
        {
            
        }
    }
}