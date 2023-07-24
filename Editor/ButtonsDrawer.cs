using System.Collections.Generic;
using System.Reflection;
using DGames.Essentials.Attributes;

namespace DGames.Essentials.Editor
{
    public class ButtonsDrawer
    {
    
        private readonly List<Button> _buttons = new();

        public ButtonsDrawer(object target)
        {
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
            var methods = target.GetType().GetMethods(flags);

            foreach (var method in methods)
            {
                var buttonAttribute = method.GetCustomAttribute<ButtonAttribute>();

                if (buttonAttribute == null)
                    continue;

                _buttons.Add(Button.Create(method, buttonAttribute));
            }
        }

   
        public void DrawButtons(IEnumerable<object> targets)
        {
            foreach (var button in _buttons)
            {
                // ReSharper disable once PossibleMultipleEnumeration
                button.Draw(targets);
            }
        }
    }
}