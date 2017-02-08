using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using Odyssey.Controls;

namespace CustomControls.Odyssey
{   
    public class OdysseyItem
    {
        public string Name { get; set; }
        public long Id { get; set; }
        public long Parent { get; set; }
        public int Level { get; set; }
        public object Object { get; set; }

        public ObservableCollection<OdysseyItem> Childrens { get; set; }

        public OdysseyItem(string name) : this (name, 0, 0, 0) {}

        public OdysseyItem(string name, long id, long parent, int level, bool insertFake)
        {
            Name = name;
            Id = id;
            Parent = parent;
            Level = level;
            Childrens = new ObservableCollection<OdysseyItem>();
            if (insertFake) Childrens.Add(new OdysseyItem("Fake", 0, 0, 999));
        }

        public OdysseyItem(string name, long id, long parent, int level) : 
            this(name, id, parent, level, false)
        {
        }

        public OdysseyItem(string name, long id, long parent, int level, object obj) : 
            this(name, id, parent, level)
        {
            Object = obj;
        }
    }

    public struct ServItem
    {
        public string Cat { get; set; }
        public string Code { get; set; }
        public string SubCode { get; set; }

        public ServItem(string cat, string code, string subCode) : this()
        {
            Cat = cat;
            Code = code;
            SubCode = subCode;
        }
    }

    public static class OdysseyItemExtensions
    {
        public static OdysseyItem FindParentItem(this OdysseyItem item, long id)
        {
            if (item.Id == id) return item;

            foreach (var child in item.Childrens)
            {
                OdysseyItem i = child.FindParentItem(id);
                if (i != null) return i;
            }

            return null;
        }

        public static int GetItemSubCount(this OdysseyItem item, long id)
        {
            if (item.Id == id) return item.Childrens.Count;

            foreach (var child in item.Childrens)
            {
                if(child.Id == id)
                {
                    return child.Childrens.Count;
                }
            }

            return 0;
        }

        public static OdysseyItem FindParentServItem(this OdysseyItem item, ServItem servItem)
        {
            if (item.Object is ServItem)
            {
                ServItem objItem = (ServItem)item.Object;

                if ((servItem.SubCode.Length == 0) ? (objItem.Cat == servItem.Cat) : ((objItem.Cat == servItem.Cat) && (objItem.Code == servItem.Code))) return item;

                foreach (var child in item.Childrens)
                {
                    OdysseyItem i = child.FindParentServItem(servItem);
                    if (i != null) return i;
                }
            }

            return null;
        }
    }

    public class Odyssey : BreadcrumbBar
    {
        private Dictionary<long, string> shrinkValues;

        public bool Populated { get; set; }

        private const int SPACE = 60;
        
        public override void EndInit()
        {
            TraceBinding = new Binding("Name");
            BorderThickness = new Thickness(0);
            IsEditable = false;
            ToolTip = "";

            PathConversion += PathConvertOdysseyItems;
            PopulateItems += PopulateOdysseyItems;
            PathChanged += delegate { Populated = false; };
            SelectedBreadcrumbChanged += SelectedItemChanged;
            BreadcrumbItemDropDownOpened += ItemDropDownOpened;

            shrinkValues = new Dictionary<long, string>();

            base.EndInit();
        }

        private void ItemDropDownOpened(object sender, BreadcrumbItemEventArgs e)
        {
            Populated = false;
            
            if (e.Item != null && e.Item.ItemsSource != null)
            {
                if (UseDynamicSource && DynamicSource != null)
                {
                    ObservableCollection<OdysseyItem> collection =
                        (ObservableCollection<OdysseyItem>) e.Item.Items.SourceCollection;
                    if (SelectedLevel > 0 && DynamicSource.Count > 0)
                    {
                        collection.Clear();
                    }
                    else if (collection.Count == 1 && collection[0].Level == 999)
                    {
                        collection.Clear();    
                        e.Item.IsDropDownPressed = false;
                    }
                    
                    if (collection.Count == 0)
                    foreach (OdysseyItem item in DynamicSource)
                    {                  
                        collection.Add(item);
                    }
                }
            }
        }

