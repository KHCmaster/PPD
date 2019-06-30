using FlowScriptDrawControl.Command;
using FlowScriptDrawControl.Dialog;
using FlowScriptDrawControl.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace FlowScriptDrawControl.Control
{
    /// <summary>
    /// FlowAreaControl.xaml の相互作用ロジック
    /// </summary>
    public partial class FlowAreaControl : UserControl
    {
        const int gridThickness = 1;
        const int gridWidth = 10;
        const int baseScale = 10;
        const int maxScale = 100;
        private Color gridColor = Color.FromRgb(0x91, 0x91, 0x91);
        private int scale = 10;

        private Regex queryRegex = new Regex(@"^@(\d+)\[(In|Out)\]\[(\w+)\]$", RegexOptions.IgnoreCase);

        private double currentScale = 1;
        private double currentShiftX;
        private double currentShiftY;

        private bool isMouseLeftDown;
        private bool isMouseMiddleDown;
        private Point mouseLeftDownPos;
        private Point mouseRightDownPos;
        private Point mouseMiddleDownPos;

        private SourceItemControl currentItem;
        private ArrowControl currentArrow;
        private KeyValuePair<SourceItemControl, ArrowControl>[] currentDestItems;
        private ItemPositionManager itemPositionManager;

        private SelectableManager selectableManager;
        private Command.CommandManager commandManager;
        private IdManager idManager;

        public event Action<string, Dictionary<string, string>> SelectionChanged;
        public event Func<string, string, bool> CanConvert;
        public event Action Modified;
        public event Func<string, Source> GetSource;

        private Guid guid = Guid.NewGuid();

        Timer autoFocusTimer;
        Timer moveTimer;

        public double CurrentShiftX
        {
            get { return currentShiftX; }
        }

        public double CurrentShiftY
        {
            get { return currentShiftY; }
        }

        public Guid Guid
        {
            get
            {
                return guid;
            }
        }

        public double CurrentScale
        {
            get { return currentScale; }
        }

        public Command.CommandManager CommandManager
        {
            get
            {
                return commandManager;
            }
        }

        public int[] SelectedSourceId
        {
            get
            {
                return GetAllSourceControls().Where(s => s.CurrentSource.IsSelected).Select(s => s.CurrentSource.Id).ToArray();
            }
        }

        public FlowAreaControl()
        {
            selectableManager = new SelectableManager();
            selectableManager.SelectionChanged += selectableManager_SelectionChanged;
            commandManager = new Command.CommandManager();
            commandManager.Modified += commandManager_Modified;
            idManager = new IdManager();

            InitializeComponent();
        }

        void commandManager_Modified()
        {
            OnModified();
        }

        void selectableManager_SelectionChanged()
        {
            if (selectableManager.SelectedSelectables.Length == 1 && selectableManager.SelectedSelectables[0] is Source)
            {
                var source = selectableManager.SelectedSelectables[0] as Source;
                OnSelectionChanged(source.FullName, source.Items.Where(i => !i.IsOut && i.Type != "event").
                    ToDictionary(i => i.Name, i => i.Value));
            }
            else
            {
                OnSelectionChanged(null, null);
            }
        }

        private void ChangeScopeColor(ScopeControl scopeControl)
        {
            var dialog = new ColorDialog
            {
                CurrentColor = scopeControl.CurrentScope.Color
            };
            dialog.ShowDialog();
            if (dialog.DialogResult.HasValue && dialog.DialogResult.Value)
            {
                scopeControl.CurrentScope.Color = dialog.CurrentColor;
            }
        }

        void scopeControl_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var scopeControl = sender as ScopeControl;
            ChangeScopeColor(scopeControl);
            e.Handled = true;
        }

        private SourceControl CreateSource(int id, string fullName)
        {
            return CreateSource(OnGetSource(fullName), id);
        }

        private SourceControl CreateSource(Source source, int id)
        {
            source.Id = id < 0 ? idManager.GetId() : id;
            return CreateSource(source);
        }

        private SourceControl CreateSource(Source source)
        {
            var sourceControl = new SourceControl
            {
                DataContext = source,
                SelectableManager = selectableManager
            };
            sourceControl.OutItemSelected += sourceControl_OutItemSelected;
            sourceControl.TryConnected += sourceControl_TryConnected;
            sourceControl.InItemSelected += sourceControl_InItemSelected;
            sourceControl.MoveStarted += control_MoveStarted;
            sourceControl.SourceChanged += sourceControl_SourceChanged;
            return sourceControl;
        }

        void sourceControl_SourceChanged(object sender, ChangeSourceEventArgs e)
        {
            var sourceControl = sender as SourceControl;
            commandManager.StartCommandSet();
            var command = new RemoveSelectablesCommand(this, new SelectableControl[] { sourceControl });
            commandManager.AddCommand(command);
            sourceControl = AddSourceAtImpl(-1, sourceControl.CurrentSource.Position.X, sourceControl.CurrentSource.Position.Y, e.AssemblyAndType.FullName);
            commandManager.EndCommandSet();
            selectableManager.ClearSelect();
            selectableManager.AddSelect(sourceControl.CurrentSource);
        }

        void control_MoveStarted(object sender, EventArgs e)
        {
            var selectableControl = sender as SelectableControl;
            var scopeJoinManager = new ScopeJoinManager(this, commandManager, selectableControl.MoveManager);
            selectableControl.MoveManager.MoveEnd += MoveManager_MoveEnd;
            EnableMove();
        }

        void MoveManager_MoveEnd(object sender, EventArgs e)
        {
            DisableMove();
        }

        public CommentControl CreateComment()
        {
            var commentControl = new CommentControl
            {
                DataContext = new Comment(),
                SelectableManager = selectableManager
            };
            commentControl.MoveStarted += control_MoveStarted;
            return commentControl;
        }

        public Point ToLocal(Point p)
        {
            return drawGrid.RenderTransform.Inverse.Transform(p);
        }

        public Point ToGlobal(Point p)
        {
            return drawGrid.RenderTransform.Transform(p);
        }

        void sourceControl_TryConnected(object sender, EventArgs e)
        {
            var item = sender as SourceItemControl;
            if (currentItem != null)
            {
                TryOutConnect(item);
            }
            else if (currentDestItems != null)
            {
                TryInConnect(item);
            }
        }

        private void TryOutConnect(SourceItemControl destItem)
        {
            try
            {
                if (currentItem == null || currentItem.CurrentItem.Source == destItem.CurrentItem.Source
                    || !IsConnectable(currentItem.CurrentItem, destItem.CurrentItem))
                {
                    DisableConnect();
                    DisableIsConnectable(true);
                    DisableMove();
                    return;
                }
                var command = new AddFlowCommand(this, currentArrow, currentItem, destItem);
                commandManager.AddCommand(command);
                currentArrow.UpdatePositionInItem();
            }
            finally
            {
                if (currentItem != null)
                {
                    currentItem.CurrentItem.IsConnectable = false;
                    currentItem = null;
                }
                itemPositionManager = null;
                DisableIsConnectable(true);
                DisableMove();
                currentArrow = null;
            }
        }

        private void TryInConnect(SourceItemControl srcItem)
        {
            try
            {
                if (currentDestItems == null || currentDestItems.Any(i => i.Key.CurrentItem.Source == srcItem.CurrentItem.Source)
                    || !currentDestItems.All(i => IsConnectable(srcItem.CurrentItem, i.Key.CurrentItem)))
                {
                    DisableConnect();
                    DisableIsConnectable(false);
                    DisableMove();
                    return;
                }
                commandManager.StartCommandSet();
                foreach (var destItem in currentDestItems)
                {
                    var command = new AddFlowCommand(this, destItem.Value, srcItem, destItem.Key);
                    commandManager.AddCommand(command);
                    destItem.Value.UpdatePositionInItem();
                }
                commandManager.EndCommandSet();
            }
            finally
            {
                if (currentDestItems != null)
                {
                    foreach (var destItem in currentDestItems)
                    {
                        destItem.Key.CurrentItem.IsConnectable = false;
                    }
                    currentDestItems = null;
                }
                itemPositionManager = null;
                DisableIsConnectable(false);
                DisableMove();
            }
        }

        void sourceControl_InItemSelected(object sender, EventArgs e)
        {
            var destItem = sender as SourceItemControl;
            if (destItem.CurrentItem.InConnection == null)
            {
                return;
            }

            if (Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
            {
                // duplicate arrow
                var sourceItem = destItem.CurrentItem.InConnection.Target;
                var source = GetAllSourceControls().FirstOrDefault(s => s.CurrentSource == sourceItem.Source);
                currentItem = source.GetItemControlByName(destItem.CurrentItem.InConnection.Target.Name, true);
                currentItem.CurrentItem.IsConnectable = true;
                currentArrow = new ArrowControl
                {
                    SrcItem = currentItem
                };
                arrowCanvas.Children.Add(currentArrow);
                EnableConnect(false);
                EnableMove();
                UpdateCurrentArrow();
            }
            else
            {
                currentArrow = GetArrowControl(destItem);
                currentItem = currentArrow.SrcItem;
                var command = new RemoveFlowCommand(this, currentArrow);
                commandManager.AddCommand(command);

                currentItem.CurrentItem.IsConnectable = true;
                currentArrow.SrcItem = currentItem;
                arrowCanvas.Children.Add(currentArrow);
                EnableConnect(false);
                EnableMove();
                UpdateCurrentArrow();
            }
        }

        public ArrowControl GetArrowControl(SourceItemControl destItem)
        {
            foreach (ArrowControl arrowControl in arrowCanvas.Children)
            {
                if (arrowControl.DestItem == destItem)
                {
                    return arrowControl;
                }
            }

            return null;
        }

        public ArrowControl GetArrowControl(Item destItem)
        {
            foreach (ArrowControl arrowControl in arrowCanvas.Children)
            {
                if (arrowControl.DestItem.CurrentItem == destItem)
                {
                    return arrowControl;
                }
            }

            return null;
        }
        private void DisableConnect()
        {
            if (currentItem != null)
            {
                currentItem.CurrentItem.IsConnectable = false;
                currentItem = null;
            }
            if (currentArrow != null)
            {
                currentArrow.SrcItem = null;
                currentArrow.DestItem = null;
                arrowCanvas.Children.Remove(currentArrow);
                currentArrow = null;
            }
            if (currentDestItems != null)
            {
                foreach (var destItem in currentDestItems)
                {
                    destItem.Key.CurrentItem.IsConnectable = false;
                    destItem.Value.SrcItem = null;
                    destItem.Value.DestItem = null;
                    arrowCanvas.Children.Remove(destItem.Value);
                }
                currentDestItems = null;
            }
            itemPositionManager = null;
        }

        private void DisableMove()
        {
            if (moveTimer != null)
            {
                moveTimer.Stop();
                moveTimer.Dispose();
                moveTimer = null;
            }
        }

        private void DisableIsConnectable(bool isOut)
        {
            foreach (Item item in GetAllSourceControls().SelectMany(sc => sc.CurrentSource.Items.Where(i => i.IsOut == isOut)))
            {
                item.IsConnectable = false;
            }
        }

        private bool IsConnectable(Item sourceItem, Item item)
        {
            if (sourceItem.Source == item.Source)
            {
                return false;
            }

            if (item.IsOut)
            {
                return false;
            }

            if (sourceItem.Type == item.Type)
            {
                return true;
            }

            return OnCanConvert(sourceItem.Type, item.Type);
        }

        private void EnableConnect(bool isOut)
        {
            foreach (Item item in GetAllSourceControls().SelectMany(sc => sc.CurrentSource.Items.Where(i => i.IsOut == isOut)))
            {
                if (isOut)
                {
                    item.IsConnectable = currentDestItems.All(i => IsConnectable(item, i.Key.CurrentItem));
                }
                else
                {
                    item.IsConnectable = IsConnectable(currentItem.CurrentItem, item);
                }
            }
        }

        private void EnableMove()
        {
            moveTimer = new Timer(16);
            moveTimer.Elapsed += (sender, e) =>
            {
                Dispatcher.Invoke((Action)(() =>
                {
                    double edge = 20;
                    double diff = 10;
                    if (!IsMouseOver)
                    {
                        return;
                    }

                    var p = Mouse.GetPosition(this);
                    if (p.X < edge)
                    {
                        currentShiftX += diff;
                    }
                    if (p.X > ActualWidth - edge)
                    {
                        currentShiftX -= diff;
                    }
                    if (p.Y < edge)
                    {
                        currentShiftY += diff;
                    }
                    if (p.Y > ActualHeight - edge)
                    {
                        currentShiftY -= diff;
                    }
                    UpdateTransform();
                    UpdateCurrentArrow();
                }));
            };
            moveTimer.Start();
        }

        void sourceControl_OutItemSelected(object sender, EventArgs e)
        {
            var selectedItem = sender as SourceItemControl;
            if (Keyboard.Modifiers.HasFlag(ModifierKeys.Shift))
            {
                var connections = selectedItem.CurrentItem.OutConnections;
                int count = 0;
                var destItems = new List<KeyValuePair<SourceItemControl, ArrowControl>>();
                commandManager.StartCommandSet();
                foreach (var connection in connections)
                {
                    var source = GetSourceControlById(connection.Target.Source.Id);
                    if (source == null)
                    {
                        continue;
                    }
                    var destItem = source.GetItemControlByName(connection.Target.Name, false);
                    if (destItem == null)
                    {
                        continue;
                    }

                    var arrow = GetArrowControl(destItem);
                    if (arrow == null)
                    {
                        continue;
                    }
                    var item = arrow.DestItem;
                    destItems.Add(new KeyValuePair<SourceItemControl, ArrowControl>(item, arrow));
                    var command = new RemoveFlowCommand(this, arrow);
                    commandManager.AddCommand(command);
                    arrow.DestItem = item;
                    arrowCanvas.Children.Add(arrow);
                    item.CurrentItem.IsConnectable = true;
                    count++;
                }
                commandManager.EndCommandSet();
                if (count > 0)
                {
                    currentDestItems = destItems.ToArray();
                    EnableConnect(true);
                    EnableMove();
                    UpdateCurrentArrow();
                }
            }
            else
            {
                currentItem = selectedItem;
                currentItem.CurrentItem.IsConnectable = true;
                currentArrow = new ArrowControl
                {
                    SrcItem = currentItem
                };
                arrowCanvas.Children.Add(currentArrow);
                EnableConnect(false);
                EnableMove();
                UpdateCurrentArrow();
            }
        }

        private void UpdateGrid()
        {
            backCanvas.Children.Clear();
            for (int i = 0; i < (int)Math.Ceiling(backCanvas.ActualWidth / gridWidth); i++)
            {
                var line = new Line
                {
                    X1 = i * gridWidth,
                    Y1 = 0,
                    X2 = i * gridWidth,
                    Y2 = backCanvas.ActualHeight,
                    Stroke = new SolidColorBrush(gridColor),
                    StrokeThickness = gridThickness
                };
                backCanvas.Children.Add(line);
            }
            for (int i = 0; i < (int)Math.Ceiling(backCanvas.ActualHeight / gridWidth); i++)
            {
                var line = new Line
                {
                    X1 = 0,
                    Y1 = i * gridWidth,
                    X2 = backCanvas.ActualWidth,
                    Y2 = i * gridWidth,
                    Stroke = new SolidColorBrush(gridColor),
                    StrokeThickness = gridThickness
                };
                backCanvas.Children.Add(line);
            }
        }

        private void UpdateTransform()
        {
            var transform = new TransformGroup();
            transform.Children.Add(new TranslateTransform(currentShiftX / currentScale, currentShiftY / currentScale));
            transform.Children.Add(new ScaleTransform(currentScale, currentScale));
            transform.Children.Add(new TranslateTransform(mainCanvas.ActualWidth / 2, mainCanvas.ActualHeight / 2));
            drawGrid.RenderTransform = transform;
        }

        private void UpdateScaleAndTranslate(int wheel, Point mousePos)
        {
            scale += wheel;
            if (scale < 1)
            {
                scale = 1;
            }
            if (scale >= maxScale)
            {
                scale = maxScale;
            }

            var p = ToLocal(mousePos);
            currentScale = (float)scale / baseScale;
            UpdateTransform();
            p = ToGlobal(p);
            currentShiftX += mousePos.X - p.X;
            currentShiftY += mousePos.Y - p.Y;
            UpdateTransform();
        }

        private void UpdateTranslate(Point mousePos)
        {
            DisposeAutoFocusTimer();
            currentShiftX += mousePos.X - mouseMiddleDownPos.X;
            currentShiftY += mousePos.Y - mouseMiddleDownPos.Y;
            mouseMiddleDownPos = mousePos;
            UpdateTransform();
        }

        private void UpdateCurrentArrow()
        {
            if (currentArrow != null)
            {
                UpdateArrow(currentArrow, currentItem);
            }
            else if (currentDestItems != null)
            {
                foreach (var destItem in currentDestItems)
                {
                    UpdateArrow(destItem.Value, destItem.Key);
                }
            }
        }

        private void UpdateArrow(ArrowControl arrow, SourceItemControl item)
        {
            Point from, to;
            if (item.CurrentItem.IsOut)
            {
                from = this.PointFromScreen(item.PointToScreen(new Point(item.ActualWidth, item.ActualHeight / 2)));
                to = Mouse.GetPosition(this);
            }
            else
            {
                from = Mouse.GetPosition(this);
                to = this.PointFromScreen(item.PointToScreen(new Point(0, item.ActualHeight / 2)));
            }
            from = ToLocal(from);
            to = ToLocal(to);
            arrow.UpdatePosition(from, to);
        }

        private void UpdateRegionSelection()
        {
            var current = Mouse.GetPosition(this);
            var topLeft = ToLocal(mouseLeftDownPos);
            var bottomRight = ToLocal(current);
            selectionRegion.UpdatePosition(topLeft, bottomRight);
        }

        public SelectableControl GetSelectableControl(Selectable selectable)
        {
            foreach (SelectableControl selectableControl in controlCanvas.Children)
            {
                if (selectableControl.CurrentSelectable == selectable)
                {
                    return selectableControl;
                }
            }
            return null;
        }

        public SourceControl[] GetAllSourceControls()
        {
            var ret = new List<SourceControl>();
            foreach (SelectableControl selectableControl in controlCanvas.Children)
            {
                if (selectableControl is SourceControl)
                {
                    ret.Add(selectableControl as SourceControl);
                }
            }
            return ret.ToArray();
        }

        public CommentControl[] GetAllCommentControls()
        {
            var ret = new List<CommentControl>();
            foreach (SelectableControl selectableControl in controlCanvas.Children)
            {
                if (selectableControl is CommentControl)
                {
                    ret.Add(selectableControl as CommentControl);
                }
            }
            return ret.ToArray();
        }

        public SelectableControl[] GetAllSelectableControls()
        {
            var ret = new List<SelectableControl>();
            foreach (SelectableControl selectableControl in controlCanvas.Children)
            {
                ret.Add(selectableControl);
            }
            return ret.ToArray();
        }

        public BoundCommentControl[] GetAllBoundCommentControls()
        {
            var ret = new List<BoundCommentControl>();
            foreach (BoundCommentControl commentControl in boundCommentCanvas.Children)
            {
                ret.Add(commentControl);
            }
            return ret.ToArray();
        }

        public ScopeControl[] GetAllScopeControls()
        {
            var ret = new List<ScopeControl>();
            foreach (ScopeControl scopeControl in scopeCanvas.Children)
            {
                ret.Add(scopeControl);
            }
            return ret.ToArray();
        }

        public ScopeControl GetScopeByPosition(Point p)
        {
            return GetAllScopeControls().LastOrDefault(c =>
            {
                var rect = new Rect(c.CurrentScope.Position, c.RenderSize);
                return rect.Contains(p);
            });
        }

        public ScopeControl GetScopeById(int id)
        {
            return GetAllScopeControls().FirstOrDefault(c => c.CurrentScope.Id == id);
        }

        private SourceControl GetSourceControlById(int id)
        {
            return GetAllSourceControls().FirstOrDefault(s => s.CurrentSource.Id == id);
        }

        private void SelectInRegionSelection()
        {
            foreach (SelectableControl selectableControl in controlCanvas.Children)
            {
                if (InSelection(selectableControl.RenderTransform.Value.OffsetX,
                    selectableControl.RenderTransform.Value.OffsetY,
                    selectableControl.ActualWidth, selectableControl.ActualHeight))
                {
                    selectableManager.AddSelect(selectableControl.CurrentSelectable);
                }
            }
        }

        private bool InSelection(double offsetX, double offsetY, double width, double height)
        {
            var selectionRect = new Rect(selectionRegion.TopLeft, selectionRegion.BottomRight);
            var marginRect = new Rect(offsetX, offsetY, width, height);
            return selectionRect.IntersectsWith(marginRect);
        }

        private void FlowAreaControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            backCanvas.Width = mainCanvas.ActualWidth;
            backCanvas.Height = mainCanvas.ActualHeight;
            UpdateTransform();
        }

        private void backCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateGrid();
        }

        private void UserControl_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            UpdateScaleAndTranslate(Math.Sign(e.Delta), Mouse.GetPosition(this));
        }

        private void UserControl_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Right || e.ChangedButton == MouseButton.Middle)
            {
                isMouseMiddleDown = true;
                mouseMiddleDownPos = Mouse.GetPosition(this);
            }
            if (e.ChangedButton == MouseButton.Left)
            {
                selectionRegion.Visibility = System.Windows.Visibility.Visible;
                isMouseLeftDown = true;
                mouseLeftDownPos = Mouse.GetPosition(this);
                selectionRegion.UpdatePosition(ToLocal(mouseLeftDownPos), ToLocal(mouseLeftDownPos));
                if (!Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
                {
                    selectableManager.ClearSelect();
                }
            }
        }

        private void UserControl_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Right)
            {
                mouseRightDownPos = Mouse.GetPosition(this);
            }
        }

        private void UserControl_MouseMove(object sender, MouseEventArgs e)
        {
            if (isMouseMiddleDown)
            {
                UpdateTranslate(Mouse.GetPosition(this));
                return;
            }
            if (isMouseLeftDown)
            {
                UpdateRegionSelection();
                return;
            }
            UpdateCurrentArrow();
        }

        private void UserControl_MouseUp(object sender, MouseButtonEventArgs e)
        {
            isMouseMiddleDown &= (e.ChangedButton != MouseButton.Right && e.ChangedButton != MouseButton.Middle);
            if (e.ChangedButton == MouseButton.Left)
            {
                if (isMouseLeftDown)
                {
                    SelectInRegionSelection();
                    selectionRegion.Visibility = System.Windows.Visibility.Hidden;
                    isMouseLeftDown = false;
                }
            }
            DisableIsConnectable(false);
            DisableIsConnectable(true);
            DisableConnect();
            DisableMove();
        }

        private void OnSelectionChanged(string name, Dictionary<string, string> propertyData)
        {
            SelectionChanged?.Invoke(name, propertyData);
        }

        private bool OnCanConvert(string from, string to)
        {
            if (CanConvert != null)
            {
                return CanConvert(from, to);
            }
            return false;
        }

        private void OnModified()
        {
            Modified?.Invoke();
        }

        private Source OnGetSource(string fullName)
        {
            if (GetSource != null)
            {
                return GetSource(fullName);
            }
            return null;
        }

        public void AddScopeAt(double x, double y)
        {
            if (selectableManager.SelectedSelectables.Length > 0)
            {
                AddScopeAtImplToSelection(x, y);
            }
            else
            {
                AddScopeAtImpl(x, y);
            }
        }

        private ScopeControl AddScopeAtImplToSelection(double x, double y)
        {
            // find common scope
            var scopes = new HashSet<Scope>();
            foreach (Selectable selectable in selectableManager.SelectedSelectables)
            {
                var scopeChild = selectable as ScopeChild;
                if (scopeChild == null)
                {
                    continue;
                }
                if (scopeChild.Scope != null)
                {
                    scopes.Add(scopeChild.Scope);
                }
                else
                {
                    scopes.Clear();
                    break;
                }
            }
            Scope targetScope = null;
            if (scopes.Count >= 1)
            {
                targetScope = scopes.First();
                foreach (Scope scope in scopes)
                {
                    if (targetScope == scope)
                    {
                        continue;
                    }
                    if (targetScope.IsParent(scope))
                    {
                        targetScope = scope;
                        continue;
                    }
                    if (scope.IsParent(targetScope))
                    {
                        continue;
                    }

                    // when no relation
                    targetScope = null;
                    break;
                }
            }

            var scopeControl = CreateScopeControl(ToLocal(new Point(x, y)), Utility.GetRandomColor(), -1);
            commandManager.StartCommandSet();
            if (targetScope == null)
            {
                var topScopes = new HashSet<Scope>();
                var command = new AddScopeCommand(this, scopeControl, null);
                commandManager.AddCommand(command);
                foreach (Selectable selectable in selectableManager.SelectedSelectables)
                {
                    var selectableControl = GetSelectableControl(selectable);
                    if (selectableControl == null)
                    {
                        continue;
                    }

                    var scopeChild = selectable as ScopeChild;
                    if (scopeChild.Scope == null)
                    {
                        var addCommand = new AddSelectableToScopeCommand(this, scopeControl, selectableControl);
                        commandManager.AddCommand(addCommand);
                    }
                    else
                    {
                        topScopes.Add(scopeChild.Scope.GetTop());
                    }
                }
                foreach (Scope topScope in topScopes)
                {
                    var topScopeControl = GetScopeById(topScope.Id);
                    var removeCommand = new RemoveScopeCommand(this, topScopeControl, null);
                    commandManager.AddCommand(removeCommand);
                    var addCommand = new AddScopeCommand(this, topScopeControl, scopeControl);
                    commandManager.AddCommand(addCommand);
                }
            }
            else
            {
                var targetScopeControl = GetScopeById(targetScope.Id);
                scopes.Remove(targetScope);
                var changeParentScopes = scopes.Select(s =>
                {
                    Scope scope = s;
                    while (scope.Parent != targetScope)
                    {
                        scope = scope.Parent;
                    }
                    return scope;
                }).Distinct().ToArray();

                var addCommand = new AddScopeCommand(this, scopeControl, targetScopeControl);
                commandManager.AddCommand(addCommand);
                // change parent
                foreach (Scope scope in changeParentScopes)
                {
                    var childScopeControl = GetScopeById(scope.Id);
                    var removeChildCommand = new RemoveScopeCommand(this, childScopeControl, targetScopeControl);
                    commandManager.AddCommand(removeChildCommand);
                    var addChildCommand = new AddScopeCommand(this, childScopeControl, scopeControl);
                    commandManager.AddCommand(addChildCommand);
                }
                foreach (Selectable selectable in selectableManager.SelectedSelectables)
                {
                    var selectableControl = GetSelectableControl(selectable);
                    if (selectableControl == null)
                    {
                        continue;
                    }

                    var scopeChild = selectable as ScopeChild;
                    if (targetScopeControl.CurrentScope == scopeChild.Scope)
                    {
                        var removeSelectableCommand = new RemoveSelectableFromScopeCommand(this, targetScopeControl, selectableControl);
                        commandManager.AddCommand(removeSelectableCommand);
                        var addSelectableCommand = new AddSelectableToScopeCommand(this, scopeControl, selectableControl);
                        commandManager.AddCommand(addSelectableCommand);
                    }
                }
            }
            commandManager.EndCommandSet();
            return scopeControl;
        }

        private ScopeControl AddScopeAtImpl(double x, double y)
        {
            return AddScopeAtImpl(ToLocal(new Point(x, y)), Utility.GetRandomColor(), -1);
        }

        private ScopeControl CreateScopeControl(Point p, Color color, int id)
        {
            var scopeControl = new ScopeControl
            {
                DataContext = new Scope
                {
                    Id = id < 0 ? idManager.GetId() : id,
                    Color = color,
                    Position = p
                }
            };
            scopeControl.MouseDoubleClick += scopeControl_MouseDoubleClick;
            return scopeControl;
        }

        private ScopeControl AddScopeAtImpl(Point p, Color color, int id)
        {
            return AddScopeAtImpl(p, color, id, GetScopeByPosition(p));
        }

        private ScopeControl AddScopeAtImpl(Point p, Color color, int id, ScopeControl parentScopeControl)
        {
            var scopeControl = CreateScopeControl(p, color, id);
            return AddScopeImpl(scopeControl, parentScopeControl);
        }

        private ScopeControl AddScopeImpl(ScopeControl scopeControl, ScopeControl parentScopeControl)
        {
            var command = new AddScopeCommand(this, scopeControl, parentScopeControl);
            commandManager.AddCommand(command);
            return scopeControl;
        }

        public void AddScopeControl(ScopeControl scopeControl, ScopeControl parentScopeControl)
        {
            if (parentScopeControl == null)
            {
                scopeCanvas.Children.Add(scopeControl);
            }
            else
            {
                var index = scopeCanvas.Children.IndexOf(parentScopeControl);
                scopeCanvas.Children.Insert(index + 1, scopeControl);
            }
        }

        public void RemoveScopeAt(double x, double y)
        {
            if (!CanRemoveScopeAt(x, y))
            {
                return;
            }

            var scopeControl = GetScopeByPosition(ToLocal(new Point(x, y)));
            if (scopeControl == null)
            {
                return;
            }
            RemoveScopeAtImpl(scopeControl);
        }

        private void RemoveScopeAtImpl(ScopeControl scopeControl)
        {
            var parentScopeControl = GetAllScopeControls().FirstOrDefault(c => c.ContainsScope(scopeControl));
            commandManager.StartCommandSet();
            foreach (Scope scope in scopeControl.ChildScopes.Select(c => c.CurrentScope))
            {
                var childScopeControl = GetScopeById(scope.Id);
                var removeCommand = new RemoveScopeCommand(this, childScopeControl, scopeControl);
                commandManager.AddCommand(removeCommand);
                var addCommand = new AddScopeCommand(this, childScopeControl, parentScopeControl);
                commandManager.AddCommand(addCommand);
            }
            foreach (SelectableControl selectableControl in GetAllSelectableControls().Where(c =>
            {
                var child = c.CurrentSelectable as ScopeChild;
                return child.Scope == scopeControl.CurrentScope;
            }))
            {
                var removeCommand = new RemoveSelectableFromScopeCommand(this, scopeControl, selectableControl);
                commandManager.AddCommand(removeCommand);
                if (parentScopeControl != null)
                {
                    var addCommand = new AddSelectableToScopeCommand(this, parentScopeControl, selectableControl);
                    commandManager.AddCommand(addCommand);
                }
            }
            var removeScopeCommand = new RemoveScopeCommand(this, scopeControl, parentScopeControl);
            commandManager.AddCommand(removeScopeCommand);
            /*var command = new RemoveScopeAndSelectablesCommand(this, scopeControl, parentScopeControl);
            commandManager.AddCommand(command);*/
            commandManager.EndCommandSet();
        }

        public void AddSourceAt(double x, double y, string fullName)
        {
            var pos = ToLocal(new Point(x, y));
            AddSourceAtImpl(-1, pos.X, pos.Y, fullName);
        }

        internal SourceControl AddSourceAtImpl(int id, double x, double y, string fullName)
        {
            var sourceControl = CreateSource(id, fullName);
            sourceControl.CurrentSource.Position = new Point(x, y);
            return AddSourceAtImpl(sourceControl);
        }

        internal SourceControl AddSourceAtImpl(SourceControl sourceControl)
        {
            var command = new AddSourceCommand(this, sourceControl);
            commandManager.AddCommand(command);
            return command.SourceControl;
        }

        public CommentControl AddCommentAt(double x, double y)
        {
            var pos = ToLocal(new Point(x, y));
            return AddCommentAtImpl(pos.X, pos.Y);
        }

        private CommentControl AddCommentAtImpl(double x, double y)
        {
            var command = new AddCommentCommand(this, x, y);
            commandManager.AddCommand(command);
            command.CommentControl.CurrentComment.Text = "Comment";
            return command.CommentControl;
        }

        public void Undo()
        {
            if (commandManager.CanUndo)
            {
                commandManager.Undo();
            }
        }

        public void Redo()
        {
            if (commandManager.CanRedo)
            {
                commandManager.Redo();
            }
        }

        public void Cut()
        {
            if (!CanCut)
            {
                return;
            }

            Copy();
            RemoveSelected();
        }

        private string CopyImpl()
        {
            var selectedSources = GetAllSourceControls().Select(s => s.CurrentSource).Where(s => s.IsSelected).ToArray();
            var selectedComments = GetAllCommentControls().Select(c => c.CurrentComment).Where(c => c.IsSelected).ToArray();
            return SelectionSerializer.Serializer.Serialize(new Selection(guid, selectedSources, selectedComments));
        }

        private void PasteImpl(Selection selection, bool connectArrows, Point point)
        {
            bool isSameAreaControl = selection.Guid == guid;
            double minX = double.PositiveInfinity, minY = double.PositiveInfinity;
            foreach (Source source in selection.Sources)
            {
                if (source.Position.Y < minY)
                {
                    minX = source.Position.X;
                    minY = source.Position.Y;
                }
                else if (source.Position.Y == minY)
                {
                    if (source.Position.X < minX)
                    {
                        minX = source.Position.X;
                    }
                }
            }
            foreach (Comment comment in selection.Comments)
            {
                if (comment.Position.X < minX)
                {
                    minX = comment.Position.X;
                    minY = comment.Position.Y;
                }
                else if (comment.Position.X == minX)
                {
                    if (comment.Position.Y < minY)
                    {
                        minY = comment.Position.Y;
                    }
                }
            }
            selectableManager.ClearSelect();
            commandManager.StartCommandSet();
            var sourceIdDict = new Dictionary<int, int>();
            var sourceDict = new Dictionary<Source, SourceControl>();
            var commentDict = new Dictionary<Comment, CommentControl>();
            foreach (Source source in selection.Sources)
            {
                int oldId = source.Id;
                var sourceControl = AddSourceAtImpl(CreateSource(source, -1));
                sourceControl.CurrentSource.Position =
                    new Point(source.Position.X - minX + point.X, source.Position.Y - minY + point.Y);
                selectableManager.AddSelect(sourceControl.CurrentSource);
                sourceControl.CurrentSource.IsCollapsed = source.IsCollapsed;
                sourceIdDict.Add(oldId, sourceControl.CurrentSource.Id);
                sourceDict.Add(source, sourceControl);
                if (source.Comment != null)
                {
                    var command = new AddBoundCommentBommand(this, sourceControl);
                    command.CommentControl.CurrentComment.Text = source.Comment.Text;
                    commandManager.AddCommand(command);
                }
            }
            foreach (Comment comment in selection.Comments)
            {
                var commentControl = AddCommentAtImpl(
                    comment.Position.X - minX + point.X,
                    comment.Position.Y - minY + point.Y);
                commentControl.CurrentComment.Text = comment.Text;
                selectableManager.AddSelect(commentControl.CurrentComment);
                commentDict.Add(comment, commentControl);
            }
            UpdateLayout();
            if (connectArrows)
            {
                foreach (ConnectionInfo connection in selection.Connections)
                {
                    SourceControl src = GetSourceControlById(sourceIdDict[connection.SrcId]),
                        dest = GetSourceControlById(sourceIdDict[connection.DestId]);
                    if (src != null && dest != null)
                    {
                        SourceItemControl srcItem = src.GetItemControlByName(connection.SrcName, true),
                            destItem = dest.GetItemControlByName(connection.DestName, false);
                        var command = new AddFlowCommand(this, new ArrowControl(), srcItem, destItem);
                        commandManager.AddCommand(command);
                    }
                }
                if (isSameAreaControl)
                {
                    foreach (ConnectionInfo connection in selection.ExternalConnections)
                    {
                        SourceControl src = GetSourceControlById(connection.SrcId),
                            dest = GetSourceControlById(sourceIdDict[connection.DestId]);
                        if (src != null && dest != null)
                        {
                            SourceItemControl srcItem = src.GetItemControlByName(connection.SrcName, true),
                                destItem = dest.GetItemControlByName(connection.DestName, false);
                            var command = new AddFlowCommand(this, new ArrowControl(), srcItem, destItem);
                            commandManager.AddCommand(command);
                        }
                    }
                }
            }
            UpdateLayout();
            var parentScopeIdList = new Dictionary<ScopeControl, int>();
            var addedScopes = new List<ScopeControl>();
            var scopeIdDict = new Dictionary<int, int>();
            foreach (Scope scope in selection.Scopes)
            {
                ScopeControl scopeControl = null;
                if (scope.ParentScopeId < 0)
                {
                    scopeControl = AddScopeAtImpl(scope.Position, scope.Color, -1, null);
                    addedScopes.Add(scopeControl);
                }
                else
                {
                    scopeControl = CreateScopeControl(scope.Position, scope.Color, -1);
                    parentScopeIdList.Add(scopeControl, scope.ParentScopeId);
                }
                scopeIdDict.Add(scope.Id, scopeControl.CurrentScope.Id);
            }
            foreach (ScopeControl scopeControl in parentScopeIdList.Keys.ToArray())
            {
                parentScopeIdList[scopeControl] = scopeIdDict[parentScopeIdList[scopeControl]];
            }
            SolveScopeDependencyAndAdd(parentScopeIdList, addedScopes.ToArray());
            UpdateLayout();
            foreach (Source source in selection.Sources)
            {
                if (source.ScopeId < 0)
                {
                    continue;
                }
                var scopeControl = GetScopeById(scopeIdDict[source.ScopeId]);
                if (scopeControl == null)
                {
                    continue;
                }

                var command = new AddSelectableToScopeCommand(this, scopeControl, sourceDict[source]);
                commandManager.AddCommand(command);
            }
            foreach (Comment comment in selection.Comments)
            {
                if (comment.ScopeId < 0)
                {
                    continue;
                }
                var scopeControl = GetScopeById(scopeIdDict[comment.ScopeId]);
                if (scopeControl == null)
                {
                    continue;
                }

                var command = new AddSelectableToScopeCommand(this, scopeControl, commentDict[comment]);
                commandManager.AddCommand(command);
            }
            commandManager.EndCommandSet();
        }

        private void UpdateMaxId()
        {
            var sources = GetAllSourceControls();
            int maxId = sources.Length == 0 ? 0 : sources.Max(s => s.CurrentSource.Id);
            var scopes = GetAllScopeControls();
            maxId = Math.Max(maxId, scopes.Length == 0 ? 0 : scopes.Max(s => s.CurrentScope.Id));
            idManager.SetMaxId(maxId);
        }

        public void Copy()
        {
            if (!CanCopy)
            {
                return;
            }

            var str = CopyImpl();
            try
            {
                Clipboard.SetText(str);
                return;
            }
            catch { }
        }

        public void PasteAt(double x, double y)
        {
            var selection = GetClipBoardSelection();
            if (selection == null)
            {
                return;
            }

            PasteImpl(selection, false, ToLocal(new Point(x, y)));
        }

        public void PasteWithLinksAt(double x, double y)
        {
            var selection = GetClipBoardSelection();
            if (selection == null)
            {
                return;
            }

            PasteImpl(selection, true, ToLocal(new Point(x, y)));
        }

        public void RemoveSelected()
        {
            if (selectableManager.SelectedSelectables.Length == 0)
            {
                return;
            }

            var controls = GetAllSelectableControls().Where(m => m.CurrentSelectable.IsSelected).ToArray();
            var command = new RemoveSelectablesCommand(this, controls);
            commandManager.AddCommand(command);
            selectableManager.ClearSelect();
        }

        public void FitToView()
        {
            if (controlCanvas.Children.Count == 0)
            {
                return;
            }
            DisposeAutoFocusTimer();
            double minX = double.PositiveInfinity, maxX = double.NegativeInfinity,
                minY = double.PositiveInfinity, maxY = double.NegativeInfinity;
            SelectableControl left = null, top = null;
            foreach (SelectableControl selectableControl in controlCanvas.Children)
            {
                if (selectableControl.RenderTransform.Value.OffsetX < minX)
                {
                    minX = selectableControl.RenderTransform.Value.OffsetX;
                    left = selectableControl;
                }
                if (selectableControl.RenderTransform.Value.OffsetX + selectableControl.ActualWidth > maxX)
                {
                    maxX = selectableControl.RenderTransform.Value.OffsetX + selectableControl.ActualWidth;
                }
                if (selectableControl.RenderTransform.Value.OffsetY < minY)
                {
                    minY = selectableControl.RenderTransform.Value.OffsetY;
                    top = selectableControl;
                }
                if (selectableControl.RenderTransform.Value.OffsetY + selectableControl.ActualHeight > maxY)
                {
                    maxY = selectableControl.RenderTransform.Value.OffsetY + selectableControl.ActualHeight;
                }
            }
            scale = (int)(Math.Min(baseScale, Math.Min(this.ActualWidth / (maxX - minX), this.ActualHeight / (maxY - minY))) * baseScale);
            if (scale < 1)
            {
                scale = 1;
            }
            if (scale > maxScale)
            {
                scale = maxScale;
            }
            currentScale = (float)scale / baseScale;
            UpdateTransform();
            var p = ToGlobal(new Point(left.RenderTransform.Value.OffsetX, top.RenderTransform.Value.OffsetY));
            currentShiftX += -p.X;
            currentShiftY += -p.Y;
            UpdateTransform();
        }

        public void Clear()
        {
            foreach (ArrowControl arrowControl in arrowCanvas.Children)
            {
                arrowControl.SrcItem = null;
                arrowControl.DestItem = null;
            }
            arrowCanvas.Children.Clear();
            controlCanvas.Children.Clear();
            boundCommentCanvas.Children.Clear();
            scopeCanvas.Children.Clear();
            commandManager.Clear();
        }

        public void ChangeSelectedSourcePropertyValue(string name, string value)
        {
            if (selectableManager.SelectedSelectables.Length == 1 && selectableManager.SelectedSelectables[0] is Source)
            {
                var source = selectableManager.SelectedSelectables[0] as Source;
                var item = source.Items.First(s => s.Name == name);
                if (item != null)
                {
                    var command = new ChangeValueCommand(item, value);
                    commandManager.AddCommand(command);
                }
            }
        }

        public void AdjustPosition()
        {
            var adjuster = new PositionAdjuster(GetAllSourceControls().ToArray(), commandManager);
            adjuster.Adjust();
        }

        public void AddBoundComment()
        {
            if (!CanAddBoundComment)
            {
                return;
            }

            var command = new AddBoundCommentBommand(this, SingleSelectedSourceControl);
            commandManager.AddCommand(command);
        }

        public void RemoveBoundComment()
        {
            if (!CanRemoveBoundComment)
            {
                return;
            }

            var comment = SingleSelectedSourceControl.CurrentSource.Comment;
            var commentControl = GetAllBoundCommentControls().FirstOrDefault(b => b.CurrentComment == comment);
            if (commentControl == null)
            {
                return;
            }

            var command = new RemoveBoundCommentCommand(this, SingleSelectedSourceControl, commentControl);
            commandManager.AddCommand(command);
        }

        public void SetBreakPoint()
        {
            if (!CanSetBreakPoint)
            {
                return;
            }

            SingleSelectedSourceControl.CurrentSource.IsBreakPointSet = true;
        }

        public void UnsetBreakPoint()
        {
            if (CanSetBreakPoint)
            {
                return;
            }

            SingleSelectedSourceControl.CurrentSource.IsBreakPointSet = false;
        }

        public int[] GetBreakPoints()
        {
            return GetAllSourceControls().Where(s => s.CurrentSource.IsBreakPointSet).Select(s => s.CurrentSource.Id).ToArray();
        }

        private bool ContainsString(string str1, string str2)
        {
            if (str1 == null || str2 == null)
            {
                return false;
            }

            return str1.ToLower().Contains(str2);
        }

        public SearchResult Search(string query)
        {
            query = query.ToLower();
            if (String.IsNullOrEmpty(query))
            {
                return new SearchResult(guid, new Source[0], new Comment[0]);
            }
            var result = SearchWithQuery(query);
            if (result != null)
            {
                return result;
            }

            return new SearchResult(guid,
                GetAllSourceControls().Select(s => s.CurrentSource).Where(
                s =>
                {
                    if (ContainsString(s.Name, query))
                    {
                        return true;
                    }

                    if (s.Comment != null && ContainsString(s.Comment.Text, query))
                    {
                        return true;
                    }

                    return s.Items.Any(i => ContainsString(i.Name, query) || ContainsString(i.Value, query) ||
                        ContainsString(String.Format("{0}={1}", i.Name, i.Value), query));
                }).ToArray(),
                GetAllCommentControls().Select(s => s.CurrentComment).Where(
                c =>
                {
                    if (ContainsString(c.Text, query))
                    {
                        return true;
                    }

                    return false;
                }).ToArray());
        }

        public void Select(Selectable selectable)
        {
            if (selectable == null || GetAllSelectableControls().FirstOrDefault(m => m.CurrentSelectable == selectable) == null)
            {
                return;
            }
            SelectImpl(selectable);
        }

        public void Select(int id)
        {
            var source = GetAllSourceControls().FirstOrDefault(s => s.CurrentSource.Id == id);
            if (source != null)
            {
                SelectImpl(source.CurrentSource);
            }
        }

        private void SelectImpl(Selectable selectable)
        {
            selectableManager.ClearSelect();
            selectableManager.AddSelect(selectable);
        }

        public void Focus(Selectable selectable)
        {
            if (selectable == null)
            {
                return;
            }

            var control = GetAllSelectableControls().FirstOrDefault(m => m.CurrentSelectable == selectable);
            if (control == null)
            {
                return;
            }
            FocusImpl(selectable, control);
        }

        public void Focus(int id)
        {
            var source = GetAllSourceControls().FirstOrDefault(s => s.CurrentSource.Id == id);
            if (source != null)
            {
                FocusImpl(source.CurrentSource, source);
            }
        }

        public void ShowError(int id, string text)
        {
            var source = GetAllSourceControls().FirstOrDefault(s => s.CurrentSource.Id == id);
            if (source != null)
            {
                var comment = new ErrorCommentControl
                {
                    DataContext = new Comment { Text = text },
                    Source = source
                };
                errorCommentCanvas.Children.Add(comment);
            }
        }

        private void DisposeAutoFocusTimer()
        {
            if (autoFocusTimer != null)
            {
                autoFocusTimer.Stop();
                autoFocusTimer.Dispose();
                autoFocusTimer = null;
            }
        }

        private void FocusImpl(Selectable selectable, SelectableControl control)
        {
            var targetShiftX = -(selectable.Position.X + control.ActualWidth / 2) * currentScale;
            var targetShiftY = -(selectable.Position.Y + control.ActualHeight / 2) * currentScale;
            DisposeAutoFocusTimer();
            autoFocusTimer = new Timer(16);
            autoFocusTimer.Elapsed += (sender, e) =>
            {
                var finish = false;
                currentShiftX = targetShiftX * 0.25f + currentShiftX * 0.75f;
                currentShiftY = targetShiftY * 0.25f + currentShiftY * 0.75f;
                if (Math.Abs(currentShiftX - targetShiftX) <= 1 && Math.Abs(currentShiftY - targetShiftY) <= 1)
                {
                    currentShiftX = targetShiftX;
                    currentShiftY = targetShiftY;
                    finish = true;
                }
                Dispatcher.Invoke((Action)(() =>
                {
                    UpdateTransform();
                    if (finish)
                    {
                        DisposeAutoFocusTimer();
                    }
                }));
            };
            autoFocusTimer.Start();
        }

        public void SelectSourceAndCommentInScopeAt(double x, double y, bool recursive)
        {
            if (!CanSelectSourceAndCommentInScopeAt(x, y))
            {
                return;
            }

            selectableManager.ClearSelect();
            var p = ToLocal(new Point(x, y));
            var scopeControl = GetScopeByPosition(p);
            SelectInScope(scopeControl, recursive);
        }

        private void SelectInScope(ScopeControl scopeControl, bool recursive)
        {
            foreach (SelectableControl selectableControl in GetAllSelectableControls().Where(s =>
                (s.CurrentSelectable as ScopeChild).Scope == scopeControl.CurrentScope))
            {
                selectableManager.AddSelect(selectableControl.CurrentSelectable);
            }

            if (recursive)
            {
                foreach (ScopeControl child in scopeControl.ChildScopes)
                {
                    SelectInScope(child, true);
                }
            }
        }

        private Selection GetClipBoardSelection()
        {
            try
            {
                var xml = Clipboard.GetText();
                return SelectionSerializer.Serializer.Deserialize(xml, OnGetSource);
            }
            catch
            {
                return null;
            }
        }

        public bool CanCut
        {
            get
            {
                return selectableManager.SelectedSelectables.Length > 0;
            }
        }

        public bool CanCopy
        {
            get
            {
                return selectableManager.SelectedSelectables.Length > 0;
            }
        }

        public bool CanPaste
        {
            get
            {
                return GetClipBoardSelection() != null;
            }
        }

        public bool CanRemoveSelected
        {
            get
            {
                return selectableManager.SelectedSelectables.Length > 0;
            }
        }

        private SourceControl SingleSelectedSourceControl
        {
            get
            {
                if (selectableManager.SelectedSelectables.Length != 1)
                {
                    return null;
                }

                var source = selectableManager.SelectedSelectables[0] as Source;
                if (source == null)
                {
                    return null;
                }

                return GetSourceControlById(source.Id);
            }
        }

        public bool CanAddBoundComment
        {
            get
            {
                SourceControl sourceControl = SingleSelectedSourceControl;
                if (sourceControl == null)
                {
                    return false;
                }

                return sourceControl.CurrentSource.Comment == null;
            }
        }

        public bool CanRemoveBoundComment
        {
            get
            {
                SourceControl sourceControl = SingleSelectedSourceControl;
                if (sourceControl == null)
                {
                    return false;
                }

                return sourceControl.CurrentSource.Comment != null;
            }
        }

        public bool CanSetBreakPoint
        {
            get
            {
                SourceControl sourceControl = SingleSelectedSourceControl;
                if (sourceControl == null)
                {
                    return false;
                }

                return !sourceControl.CurrentSource.IsBreakPointSet;
            }
        }

        public bool CanUnsetBreakPoint
        {
            get
            {
                SourceControl sourceControl = SingleSelectedSourceControl;
                if (sourceControl == null)
                {
                    return false;
                }

                return sourceControl.CurrentSource.IsBreakPointSet;
            }
        }

        public bool CanFindLinkedItem(double x, double y)
        {
            return GetSourceAndItem(x, y, out SourceControl source, out SourceItemControl item);
        }

        public string FindLinkedItemQuery(double x, double y)
        {
            GetSourceAndItem(x, y, out SourceControl source, out SourceItemControl item);
            var isIn = source.CurrentSource.InItems.FirstOrDefault(i => i == item.CurrentItem) != null;
            return String.Format("@{0}[{1}][{2}]", source.CurrentSource.Id, isIn ? "In" : "Out", item.CurrentItem.Name);
        }

        private SearchResult SearchWithQuery(string query)
        {
            var match = queryRegex.Match(query);
            if (!match.Success)
            {
                return null;
            }
            var id = int.Parse(match.Groups[1].Value);
            var isOut = match.Groups[2].Value.ToLower() == "out";
            var propertyName = match.Groups[3].Value.ToLower();

            var source = GetAllSourceControls().FirstOrDefault(s => s.CurrentSource.Id == id);
            if (source == null)
            {
                return null;
            }
            Source[] sources;
            if (isOut)
            {
                var outItem = source.CurrentSource.OutItems.FirstOrDefault(i => i.Name.ToLower() == propertyName);
                if (outItem == null)
                {
                    return null;
                }
                sources = outItem.OutConnections.Select(c => c.Target.Source).ToArray();
            }
            else
            {
                var inItem = source.CurrentSource.InItems.FirstOrDefault(i => i.Name.ToLower() == propertyName);
                if (inItem == null)
                {
                    return null;
                }
                sources = new Source[] { inItem.InConnection.Target.Source };
            }
            return new SearchResult(guid, sources, new Comment[0]);
        }

        private bool GetSourceAt(double x, double y, out SourceControl source)
        {
            var p = ToLocal(new Point(x, y));
            source = GetAllSourceControls().Reverse().FirstOrDefault(s =>
                new Rect(s.CurrentPositionable.Position, new Size(s.ActualWidth, s.ActualHeight)).Contains(p));
            return source != null;
        }

        private bool GetSourceAndItem(double x, double y, out SourceControl source, out SourceItemControl item)
        {
            item = null;
            if (!GetSourceAt(x, y, out source))
            {
                return false;
            }
            var p = ToLocal(new Point(x, y));
            var target = (DependencyObject)source.InputHitTest(new Point(p.X - source.CurrentPositionable.Position.X, p.Y - source.CurrentPositionable.Position.Y));
            while (target != null)
            {
                if (target is SourceItemControl)
                {
                    item = (SourceItemControl)target;
                    break;
                }
                target = VisualTreeHelper.GetParent(target);
                if (target == source)
                {
                    break;
                }
            }
            return item != null;
        }

        public bool CanRemoveScopeAt(double x, double y)
        {
            return GetScopeByPosition(ToLocal(new Point(x, y))) != null;
        }

        public bool CanSelectSourceAndCommentInScopeAt(double x, double y)
        {
            return GetScopeByPosition(ToLocal(new Point(x, y))) != null;
        }

        public Point MouseRightDownPos
        {
            get
            {
                return mouseRightDownPos;
            }
        }

        public void ExpandAll()
        {
            foreach (var source in GetAllSourceControls())
            {
                source.CurrentSource.IsCollapsed = false;
            }
        }

        public void CollapseAll()
        {
            foreach (var source in GetAllSourceControls())
            {
                source.CurrentSource.IsCollapsed = true;
            }
        }

        private void UserControl_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyboardDevice.IsKeyDown(Key.Tab))
            {
                if (currentItem != null)
                {
                    GoToConnectedNodes(e.KeyboardDevice.IsKeyDown(Key.LeftShift));
                }
                else
                {
                    GoToSrcItem();
                }
                e.Handled = true;
            }
        }

        private void GoToConnectedNodes(bool isShiftKey)
        {
            var skipConnected = isShiftKey;
            if (itemPositionManager == null)
            {
                var position = Mouse.GetPosition(this);
                if (!GetSourceAndItem(position.X, position.Y, out SourceControl source, out SourceItemControl item))
                {
                    itemPositionManager = new ItemPositionManager(this.PointToScreen(Mouse.GetPosition(this)),
                        GetAllSourceControls().ToArray(), currentShiftX, currentShiftY);
                }
                else
                {
                    var isIn = source.CurrentSource.InItems.FirstOrDefault(i => i == item.CurrentItem) != null;
                    var points = new List<Point>();
                    if (isIn)
                    {
                        var outSource = GetSourceControlById(item.CurrentItem.InConnection.Target.Source.Id);
                        var outItem = outSource.OutItems.FirstOrDefault(o => o.CurrentItem == item.CurrentItem.InConnection.Target);
                        points.Add(outItem.PointToScreen(new Point(10, outItem.ActualHeight / 2)));
                    }
                    else
                    {
                        foreach (var connection in item.CurrentItem.OutConnections)
                        {
                            var inSource = GetSourceControlById(connection.Target.Source.Id);
                            var inItem = inSource.InItems.FirstOrDefault(i => i.CurrentItem == connection.Target);
                            points.Add(inItem.PointToScreen(new Point(10, inItem.ActualHeight / 2)));
                        }
                    }
                    itemPositionManager = new ItemPositionManager(this.PointToScreen(Mouse.GetPosition(this)),
                        points.ToArray(), currentShiftX, currentShiftY);
                }
                itemPositionManager.Create();
            }
            if (itemPositionManager.HasPositions)
            {
                var pos = itemPositionManager.Next(skipConnected);
                var nextPos = this.PointFromScreen(pos);
                var bound = new Rect(100, 100, ActualWidth - 100, ActualHeight - 100);
                double diffX = 0.0, diffY = 0.0;
                if (!bound.Contains(nextPos))
                {
                    if (nextPos.X < 100)
                    {
                        diffX = nextPos.X - 100;
                    }
                    else if (nextPos.X > ActualWidth - 100)
                    {
                        diffX = nextPos.X - (ActualWidth - 100);
                    }
                    if (nextPos.Y < 100)
                    {
                        diffY = nextPos.Y - 100;
                    }
                    else if (nextPos.Y > ActualHeight - 100)
                    {
                        diffY = nextPos.Y - (ActualHeight - 100);
                    }
                }
                currentShiftX = itemPositionManager.CurrentShiftX - diffX;
                currentShiftY = itemPositionManager.CurrentShiftY - diffY;
                UpdateTransform();
                System.Windows.Forms.Cursor.Position = new System.Drawing.Point((int)(pos.X - diffX), (int)(pos.Y - diffY));
            }
        }

        private void GoToSrcItem()
        {
            var position = Mouse.GetPosition(this);
            if (!GetSourceAndItem(position.X, position.Y, out SourceControl source, out SourceItemControl item))
            {
                return;
            }
            var isIn = source.CurrentSource.InItems.FirstOrDefault(i => i == item.CurrentItem) != null;
            if (!isIn)
            {
                return;
            }
            if (item.CurrentItem.InConnection == null)
            {
                return;
            }
            var outSource = GetSourceControlById(item.CurrentItem.InConnection.Target.Source.Id);
            var outItem = outSource.OutItems.FirstOrDefault(o => o.CurrentItem == item.CurrentItem.InConnection.Target);
            var pos = outItem.PointToScreen(new Point(10, outItem.ActualHeight / 2));
            var nextPos = this.PointFromScreen(pos);
            var bound = new Rect(100, 100, ActualWidth - 100, ActualHeight - 100);
            double diffX = 0.0, diffY = 0.0;
            if (!bound.Contains(nextPos))
            {
                if (nextPos.X < 100)
                {
                    diffX = nextPos.X - 100;
                }
                else if (nextPos.X > ActualWidth - 100)
                {
                    diffX = nextPos.X - (ActualWidth - 100);
                }
                if (nextPos.Y < 100)
                {
                    diffY = nextPos.Y - 100;
                }
                else if (nextPos.Y > ActualHeight - 100)
                {
                    diffY = nextPos.Y - (ActualHeight - 100);
                }
            }
            currentShiftX -= diffX;
            currentShiftY -= diffY;
            UpdateTransform();
            System.Windows.Forms.Cursor.Position = new System.Drawing.Point((int)(pos.X - diffX), (int)(pos.Y - diffY));
        }

        public bool CanGetNodeName(double x, double y)
        {
            return GetSourceAt(x, y, out SourceControl source);
        }


        public string GetNodeName(double x, double y)
        {
            if (GetSourceAt(x, y, out SourceControl source))
            {
                return source.CurrentSource.Name;
            }
            return null;
        }

        public bool GetNodeAndPropertyName(double x, double y, out string nodeName, out string propertyName, out string propertyValue)
        {
            nodeName = propertyName = propertyValue = null;
            if (GetSourceAndItem(x, y, out SourceControl source, out SourceItemControl item))
            {
                nodeName = source.CurrentSource.Name;
                propertyName = item.CurrentItem.Name;
                propertyValue = item.CurrentItem.Value;
                return true;
            }
            return false;
        }
    }
}
