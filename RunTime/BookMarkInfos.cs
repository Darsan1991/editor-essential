using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

#if UNITY_EDITOR
using UnityEditor.SceneManagement;
using UnityEditor;
#endif

namespace DGames.Essentials.Unity
{
    public partial class BookMarkInfos : ScriptableObject
    {
        private static BookMarkInfos _default;

        public static BookMarkInfos Default
        {
            get
            {
#if UNITY_EDITOR
                if (!_default)
                    _default = ScriptableEditorUtils.GetOrCreate<BookMarkInfos>(childrenPath: DEFAULT_FOLDER_PATH);
#endif
                return _default;
            }
        }

        public const string DEFAULT_FOLDER_PATH = "System";


        [SerializeField] private List<SceneSection> _sceneSections = new();
        [SerializeField] private List<AssetSection> _assetSections = new();

        public IEnumerable<Section> Sections => _assetSections.Cast<Section>().Concat(_sceneSections);

        private void Awake()
        {
            if (!_assetSections.Any())
            {
                _assetSections.Add(new AssetSection());
                MarkDirtyIfEditor();
            }
        }

        private void MarkDirtyIfEditor()
        {
#if UNITY_EDITOR
            EditorUtility.SetDirty(this);
#endif
        }

        public void AddBookMark(Object obj)
        {
            if (ContainsBookMark(obj))
            {
                return;
            }
            var section = Sections.FirstOrDefault(s => s.CanAdd(obj));

            if (section == null)
            {
                AddSection(obj);
            }
            else
            {
                section.Add(obj);
            }

            MarkDirtyIfEditor();
        }

        public void RemoveBookMark(string id)
        {
            var section = Sections.FirstOrDefault(section => section.Any(info => info.Id == id));

            section?.Remove(id);

            MarkDirtyIfEditor();
        }
        
        public void RemoveBookMark(Object obj)
        {
            var section = Sections.FirstOrDefault(section => section.Contains(obj));

            section?.Remove(obj);

            MarkDirtyIfEditor();
        }

        
        public void UpdateBookMark(string id)
        {
            var info = Sections.SelectMany(s=>s).FirstOrDefault(info =>info.Id == id);

            info?.Update();

            MarkDirtyIfEditor();
        }

        public bool ContainsBookMark(Object obj)
        {
            return Sections.Any(s => s.Contains(obj));
        }

        public void AddInSection(Object obj, Section section)
        {
            if (!section.CanAdd(obj))
            {
                return;
            }

            section.Add(obj);

            MarkDirtyIfEditor();
        }
        
    

        private void AddSection(Object obj)
        {
#if UNITY_EDITOR
            if (!string.IsNullOrEmpty(AssetDatabase.GetAssetPath(obj)))
            {
                var assetSection = new AssetSection { obj };
                _assetSections.Add(assetSection);
            }
            else if (obj is GameObject go)
            {
                _sceneSections.Add(new SceneSection(go.scene) { go });
            }
#endif
        }

        #if UNITY_EDITOR
        public void AddSectionFor(Scene scene)
        {
            var id = AssetDatabase.GUIDFromAssetPath(scene.path).ToString();
            if (_sceneSections.All(s => s.SceneID != id))
            {
                _sceneSections.Add(new SceneSection(scene));
            }
        }

        public bool HasSectionFor(Scene scene)
        {
            var id = AssetDatabase.GUIDFromAssetPath(scene.path).ToString();

            return _sceneSections.Any(s => s.SceneID == id);
        }
        #endif
        
        
    }
    
    
    public partial class BookMarkInfos{


    [Serializable]
        public abstract class Section : IEnumerable<BookMarkInfo>
        {
            [SerializeField] protected string name;
            public string Name => name;
            public abstract IEnumerator<BookMarkInfo> GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            protected Section(string name)
            {
                this.name = name;
            }

#if UNITY_EDITOR
            public abstract bool CanAdd(Object obj);

