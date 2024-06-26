using System;
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
        private static readonly List<IObjectIconsProvider> _iconProviders = new();

        static HierarchyIcons()
        {
            EditorApplication.hierarchyWindowItemOnGUI += HierarchyItemOnGUI;
        }

        public static void RegisterIconProvider(IObjectIconsProvider provider) => _iconProviders.Add(provider);

        private static void HierarchyItemOnGUI(int instanceID, Rect selectionRect)
        {
            if (!HierarchyIconSettings.Default.Enable)
                return;
            if (EditorUtility.InstanceIDToObject(instanceID) is not GameObject go) return;

            var behaviourAndIcons = FilterTypes(go.GetComponents<MonoBehaviour>().Where(m=>m))
                .Select(b => (b, EditorGUIUtility.GetIconForObject(b)))
                .Where(t => t.Item2).ToArray();

            var childGroups = GetChildrenBehaviourGroups(go);

            var otherIcons = _iconProviders.Select(p => p.GetIcons(go)).SelectMany(icons => icons).ToArray();

            var rects = GetRects(
                otherIcons.Select(i=> 16f)
                    .Concat(behaviourAndIcons.Select(_ => 16f))
                    .Concat(childGroups.Any() ? new[] { 16f } : Array.Empty<float>())
                    .Concat(childGroups.Select(g => g.Count() >= 10 ? 28f : 20f))
                    .ToArray(),
                selectionRect,
                EditorStyles.label.CalcSize(new GUIContent(go.name)).x + 20).ToArray();

            DrawOtherIcons(go,otherIcons,rects.Take(otherIcons.Length).ToArray());
            var startIndex = otherIcons.Length;

            DrawOwnIcons(behaviourAndIcons, rects.Skip(startIndex).Take(behaviourAndIcons.Length).ToArray());

            startIndex += behaviourAndIcons.Length;


            if (rects.Length > startIndex)
                GUI.Label(rects[startIndex], "|", EditorStyles.centeredGreyMiniLabel);
            

            for (var i = 0; i < childGroups.Length; i++)
            {
                var rect = rects[i + startIndex + 1];
                DrawMiniIconGroup(rect, childGroups, i);
            }
        }

        private static IGrouping<Type, MonoBehaviour>[] GetChildrenBehaviourGroups(GameObject go)
        {
            var childBehaviours =
                HierarchyIconSettings.Default.DrawChildrenIcons
                    ? FilterTypes(go.GetComponentsInChildren<MonoBehaviour>().Where(m => m && m.gameObject != go))
                        .Where(m => EditorGUIUtility.GetIconForObject(m)).ToArray()
                    : Array.Empty<MonoBehaviour>();
            var childGroups = childBehaviours.GroupBy(c => c.GetType()).ToArray();
            return childGroups;
        }

        private static void DrawOwnIcons((MonoBehaviour b, Texture2D)[] behaviourAndIcons, Rect[] rects)
        {
            for (var i = 0; i < behaviourAndIcons.Length; i++)
            {
                var rect = rects[i];
                var height = 13f;
                rect.y += (rect.height - height) / 2;
                rect.height = height;
                EditorGUI.BeginDisabledGroup(!(behaviourAndIcons[i].Item1 is Behaviour
                {
                    enabled: true, gameObject: { activeSelf: true }
                }));
                GUI.Label(rect, behaviourAndIcons[i].Item2);
                EditorGUI.EndDisabledGroup();
            }
        }
        
        private static void DrawOtherIcons(GameObject go,IReadOnlyList<Texture> icons, IReadOnlyList<Rect> rects)
        {
            for (var i = 0; i < icons.Count; i++)
            {
                var rect = rects[i];
                var height = 13f;
                rect.y += (rect.height - height) / 2;
                rect.height = height;
                EditorGUI.BeginDisabledGroup(!go.activeSelf);
                GUI.Label(rect, icons[i]);
                EditorGUI.EndDisabledGroup();
            }
        }

        private static void DrawMiniIconGroup(Rect rect, IGrouping<Type, MonoBehaviour>[] childGroups, int i)
        {
            var height = 10f;
            rect.y += (rect.height - height) / 2;
            rect.height = height;
            GUI.Label(rect,
                new GUIContent(childGroups[i].Count() + " ",
                    EditorGUIUtility.GetIconForObject(childGroups[i].First())), EditorStyles.miniLabel);
        }

        private static IEnumerable<T> FilterTypes<T>(IEnumerable<T> behaviours)
        {
            return behaviours.Where(b =>
            {
                var fullName = b.GetType().FullName;
                return HierarchyIconSettings.Default.IgnoreTypes.All(name =>
                    fullName == null || !fullName.Contains(name));
            });
        }

        // ReSharper disable once TooManyArguments
        private static IEnumerable<Rect> GetRects(IReadOnlyList<float> sizes, Rect container, float contentSize,
            float spacing = 1)
        {
            var r = new Rect(container);
            var totalWidth = sizes.Sum() + spacing * (sizes.Count - 1);
            r.x = Mathf.Max(r.x + r.width - totalWidth, r.x + contentSize);

            foreach (var size in sizes)
            {
                r.width = size;
                yield return r;
                r.x += r.width + spacing;
            }
        }
    }
}