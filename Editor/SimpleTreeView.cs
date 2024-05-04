using System.Collections.Generic;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace DGames.Essentials.Editor
{
    class SimpleTreeView : TreeView
    {
        public SimpleTreeView(TreeViewState treeViewState)
            : base(treeViewState)
        {
            Reload();
        }

        protected override TreeViewItem BuildRoot()
        {
            // BuildRoot is called every time Reload is called to ensure that TreeViewItems 
            // are created from data. Here we create a fixed set of items. In a real world example,
            // a data model should be passed into the TreeView and the items created from the model.

            // This section illustrates that IDs should be unique. The root item is required to 
            // have a depth of -1, and the rest of the items increment from that.
            var root = new TreeViewItem
            {
                id = 0, depth = -1, displayName = "Root",
                children = new List<TreeViewItem>
                {
                    new()
                    {
                        id = 1, depth = 0, displayName = "Animals",
                        icon = (Texture2D)EditorGUIUtility.IconContent("d_Folder Icon").image,
                    },
                    new TreeViewItem { id = 2, depth = 0, displayName = "Mammals" },
                    new TreeViewItem { id = 3, depth = 0, displayName = "Tiger" },
                    new TreeViewItem { id = 4, depth = 2, displayName = "Elephant" },
                    new TreeViewItem { id = 5, depth = 2, displayName = "Okapi" },
                    new TreeViewItem { id = 6, depth = 2, displayName = "Armadillo" },
                    new TreeViewItem { id = 7, depth = 1, displayName = "Reptiles" },
                    new TreeViewItem { id = 8, depth = 2, displayName = "Crocodile" },
                    new TreeViewItem { id = 9, depth = 2, displayName = "Lizard" },

                    new TreeViewItem { id = 12, depth = 1, displayName = "Mammals" },
                    new TreeViewItem { id = 31, depth = 2, displayName = "Tiger" },
                    new TreeViewItem { id = 14, depth = 2, displayName = "Elephant" },
                    new TreeViewItem { id = 15, depth = 2, displayName = "Okapi" },
                    new TreeViewItem { id = 16, depth = 2, displayName = "Armadillo" },
                    new TreeViewItem { id = 17, depth = 1, displayName = "Reptiles" },
                    new TreeViewItem { id = 18, depth = 2, displayName = "Crocodile" },
                    new TreeViewItem { id = 19, depth = 2, displayName = "Lizard" },

                    new TreeViewItem { id = 22, depth = 1, displayName = "Mammals" },
                    new TreeViewItem { id = 21, depth = 2, displayName = "Tiger" },
                    new TreeViewItem { id = 24, depth = 2, displayName = "Elephant" },
                    new TreeViewItem { id = 25, depth = 2, displayName = "Okapi" },
                    new TreeViewItem { id = 26, depth = 2, displayName = "Armadillo" },
                    new TreeViewItem { id = 27, depth = 1, displayName = "Reptiles" },
                    new TreeViewItem { id = 28, depth = 2, displayName = "Crocodile" },
                    new TreeViewItem { id = 29, depth = 2, displayName = "Lizard" },
                }
            };

            //
            // // Utility method that initializes the TreeViewItem.children and .parent for all items.
            // SetupParentsAndChildrenFromDepths(root, allItems);
            //
            // Return root of the tree
            return root;
        }
        //
        // protected override TreeViewItem BuildRoot()
        // {
        //     throw new NotImplementedException();
        // }

        // protected override IList<TreeViewItem> BuildRows(TreeViewItem root)
        // {
        //     var allItems = new List<TreeViewItem>
        //     {
        //         new()
        //         {
        //             id = 1, depth = 0, displayName = "Animals",
        //             icon = (Texture2D)EditorGUIUtility.IconContent("d_Folder Icon").image,
        //         },
        //         new TreeViewItem { id = 2, depth = 1, displayName = "Mammals" },
        //         new TreeViewItem { id = 3, depth = 2, displayName = "Tiger" },
        //         new TreeViewItem { id = 4, depth = 2, displayName = "Elephant" },
        //         new TreeViewItem { id = 5, depth = 2, displayName = "Okapi" },
        //         new TreeViewItem { id = 6, depth = 2, displayName = "Armadillo" },
        //         new TreeViewItem { id = 7, depth = 1, displayName = "Reptiles" },
        //         new TreeViewItem { id = 8, depth = 2, displayName = "Crocodile" },
        //         new TreeViewItem { id = 9, depth = 2, displayName = "Lizard" },
        //         
        //         new TreeViewItem { id = 12, depth = 1, displayName = "Mammals" },
        //         new TreeViewItem { id = 31, depth = 2, displayName = "Tiger" },
        //         new TreeViewItem { id = 14, depth = 2, displayName = "Elephant" },
        //         new TreeViewItem { id = 15, depth = 2, displayName = "Okapi" },
        //         new TreeViewItem { id = 16, depth = 2, displayName = "Armadillo" },
        //         new TreeViewItem { id = 17, depth = 1, displayName = "Reptiles" },
        //         new TreeViewItem { id = 18, depth = 2, displayName = "Crocodile" },
        //         new TreeViewItem { id = 19, depth = 2, displayName = "Lizard" },
        //         
        //         new TreeViewItem { id = 22, depth = 1, displayName = "Mammals" },
        //         new TreeViewItem { id = 21, depth = 2, displayName = "Tiger" },
        //         new TreeViewItem { id = 24, depth = 2, displayName = "Elephant" },
        //         new TreeViewItem { id = 25, depth = 2, displayName = "Okapi" },
        //         new TreeViewItem { id = 26, depth = 2, displayName = "Armadillo" },
        //         new TreeViewItem { id = 27, depth = 1, displayName = "Reptiles" },
        //         new TreeViewItem { id = 28, depth = 2, displayName = "Crocodile" },
        //         new TreeViewItem { id = 29, depth = 2, displayName = "Lizard" },
        //     };
        //
        //     return allItems;
        // }
    }
}