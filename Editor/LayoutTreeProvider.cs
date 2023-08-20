using System.Collections.Generic;
using System.Linq;

namespace DGames.Essentials.Editor
{
    public class LayoutTreeProvider
    {
        private readonly List<LayoutGroupElement> _groups = new();

        public IEnumerable<LayoutGroupElement> Groups => _groups;

        public LayoutTreeProvider(IEnumerable<ItemInfo> items)
        {
            foreach (var itemInfo in items)
            {
                ProcessItemInfo(itemInfo);
            }
        }

        private void ProcessItemInfo(ItemInfo itemInfo)
        {
            LayoutElement element=null;
            foreach (var info in itemInfo.Infos)
            {
                element = ToLayoutElement(element, info);
            }
        
            element!.AddItem(itemInfo.Item);
        }

        private LayoutElement ToLayoutElement(LayoutElement parent, LayoutInfo info)
        {
            var paths = info.FullSegments.ToArray();
            var element = parent;
            for (var i = 0; i < paths.Length; i++)
            {
                element = i % 2 == 0 ? ProcessGroupElement(element, paths[i], info.LayoutType) : ProcessLayoutElement((LayoutGroupElement)element, paths[i]);
            }

            return element;
        }

        private LayoutGroupElement ProcessGroupElement(LayoutElement parent, string name,object layoutType)
        {
            var groupElement = (parent?.Groups ?? _groups).FirstOrDefault(g=>g.Name == name && Equals(g.LayoutType, layoutType));

            if (groupElement == null)
            {
                groupElement = new LayoutGroupElement
                {
                    Name = name,
                    LayoutType = layoutType,
                    Parent = parent
                };
            
                if(parent!=null)
                    parent.AddItem(groupElement);
                else
                {
                    _groups.Add(groupElement);
                }
            }
            return groupElement;
        }
    
        private LayoutElement ProcessLayoutElement(LayoutGroupElement parent, string name)
        {
            var element = parent.Children.FirstOrDefault(e=>e.Name == name );

            if (element == null)
            {
                element = new LayoutElement()
                {
                    Name = name,
                    Parent = parent
                };
                parent.AddChildren(element);
            }
            return element;
        }

        public struct ItemInfo
        {
            public object Item { get; set; }
            public LayoutInfo[] Infos { get; set; }
        }
    }
}