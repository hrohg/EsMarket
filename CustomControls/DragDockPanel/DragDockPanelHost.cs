

using System.Windows.Input;
using CustomControls.ValidPanel;

namespace RealEstateApp.CustomControls
{
    using System;
    using System.Windows;
    using System.Windows.Controls;
    using System.Collections.Generic;

    /// <summary>
    /// The dock panel docking positions.
    /// </summary>
    public enum MinimizedPositions
    {
        /// <summary>
        /// Docks the panels along the top.
        /// </summary>
        Top,

        /// <summary>
        /// Docks the panels along the bottom.
        /// </summary>
        Bottom,

        /// <summary>
        /// Docks the panels down the left side.
        /// </summary>
        Left,

        /// <summary>
        /// Docks the panels down the rights side,
        /// </summary>
        Right
    }

    /// <summary>
    /// A draggable, dockable, expandable host class.
    /// </summary>
    public class DragDockPanelHost : Canvas
    {
        #region Private members

        /// <summary>
        /// Can customize min panel height.
        /// </summary>
        private bool canCustomizeDockHeight;

        /// <summary>
        /// Customzi minimized row height.
        /// </summary>
        private double customMinimizedRowHeight ;

        /// <summary>
        /// A local store of the number of rows
        /// </summary>
        private int rows = 1;

        /// <summary>
        /// A local store of the number of columns
        /// </summary>
        private int columns = 1;

        /// <summary>
        /// Stores the max columns (0 for no maximum). Max rows takes priority over max columns.
        /// </summary>
        private int maxColumns = 0;

        /// <summary>
        /// Stores the max rows (0 for no maximum). Max rows takes priority over max columns.
        /// </summary>
        private int maxRows = 0;

        /// <summary>
        /// Stores whether the panels have been initialised in the Loaded event.
        /// </summary>
        private bool panelsInitialised;

        /// <summary>
        /// The panel currently being dragged
        /// </summary>
        private DragDockPanel draggingPanel = null;

        /// <summary>
        /// The currently maxmised panel
        /// </summary>
        private DragDockPanel maximizedPanel = null;

        /// <summary>
        /// The currently headeronly panel
        /// </summary>
        private DragDockPanel headeronlyPanel = null;

        /// <summary>
        /// Stores the minimized column width.
        /// </summary>
        private double minimizedColumnWidth = 275.0;

        /// <summary>
        /// Stores the minimized row height.
        /// </summary>
        private double minimizedRowHeight = 75.0;

        /// <summary>
        /// Stores the dockiing position.
        /// </summary>
        private MinimizedPositions minimizedPosition = MinimizedPositions.Right;

        private IInputElement lastFocusedElement;

        #endregion

        #region Constructors
        /// <summary>
        /// DragDockPanelHost Constructor
        /// </summary>
        public DragDockPanelHost()
        {
            this.Loaded += new RoutedEventHandler(this.DragDockPanelHost_Loaded);
            this.SizeChanged += new SizeChangedEventHandler(this.DragDockPanelHost_SizeChanged);
        }
        #endregion

        #region Public members
        private int panelHeaderHeight = 36;
        public int PanelHeaderHeight
        {
            get
            {
                return panelHeaderHeight;
            }
            set
            {
                panelHeaderHeight = value;
                UpdatePanelLayout();
            }
        }

        /// <summary>
        /// Gets or sets the width for the minimzed column on the right or left side.
        /// </summary>
        [System.ComponentModel.Category("Layout"), System.ComponentModel.Description("Sets custom minimized dock height.")]
        public double CustomMinimizedRowHeight
        {
            get
            {
                return this.customMinimizedRowHeight;
            }

            set
            {
                this.customMinimizedRowHeight = value;
                this.UpdatePanelLayout();
            }
        }

        /// <summary>
        /// Gets or sets the width for the minimzed column on the right or left side.
        /// </summary>
        [System.ComponentModel.Category("Layout"), System.ComponentModel.Description("Can change minimized dock height.")]
        public bool CanCustomizeDockHeight
        {
            get
            {
                return this.canCustomizeDockHeight;
            }

            set
            {
                this.canCustomizeDockHeight = value;
                this.UpdatePanelLayout();
            }
        }


        /// <summary>
        /// Gets or sets the width for the minimzed column on the right or left side.
        /// </summary>
        [System.ComponentModel.Category("Layout"), System.ComponentModel.Description("Sets the minimized column width.")]
        public double MinimizedColumnWidth
        {
            get
            {
                return this.minimizedColumnWidth;
            }

            set
            {
                this.minimizedColumnWidth = value;
                this.UpdatePanelLayout();
            }
        }

        /// <summary>
        /// Gets or sets the height for the minimized row on the top or bottom side.
        /// </summary>
        [System.ComponentModel.Category("Layout"), System.ComponentModel.Description("Sets the minimized row height.")]
        public double MinimizedRowHeight
        {
            get
            {
                return this.minimizedRowHeight;
            }

            set
            {
                this.minimizedRowHeight = value;
                this.UpdatePanelLayout();
            }
        }

