using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Xceed.Wpf.AvalonDock.Controls;
using Xceed.Wpf.AvalonDock.Layout;
using Xceed.Wpf.AvalonDock.Layout.Serialization;

namespace Xceed.Wpf.AvalonDock.ExtendedAvalonDock.Controls
{
    public class DockingManagerBase : Control
    {
        #region Internal Fields
        private static string _layoutFilePath;
        private bool _canSerialize;
        private LayoutRoot _layoutRootBeforeDeSerializing;
        public ObservableCollection<ExtendedLayoutAnchorable> AnchorableChildPanes { get; set; }
        public IList<ExtendedLayoutAnchorable> AnchorableToolsPanels { get; set; }
        #endregion //Internal Fields

        #region External Properties
        public virtual bool IsAutoHiding { get; set; }
        public virtual bool IsFloating { get; set; }
        public virtual bool IsMiddleBottomSideEnabled { get; set; }
        #endregion External Properties

        #region Constructors
        public DockingManagerBase()
        {
            _layoutFilePath = null;
            _canSerialize = true;
            AnchorableChildPanes = new ObservableCollection<ExtendedLayoutAnchorable>();
            AnchorableToolsPanels = new List<ExtendedLayoutAnchorable>();
        }
        #endregion //Constructors
        #region External Methods
        #endregion //External Methods
        public virtual void AnchorableTabItemPreviewMouseDown(ExtendedLayoutAnchorable anchorable, MouseButtonEventArgs e) { }
        public virtual int GetTabItemIndex(object tabItem) { return 0; }
        public virtual void SetTabItemIndex(object tabItem, int index) { }
        public virtual void PaneContentLoaded(ExtendedLayoutAnchorable anchorable, bool isLoaded) { }
        public virtual void PreviewToggleAutoHide(ExtendedLayoutAnchorable customLayoutAnchorable) { }
        public virtual void ToggleAutoHided(ExtendedLayoutAnchorable customLayoutAnchorable) { }
        public virtual void PreviewLayoutDocumentColllectionChanged(object sender, NotifyCollectionChangedEventArgs e) { }
        public virtual bool CanForceDeserialization() { return true; }

