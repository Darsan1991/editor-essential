using UnityEngine;

namespace DGames.Essentials.Attributes
{
    public sealed class SectionTitleAttribute : PropertyAttribute
    {
        public string Title { get; }
        public int Above { get; }
        public int Below { get; }

        public SectionTitleAttribute(string title,int above=1,int below=1)
        {
            Title = title;
            Above = above;
            Below = below;
        }
        

    }
}