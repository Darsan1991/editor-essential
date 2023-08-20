#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace DGames.Essentials.EditorHelpers
{
    public static class AudioEditorUtils
    {
        private static readonly List<AudioSource> _activeAudios = new();

        static AudioEditorUtils()
        {
            EditorApplication.update += EditorApplicationOnUpdate;
        }

        private static void EditorApplicationOnUpdate()
        {
            _activeAudios.RemoveAll(a => a == null);
            foreach (var audioSource in _activeAudios.Where(a => !a.isPlaying).ToList())
            {
                _activeAudios.Remove(audioSource);
                Object.DestroyImmediate(audioSource.gameObject);
            }
        }

        public static void PlayClip(AudioClip clip)
        {
            var gameObject = new GameObject
            {
                hideFlags = HideFlags.DontSave | HideFlags.HideInHierarchy | HideFlags.HideInInspector |
                            HideFlags.NotEditable
            };
            var audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.clip = clip;
            audioSource.Play();
            _activeAudios.Add(audioSource);
        }
    }
}
#endif