        /// <summary>
        /// Gets or sets the docking position.
        /// </summary>
        [System.ComponentModel.Category("Layout"), System.ComponentModel.Description("Sets the minimized panels' position.")]
        public MinimizedPositions MinimizedPosition
        {
            get
            {
                return this.minimizedPosition;
            }

            set
            {
                this.minimizedPosition = value;
                this.AnimatePanelSizes();
                this.AnimatePanelLayout();
            }
        }

        /// <summary>
        /// Gets or sets the max rows. 0 for no maximum. Max rows takes priority over max columns.
        /// </summary>
        [System.ComponentModel.Category("Layout"), System.ComponentModel.Description("Sets the maximum rows in the host. Max rows takes priority over max columns.")]
        public int MaxRows
        {
            get
            {
                return this.maxRows;
            }

            set
            {
                this.maxRows = value;

                if (this.panelsInitialised)
                {
                    this.SetRowsAndColumns(this.GetOrderedPanels());
                    this.AnimatePanelSizes();
                    this.AnimatePanelLayout();
                }
            }
        }

        /// <summary>
        /// Gets or sets the max columns. 0 for no maximum. Max rows takes priority over max columns.
        /// </summary>
        [System.ComponentModel.Category("Layout"), System.ComponentModel.Description("Sets the maximum columns in the host. Max rows takes priority over max columns.")]
        public int MaxColumns
        {
            get
            {
                return this.maxColumns;
            }

            set
            {
                this.maxColumns = value;

                if (this.panelsInitialised)
                {
                    this.SetRowsAndColumns(this.GetOrderedPanels());
                    this.AnimatePanelSizes();
                    this.AnimatePanelLayout();
                }
            }
        }

        public ushort Functional { get; set; }
                
        #endregion

        #region Panel management methods
        /// <summary>
        /// Adds a panel to the host.
        /// </summary>
        /// <param name="panel">The panel to add.</param>
        public void AddPanel(DragDockPanel panel)
        {
            Dictionary<int, DragDockPanel> orderedPanels = this.GetOrderedPanels();

            panel.AllowHeaderOnly &= MinimizedPosition == MinimizedPositions.Right || MinimizedPosition == MinimizedPositions.Left;

            orderedPanels.Add(this.Children.Count, panel);
            this.Children.Add(panel);
            this.PreparePanel(panel);
            this.SetRowsAndColumns(orderedPanels);

            this.AnimatePanelSizes();
            this.AnimatePanelLayout();
        }

        /// <summary>
        /// Removes a panel from the host.
        /// </summary>
        /// <param name="panel">The panel to remove.</param>
        public void RemovePanel(DragDockPanel panel)
        {
            Dictionary<int, DragDockPanel> orderedPanels = new Dictionary<int, DragDockPanel>();
            List<int> indexes = new List<int>();

            // Loop through children to order them according to their
            // current row and column...
            foreach (UIElement child in this.Children)
            {
                DragDockPanel childPanel = (DragDockPanel)child;

                orderedPanels.Add(
                    (Grid.GetRow(childPanel) * this.columns) + Grid.GetColumn(childPanel),
                    childPanel);

                indexes.Add((Grid.GetRow(childPanel) * this.columns) + Grid.GetColumn(childPanel));
            }

            orderedPanels.Remove((Grid.GetRow(panel) * this.columns) + Grid.GetColumn(panel));
            indexes.Remove((Grid.GetRow(panel) * this.columns) + Grid.GetColumn(panel));
            this.Children.Remove(panel);

            Dictionary<int, DragDockPanel> reorderedPanels = new Dictionary<int, DragDockPanel>();

            for (int i = 0; i < indexes.Count; i++)
            {
                reorderedPanels.Add(i, orderedPanels[indexes[i]]);
            }

            this.SetRowsAndColumns(reorderedPanels);

            if (this.maximizedPanel == panel || reorderedPanels.Count == 1)
            {
                reorderedPanels[0].PanelMaximized = false;
                this.maximizedPanel = null;

                foreach (UIElement child in this.Children)
                {
                    DragDockPanel childPanel = (DragDockPanel)child;
                    childPanel.DraggingEnabled = true;
                }
            }

            this.AnimatePanelSizes();
            this.AnimatePanelLayout();
        }

        /// <summary>
        /// Removes a panel at a specified position index.
        /// </summary>
        /// <param name="index">The position index.</param>
        public void RemovePanelAt(int index)
        {
            if (index >= 0 && index < this.Children.Count)
            {
                Dictionary<int, DragDockPanel> orderedPanels = new Dictionary<int, DragDockPanel>();

                // Loop through children to order them according to their
                // current row and column...
                foreach (UIElement child in this.Children)
                {
                    DragDockPanel childPanel = (DragDockPanel)child;

                    orderedPanels.Add(
                        (Grid.GetRow(childPanel) * this.columns) + Grid.GetColumn(childPanel),
                        childPanel);
                }

                this.RemovePanel(orderedPanels[index]);
            }
        }

