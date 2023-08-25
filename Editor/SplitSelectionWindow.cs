using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace DGames.Essentials.Editor
{
    public abstract class SplitSelectionWindow : EditorWindow, IContentViewer, IWindow, IMenuContentEditorProvider
    {
        public event Action<string, object> EventEmitted;

        [SerializeField] protected int selectedIndex;


        protected VisualElement rightPane;
        protected ListView leftPane;
        protected VisualElement topBar;

        protected readonly Dictionary<IMenuItem, BaseMenuItemEditor> itemVsEditors = new();


        public virtual void CreateGUI()
        {
            var splitView = new TwoPaneSplitView(0, 250, TwoPaneSplitViewOrientation.Horizontal);

            topBar = new VisualElement();
            rootVisualElement.Add(topBar);
            rootVisualElement.Add(splitView);

            AddLeftPane(splitView);
            AddRightPane(splitView);

            SetupLeftPane();

            Refresh();
        }

        private void SetupLeftPane()
        {
            leftPane.makeItem = LeftPaneMakeItem;
            leftPane.bindItem = OnBindItem;

            leftPane.onSelectionChange += OnItemSelectionChange;
            leftPane.itemsSourceChanged += LeftPaneOnItemsSourceChanged;
            leftPane.selectedIndex = selectedIndex;
            leftPane.onSelectionChange += (_) => { selectedIndex = leftPane.selectedIndex; };
        }

        protected virtual void OnBindItem(VisualElement element, int index)
        {
            var paddingLabel = (Label)element.Children().ElementAt(0);

            var image = (Image)element.Children().ElementAt(1);
            var label = (Label)element.Children().ElementAt(2);

            var menuItem = ((IList<IMenuItem>)leftPane.itemsSource)[index];
            paddingLabel.text = new string(menuItem.FullName.TakeWhile(c => c == ' ').Select(_=> 'O').ToArray());
            label.text = menuItem.FullName.TrimStart();
            image.image = menuItem.Icon;

            image.visible = menuItem.Icon != null;
            // label.style.paddingLeft = image.visible ? 20 : 5;
        }

        protected virtual void LeftPaneOnItemsSourceChanged()
        {
            RemoveAllEditors();
            CacheEditorsIfNotAlready();
        }

        protected void RemoveAllEditors()
        {
            itemVsEditors.Keys.ToList().ForEach(RemoveEditor);
        }

        protected virtual VisualElement LeftPaneMakeItem()
        {
            var box = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                    flexGrow = 1f,
                    flexShrink = 0f,
                    flexBasis = 0f,
                    // paddingBottom = 5,
                    borderBottomWidth = 1,
                    alignItems = new StyleEnum<Align>(Align.Center),
                    borderBottomColor = new StyleColor(new Color(0.15f, 0.15f, 0.15f))
                }
            };
            box.Add(new Label
                { style =
                {
                    unityTextAlign = new StyleEnum<TextAnchor>(TextAnchor.MiddleLeft),//backgroundColor = Color.red,
                    color = Color.clear
                } });
            box.Add(new Image
            {
                style = { minWidth = 25,maxWidth = 25,height = 20,paddingLeft = 10}
            });
            box.Add(new Label
                { style =
                {
                    unityTextAlign = new StyleEnum<TextAnchor>(TextAnchor.MiddleLeft),paddingLeft = 5
                } });


            return box;
        }

        private void AddRightPane(TwoPaneSplitView splitView)
        {
            var rightElement = new VisualElement();

            rightPane = new ScrollView(ScrollViewMode.VerticalAndHorizontal);
            rightPane.style.paddingRight = 20;
            rightPane.style.paddingTop = 20;
            rightPane.style.paddingLeft = 20;
            rightElement.Add(rightPane);
            splitView.Add(rightElement);
        }

        private void AddLeftPane(TwoPaneSplitView splitView)
        {
            leftPane = new ListView
            {
                selectionType = SelectionType.Single
            };


            var element = new VisualElement();
            element.Add(leftPane);
            splitView.Add(element);
        }

        public virtual void Refresh()
        {
            
            rightPane.Clear();
            DisposeMenuItems();
            var items = GetMenuItems().SelectMany(t => t).Cast<IMenuItem>().ToList();
            items.ForEach(item => item.ContentViewer = this);
            leftPane.itemsSource = items;

            leftPane.RefreshItems();
            selectedIndex = Mathf.Min(items.Count - 1, selectedIndex);
            leftPane.selectedIndex = selectedIndex;
        }

        private void DisposeMenuItems()
        {
            if (leftPane.itemsSource==null)
                return;
            foreach (var disposable in leftPane.itemsSource.OfType<IDisposable>())
            {
                disposable.Dispose();
            }
        }


        protected virtual void OnItemSelectionChange(IEnumerable<object> selectedItems)
        {
            // Clear all previous content from the pane
            rightPane.Clear();

            // Get the selected sprite
            var selectedItem = selectedItems.FirstOrDefault() as IMenuItem;
            if (selectedItem == null)
                return;

            CacheEditorsIfNotAlready();
                rightPane.Add(new IMGUIContainer(() => { OnDrawItemEditor(selectedItem); }));
        }

        protected virtual void OnDrawItemEditor(IMenuItem selectedItem)
        {
            if (itemVsEditors.ContainsKey(selectedItem))
                itemVsEditors[selectedItem]?.OnInspectorGUI();
        }


        public virtual void RefreshItem(IMenuItem item)
        {
            leftPane.RefreshItem(leftPane.itemsSource.IndexOf(item));
        }

        public virtual void SelectItem(IMenuItem item)
        {
            selectedIndex = leftPane.itemsSource.IndexOf(item);
            leftPane.SetSelection(selectedIndex);
        }


        protected void RemoveEditor(IMenuItem item)
        {
            if (itemVsEditors.ContainsKey(item))
            {
                itemVsEditors[item]?.OnExit();
                itemVsEditors.Remove(item);
            }
        }

        protected void CacheEditorsIfNotAlready()
        {
            if (itemVsEditors.Any())
            {
                return;
            }

            foreach (IMenuItem item in leftPane.itemsSource)
            {
                var baseMenuItemEditor = CreateEditor(item);
                itemVsEditors.Add(item, baseMenuItemEditor);
            }
        }


        protected void CallEventEmittedEvent(string eventName, object arg) => EventEmitted?.Invoke(eventName, arg);


        protected BaseMenuItemEditor CreateEditor(IMenuItem item)
        {
            var baseMenuItemEditor = BaseMenuItemEditor.CreateEditor(item);
            baseMenuItemEditor?.OnEnter(this);
            return baseMenuItemEditor;
        }

        public BaseMenuItemEditor GetEditor(IMenuItem item)
        {
            CacheEditorsIfNotAlready();
            return itemVsEditors.ContainsKey(item) ? itemVsEditors[item] : null;
        }

        private void OnDestroy()
        {
            DisposeMenuItems();
            RemoveAllEditors();
        }

        protected abstract IEnumerable<IMenuItem> GetMenuItems();
    }
}