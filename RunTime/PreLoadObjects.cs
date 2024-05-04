using System.Collections.Generic;
using System.Linq;
using DGames.Essentials.Attributes;
using DGames.Essentials.Editor;
using UnityEngine;

namespace DGames.Essentials.Infos
{
    [HideScriptField]
    [TypeMessage("You can set the game objects that need to instantiate before any scene start here.")]
    [DashboardResourceItem(tabPath:"Games/Config",displayName:"Pre Load Objects")]
    public class PreLoadObjects:ScriptableObject
    {
        private static PreLoadObjects _default;
        public static PreLoadObjects Default => _default??= Resources.Load<PreLoadObjects>(nameof(PreLoadObjects));

        [NoLabel][SerializeField] private List<GameObject> _objects = new();


        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Create()
        {
            foreach (var gameObject in Default._objects.Where(obj=>obj))
            {
                Instantiate(gameObject);
            }
        }

#if UNITY_EDITOR
        public class PreLoadObjectsWindow : ScriptableObjectWindow<PreLoadObjects>
        {
            [UnityEditor.MenuItem("MyGames/Scriptable/PreLoad Objects Window")]
            [UnityEditor.MenuItem("MyGames/Pre Load Objects Window")]
            public static void OpenWindow()
            {
                Open<PreLoadObjectsWindow>(ScriptableEditorUtils.GetOrCreate<PreLoadObjects>(),"Pre Load Items","d_GameObject Icon");
            }
        }
#endif
        
    }
}