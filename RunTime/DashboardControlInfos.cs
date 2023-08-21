using System;
using System.Collections.Generic;
using System.Linq;
using DGames.Essentials.Attributes;
using UnityEngine;

namespace DGames.Essentials.Infos
{
    public partial class DashboardControlInfos:ScriptableObject
    {
        [SerializeField] private bool _development;
        [Inline(250f)][SerializeField] private List<TabStateInfo> _blackList = new();

        public IEnumerable<string> BlackList => _development ? Array.Empty<string>() : _blackList.Where(s=>s.active).Select(s=>s.path);

        
        

        [Serializable]
        public struct TabStateInfo
        {
            public string path;
            public bool active;
        }
    }

    public partial class DashboardControlInfos
    {
        private static DashboardControlInfos _default;
        public static DashboardControlInfos Default => _default??= Resources.Load<DashboardControlInfos>(nameof(DashboardControlInfos));

#if UNITY_EDITOR
        [UnityEditor.MenuItem("MyGames/Settings/Dashboard")]
        public static void Open()
        {
            ScriptableEditorUtils.OpenOrCreateDefault<DashboardControlInfos>();
        }
#endif
    }
}