        /// <summary>
        /// Prepares a panel for the UI. Override for hooking custom events.
        /// </summary>
        /// <param name="panel">The panel to prepare.</param>
        protected virtual void PreparePanel(DragDockPanel panel)
        {
            // Hook up panel events
            panel.DragStarted +=
                new DragEventHander(this.DragDockPanel_DragStarted);
            panel.DragFinished +=
                new DragEventHander(this.DragDockPanel_DragFinished);
            panel.DragMoved +=
                new DragEventHander(this.DragDockPanel_DragMoved);
            panel.Maximized +=
                new EventHandler(this.DragDockPanel_Maximized);
            panel.Minimized +=
                new EventHandler(this.DragDockPanel_Minimized);
            panel.HeaderOnly += new EventHandler(DragDockPanel_HeaderOnly);

            panel.AllowHeaderOnly &= MinimizedPosition == MinimizedPositions.Left || MinimizedPosition == MinimizedPositions.Right;

            if (panel.PanelMaximized)
            {
                this.maximizedPanel = panel;
            }
        }
        #endregion

        #region Panel layout methods
        /// <summary>
        /// Sets the rows and columns on an ordered list of panels.
        /// </summary>
        /// <param name="orderedPanels">The ordered panels.</param>
        private void SetRowsAndColumns(Dictionary<int, DragDockPanel> orderedPanels)
        {
            if (orderedPanels.Count == 0)
            {
                return;
            }

            // Calculate the number of rows and columns required
            this.rows =
                (int)Math.Floor(Math.Sqrt((double)this.Children.Count));

            if (this.maxRows > 0)
            {
                if (this.rows > this.maxRows)
                {
                    this.rows = this.maxRows;
                }

                this.columns =
                    (int)Math.Ceiling((double)this.Children.Count / (double)this.rows);
            }
            else if (this.maxColumns > 0)
            {
                this.columns =
                (int)Math.Ceiling((double)this.Children.Count / (double)this.rows);

                if (this.columns > this.maxColumns)
                {
                    this.columns = this.maxColumns;
                    this.rows = (int)Math.Ceiling((double)this.Children.Count / this.columns);
                }
            }
            else
            {
                this.columns =
                    (int)Math.Ceiling((double)this.Children.Count / (double)this.rows);
            }

            int childCount = 0;

            // Loop through the rows and columns and assign to children
            for (int r = 0; r < this.rows; r++)
            {
                for (int c = 0; c < this.columns; c++)
                {
                    Grid.SetRow(orderedPanels[childCount], r);
                    Grid.SetColumn(orderedPanels[childCount], c);
                    childCount++;

                    // if we are on the last child, break out of the loop
                    if (childCount == this.Children.Count)
                    {
                        break;
                    }
                }

                // if we are on the last child, break out of the loop
                if (childCount == this.Children.Count)
                {
                    break;
                }
            }
        }

        /// <summary>
        /// Gets the panels in order.
        /// </summary>
        /// <returns>The ordered panels.</returns>
        private Dictionary<int, DragDockPanel> GetOrderedPanels()
        {
            Dictionary<int, DragDockPanel> orderedPanels = new Dictionary<int, DragDockPanel>();

            // Loop through children to order them according to their
            // current row and column...
            foreach (UIElement child in this.Children)
            {
                DragDockPanel childPanel = (DragDockPanel)child;

                orderedPanels.Add(
                    (Grid.GetRow(childPanel) * this.columns) + Grid.GetColumn(childPanel),
                    childPanel);
            }

            return orderedPanels;
        }
        #endregion

        #region DragDockPanelHost events

        public RoutedEventHandler OnPanelsInitialised;
        /// <summary>
        /// Updates the panel layouts.
        /// </summary>
        /// <param name="sender">The drag dock panel host.</param>
        /// <param name="e">Size changed event args.</param>
        private void DragDockPanelHost_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            this.UpdatePanelLayout();
        }

        /// <summary>
        /// Sorts out the rows and columns when the control loads.
        /// </summary>
        /// <param name="sender">The drag dock panel host.</param>
        /// <param name="e">Routed event args.</param>
        private void DragDockPanelHost_Loaded(object sender, RoutedEventArgs e)
        {
            Dictionary<int, DragDockPanel> orderedPanels = new Dictionary<int, DragDockPanel>();

            for (int i = 0; i < this.Children.Count; i++)
            {
                if (this.Children[i].GetType() == typeof(DragDockPanel))
                {
                    DragDockPanel panel = (DragDockPanel)this.Children[i];
                    this.PreparePanel(panel);
                    orderedPanels.Add(i, panel);
                }
            }

            if (Children.Count % 2 == 1 && Children.Count > 1)
            {
                DragDockPanel panel = new DragDockPanel {Visibility = Visibility.Hidden};
                orderedPanels.Add(Children.Count, panel);
                Children.Add(panel);
            }

            this.SetRowsAndColumns(orderedPanels);
            this.panelsInitialised = true;
            this.UpdatePanelLayout();

            if (OnPanelsInitialised != null)
                OnPanelsInitialised(this, null);

            if (Keyboard.FocusedElement != null) lastFocusedElement = Keyboard.FocusedElement;
            
            DependencyObject obj = AdvTreeHelper.GetVisualFocusableChildElement(this);
            if (obj != null) ((IInputElement)obj).Focus();
        }

        #endregion

        #region Panel dragging events
        /// <summary>
        /// Keeps a reference to the dragging panel.
        /// </summary>
        /// <param name="sender">The dragging panel.</param>
        /// <param name="args">Drag event args.</param>
        private void DragDockPanel_DragStarted(object sender, DragEventArgs args)
        {
            DragDockPanel panel = sender as DragDockPanel;

            // Keep reference to dragging panel
            this.draggingPanel = panel;
        }

