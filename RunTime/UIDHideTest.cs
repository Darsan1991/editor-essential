using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace DGames.Essentials.Unity
{
    public class UIDHideTest : MonoBehaviour
    {
        [HideInInspector][SerializeField] private string _id;

        public string ID
        {
            get
            {
                if (string.IsNullOrEmpty(_id))
                {
                    GenerateNewId();
                }

                return _id;
            }
        }

        private void Awake()
        {
#if UNITY_EDITOR
            if(PrefabUtility.IsPartOfPrefabAsset(this))
                return;
            
            
#endif
            
            if(  GetInstanceID()<0   && !Application.isPlaying && GetFileLocalIdentifier() == 0)
            {
                GenerateNewId();
                // Debug.Log("Generate New id - Duplicated/Copied - " + _id);
            }
        }

        private int GetFileLocalIdentifier()
        {

#if UNITY_EDITOR
            var inspectorModeInfo =
                typeof(SerializedObject).GetProperty("inspectorMode", BindingFlags.NonPublic | BindingFlags.Instance);
 
            using var serializedObject = new SerializedObject(this);
            inspectorModeInfo!.SetValue(serializedObject, InspectorMode.Debug, null);
 
            var localIdProp =
                serializedObject.FindProperty("m_LocalIdentfierInFile");   //note the misspelling!
 
            return localIdProp.intValue;
#else

            return 0;
#endif
        }
        
        private void Reset()
        {
            GenerateNewId();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.H))
            {
                hideFlags = hideFlags == HideFlags.HideInInspector  ? HideFlags.None: HideFlags.HideInInspector;

            }
        }

        private void GenerateNewId()
        {
            _id = Guid.NewGuid().ToString();
            
#if UNITY_EDITOR
            EditorUtility.SetDirty(this);
#endif
        }

        [ContextMenu(nameof(PrintId))]
        public void PrintId()
        {
            Debug.Log(_id);
        }
    }
}