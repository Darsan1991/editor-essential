using System.Collections.Generic;
using System.Linq;
using DGames.Essentials.Unity;
using UnityEditor;
using UnityEngine;

namespace DGames.Essentials.Editor
{
    [InitializeOnLoad]
    public static class HierarchyIcons
    {
        private static readonly HierarchyIconInfos _hierarchyIconInfos;

        static HierarchyIcons ()
        {
            EditorApplication.hierarchyWindowItemOnGUI += HierarchyItemOnGUI;
            _hierarchyIconInfos = HierarchyIconInfos.Default;
        }

        private static void HierarchyItemOnGUI (int instanceID, Rect selectionRect)
        {
            if (EditorUtility.InstanceIDToObject(instanceID) is not GameObject go) return;
            
            var behaviours = go.GetComponents<MonoBehaviour>();

            var icons = FilterTypes(behaviours).Select(EditorGUIUtility.GetIconForObject).Where(icon=>icon).ToArray();
            var rects = GetRects(icons.Length,selectionRect,EditorStyles.label.CalcSize(new GUIContent(go.name)).x+20).ToArray();

            for (var i = 0; i < icons.Length; i++)
            {
                GUI.Label(rects[i],icons[i]);
            }

        }

        private static IEnumerable<MonoBehaviour> FilterTypes(IEnumerable<MonoBehaviour> behaviours)
        {
            return behaviours.Where(b =>
            {
                var fullName = b.GetType().FullName;
                return _hierarchyIconInfos.IgnoreTypes.All(name => fullName == null || !fullName.Contains(name));
            });
        }

        // ReSharper disable once TooManyArguments
        private static IEnumerable<Rect> GetRects(int count, Rect container,float contentSize, float spacing=1,float size=16)
        {
            var r = new Rect (container);
            var totalWidth = count*size + spacing*(count-1);
            r.x = Mathf.Max(r.x +  r.width - totalWidth,r.x+contentSize);
            r.width = size;

            for (var i = 0; i < count; i++)
            {
                yield return r;
                r.x += r.width + spacing;
            }
            
            
        }
    }
}