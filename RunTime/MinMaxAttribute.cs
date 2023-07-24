using UnityEngine;

namespace DGames.Essentials.Attributes
{
    public sealed class MinMaxAttribute : PropertyAttribute
    {
        public float Min { get; }
        public float MAX { get; }
        public bool ShowFields { get; }
        public bool DisableField { get; }

        // ReSharper disable once TooManyDependencies
        public MinMaxAttribute(float min=0,float max=1,bool showFields=true,bool disableField = true)
        {
            Min = min;
            MAX = max;
            ShowFields = showFields;
            DisableField = disableField;
        }

    }
}