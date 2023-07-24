using UnityEngine;

namespace DGames.Essentials.Attributes
{
    public class ShortLabelAttribute : PropertyAttribute
    {
        public int LettersCount { get; }

        public ShortLabelAttribute(int lettersCount=1)
        {
            LettersCount = lettersCount;
        }
        

    }
}