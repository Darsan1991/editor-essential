using System;
using System.Collections.Generic;
using System.Linq;

namespace DGames.Essentials.Editor
{
    public class LayoutGroupElement : LayoutElement
    {        
        public IEnumerable<LayoutElement> Children => _elements;
        private readonly List<LayoutElement> _elements = new();
        public object LayoutType { get; set; }

        
        public void AddChildren(LayoutElement item)
        {
            _elements.Add(item);
        }

        public override void AddItem(object item)
        {
            throw new Exception();
        }
    }
    
    public class LayoutElement 
    {
        public string Name { get; set; }
        

        public IEnumerable<object> Items => _items;
        private readonly List<object> _items = new();

        public IEnumerable<LayoutGroupElement> Groups => _items.OfType<LayoutGroupElement>();

        public string Path => Parent != null ? Parent.Path + "/" + Name : Name;
        
        public LayoutElement Parent { get; set; }

    

        public virtual void AddItem(object item)
        {
            _items.Add(item);
        }
    }

}