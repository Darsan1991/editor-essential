using DGames.Essentials.EditorHelpers;
using UnityEngine;

namespace DGames.Essentials.Attributes
{
    public sealed class HelpBoxAttribute : PropertyAttribute
    {
       
        public string Message { get; }
        public MessageType Type { get; }



        public HelpBoxAttribute(string message, MessageType type=MessageType.Info)
        {
            Type = type;
            Message = message;
        }
        
#if UNITY_EDITOR
        public bool ConditionSatisfy(UnityEditor.SerializedProperty so)
        {
            return !so.IsArrayChildElement();
        }
#endif

    }
    
    public enum MessageType
    {
        None,Info,Warning,Error
    }
}