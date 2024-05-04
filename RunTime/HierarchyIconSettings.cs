using System.Collections.Generic;
using DGames.Essentials.Attributes;
using UnityEngine;

namespace DGames.Essentials.Unity
{
    [DashboardResourceItem(tabPath:"System/Editor",displayName:"Hierarchy Icon Settings")]
    public partial class HierarchyIconSettings:ScriptableObject
    {
        [SerializeField] private bool _enable = true;
        [SerializeField] private bool _drawChildrenIcons = true;
        [NoLabel][SerializeField] private List<string> _ignoreTypes = new();


        public bool DrawChildrenIcons => _drawChildrenIcons;
        public bool Enable => _enable;
        public IEnumerable<string> IgnoreTypes => _ignoreTypes;
    }
    
    
    public partial class HierarchyIconSettings
    {
        private static HierarchyIconSettings _default;
        public static HierarchyIconSettings Default => _default??= Resources.Load<HierarchyIconSettings>(nameof(HierarchyIconSettings)) 
        #if UNITY_EDITOR
        ?? ScriptableEditorUtils.GetOrCreate<HierarchyIconSettings>(childrenPath: "System")
        #endif
        ;

#if UNITY_EDITOR
        [UnityEditor.MenuItem("MyGames/System/Hierarchy Icon Infos")]
        public static void Open()
        {
            ScriptableEditorUtils.OpenOrCreateDefault<HierarchyIconSettings>(childrenPath:"System");
        }
#endif
    }
}