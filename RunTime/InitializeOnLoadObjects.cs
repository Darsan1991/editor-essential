using System.Collections.Generic;
using System.Linq;
using DGames.Essentials.Attributes;
using DGames.Essentials.Editor;
using UnityEngine;

namespace DGames.Essentials.Infos
{
    [HideScriptField]
    public class InitializeOnLoadObjects:ScriptableObject
    {
        private static InitializeOnLoadObjects _default;
        public static InitializeOnLoadObjects Default => _default??= Resources.Load<InitializeOnLoadObjects>(nameof(InitializeOnLoadObjects));

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
        public class InitializeOnLoadObjectsWindow : ScriptableObjectWindow<InitializeOnLoadObjects>
        {
            [UnityEditor.MenuItem("MyGames/Scriptable/Initialize On Load Objects Window")]
            [UnityEditor.MenuItem("MyGames/Initialize On Load Objects Window")]
            public static void OpenWindow()
            {
                Open<InitializeOnLoadObjectsWindow>(ScriptableEditorUtils.GetOrCreate<InitializeOnLoadObjects>(),"Load Items");
            }
        }
#endif
        
    }
}