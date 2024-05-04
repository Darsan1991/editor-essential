using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace DGames.Essentials.Editor
{
    public abstract class AssetItemDrawer
    {
        public abstract event System.Action<AssetItem, Object> Clicked;
        public abstract event System.Action<AssetItem, Object> DoubleClicked;
        public abstract event System.Action<AssetItem, Object> LabelClicked;
        // public abstract event System.Action<AssetItem, Object> DragStarted;
        // public abstract event System.Action<AssetItem, Object,bool> Dragged;
        public abstract Action Repaint { get; set; }
        public float Width { get; set; }
        public abstract IEnumerable<(AssetItem,ItemWithLabel[])> DrawAssets(IEnumerable<AssetItem> items);


       
        
        public struct ItemWithLabel
        {
            public Rect TotalRect { get; set; }
            public Rect Label { get; set; }
            
            public void Move(Vector2 delta)
            {
                var label = Label;
                label.position += delta;
                Label = label;
                var totalRect = TotalRect;
                totalRect.position += delta;
                TotalRect = totalRect;

           
            }
        }
    }
}