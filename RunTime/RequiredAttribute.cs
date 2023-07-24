using UnityEngine;

namespace DGames.Essentials.Attributes
{
    public sealed class RequiredAttribute : PropertyAttribute
    {
        public string Message { get; }

        public RequiredAttribute(string message=null)
        {
            Message = string.IsNullOrEmpty(message) ? "Required!" : message;
        }
    }
}