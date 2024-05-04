using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace DGames.Essentials.Editor
{
    public class EditorInputDialog : EditorWindow
    {
        string  description, inputText;
        string  okButton, cancelButton;
        bool    initializedPosition = false;
        Action  onOKButton;
 
        bool    shouldClose = false;
 
        #region OnGUI()
        void OnGUI()
        {
            // Check if Esc/Return have been pressed
            var e = Event.current;
            if( e.type == EventType.KeyDown )
            {
                switch( e.keyCode )
                {
                    // Escape pressed
                    case KeyCode.Escape:
                        shouldClose = true;
                        break;
 
                    // Enter pressed
                    case KeyCode.Return:
                    case KeyCode.KeypadEnter:
                        onOKButton?.Invoke();
                        shouldClose = true;
                        break;
                }
            }
 
            if( shouldClose ) {  // Close this dialog
                Close();
                //return;
            }
 
            // Draw our control
            var rect = EditorGUILayout.BeginVertical();
 
            EditorGUILayout.Space( 12 );
            EditorGUILayout.LabelField( description );
 
            EditorGUILayout.Space( 8 );
            GUI.SetNextControlName( "inText" );
            inputText = EditorGUILayout.TextField( "", inputText );
            GUI.FocusControl( "inText" );   // Focus text field
            EditorGUILayout.Space( 12 );
 
            // Draw OK / Cancel buttons
            var r = EditorGUILayout.GetControlRect();
            r.width /= 2;
            if( GUI.Button( r, okButton ) ) {
                onOKButton?.Invoke();
                shouldClose = true;
            }
 
            r.x += r.width;
            if( GUI.Button( r, cancelButton ) ) {
                inputText = null;   // Cancel - delete inputText
                shouldClose = true;
            }
 
            EditorGUILayout.Space( 8 );
            EditorGUILayout.EndVertical();
 
            // Force change size of the window
            if( rect.width != 0 && minSize != rect.size ) {
                minSize = maxSize = rect.size;
            }
 
            // Set dialog position next to mouse position
            if( !initializedPosition ) {
                var mousePos = GUIUtility.GUIToScreenPoint( Event.current.mousePosition );
                position = new Rect( mousePos.x + 32, mousePos.y, position.width, position.height );
                initializedPosition = true;
            }
        }
        #endregion OnGUI()
 
        #region Show()
        /// <summary>
        /// Returns text player entered, or null if player cancelled the dialog.
        /// </summary>
        /// <param name="title"></param>
        /// <param name="description"></param>
        /// <param name="inputText"></param>
        /// <param name="okButton"></param>
        /// <param name="cancelButton"></param>
        /// <returns></returns>
        public static string Show( string title, string description, string inputText, string okButton = "OK", string cancelButton = "Cancel" )
        {
            string ret = null;
            //var window = EditorWindow.GetWindow<InputDialog>();
            var window = CreateInstance<EditorInputDialog>();
            window.titleContent = new GUIContent( title );
            window.description = description;
            window.inputText = inputText;
            window.okButton = okButton;
            window.cancelButton = cancelButton;
            window.onOKButton += () => ret = window.inputText;
            window.ShowModal();
 
            return ret;
        }

       
        #endregion Show()
    }

    public class PopUpWithContextTest : PopupWindowContent
    {
        public override void OnGUI(Rect rect)
        {
            EditorGUI.DrawRect(rect,Color.red);
        }
    }

    public static class EditorUtils
    {
        [MenuItem("Tools/Toggle Inspector Mode &d")]//Change the shortcut here
        static void ToggleInspectorDebug()
        {
            var targetInspector = EditorWindow.mouseOverWindow; // "EditorWindow.focusedWindow" can be used instead
 
            if (targetInspector != null  && targetInspector.GetType().Name == "InspectorWindow")
            {
                var type = Assembly.GetAssembly(typeof(UnityEditor.Editor)).GetType("UnityEditor.InspectorWindow");    //Get the type of the inspector window to find out the variable/method from
                var field = type.GetField("m_InspectorMode", BindingFlags.NonPublic | BindingFlags.Instance);    //get the field we want to read, for the type (not our instance)
                
                var mode = (InspectorMode)field.GetValue(targetInspector);                                    //read the value for our target inspector
                mode = (mode == InspectorMode.Normal ? InspectorMode.Debug : InspectorMode.Normal);                    //toggle the value
                //Debug.Log("New Inspector Mode: " + mode.ToString());
                
                var method = type.GetMethod("SetMode", BindingFlags.NonPublic | BindingFlags.Instance);          //Find the method to change the mode for the type
                method!.Invoke(targetInspector, new object[] {mode});                                                    //Call the function on our targetInspector, with the new mode as an object[]
            
                targetInspector.Repaint();       //refresh inspector
            }
        }
    }

}