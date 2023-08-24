using System.Collections.Generic;
using UnityEngine;

namespace DGames.Essentials.Unity
{
    public partial class HierarchyIconInfos:ScriptableObject
    {
        [SerializeField] private List<string> _ignoreTypes = new();


        public IEnumerable<string> IgnoreTypes => _ignoreTypes;
    }
    
    public partial class HierarchyIconInfos
    {
        private static HierarchyIconInfos _default;
        public static HierarchyIconInfos Default => _default??= Resources.Load<HierarchyIconInfos>(nameof(HierarchyIconInfos)) 
        #if UNITY_EDITOR
        ?? ScriptableEditorUtils.GetOrCreate<HierarchyIconInfos>(childrenPath: "System")
        #endif
        ;

#if UNITY_EDITOR
        [UnityEditor.MenuItem("MyGames/System/Hierarchy Icon Infos")]
        public static void Open()
        {
            ScriptableEditorUtils.OpenOrCreateDefault<HierarchyIconInfos>(childrenPath:"System");
        }
#endif
    }
}