        /// <summary>
        /// Shuffles the panels around.
        /// </summary>
        /// <param name="sender">The dragging panel.</param>
        /// <param name="args">Drag event args.</param>
        private void DragDockPanel_DragMoved(object sender, DragEventArgs args)
        {
            Point mousePosInHost =
                args.MouseEventArgs.GetPosition(this);

            int currentRow =
                (int)Math.Floor(mousePosInHost.Y /
                (this.ActualHeight / (double)this.rows));

            int currentColumn =
                (int)Math.Floor(mousePosInHost.X /
                (this.ActualWidth / (double)this.columns));

            // Stores the panel we will swap with
            DragDockPanel swapPanel = null;

            // Loop through children to see if there is a panel to swap with
            foreach (UIElement child in this.Children)
            {
                DragDockPanel panel = child as DragDockPanel;

                // If the panel is not the dragging panel and is in the current row
                // or current column... mark it as the panel to swap with
                if (panel != this.draggingPanel &&
                    Grid.GetColumn(panel) == currentColumn &&
                    Grid.GetRow(panel) == currentRow)
                {
                    swapPanel = panel;
                    break;
                }
            }

            // If there is a panel to swap with
            if (swapPanel != null)
            {
                // Store the new row and column
                int draggingPanelNewColumn = Grid.GetColumn(swapPanel);
                int draggingPanelNewRow = Grid.GetRow(swapPanel);

                // Update the swapping panel row and column
                Grid.SetColumn(swapPanel, Grid.GetColumn(this.draggingPanel));
                Grid.SetRow(swapPanel, Grid.GetRow(this.draggingPanel));

                // Update the dragging panel row and column
                Grid.SetColumn(this.draggingPanel, draggingPanelNewColumn);
                Grid.SetRow(this.draggingPanel, draggingPanelNewRow);

                // Animate the layout to the new positions
                this.AnimatePanelLayout();
            }
        }

        /// <summary>
        /// Drops the dragging panel.
        /// </summary>
        /// <param name="sender">The dragging panel.</param>
        /// <param name="args">Drag event args.</param>
        private void DragDockPanel_DragFinished(object sender, DragEventArgs args)
        {
            // Set dragging panel back to null
            this.draggingPanel = null;

            // Update the layout (to reset all panel positions)
            this.UpdatePanelLayout();
        }
        #endregion

        #region Panel maximized / minimized events
        private bool inHOevent = false;
        void DragDockPanel_HeaderOnly(object sender, EventArgs e)
        {
            if (inHOevent) return;
            inHOevent = true;
            DragDockPanel ho =
                sender as DragDockPanel;

            this.headeronlyPanel = ho.PanelHeaderOnly ? ho : null;

            foreach (UIElement child in this.Children)
            {
                DragDockPanel panel =
                    child as DragDockPanel;

                if (panel != this.maximizedPanel && panel != this.headeronlyPanel)
                {
                    panel.PanelHeaderOnly = false;

                    // выключаем панельки чтобы фокус на них не переходил
                    //panel.IsEnabled = this.headeronlyPanel == null;
                }
            }

            inHOevent = false;
            // Update sizes and layout
            this.AnimatePanelSizes();
            this.AnimatePanelLayout();
        }

        /// <summary>
        /// Puts all of the panel back to a grid view.
        /// </summary>
        /// <param name="sender">The minimising panel.</param>
        /// <param name="e">Event args.</param>
        private void DragDockPanel_Minimized(object sender, EventArgs e)
        {
            // Set max'ed panel to null
            this.maximizedPanel = null;

            // Loop through children to disable dragging
            foreach (UIElement child in this.Children)
            {
                DragDockPanel panel =
                    child as DragDockPanel;
                panel.DraggingEnabled = true;
                panel.PanelMinimized = false;
                panel.PanelNormal = true;
                panel.PanelHeaderOnly = false;
                panel.IsEnabled = true;
            }

            // Update sizes and layout
            this.AnimatePanelSizes();
            this.AnimatePanelLayout();
        }

        /// <summary>
        /// Maximises a panel.
        /// </summary>
        /// <param name="sender">the panel to maximise.</param>
        /// <param name="e">Event args.</param>
        private void DragDockPanel_Maximized(object sender, EventArgs e)
        {
            DragDockPanel maximizedPanel =
                sender as DragDockPanel;

            // Store max'ed panel
            this.maximizedPanel = maximizedPanel;
            maximizedPanel.PanelMinimized = false;
            maximizedPanel.IsEnabled = true;

            // Loop through children to disable dragging
            foreach (UIElement child in this.Children)
            {
                DragDockPanel panel =
                    child as DragDockPanel;

                panel.DraggingEnabled = false;
                panel.PanelNormal = false;

                if (panel != this.maximizedPanel)
                {
                    panel.PanelMaximized = false;
                    panel.PanelFullSize = false;
                    panel.PanelMinimized = true;

                    // выключаем панельки чтобы фокус на них не переходил
                    panel.IsEnabled = !maximizedPanel.PanelFullSize;
                }
            }

            // Update sizes and layout
            this.AnimatePanelSizes();
            this.AnimatePanelLayout();
        }
        #endregion