        private void SelectedItemChanged(object sender, RoutedEventArgs e)
        {
            if (((Odyssey)sender).IsRootSelected && SelectedItem != null && ((OdysseyItem)SelectedItem).Id > 0)
            {
                if (!UseDynamicSource || ((ObservableCollection<object>)CollapsedTraces)[0] != null) 
                    SelectedId = SelectedParent = SelectedLevel = ChildrensCount = 0;
                else DynamicSource = null;
            }
        }

        private void PathConvertOdysseyItems(object sender, PathConversionEventArgs e)
        {
            if (ShrinkToWidth && e.Mode == PathConversionEventArgs.ConversionMode.DisplayToEdit)
            {
                double actualWidth = ActualWidth > 0 ? ActualWidth : 260;
                int index = e.DisplayPath.LastIndexOf(SeparatorString);
                if (index > -1) e.DisplayPath = e.DisplayPath.Substring(index + 1);
                double measured = MeasureWidth(e.DisplayPath);
                if (actualWidth > SPACE && measured > (actualWidth - SPACE))
                {
                    if (shrinkValues.ContainsKey(SelectedId)) shrinkValues[SelectedId] = e.DisplayPath;
                    else shrinkValues.Add(SelectedId, e.DisplayPath);
                    
                    int charsToDelete = (int)Math.Round(e.DisplayPath.Length*(1 - (actualWidth - SPACE)/measured)) + 3;

                    ((OdysseyItem) SelectedItem).Name =
                        e.DisplayPath.Remove(e.DisplayPath.Length - charsToDelete, charsToDelete) + "...";
                }
            }
        }

        private double MeasureWidth(string text)
        {
            FormattedText ft = new FormattedText(text, CultureInfo.CurrentCulture, FlowDirection.LeftToRight,
                 new Typeface(FontFamily, FontStyle, FontWeight, FontStretch),
                 FontSize, Brushes.Black);

            return ft.Width;
        }

        public bool ShrinkToWidth
        {
            get { return (bool)GetValue(ShrinkToWidthProperty); }
            set { SetValue(ShrinkToWidthProperty, value); }
        }

        public bool UseDynamicSource
        {
            get { return (bool)GetValue(UseDynamicSourceProperty); }
            set { SetValue(UseDynamicSourceProperty, value); }
        }

        public ObservableCollection<OdysseyItem> DynamicSource
        {
            get { return (ObservableCollection<OdysseyItem>)GetValue(DynamicSourceProperty); }
            set { SetValue(DynamicSourceProperty, value); }
        }

        public OdysseyItem ItemsSource
        {
            get { return (OdysseyItem)GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }

        public string SelectedText
        {
            get { return (string)GetValue(SelectedTextProperty); }
            set { SetValue(SelectedTextProperty, value); }
        }

        public long SelectedId
        {
            get { return (long)GetValue(SelectedIdProperty); }
            set { SetValue(SelectedIdProperty, value); }
        }

        public long SelectedParent
        {
            get { return (long)GetValue(SelectedParentProperty); }
            set { SetValue(SelectedParentProperty, value); }
        }

        public int SelectedLevel
        {
            get { return (int)GetValue(SelectedLevelProperty); }
            set { SetValue(SelectedLevelProperty, value); }
        }

        public int RootLevel
        {
            get { return (int)GetValue(RootLevelProperty); }
            set { SetValue(RootLevelProperty, value); }
        }

        public int ChildrensCount
        {
            get { return (int)GetValue(ChildrensCountProperty); }
            set { SetValue(ChildrensCountProperty, value); }
        }

        public object SelectedObject
        {
            get { return GetValue(SelectedObjectProperty); }
            set { SetValue(SelectedObjectProperty, value); }
        }

        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register("ItemsSource", typeof(OdysseyItem), typeof(Odyssey), new PropertyMetadata(ItemsSourceChangedCallback));

        public static readonly DependencyProperty SelectedIdProperty =
            DependencyProperty.Register("SelectedId", typeof(long), typeof(Odyssey));

        public static readonly DependencyProperty SelectedTextProperty =
            DependencyProperty.Register("SelectedText", typeof(string), typeof(Odyssey));

        public static readonly DependencyProperty SelectedParentProperty =
            DependencyProperty.Register("SelectedParent", typeof(long), typeof(Odyssey));

        public static readonly DependencyProperty SelectedLevelProperty =
            DependencyProperty.Register("SelectedLevel", typeof(int), typeof(Odyssey));

        public static readonly DependencyProperty RootLevelProperty =
            DependencyProperty.Register("RootLevel", typeof(int), typeof(Odyssey));

        public static readonly DependencyProperty ChildrensCountProperty =
            DependencyProperty.Register("ChildrensCount", typeof(int), typeof(Odyssey));

        public static readonly DependencyProperty SelectedObjectProperty =
            DependencyProperty.Register("SelectedObject", typeof(object), typeof(Odyssey));

        public static readonly DependencyProperty UseDynamicSourceProperty =
            DependencyProperty.Register("UseDynamicSource", typeof(bool), typeof(Odyssey), new PropertyMetadata(false));

        public static readonly DependencyProperty DynamicSourceProperty =
            DependencyProperty.Register("DynamicSource", typeof(ObservableCollection<OdysseyItem>), typeof(Odyssey), new PropertyMetadata(new ObservableCollection<OdysseyItem>()));

        public static readonly DependencyProperty ShrinkToWidthProperty =
            DependencyProperty.Register("ShrinkToWidth", typeof(bool), typeof(Odyssey), new PropertyMetadata(true));

        private static void ItemsSourceChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue != null)
            {
                d.SetValue(RootProperty, e.NewValue);
            }
        }