            public abstract void Add(Object obj);

            public abstract void Remove(string id);
            public abstract void Remove(Object obj);

            public abstract bool Contains(Object obj);
            
#endif
            
        }

        [Serializable]
        public class SceneSection : Section
        {
            [SerializeField] protected string sceneID;
            [SerializeField] protected List<GameObjectBookMarkInfo> bookMarkInfos = new();

            public string SceneID => sceneID;


            public SceneSection(Scene scene) : base(scene.name)
            {
#if UNITY_EDITOR
                sceneID = AssetDatabase.GUIDFromAssetPath(scene.path).ToString();
#endif
            }

            public override IEnumerator<BookMarkInfo> GetEnumerator()
            {
                return bookMarkInfos.GetEnumerator();
            }
#if UNITY_EDITOR
            public override bool CanAdd(Object obj)
            {
                if (obj is not GameObject go)
                {
                    return false;
                }

                var scenePath = AssetDatabase.GUIDToAssetPath(sceneID);

                return go.scene.path == scenePath && !Contains(obj);
            }
            
            public override void Add(Object obj)
            {
                if (!CanAdd(obj))
                {
                    return;
                }
                
                bookMarkInfos.Add(new GameObjectBookMarkInfo((GameObject)obj));
            }

            public override void Remove(string id)
            {
                var removeBookMarks = bookMarkInfos.Where(info => info.Id == id).ToArray();
                foreach (var bookMark in removeBookMarks)
                {
                    bookMark.OnRemove();
                    bookMarkInfos.Remove(bookMark);
                }
            }

            public override void Remove(Object obj)
            {
                Remove(GameObjectBookMarkInfo.GetId((GameObject)obj));
            }

            public override bool Contains(Object obj)
            {
                if (obj is not GameObject go)
                {
                    return false;
                }

                return go.GetComponent<UID>() is  { } uid && bookMarkInfos.Any(b => b.Id == uid.ID);
            }
#endif
        }
        
        
        [Serializable]
        public class AssetSection : Section
        {
            [SerializeField] protected List<AssetBookMarkInfo> bookMarkInfos = new();

            public AssetSection(string name=null) : base(name??"Assets")
            {
#if UNITY_EDITOR
#endif
            }

            public override IEnumerator<BookMarkInfo> GetEnumerator()
            {
                return bookMarkInfos.GetEnumerator();
            }
#if UNITY_EDITOR
            public override bool CanAdd(Object obj)
            {
                var assetPath = AssetDatabase.GetAssetPath(obj);
                if (string.IsNullOrEmpty(assetPath))
                {
                    return false;
                }

                return !Contains(obj);
            }
            
            public override void Add(Object obj)
            {
                if (!CanAdd(obj))
                {
                    return;
                }
                
                bookMarkInfos.Add(new AssetBookMarkInfo(obj));
            }

            public override void Remove(string id)
            {
                var removeBookMarks = bookMarkInfos.Where(info => info.Id == id).ToArray();
                foreach (var bookMark in removeBookMarks)
                {
                    bookMark.OnRemove();
                    bookMarkInfos.Remove(bookMark);
                }
                
            }

            public override void Remove(Object obj)
            {
                Remove(AssetBookMarkInfo.GetId(obj));
            }

            public override bool Contains(Object obj)
            {
                var assetPath = AssetDatabase.GetAssetPath(obj);
                if (string.IsNullOrEmpty(assetPath))
                {
                    return false;
                }

                var guid = AssetDatabase.AssetPathToGUID(assetPath);
                return bookMarkInfos.Any(b => b.Id == guid);
            }
#endif
        }


        [Serializable]
        public abstract class BookMarkInfo
        {
            [SerializeField] protected string name;
            public string Name => name;
            public abstract string Id { get; }
            public abstract void Action();

            protected BookMarkInfo(string name)
            {
                this.name = name;
            }

            public abstract void Update();