        #region Private layout methods
        /// <summary>
        /// Updates the panel layout without animation
        /// This does size and position without animation
        /// </summary>
        private void UpdatePanelLayout()
        {
            if (double.IsInfinity(this.ActualWidth) || double.IsNaN(this.ActualWidth) || this.ActualWidth == 0)
            {
                return;
            }

            // If we are not in max'ed panel mode...
            if (this.maximizedPanel == null)
            {
                // Layout children as per rows and columns
                foreach (UIElement child in this.Children)
                {
                    DragDockPanel panel = (DragDockPanel)child;

                    double width = (this.ActualWidth / (double)this.columns) - panel.Margin.Left - panel.Margin.Right;
                    double height = (this.ActualHeight / (double)this.rows) - panel.Margin.Top - panel.Margin.Bottom;
                    double left = Grid.GetColumn(panel) * (this.ActualWidth / (double)this.columns);
                    double top = Grid.GetRow(panel) * (this.ActualHeight / (double)this.rows);

                    panel.SetSize(left, top, width, height);
                    panel.CalculateHeaderWidth();
                    panel.IsEnabled = true;
                }
            }
            else
            {
                Dictionary<int, DragDockPanel> orderedPanels = new Dictionary<int, DragDockPanel>();

                // Loop through children to order them according to their
                // current row and column...
                foreach (UIElement child in this.Children)
                {
                    DragDockPanel panel = (DragDockPanel)child;

                    orderedPanels.Add(
                        (Grid.GetRow(panel) * this.columns) + Grid.GetColumn(panel),
                        panel);
                }

                // Set initial top of minimized panels to 0
                double currentOffset = 0.0;

                // For each of the panels (as ordered in the grid)
                for (int i = 0; i < orderedPanels.Count; i++)
                {
                    // If the current panel is not the maximized panel
                    if (orderedPanels[i] != this.maximizedPanel)
                    {
                        double newX = 0;
                        double newY = currentOffset;

                        double newWidth = this.minimizedColumnWidth - orderedPanels[i].Margin.Left - orderedPanels[i].Margin.Right;
                        double newHeight;

                        #region determine new size and position depending on docking axis

                        if (headeronlyPanel == null || !orderedPanels[i].AllowHeaderOnly)
                        {
                            if (CanCustomizeDockHeight)
                            {
                                newHeight = CustomMinimizedRowHeight;
                            }
                            else
                            {
                                newHeight = (this.ActualHeight / (double)(this.Children.Count - 1)) - orderedPanels[i].Margin.Top - orderedPanels[i].Margin.Bottom;
                            }


                            //double newHeight = (this.ActualHeight / (double)(this.Children.Count - 1)) - orderedPanels[i].Margin.Top - orderedPanels[i].Margin.Bottom;
                            if (this.minimizedPosition.Equals(MinimizedPositions.Bottom) || this.minimizedPosition.Equals(MinimizedPositions.Top))
                            {
                                newWidth = (this.ActualWidth / (double)(this.Children.Count - 1)) - orderedPanels[i].Margin.Left - orderedPanels[i].Margin.Right;
                                newHeight = this.minimizedRowHeight - orderedPanels[i].Margin.Top - orderedPanels[i].Margin.Bottom;
                            }

                            #region determin new docking coordinates
                            if (this.minimizedPosition.Equals(MinimizedPositions.Right))
                            {
                                newX = maximizedPanel.PanelFullSize ? this.ActualWidth : this.ActualWidth - this.minimizedColumnWidth;
                                newY = currentOffset;
                            }
                            else if (this.minimizedPosition.Equals(MinimizedPositions.Left))
                            {
                                newX = maximizedPanel.PanelFullSize ? -this.minimizedColumnWidth : 0;
                                newY = currentOffset;
                            }
                            else if (this.minimizedPosition.Equals(MinimizedPositions.Bottom))
                            {
                                newX = currentOffset;
                                newY = maximizedPanel.PanelFullSize ? this.ActualHeight : this.ActualHeight - this.minimizedRowHeight;
                            }
                            else if (this.minimizedPosition.Equals(MinimizedPositions.Top))
                            {
                                newX = currentOffset;
                                newY = maximizedPanel.PanelFullSize ? -this.minimizedRowHeight : 0;
                            }
                            #endregion

                            if (this.minimizedPosition.Equals(MinimizedPositions.Left) || this.minimizedPosition.Equals(MinimizedPositions.Right))
                            {
                                // Increment current top
                                currentOffset += this.ActualHeight / (double)(this.Children.Count - 1);
                            }
                            else
                            {
                                // Increment current left
                                currentOffset += this.ActualWidth / (double)(this.Children.Count - 1);
                            }
                        }
                        else
                        {
                            if (orderedPanels[i] != this.headeronlyPanel)
                            {
                                newHeight = PanelHeaderHeight;
                                                                
                                if (this.minimizedPosition.Equals(MinimizedPositions.Right))
                                {
                                    newX = this.ActualWidth - this.minimizedColumnWidth;
                                    newY = currentOffset;
                                }
                                else if (this.minimizedPosition.Equals(MinimizedPositions.Left))
                                {
                                    newX = 0;
                                    newY = currentOffset;
                                }
                                else if (this.minimizedPosition.Equals(MinimizedPositions.Bottom))
                                {
                                    newX = currentOffset;
                                    newY = this.ActualHeight - this.minimizedRowHeight;
                                }
                                else if (this.minimizedPosition.Equals(MinimizedPositions.Top))
                                {
                                    newX = currentOffset;
                                    newY = 0;
                                }                                
                            }
                            else
                            {
                                newHeight = this.ActualHeight - (this.Children.Count - 2) * (PanelHeaderHeight) - 8;
                                //newHeight = (this.ActualHeight / (double)(this.Children.Count - 1)) - orderedPanels[i].Margin.Top - orderedPanels[i].Margin.Bottom;
                                                                
                                if (this.minimizedPosition.Equals(MinimizedPositions.Right))
                                {
                                    newX = maximizedPanel.PanelFullSize ? this.ActualWidth : this.ActualWidth - this.minimizedColumnWidth;
                                    newY = currentOffset;
                                }
                                else if (this.minimizedPosition.Equals(MinimizedPositions.Left))
                                {
                                    newX = maximizedPanel.PanelFullSize ? -this.minimizedColumnWidth : 0;
                                    newY = currentOffset;
                                }
                            }

                            currentOffset += newHeight;// свернутые панельки пока только для Left и Rigth
                        }
                        #endregion
                        
                        // Set the size of the panel
                        orderedPanels[i].SetSize(newX, newY, newWidth, newHeight);                        
                    }
                    else
                        if (maximizedPanel.PanelFullSize)
                        {
                            #region determine new width & height depending on docking axis
                            double newWidth = this.ActualWidth - orderedPanels[i].Margin.Left - orderedPanels[i].Margin.Right;
                            double newHeight = this.ActualHeight - orderedPanels[i].Margin.Top - orderedPanels[i].Margin.Bottom;
                            #endregion

                            // Set the size of the panel
                            orderedPanels[i].SetSize(0, 0, newWidth, newHeight);
                        }
                        else
                        {

                            #region determine new width & height depending on docking axis
                            double newWidth = this.ActualWidth - this.minimizedColumnWidth - orderedPanels[i].Margin.Left - orderedPanels[i].Margin.Right;
                            double newHeight = this.ActualHeight - orderedPanels[i].Margin.Top - orderedPanels[i].Margin.Bottom;
                            if (this.minimizedPosition.Equals(MinimizedPositions.Bottom) || this.minimizedPosition.Equals(MinimizedPositions.Top))
                            {
                                newWidth = this.ActualWidth - orderedPanels[i].Margin.Left - orderedPanels[i].Margin.Right;
                                newHeight = this.ActualHeight - this.minimizedRowHeight - orderedPanels[i].Margin.Top - orderedPanels[i].Margin.Bottom;
                            }
                            #endregion

                            #region determine new docking position
                            double newX = 0;
                            double newY = 0;
                            if (this.minimizedPosition.Equals(MinimizedPositions.Right))
                            {
                                newX = 0;
                                newY = 0;
                            }
                            else if (this.minimizedPosition.Equals(MinimizedPositions.Left))
                            {
                                newX = this.minimizedColumnWidth;
                                newY = 0;
                            }
                            else if (this.minimizedPosition.Equals(MinimizedPositions.Bottom))
                            {
                                newX = 0;
                                newY = 0;
                            }
                            else if (this.minimizedPosition.Equals(MinimizedPositions.Top))
                            {
                                newX = 0;
                                newY = this.minimizedRowHeight;
                            }
                            #endregion

                            // Set the size of the panel
                            orderedPanels[i].SetSize(newX, newY, newWidth, newHeight);
                        }
                }
            }
        }

