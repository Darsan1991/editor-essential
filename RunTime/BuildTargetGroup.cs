using System;

namespace DGames.Essentials.Unity
{
    [Flags]
    public enum BuildTargetGroup
    {
        Android = 1,
        // ReSharper disable once UnusedMember.Global
        // ReSharper disable once InconsistentNaming
        iOS = 2,
    }
    
    #if UNITY_EDITOR

    public static class BuildTargetGroupExtensions
    {
        public static UnityEditor.BuildTargetGroup ToUnityGroup(this BuildTargetGroup group)
        {
            return group switch
            {
                BuildTargetGroup.Android => UnityEditor.BuildTargetGroup.Android,
                BuildTargetGroup.iOS => UnityEditor.BuildTargetGroup.iOS,
                _ => throw new ArgumentOutOfRangeException(nameof(group), group, null)
            };

        }
    }
    
    #endif
}