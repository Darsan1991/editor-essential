using System;
using System.Reflection;
using DGames.Essentials.Attributes;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace DGames.Essentials.Unity
{
    [AddComponentMenu("Identifier/UID")]
    [HideScriptField]
    [ExecuteInEditMode]
    public class UID : MonoBehaviour
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