        /// <summary>
        /// Animates the panel sizes
        /// </summary>
        private void AnimatePanelSizes()
        {
            if (double.IsInfinity(this.ActualWidth) || double.IsNaN(this.ActualWidth) || this.ActualWidth == 0)
            {
                return;
            }

            // If there is not a maxmized panel...
            if (this.maximizedPanel == null)
            {
                // Animate the panel sizes to row / column sizes
                foreach (UIElement child in this.Children)
                {
                    DragDockPanel panel = (DragDockPanel)child;

                    double width = (this.ActualWidth / (double)this.columns) - panel.Margin.Left - panel.Margin.Right;
                    double height = (this.ActualHeight / (double)this.rows) - panel.Margin.Top - panel.Margin.Bottom;

                    if (width < 0)
                    {
                        width = 0;
                    }

                    if (height < 0)
                    {
                        height = 0;
                    }

                    panel.AnimateSize(width, height);
                }
            }
            else
            {
                // Loop through the children
                foreach (UIElement child in this.Children)
                {
                    DragDockPanel panel =
                        (DragDockPanel)child;

                    // Set the size of the non 
                    // maximized children
                    if (panel != this.maximizedPanel)
                    {
                        double newWidth = this.minimizedColumnWidth - panel.Margin.Left - panel.Margin.Right;
                        double newHeight;

                        if (headeronlyPanel == null || !panel.AllowHeaderOnly)
                        {
                            if (CanCustomizeDockHeight)
                            {
                                newHeight = CustomMinimizedRowHeight;
                            }
                            else
                            {
                                newHeight = (this.ActualHeight / (double)(this.Children.Count - 1)) - panel.Margin.Top - panel.Margin.Bottom;
                            }

                            if (this.minimizedPosition.Equals(MinimizedPositions.Bottom) || this.minimizedPosition.Equals(MinimizedPositions.Top))
                            {
                                newWidth = (this.ActualWidth / (double)(this.Children.Count - 1)) - panel.Margin.Left - panel.Margin.Right;
                                newHeight = this.minimizedRowHeight - panel.Margin.Top - panel.Margin.Bottom;
                            }

                            if (newHeight < 0)
                            {
                                newHeight = 0;
                            }

                            if (newWidth < 0)
                            {
                                newWidth = 0;
                            }
                        }
                        else
                        {
                            if (panel != headeronlyPanel)
                            {
                                newHeight = PanelHeaderHeight;
                            }
                            else
                            {
                                newHeight = this.ActualHeight - (this.Children.Count - 2) * (PanelHeaderHeight) - 8;
                            }
                        }

                        panel.AnimateSize(newWidth, newHeight);
                    }
                    else
                        if (maximizedPanel.PanelFullSize)
                        {
                            #region determine new width & height depending on docking axis
                            double newWidth = this.ActualWidth - panel.Margin.Left - panel.Margin.Right;
                            double newHeight = this.ActualHeight - panel.Margin.Top - panel.Margin.Bottom;                            
                            #endregion

                            if (newHeight < 0)
                            {
                                newHeight = 0;
                            }

                            if (newWidth < 0)
                            {
                                newWidth = 0;
                            }

                            panel.AnimateSize(newWidth, newHeight);
                        }
                        else
                        {
                            #region determine new width & height depending on docking axis
                            double newWidth = this.ActualWidth - this.minimizedColumnWidth - panel.Margin.Left - panel.Margin.Right;
                            double newHeight = this.ActualHeight - panel.Margin.Top - panel.Margin.Bottom;
                            if (this.minimizedPosition.Equals(MinimizedPositions.Bottom) || this.minimizedPosition.Equals(MinimizedPositions.Top))
                            {
                                newWidth = this.ActualWidth - panel.Margin.Left - panel.Margin.Right;
                                newHeight = this.ActualHeight - this.minimizedRowHeight - panel.Margin.Top - panel.Margin.Bottom;
                            }
                            #endregion

                            if (newHeight < 0)
                            {
                                newHeight = 0;
                            }

                            if (newWidth < 0)
                            {
                                newWidth = 0;
                            }

                            panel.AnimateSize(newWidth, newHeight);
                        }
                }
            }
        }

