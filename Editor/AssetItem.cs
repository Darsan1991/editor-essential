using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace DGames.Essentials.Editor
{
    public class AssetItem
    {
        public string RenamingText { get; set; }

        public bool IsRenaming
        {
            get => _isRenaming;
            set
            {
                if (value)
                    RenamingText = Name;
                _isRenaming = value;
            }
        }

        public bool IsHighlighting { get; set; }

        public bool IsCreating
        {
            get => _isCreating;
            set
            {
                _isCreating = value;
                if (!value)
                    _icon = null;
            }
        }

        public string Path { get; set; }
        public bool Expanded { get; set; }

        public Texture Icon
        {
            get
            {
                return _icon ??= Object != null && AssetPreview.GetAssetPreview(Object)
                    ? AssetPreview.GetAssetPreview(Object)
                    : !string.IsNullOrEmpty(Path)
                        ? AssetDatabase.GetCachedIcon(Path)
                        : Object
                            ? AssetDatabase.FindAssets($"t:{Object.GetType().Name}")
                                .Select(id => AssetDatabase.GetCachedIcon(AssetDatabase.GUIDToAssetPath(id)))
                                .FirstOrDefault()
                            : null;
            }
        }

        public Object Object
        {
            get
            {
                if (!_object)
                {
                    _object = AssetDatabase.LoadMainAssetAtPath(Path);
                    if(_object)
                        _icon = null;
                }

                return _object;
            }
            set => _object = value;
        }

        public string Name => Object != null
            ? Object.name
            : Path.Split(System.IO.Path.DirectorySeparatorChar).Last().Split('.').FirstOrDefault();

        private readonly List<Object> _subAssets = new();
        private Object _object;
        private Texture _icon;
        private bool _isCreating;
        private bool _isRenaming;

        public IEnumerable<Object> SubAssets
        {
            get
            {
                if (Object is not (GameObject or SceneAsset) && !_subAssets.Any())
                {
                    _subAssets.AddRange(AssetDatabase.LoadAllAssetsAtPath(Path).Except(new[] { Object }));
                }


                return _subAssets;
            }
        }

        public AssetItem(string path)
        {
            Path = path;
        }
    }
}