using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace DGames.Essentials.Editor
{
    [CustomMenuEditor(typeof(GroupObjectMenuItem), true)]
    public class GroupMenuItemEditor : BaseMenuItemEditor
    {
        public GroupObjectMenuItem MenuItem => (GroupObjectMenuItem)Item;

        private readonly Dictionary<BaseMenuItemEditor, bool> _editorsVsFold = new();
        private GUIStyle _editorLabelStyle;


        public override void OnInspectorGUI()
        {
            var provider = MenuItem.ContentViewer as IMenuContentEditorProvider;
            if (provider == null)
                return;

            var editors = MenuItem.Children.Select(item => provider.GetEditor((IMenuItem)item)).ToArray();
            if (!_editorsVsFold.Any())
            {
                foreach (var itemEditor in editors)
                {
                    _editorsVsFold.Add(itemEditor, true);
                }
            }

            CacheEditorLabelStyle();

            foreach (var itemEditor in editors)
            {
                EditorGUILayout.LabelField(GetTitleWithDashes(itemEditor.Item.FullName.Trim(), 100),_editorLabelStyle);

                EditorGUILayout.Space();
                _editorsVsFold[itemEditor] =
                    EditorGUILayout.Foldout(_editorsVsFold[itemEditor], itemEditor.Item.FullName.Trim());
                if (_editorsVsFold[itemEditor])
                    itemEditor.OnInspectorGUI();
                EditorGUILayout.Space();
            }
            EditorGUILayout.LabelField(GetTitleWithDashes("", 100));

        }

        private void CacheEditorLabelStyle()
        {
            if (_editorLabelStyle!=null)
            {
                return;
            }

            _editorLabelStyle = new GUIStyle(EditorStyles.label) { alignment = TextAnchor.MiddleCenter };
        }


        public static string GetTitleWithDashes(string title, int countPerSide = 70)
        {
            var dashes = string.Join("", Enumerable.Repeat("-", countPerSide));

            return $"{dashes}{title}{dashes}";
        }

        public GroupMenuItemEditor(GroupObjectMenuItem item) : base(item)
        {
        }
    }
}