        /// <summary>
        /// Animate the panel positions
        /// </summary>
        private void AnimatePanelLayout()
        {
            if (double.IsInfinity(this.ActualWidth) || double.IsNaN(this.ActualWidth) || this.ActualWidth == 0)
            {
                return;
            }

            // If we are not in max'ed panel mode...
            if (this.maximizedPanel == null)
            {
                // Loop through children and size to row and columns
                foreach (UIElement child in this.Children)
                {
                    DragDockPanel panel = (DragDockPanel)child;

                    if (panel != this.draggingPanel)
                    {
                        panel.AnimatePosition(
                            Grid.GetColumn(panel) * (this.ActualWidth / (double)this.columns),
                            Grid.GetRow(panel) * (this.ActualHeight / (double)this.rows));
                    }
                }
            }
            else
            {
                Dictionary<int, DragDockPanel> orderedPanels = new Dictionary<int, DragDockPanel>();

                // Loop through children to order them according to their
                // current row and column...
                foreach (UIElement child in this.Children)
                {
                    DragDockPanel panel = (DragDockPanel)child;

                    orderedPanels.Add(
                        (Grid.GetRow(panel) * this.columns) + Grid.GetColumn(panel),
                        panel);
                }

                // Set initial top of minimized panels to 0
                double currentOffset = 0.0;

                // For each of the panels (as ordered in the grid)
                for (int i = 0; i < orderedPanels.Count; i++)
                {
                    // If the current panel is not the maximized panel
                    if (orderedPanels[i] != this.maximizedPanel)
                    {
                        double newX = 0;
                        double newY = currentOffset;

                        if (this.headeronlyPanel == null || !orderedPanels[i].AllowHeaderOnly)
                        {
                            if (this.minimizedPosition.Equals(MinimizedPositions.Right))
                            {
                                newX = maximizedPanel.PanelFullSize ? this.ActualWidth : this.ActualWidth - this.minimizedColumnWidth;
                                newY = currentOffset;
                            }
                            else if (this.minimizedPosition.Equals(MinimizedPositions.Left))
                            {
                                newX = maximizedPanel.PanelFullSize ? -this.minimizedColumnWidth : 0;
                                newY = currentOffset;
                            }
                            else if (this.minimizedPosition.Equals(MinimizedPositions.Bottom))
                            {
                                newX = currentOffset;
                                newY = maximizedPanel.PanelFullSize ? this.ActualHeight : this.ActualHeight - this.minimizedRowHeight;
                            }
                            else if (this.minimizedPosition.Equals(MinimizedPositions.Top))
                            {
                                newX = currentOffset;
                                newY = maximizedPanel.PanelFullSize ? -this.minimizedRowHeight : 0;
                            }                            

                            if (this.minimizedPosition.Equals(MinimizedPositions.Left) || this.minimizedPosition.Equals(MinimizedPositions.Right))
                            {
                                // Increment current top
                                currentOffset += this.ActualHeight / (double)(this.Children.Count - 1);
                            }
                            else
                            {
                                // Increment current left
                                currentOffset += this.ActualWidth / (double)(this.Children.Count - 1);
                            }
                        }
                        else
                            if (orderedPanels[i] != this.headeronlyPanel)
                            {
                                if (this.minimizedPosition.Equals(MinimizedPositions.Right))
                                {
                                    newX = this.ActualWidth - this.minimizedColumnWidth;
                                    newY = currentOffset;
                                }
                                else if (this.minimizedPosition.Equals(MinimizedPositions.Left))
                                {
                                    newX = 0;
                                    newY = currentOffset;
                                }

                                currentOffset += PanelHeaderHeight;
                            }
                            else
                            {
                                if (this.minimizedPosition.Equals(MinimizedPositions.Right))
                                {
                                    newX = this.ActualWidth - this.minimizedColumnWidth;
                                    newY = currentOffset;
                                }
                                else if (this.minimizedPosition.Equals(MinimizedPositions.Left))
                                {
                                    newX = 0;
                                    newY = currentOffset;
                                }
                                                                
                                currentOffset += this.ActualHeight - (this.Children.Count - 2) * (PanelHeaderHeight) - 8;
                            }

                        // Animate the position
                        orderedPanels[i].AnimatePosition(newX, newY);
                    }
                    else
                    {
                        #region determine new docking position
                        double newX = 0;
                        double newY = 0;
                        if (this.minimizedPosition.Equals(MinimizedPositions.Right))
                        {
                            newX = 0;
                            newY = 0;
                        }
                        else if (this.minimizedPosition.Equals(MinimizedPositions.Left))
                        {
                            newX = this.minimizedColumnWidth;
                            newY = 0;
                        }
                        else if (this.minimizedPosition.Equals(MinimizedPositions.Bottom))
                        {
                            newX = 0;
                            newY = 0;
                        }
                        else if (this.minimizedPosition.Equals(MinimizedPositions.Top))
                        {
                            newX = 0;
                            newY = this.minimizedRowHeight;
                        }
                        #endregion
                        // Animate maximized panel                        
                        orderedPanels[i].AnimatePosition(newX, newY);
                    }
                }
            }
        }
        #endregion
        
        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            DependencyObject sender = (DependencyObject)e.Source;
            DragDockPanel panel;
            if (sender is DragDockPanel) panel = (DragDockPanel)sender;
            else panel = AdvTreeHelper.GetVisualParentElement<DragDockPanel>(sender);

