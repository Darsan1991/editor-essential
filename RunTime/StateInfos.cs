using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DGames.Essentials.Unity
{
    public partial class StateInfos : ScriptableObject
    {
        private static StateInfos _default;

        public static StateInfos Default
        {
            get
            {
#if UNITY_EDITOR
                if (!_default)
                    _default = ScriptableEditorUtils.GetOrCreate<StateInfos>(childrenPath: DEFAULT_FOLDER_PATH);
#endif
                return _default;
            }
        }

        public const string DEFAULT_FOLDER_PATH = "System";


        [SerializeField] private List<SceneStatesInfo> _sceneStatesInfos = new();


        public IEnumerable<StateInfo> GetStates(string sceneId)
        {
            return _sceneStatesInfos.FirstOrDefault(s => s.sceneId == sceneId).stateInfos ?? new List<StateInfo>();
        }
        
        public StateInfo GetState(string sceneId,string stateName)
        {
            return (_sceneStatesInfos.FirstOrDefault(s => s.sceneId == sceneId).stateInfos ?? new List<StateInfo>())
                .FirstOrDefault(s => s.name == stateName);
        }

        public bool HasSceneState(string sceneId, string name) => _sceneStatesInfos.Where(s => s.sceneId == sceneId)
            .SelectMany(s => s.stateInfos).Any(info => info.name == name);

        public void AddState(string sceneId, StateInfo stateInfo)
        {
            var index = FindOrAddSceneStateInfo(sceneId);

            _sceneStatesInfos[index].stateInfos.Add(stateInfo);
            MarkDirtyIfEditor();
        }

        private int FindOrAddSceneStateInfo(string sceneId)
        {
            var index = _sceneStatesInfos.FindIndex(s => s.sceneId == sceneId);

            if (index < 0)
            {
                _sceneStatesInfos.Add(new SceneStatesInfo
                {
                    sceneId = sceneId,
                    stateInfos = new()
                });
                index = _sceneStatesInfos.Count - 1;
            }

            MarkDirtyIfEditor();
            return index;
        }

        public void UpdateState(string sceneId, StateInfo stateInfo,string stateName)
        {
            var index = FindOrAddSceneStateInfo(sceneId);

            var stateIndex = _sceneStatesInfos[index].stateInfos.FindIndex(s => s.name == stateName);
            _sceneStatesInfos[index].stateInfos[stateIndex] = stateInfo;
            MarkDirtyIfEditor();
        }

        public void RemoveState(string sceneId, string stateName)
        {
            var index = _sceneStatesInfos.FindIndex(s => s.sceneId == sceneId);
            if (index < 0)
                return;
            _sceneStatesInfos[index].stateInfos.RemoveAll(s => s.name == stateName);
            MarkDirtyIfEditor();
        }

        private void MarkDirtyIfEditor()
        {
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
#endif
        }

        [Serializable]
        public struct SceneStatesInfo
        {
            public string sceneId;
            public List<StateInfo> stateInfos;
        }
    }

    [Serializable]
    public struct GameObjectStateInfo
    {
        public string id;
        public bool active;
        public ComponentInfo[] componentInfos;


        public static GameObjectStateInfo CreateInfo(GameObject gameObject)
        {
            if (gameObject.GetComponent<UID>() is not { } uid)
            {
                uid = gameObject.AddComponent<UID>();
            }

            var componentInfos = gameObject.GetComponentsInChildren<Behaviour>().Where(b=>b).Select(b => new ComponentInfo
            {
                enable = b.enabled,
                name = b.GetType().FullName
            }).ToArray();

            return new GameObjectStateInfo
            {
                id = uid.ID,
                active = gameObject.activeSelf,
                componentInfos = componentInfos
            };
        }

        public void Restore(GameObject gameObject)
        {
            if (gameObject.GetComponent<UID>() is not { } uid || uid.ID != id)
            {
                return;
            }

            gameObject.SetActive(active);

            foreach (var behaviour in gameObject.GetComponentsInChildren<Behaviour>().Where(b=>b))
            {
                var info = (componentInfos ?? Array.Empty<ComponentInfo>()).FirstOrDefault(c =>
                    c.name == behaviour.GetType().FullName);
                behaviour.enabled = string.IsNullOrEmpty(info.name) ? behaviour.enabled : info.enable;
            }
        }

        [Serializable]
        public struct ComponentInfo
        {
            public string name;
            public bool enable;
        }
    }

    public partial class StateInfos:IEnumerable<StateInfos.SceneStatesInfo>
    {
        public IEnumerator<SceneStatesInfo> GetEnumerator() => _sceneStatesInfos.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    [Serializable]
    public struct StateInfo
    {
        public string name;
        public GameObjectStateInfo[] GameObjectInfos;
        public SceneInfo sceneInfo;
    }
    
    [Serializable]
    public class SceneInfo
    {
        public Vector3 pos;
        public Vector3 rot;
        public Vector3 pivot;
        public float size;
        public bool twoD;
        public bool ortho;
        
        #if UNITY_EDITOR
        public static SceneInfo Create(UnityEditor.SceneView aView)
        {
            return new SceneInfo()
            {
                pos = aView.camera.transform.position,
                pivot = aView.pivot,
                rot = aView.rotation.eulerAngles,
                size = aView.size,
                twoD = aView.in2DMode,
                ortho = aView.orthographic
            };
        }
        public void Restore(UnityEditor.SceneView aView)
        {
            aView.in2DMode = twoD;
            aView.camera.transform.position = pos;
            aView.LookAt(pivot, Quaternion.Euler(rot), size, ortho);
        }
        #endif
    }
}