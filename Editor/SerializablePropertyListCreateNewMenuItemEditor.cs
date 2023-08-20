using UnityEngine;

namespace DGames.Essentials.Editor
{
    [CustomMenuEditor(typeof(SerializablePropertyListCreateNewMenuItem), true)]
    // ReSharper disable once UnusedType.Global
    public class SerializablePropertyListCreateNewMenuItemEditor : BaseMenuItemEditor
    {
        public SerializablePropertyListCreateNewMenuItem MenuItem => (SerializablePropertyListCreateNewMenuItem)Item;

        public override void OnInspectorGUI()
        {
            // EditorGUI.BeginChangeCheck();
            base.OnInspectorGUI();


            var property = MenuItem.SerializedObject.FindRelativePropertyAd(MenuItem.PropertyPath);
            if (GUILayout.Button("Create New"))
            {
                property.InsertArrayElementAtIndex(property.arraySize);
                property.serializedObject.ApplyModifiedProperties();
                MenuItem.ContentViewer.Refresh();
            }
        }

        public SerializablePropertyListCreateNewMenuItemEditor(SerializablePropertyListCreateNewMenuItem item) :
            base(item)
        {
        }
    }
}