        protected void HideShowAnchorable(ExtendedLayoutAnchorable layoutAnchorable, bool isShow = false)
        {
            if (layoutAnchorable == null) return;
            if (layoutAnchorable.IsAutoHidden)
                layoutAnchorable.ToggleAutoHide();
            if (!isShow)
                layoutAnchorable.ToggleAutoHide();
        }
        protected void DeserializeAndLoad(List<AnchorableType> pinAnchorablesOnLoad)
        {
            var manager = this as DockingManager;
            if (manager == null) return;
            try
            {
                DeserializeAndUpdate(pinAnchorablesOnLoad);
            }
            catch (Exception ex)
            {
                AnchorableChildPanes = new ObservableCollection<ExtendedLayoutAnchorable>();
                AnchorableToolsPanels = new List<ExtendedLayoutAnchorable>();
                manager.Layout = _layoutRootBeforeDeSerializing;
                if (CanForceDeserialization())
                    DeserializeAndUpdate(pinAnchorablesOnLoad, false);
                else
                    Application.Current.Shutdown();
            }
        }
        protected void SerializeAndUnLoad()
        {
            if (!_canSerialize) return;
            SerializeAndUpdate();
        }
        protected virtual void SetDefaultLayout()
        {
            var manager = this as DockingManager;
            if (manager == null || !_canSerialize) return;
            var assembly = GetType().Assembly;
            string result = null;
            using (var stream = assembly.GetManifestResourceStream(GetDefaultLayoutPath()))
            {
                if (stream == null) return;
                using (var reader = new StreamReader(stream))
                {
                    result = reader.ReadToEnd();
                }
            }
            File.WriteAllText(GetLayoutFilePath(), result);
            _canSerialize = false;
        }
        protected virtual string GetLayoutPath() { return ""; }
        private string GetLayoutFilePath() { return _layoutFilePath ?? (_layoutFilePath = GetLayoutPath()); }
        protected virtual string GetDefaultLayoutPath() { return ""; }
        #region Internal Methods
        #region Serialization
        private void DeserializeAndUpdate(List<AnchorableType> pinAnchorablesOnLoad, bool isDeserialize = true)
        {
            var manager = this as DockingManager;
            if (manager == null) return;
            /*this is more necessary to get BottomSidePanel, for integrating new added LayoutAnchorables*/
            var notDeserializedSidePanels = new[] { manager.LeftSidePanel, manager.RightSidePanel, manager.TopSidePanel, manager.BottomSidePanel };
            var notDeserializedAnchorables = manager.Layout.Descendents().OfType<ExtendedLayoutAnchorable>().ToList();
            var isDeserialized = isDeserialize && Deserialize();
            var deserializedSidePanels = new[] { manager.LeftSidePanel, manager.RightSidePanel, manager.TopSidePanel, manager.BottomSidePanel };
            var anchorables = manager.Layout.Descendents().OfType<ExtendedLayoutAnchorable>().ToList();

            if (isDeserialized)
            {
                UpdateDeletedAnchorables(notDeserializedAnchorables, anchorables);
                for (var i = 0; i < notDeserializedSidePanels.Length; i++)
                    UpdateNotSerializedAnchorables(notDeserializedSidePanels[i], deserializedSidePanels[i], anchorables);
                UpdateProperties(notDeserializedAnchorables, anchorables);
            }

            if (!isDeserialized)
            {
                LayoutAnchorable anchorable = null;
                foreach (var layoutAnchorGroupControl in manager.BottomSidePanel.Children)
                {
                    var layoutContainer = layoutAnchorGroupControl.Model as ILayoutContainer;
                    if (layoutContainer == null) continue;
                    var customLayoutAnchorable = layoutContainer.Children.FirstOrDefault() as ExtendedLayoutAnchorable;
                    if (customLayoutAnchorable == null) continue;
                    anchorable = customLayoutAnchorable;
                    break;
                }
                //we should reset ToggleAutoHide for side controls layout, for inserting it in LayoutPanelControl
                //also we can not change for to foreach because ToggleAutoHide is removing layout from container
                if (anchorable != null)
                    anchorable.ToggleAutoHide();
                foreach (var anchorableType in pinAnchorablesOnLoad)
                {
                    var anchor = anchorables.FirstOrDefault(x => x.Type == anchorableType);
                    if (anchor != null)
                        anchor.ToggleAutoHide();
                }
            }
            var layoutContainers = new List<ILayoutContainer>();
            foreach (var layoutAnchorable in anchorables)
            {
                if (AnchorableChildPanes.Contains(layoutAnchorable)) continue;
                if (isDeserialized && !layoutContainers.Contains(layoutAnchorable.Parent))
                    layoutContainers.Add(layoutAnchorable.Parent);
                AnchorableChildPanes.Add(layoutAnchorable);
                if (layoutAnchorable is ExtendedLayoutAnchorable)
                    AnchorableToolsPanels.Add(layoutAnchorable as ExtendedLayoutAnchorable);
            }
            foreach (var layoutContainer in layoutContainers)
            {
                var layoutAnchorable = layoutContainer.Children.FirstOrDefault() as ExtendedLayoutAnchorable;
                if (layoutAnchorable != null && layoutAnchorable.IsToggleAutoHide)
                    layoutAnchorable.ToggleAutoHide();
            }
            IsAutoHiding = false;
        }
        private void UpdateDeletedAnchorables(List<ExtendedLayoutAnchorable> notDeserializedAnchorables, List<ExtendedLayoutAnchorable> anchorables)
        {
            var removeAnchorables = new List<ExtendedLayoutAnchorable>();
            foreach (var layoutAnchorable in anchorables)
            {
                var isContains = notDeserializedAnchorables.Any(x => x.ContentId == layoutAnchorable.ContentId);
                if (isContains) continue;
                removeAnchorables.Add(layoutAnchorable);
            }
            foreach (var layoutAnchorable in removeAnchorables)
            {
                anchorables.Remove(layoutAnchorable);
                var parent = layoutAnchorable.Parent;
                parent.RemoveChild(layoutAnchorable);
            }
        }
        private void UpdateProperties(List<ExtendedLayoutAnchorable> notDeserializedAnchorables, List<ExtendedLayoutAnchorable> anchorables)
        {
            foreach (var notDeserializedAnchorable in notDeserializedAnchorables)
            {
                var anchorable = anchorables.FirstOrDefault(x => x.ContentId == notDeserializedAnchorable.ContentId);
                if (anchorable == null) continue;
                anchorable.SetCustomProperties(notDeserializedAnchorable);
            }
        }
        private void UpdateNotSerializedAnchorables(LayoutAnchorSideControl notSerializedSidePanel, LayoutAnchorSideControl serializedSidePanel, List<ExtendedLayoutAnchorable> anchorables)
        {
            if (notSerializedSidePanel == null || notSerializedSidePanel.Model == null) return;
            if (ReferenceEquals(notSerializedSidePanel, serializedSidePanel)) return;
            var notSerializedAnchorables = new List<ExtendedLayoutAnchorable>();
            var xamlnotSerializedAnchorables = notSerializedSidePanel.Model.Descendents().OfType<ExtendedLayoutAnchorable>().ToList();

            foreach (var layoutAnchorable in xamlnotSerializedAnchorables)
            {
                var anchorable = layoutAnchorable;
                var isContains = anchorables.Any(x => x.ContentId == anchorable.ContentId);
                if (!isContains)
                    notSerializedAnchorables.Add(layoutAnchorable);
            }

            var layoutAnchorSide = serializedSidePanel.Model as LayoutAnchorSide;
            if (layoutAnchorSide != null && notSerializedAnchorables.Count > 0)
            {
                var layoutAnchorGroup = layoutAnchorSide.Children.FirstOrDefault();
                if (layoutAnchorGroup == null)
                {
                    layoutAnchorGroup = new LayoutAnchorGroup();
                    layoutAnchorSide.Children.Add(layoutAnchorGroup);
                }
                var firstChild = layoutAnchorGroup.Children.FirstOrDefault() as ExtendedLayoutAnchorable;
                var isPinned = firstChild != null && firstChild.IsPinned;
                var isToggleAutoHide = firstChild != null && firstChild.IsToggleAutoHide;
                foreach (var notSerializedAnchorable in notSerializedAnchorables)
                {
                    notSerializedAnchorable.IsPinned = isPinned;
                    notSerializedAnchorable.IsToggleAutoHide = isToggleAutoHide;
                    layoutAnchorGroup.InsertChildAt(0, notSerializedAnchorable);
                    anchorables.Add(notSerializedAnchorable);
                }
            }
        }
        private void SerializeAndUpdate()
        {
            var manager = this as DockingManager;
            if (manager == null || !_canSerialize) return;
            foreach (var layoutAnchorable in AnchorableChildPanes)
            {
                if (layoutAnchorable.IsAnchorablePaneAutoHidden && !layoutAnchorable.IsLayoutVisible && layoutAnchorable.IsAutoHidden)
                {
                    layoutAnchorable.IsToggleAutoHide = true;
                    continue;
                }
                if (layoutAnchorable.CanHide || layoutAnchorable.IsFloating)
                {
                    layoutAnchorable.IsToggleAutoHide = false;
                    continue;
                }

                layoutAnchorable.IsToggleAutoHide = !layoutAnchorable.IsAutoHidden;
                HideShowAnchorable(layoutAnchorable);
            }
            var layoutDocuments = manager.Layout.Descendents().OfType<LayoutDocument>().ToArray();
            foreach (var layoutDocument in layoutDocuments)
                layoutDocument.Close();
            Serialize();
        }
        private bool Deserialize()
        {
            var manager = this as DockingManager;
            if (manager == null) return false;
            _layoutRootBeforeDeSerializing = manager.Layout;
            var serializer = new XmlLayoutSerializer(this as DockingManager);
            serializer.LayoutSerializationCallback += (s, args) =>
            {
                args.Content = args.Content;
            };

            bool isFileExists = File.Exists(GetLayoutFilePath());
            if (isFileExists)
                serializer.Deserialize(GetLayoutFilePath());
            return isFileExists;
        }
        private void Serialize()
        {
            var manager = this as DockingManager;
            if (manager == null || !_canSerialize) return;
            var serializer = new XmlLayoutSerializer(manager);
            serializer.Serialize(GetLayoutFilePath());
        }
        #endregion //Serialization
        #endregion //Internal Methods
    }
}
