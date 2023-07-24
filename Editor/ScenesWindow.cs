using UnityEditor;

namespace DGames.Essentials.Editor
{
    public class ScenesWindow : AssetsWindow
    {
        
        [MenuItem("Window/Scenes")]
        public static void Open()
        {
            Open<ScenesWindow>("t:Scene","Scenes",icon:"Scene");
        }
    }
}