            public abstract void OnRemove();
        }
        
        [Serializable]
        public class AssetBookMarkInfo : BookMarkInfo
        {
            [SerializeField] protected string id;

            public override string Id => id;

            public AssetBookMarkInfo(Object obj) : base(new string(obj.name.Substring(0,Mathf.Min(20,obj.name.Length))))
            {
#if UNITY_EDITOR
                id = GetId(obj);
                AssetDatabase.SetLabels(obj,AssetDatabase.GetLabels(obj).Append("BookMark").ToArray());

#endif
            }
            
            public static string GetId(Object obj)
            {
                return AssetDatabase.GUIDFromAssetPath(AssetDatabase.GetAssetPath(obj)).ToString();
            }

            public override void Action()
            {
#if UNITY_EDITOR
                Selection.activeObject = AssetDatabase.LoadMainAssetAtPath(AssetDatabase.GUIDToAssetPath(id));
                EditorUtility.FocusProjectWindow();
#endif
            }
            
            public override void Update()
            {
#if UNITY_EDITOR

                name = AssetDatabase.LoadMainAssetAtPath(AssetDatabase.GUIDToAssetPath(id)).name;
                #endif
            }

            public override void OnRemove()
            {
#if UNITY_EDITOR
                var asset = AssetDatabase.LoadMainAssetAtPath(AssetDatabase.GUIDToAssetPath(id));
                AssetDatabase.SetLabels(asset,AssetDatabase.GetLabels(asset).Where(l=>l!="BookMark").ToArray());
#endif
            }
        }

        [Serializable]
        public class GameObjectBookMarkInfo : BookMarkInfo
        {
            [SerializeField] protected string sceneID;
            [SerializeField] protected string id;

            public override string Id => id;

            public GameObjectBookMarkInfo(GameObject go) : base(go.name)
            {
                id = GetId(go);

#if UNITY_EDITOR
                sceneID = AssetDatabase.GUIDFromAssetPath(go.scene.path).ToString();
#endif
            }

            public static string GetId(GameObject go,bool addComponent = true)
            {
                var component = go.GetComponent<UID>() ?? (addComponent ? go.AddComponent<UID>():null);
                return component != null ? component.ID : null;
            }

            public override void Action()
            {
#if UNITY_EDITOR
                var scenePath = AssetDatabase.GUIDToAssetPath(sceneID);
                var needToLoad = Enumerable.Range(0, SceneManager.sceneCount)
                    .All(i => SceneManager.GetSceneAt(i).path != scenePath);

                if (needToLoad)
                {
                    if (Application.isPlaying) return;
                  
                        EditorSceneManager.SaveModifiedScenesIfUserWantsTo( Enumerable.Range(0, SceneManager.sceneCount).Select(SceneManager.GetSceneAt).ToArray());
                        EditorSceneManager.OpenScene(scenePath);
                }

                Selection.activeObject = FindObjectsOfType<UID>(true).FirstOrDefault(uid => uid.ID == id);

                if (Event.current is { alt: true })
                {
                    SceneView.FrameLastActiveSceneView();
                }
                
                
#endif
            }
            
            public override void Update()
            {
#if UNITY_EDITOR

                var sel = FindObjectsOfType<UID>(true).FirstOrDefault(uid => uid.ID == id);
                name = sel? sel.gameObject.name : name;
#endif

            }

            public override void OnRemove()
            {
                
            }
        }
    }

    public partial class BookMarkInfos : IObjectIconsProvider
    {
        public IEnumerable<Texture> GetIcons(Object obj)
        {
            if (obj is not GameObject go || !ContainsBookMark(go))
            {
                return ArraySegment<Texture>.Empty;
            }

            return new []{EditorGUIUtility.IconContent("d_Favorite@2x").image};
        }


        
    }

    public interface IObjectIconsProvider
    {
        IEnumerable<Texture> GetIcons(Object obj);
    }
}