        private void PopulateOdysseyItems(object sender, BreadcrumbItemEventArgs e)
        {
            if (!Populated)
            {
                if (e.Item.SelectedItem != null && ((OdysseyItem) ((Odyssey) sender).SelectedItem).Level != ((OdysseyItem) e.Item.SelectedItem).Level)
                {
                    SelectedLevel = ((OdysseyItem) ((Odyssey) sender).SelectedItem).Level;
                    SelectedParent = ((OdysseyItem) ((Odyssey) sender).SelectedItem).Parent;
                    SelectedObject = ((OdysseyItem) ((Odyssey) sender).SelectedItem).Object;
                    ChildrensCount = ((OdysseyItem) ((Odyssey) sender).SelectedItem).Childrens.Count;
                    SelectedText = ((OdysseyItem) ((Odyssey) sender).SelectedItem).Name;
                    SelectedId = ((OdysseyItem) ((Odyssey) sender).SelectedItem).Id;
                    Populated = true;
                    return;
                }
                
                if (((OdysseyItem)e.Item.Data).Level == RootLevel && e.Item.SelectedItem != null)
                {
                    SelectedLevel = ((OdysseyItem)e.Item.SelectedItem).Level;
                    SelectedParent = ((OdysseyItem)e.Item.SelectedItem).Parent;
                    SelectedObject = ((OdysseyItem)e.Item.SelectedItem).Object;
                    ChildrensCount = ((OdysseyItem)e.Item.SelectedItem).Childrens.Count;
                    SelectedText = ((OdysseyItem)e.Item.SelectedItem).Name;
                    SelectedId = ((OdysseyItem)e.Item.SelectedItem).Id;
                }
                else
                {
                    SelectedLevel = ((OdysseyItem)e.Item.Data).Level;
                    SelectedParent = ((OdysseyItem)e.Item.Data).Parent;
                    SelectedObject = ((OdysseyItem)e.Item.Data).Object;
                    ChildrensCount = ((OdysseyItem)e.Item.Data).Childrens.Count;
                    SelectedText = ((OdysseyItem)e.Item.Data).Name;
                    if (!(((OdysseyItem)e.Item.Data).Id == 0 && ((OdysseyItem)e.Item.Data).Name != "Выберите"))
                        SelectedId = ((OdysseyItem)e.Item.Data).Id;
                }

                Populated = true;
            }

            e.Handled = true;
        }

        protected override void OnToolTipOpening(ToolTipEventArgs e)
        {
            if ((bool)GetValue(Validation.HasErrorProperty))
            {
                ToolTip = ((IList<ValidationError>) GetValue(Validation.ErrorsProperty))[0].ErrorContent;
                return;
            }
            if (SelectedItem != null && ((OdysseyItem)SelectedItem).Id >= 0)
            {
                ToolTip = !shrinkValues.ContainsKey(SelectedId) ? ((OdysseyItem) SelectedItem).Name : shrinkValues[SelectedId];
            }
            else e.Handled = true;
        }

        public string Text
        {
            get { return SelectedText; }
        }
    }
}