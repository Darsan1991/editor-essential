using DGames.Essentials.EditorHelpers;
using UnityEditor;
using UnityEngine;

namespace DGames.Essentials.Editor
{
    [CustomMenuEditor(typeof(SerializablePropertyMenuItem), true)]
    // ReSharper disable once UnusedType.Global
    public class SerializePropertyMenuEditor : BaseMenuItemEditor
    {
        public SerializablePropertyMenuItem MenuItem => (SerializablePropertyMenuItem)Item;

        public override void OnInspectorGUI()
        {
            // EditorGUI.BeginChangeCheck();
            base.OnInspectorGUI();


            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            var showPopUp = GUILayout.Button(new GUIContent(EditorGUIUtility.IconContent("SettingsIcon")),
                GUILayout.MaxWidth(25));
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();

            DrawTheProperty();

            MenuItem.ContentViewer.RefreshItem(MenuItem);

            if (showPopUp)
            {
                ShowPopup();
            }
        }

        private void DrawTheProperty()
        {
            var property = MenuItem.SerializedObject.FindRelativePropertyAd(MenuItem.PropertyPath);
            property.isExpanded = true;
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(property, GUIContent.none);
            if (EditorGUI.EndChangeCheck())
            {
                property.serializedObject.ApplyModifiedProperties();
            }
        }

        public SerializePropertyMenuEditor(SerializablePropertyMenuItem item) : base(item)
        {
        }

        private void ShowPopup()
        {
            var menu = new GenericMenu();

            menu.AddItem(new GUIContent($"Duplicate"), false, OnClickDuplicate);
            menu.AddItem(new GUIContent($"Delete"), false, OnClickDelete);

            menu.ShowAsContext();
        }
     
        private void OnClickDuplicate()
        {
            var property = MenuItem.SerializedObject.FindRelativePropertyAd(MenuItem.PropertyPath);
            if (property.IsArrayChildElement(out var parentPath,out var index))
            {
                var parent = MenuItem.SerializedObject.FindProperty(parentPath);
                parent.InsertArrayElementAtIndex(index+1);
                parent.serializedObject.ApplyModifiedProperties();
            }
            MenuItem.ContentViewer.Refresh();
        }
        
        private void OnClickDelete()
        {
            var property = MenuItem.SerializedObject.FindRelativePropertyAd(MenuItem.PropertyPath);
            if (property.IsArrayChildElement(out var parentPath,out var index))
            {
                var parent = MenuItem.SerializedObject.FindProperty(parentPath);
                if (property.propertyType == SerializedPropertyType.ObjectReference)
                {
                    property.objectReferenceValue = null;
                }
                
                parent.DeleteArrayElementAtIndex(index);
                parent.serializedObject.ApplyModifiedProperties();
            }
            MenuItem.ContentViewer.Refresh();
        }
    }
}