            if ((e.Key == Key.Add || e.Key == Key.OemPlus) && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                if (!panel.MaximizeToggle.IsChecked.Value)
                {
                    panel.MaximizeToggle_Checked(this, new RoutedEventArgs());
                    DependencyObject obj = AdvTreeHelper.GetVisualFocusableChildElement(panel);
                    if (obj != null) ((IInputElement) obj).Focus();
                }
            }

            if ((e.Key == Key.Subtract || e.Key == Key.OemMinus) && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                if (panel.MaximizeToggle.IsChecked.Value)
                {
                    panel.MaximizeToggle_Unchecked(this, new RoutedEventArgs());
                    DependencyObject obj = AdvTreeHelper.GetVisualFocusableChildElement(panel);
                    if (obj != null) ((IInputElement)obj).Focus();
                }
            }

            if (e.Key == Key.F12)
            {
                if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control && lastFocusedElement != null)
                {
                    lastFocusedElement.Focus();
                    base.OnPreviewKeyDown(e);
                    return;
                }
                
                DragDockPanelHost host = AdvTreeHelper.GetLogicalParentElement<DragDockPanelHost>(panel);
                DragDockPanel nextPanel = GetNextPanel(host, panel);
                if (nextPanel == null)
                {
                    nextPanel = AdvTreeHelper.GetLogicalChildElement<DragDockPanel>(host);
                }
                if (nextPanel != null)
                {
                    if (maximizedPanel != null && maximizedPanel.PanelFullSize)
                    {
                        maximizedPanel.FullSize_Unchecked(this, new RoutedEventArgs());
                    }
                    nextPanel.MaximizeToggle_Checked(this, new RoutedEventArgs());
                    DependencyObject obj = AdvTreeHelper.GetVisualFocusableChildElement(nextPanel);
                    if (obj != null) ((IInputElement)obj).Focus();
                }
            }
            base.OnPreviewKeyDown(e);
        }

        private static DragDockPanel GetNextPanel(Panel host, DragDockPanel panel)
        {
            for (int i = 0; i < host.Children.Count; i++)
            {
                if (host.Children[i] == panel)
                {                    
                    if (i < host.Children.Count - 1)
                    {
                        if (host.Children[i + 1].IsVisible)
                        return (DragDockPanel) host.Children[i + 1];
                    }
                    return (DragDockPanel) host.Children[0];
                }
            }
            return null;
        }
    }
}
