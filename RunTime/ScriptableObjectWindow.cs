

using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace DGames.Essentials.Editor
{
#if UNITY_EDITOR
  public class ScriptableObjectWindow<T> : EditorWindow where T : ScriptableObject
  {
    private T _o;

    public T Object
    {
      get => _o;
      set
      {
        if (_o != null && _editor)
        {
          DestroyImmediate(_editor);
          _editor = null;
        }

        _o = value;

        if (_o != null)
        {
          _editor = (UnityEditor.Editor)UnityEditor.Editor.CreateEditor(value);
        }
      }
    }

    private UnityEditor.Editor _editor;

    public static void Open<TWindow>(T item,string title=null) where TWindow:ScriptableObjectWindow<T>
    {
      var window = GetWindow<TWindow>();
      window.Object = item;
      window.titleContent =  new GUIContent(!string.IsNullOrEmpty(title)? title : item ? item.name : typeof(T).Name);
      window.minSize = new Vector2(450, 300);
      window.Show();
    }
        
    public static void Open<TWindow>(string title=null) where TWindow : ScriptableObjectWindow<T>
    {
      Open<TWindow>(Resources.Load<T>(typeof(T).Name),title);
    }

    private void OnGUI()
    {
      if (_editor)
        _editor.OnInspectorGUI();
    }

    private void OnDestroy()
    {
      Object = null;
    }
  }
#endif
}