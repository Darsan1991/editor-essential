using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace DGames.Essentials.Editor
{
    public class ScenesWindow : AssetsWindow
    {
        
        [MenuItem("Window/Scenes")]
        public static void Open()
        {
            Open<ScenesWindow>("t:Scene","Scenes",icon:"Scene");
        }

        protected override IEnumerable<Object> FindAssets()
        {
            var scenes = base.FindAssets().Select(AssetDatabase.GetAssetPath).ToArray();
            var buildScenes = EditorBuildSettings.scenes.Select(es=>es.path).ToArray();

            return buildScenes.Concat(scenes.Except(buildScenes)).Select(AssetDatabase.LoadMainAssetAtPath);
        }
    }
}