using FlowScriptControl.Classes;
using FlowScriptDrawControl.Control;
using FlowScriptDrawControl.Model;
using FlowScriptEngine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace FlowScriptControl.Controls
{
    /// <summary>
    /// FlowScript を編集するパネルです（ノードを配置する領域と、右クリックのメニュー）。
    /// </summary>
    public partial class FlowDrawPanel : UserControl, IDisposable
    {
        public event EventHandler Modified;
        public event Action<string> SearchRequired;
        public event EventHandler SelectionChanged;

        private static readonly List<AssemblyAndType> flowSources;
        private static readonly Dictionary<string, System.Drawing.Color> colorDictionary;
        private static readonly Dictionary<string, string> typeAliasDictionary;
        private static readonly List<NodeFilterInfoBase> filters;
        private static readonly Dictionary<string, FlowSourceDumper> dumpersByTypeName = [];
        private static Dictionary<string, AssemblyAndTypeAndSource[]> sourceCache;
        private static bool initialized;

        private readonly List<Classes.IToolTipText> toolTipTexts = [];
        private readonly List<FlowNodeEntry> nodeEntries = [];
        private FlowAreaControl areaControl;
        private ILanguageProvider languageProvider;

        static FlowDrawPanel()
        {
            flowSources = [];
            colorDictionary = new Dictionary<string, System.Drawing.Color>
            {
                { "event", System.Drawing.Color.FromArgb(0, 255, 0) }
            };
            typeAliasDictionary = [];
            filters =
            [
                NodeFilterInfo.DefaultFilter
            ];
        }

        public FlowDrawPanel()
        {
            InitializeComponent();
            UpdateLanguage();
            Loaded += FlowDrawPanel_Loaded;
        }

        public bool IsControlFocused
        {
            get
            {
                return IsKeyboardFocusWithin;
            }
        }

        public static Dictionary<string, string> TypeAliasDictionary
        {
            get
            {
                return typeAliasDictionary;
            }
        }

        public static string EmptyFileString
        {
            get
            {
                return "<?xml version=\"1.0\" encoding=\"utf-8\"?><Root><Sources/><Flows/><Comments/></Root>";
            }
        }

        public Guid Guid
        {
            get
            {
                return areaControl.Guid;
            }
        }

        public FlowPropertyPanel FlowPropertyPanel
        {
            get;
            set;
        }

        public FlowTreeView FlowTreeView
        {
            get;
            set;
        }

        public ILanguageProvider LanguageProvider
        {
            get { return languageProvider ?? DefaultLanguageProvider.Default; }
            set
            {
                languageProvider = value;
                UpdateLanguage();
            }
        }

        private void FlowDrawPanel_Loaded(object sender, RoutedEventArgs e)
        {
            // タブの切り替えなどで Loaded は何度も呼ばれるので、初期化は 1 回だけ行う
            if (areaControl != null)
            {
                return;
            }

            EnsureSourceCache();
            foreach (var group in sourceCache)
            {
                foreach (var source in group.Value)
                {
                    try
                    {
                        AddNodeEntry(source);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
#if DEBUG
                        MessageBox.Show(ex.StackTrace);
#endif
                    }
                }
            }
            SortEntries(nodeEntries, true);
            addNodeMenuItem.ItemsSource = nodeEntries;

            areaControl = new FlowAreaControl
            {
                AllowDrop = true
            };
            areaControl.DragEnter += AreaControl_DragOver;
            areaControl.DragOver += AreaControl_DragOver;
            areaControl.Drop += AreaControl_Drop;
            areaControl.SelectionChanged += AreaControl_SelectionChanged;
            areaControl.CanConvert += AreaControl_CanConvert;
            areaControl.Modified += AreaControl_Modified;
            areaControl.GetSource += AreaControl_GetSource;
            rootGrid.Children.Add(areaControl);

            FinishInitialize();
        }

        private static void EnsureSourceCache()
        {
            if (sourceCache != null)
            {
                return;
            }
            sourceCache = [];
            foreach (IGrouping<string, AssemblyAndTypeAndSource> ss in flowSources.Select(a => new AssemblyAndTypeAndSource(a)).GroupBy(a => a.Source.Name).ToArray())
            {
                var sources = ss.ToArray();
                Array.Sort(sources, CompareSources);
                sourceCache.Add(ss.Key, sources);
                foreach (var source in sources)
                {
                    dumpersByTypeName[source.AssemblyAndType.Type.FullName] = source.Dumper;
                }
            }
        }

        // 同じ名前のノード（オーバーロード）の並び順。プロパティやメソッドが少ないものを先にする
        private static int CompareSources(AssemblyAndTypeAndSource a1, AssemblyAndTypeAndSource a2)
        {
            int count1 = a1.Source.InProperties.Count + a1.Source.InMethods.Count + a1.Source.OutProperties.Count + a1.Source.OutMethods.Count;
            int count2 = a2.Source.InProperties.Count + a2.Source.InMethods.Count + a2.Source.OutProperties.Count + a2.Source.OutMethods.Count;
            if (count1 != count2)
            {
                return count1 > count2 ? 1 : -1;
            }
            if (a1.Source.InProperties.Count != a2.Source.InProperties.Count)
            {
                return a1.Source.InProperties.Count > a2.Source.InProperties.Count ? 1 : -1;
            }
            if (a1.Source.OutProperties.Count != a2.Source.OutProperties.Count)
            {
                return a1.Source.OutProperties.Count > a2.Source.OutProperties.Count ? 1 : -1;
            }
            if (a1.Source.InMethods.Count != a2.Source.InMethods.Count)
            {
                return a1.Source.InMethods.Count > a2.Source.InMethods.Count ? 1 : -1;
            }
            if (a1.Source.OutMethods.Count != a2.Source.OutMethods.Count)
            {
                return a1.Source.OutMethods.Count > a2.Source.OutMethods.Count ? 1 : -1;
            }

            int point = 0;
            foreach (CustomMemberInfo<PropertyInfo> propertyInfo1 in a1.Source.InProperties)
            {
                var propertyInfo2 = a2.Source.InProperties.FirstOrDefault(c => c.MemberInfo.Name == propertyInfo1.MemberInfo.Name);
                if (propertyInfo2 != null && propertyInfo1.MemberInfo.PropertyType != propertyInfo2.MemberInfo.PropertyType)
                {
                    if (TypeConverterManager.CanConvert(propertyInfo2.MemberInfo.PropertyType, propertyInfo1.MemberInfo.PropertyType))
                    {
                        point--;
                    }
                    else
                    {
                        point++;
                    }
                }
            }
            return point;
        }

        private void AddNodeEntry(AssemblyAndTypeAndSource source)
        {
#if DEBUG
            if (!initialized)
            {
                source.Source.AutoTest();
            }
#endif
            toolTipTexts.Add(source.Dumper);
            source.Dumper.UpdateLanguage(null);
            if (languageProvider != null)
            {
                source.Dumper.UpdateLanguage(languageProvider);
            }
            var nameSplits = source.Source.Name.Split('.');
            IList<FlowNodeEntry> target = nodeEntries;
            for (int i = 0; i < nameSplits.Length - 1; i++)
            {
                var folderEntry = target.FirstOrDefault(entry => entry.Name == nameSplits[i]);
                if (folderEntry == null)
                {
                    var folder = new FlowSourceFolder(String.Join(".", nameSplits, 0, i + 1));
                    toolTipTexts.Add(folder);
                    if (languageProvider != null)
                    {
                        folder.UpdateLanguage(languageProvider);
                    }
                    folderEntry = new FlowNodeEntry(nameSplits[i], nameSplits[i], folder, null);
                    target.Add(folderEntry);
                }
                target = folderEntry.Children;
            }
            var name = nameSplits[^1];
            var existingEntry = target.FirstOrDefault(entry => entry.Name == name);
            if (existingEntry == null)
            {
                target.Add(new FlowNodeEntry(name, name, source.Dumper, source.Dumper));
            }
            else
            {
                // 同じ名前のノードが複数あるときは、それぞれをプロパティの型で区別できるようにする
                if (existingEntry.Children.Count == 0)
                {
                    existingEntry.Children.Add(new FlowNodeEntry(name, CreateTextFromDumper(existingEntry.Dumper), existingEntry.Dumper, existingEntry.Dumper));
                }
                existingEntry.Children.Add(new FlowNodeEntry(name, CreateTextFromDumper(source.Dumper), source.Dumper, source.Dumper));
            }
        }

        // 一番上とフォルダーの中だけ、表示する文字列の順に並べる（同じ名前のノードの並びは、CompareSources の順のまま）
        private static void SortEntries(IList<FlowNodeEntry> entries, bool sort)
        {
            foreach (var entry in entries)
            {
                SortEntries(entry.Children, entry.IsFolder);
            }
            if (sort)
            {
                var sorted = entries.OrderBy(entry => entry.Text, StringComparer.CurrentCulture).ToArray();
                entries.Clear();
                foreach (var entry in sorted)
                {
                    entries.Add(entry);
                }
            }
        }

        public static void EnumerateClass(string filePath)
        {
            foreach (AssemblyAndType asmAndType in FlowSourceEnumerator.EnumerateFromFile(filePath, [typeof(FlowSourceObjectBase), typeof(TypeColorBase), typeof(TypeAliasBase)]))
            {
                if (asmAndType.Type.IsSubclassOf(typeof(FlowSourceObjectBase)))
                {
                    flowSources.Add(asmAndType);
                }
                else if (asmAndType.Type.IsSubclassOf(typeof(TypeColorBase)))
                {
                    AddColorDefinition(asmAndType);
                }
                else if (asmAndType.Type.IsSubclassOf(typeof(TypeAliasBase)))
                {
                    AddTypeAlias(asmAndType);
                }
            }
        }

        public static void EnumerateClasses(string directoryPath, string[] exceptionFileList)
        {
            foreach (string dllFile in Directory.GetFiles(directoryPath, "*.dll"))
            {
                if (exceptionFileList.Any(name => name.Equals(Path.GetFileNameWithoutExtension(dllFile), StringComparison.CurrentCultureIgnoreCase)))
                {
                    continue;
                }

                EnumerateClass(dllFile);
            }
        }

        public static void AddFilter(NodeFilterInfoBase filter)
        {
            filters.Add(filter);
        }

        private void FinishInitialize()
        {
            if (!initialized)
            {
                if (FlowTreeView != null)
                {
                    FlowTreeView.SetEntries(nodeEntries.Select(entry => entry.Clone()));
                    FlowTreeView.SetFilters([.. filters]);
                }
#if DEBUG
                FlowSourceDumper.WriteToText("text.ini");
                FlowSourceDumper.CheckPropertyType();
                FlowSourceDumper.CheckPropertyColor(colorDictionary);
                FlowSourceDumper.CheckEnumTypeFormatter();
                FlowSourceDumper.CheckTypeSerializer();
#endif
            }
            if (FlowTreeView != null)
            {
                FlowTreeView.SelectedFilterChanged += FlowTreeView_SelectedFilterChanged;
                ApplyMenuFilter(nodeEntries, FlowTreeView.SelectedFilter);
            }
            initialized = true;
        }

        void FlowTreeView_SelectedFilterChanged(object sender, EventArgs e)
        {
            ApplyMenuFilter(nodeEntries, FlowTreeView.SelectedFilter);
        }

        // フィルターに合わないノードを、ノードの追加のメニューから隠す。子が全て隠れたフォルダーも隠す
        private static bool ApplyMenuFilter(IEnumerable<FlowNodeEntry> entries, NodeFilterInfoBase filterInfo)
        {
            bool hasVisibleNodes = false;
            foreach (var entry in entries)
            {
                bool visible;
                if (entry.Children.Count > 0)
                {
                    visible = ApplyMenuFilter(entry.Children, filterInfo);
                }
                else
                {
                    visible = entry.Dumper == null || filterInfo == null || !filterInfo.IsHide(entry.Dumper);
                }
                entry.IsVisible = visible;
                hasVisibleNodes |= visible;
            }
            return hasVisibleNodes;
        }

        private static void AddTypeAlias(AssemblyAndType asmAndType)
        {
            var typeAlias = (TypeAliasBase)asmAndType.Assembly.CreateInstance(asmAndType.Type.FullName);
            foreach (KeyValuePair<Type, string> kvp in typeAlias.EnumerateAlias())
            {
                typeAliasDictionary.TryAdd(kvp.Key.FullName, kvp.Value);
            }
        }

        private static void AddColorDefinition(AssemblyAndType asmAndType)
        {
            var temp = (TypeColorBase)asmAndType.Assembly.CreateInstance(asmAndType.Type.FullName);
            foreach (KeyValuePair<Type, System.Drawing.Color> kvp in temp.EnumerateColors())
            {
                colorDictionary[kvp.Key.FullName] = kvp.Value;
            }
        }

        private static string CreateTextFromDumper(FlowSourceDumper dumper)
        {
            var ret = new StringBuilder();
            if (dumper.InProperties.Length > 0)
            {
                ret.Append("In: ");
                ret.Append(String.Join(", ", dumper.InProperties.Select(p => String.Format("[{1}]{0}", p.Name, typeAliasDictionary[p.PropertyType.FullName])).ToArray()));
            }
            if (dumper.OutProperties.Length > 0)
            {
                if (ret.Length > 0)
                {
                    ret.Append("    ");
                }
                ret.Append("Out: ");
                ret.Append(String.Join(", ", dumper.OutProperties.Select(p => String.Format("[{1}]{0}", p.Name, typeAliasDictionary[p.PropertyType.FullName])).ToArray()));
            }
            return ret.ToString();
        }

        private static void PreProcessDumper(FlowSourceDumper dumper)
        {
            foreach (Item item in dumper.InEvents.Concat(dumper.OutEvents.Concat(dumper.InProperties.Concat(dumper.OutProperties))))
            {
                var c = colorDictionary[item.Type];
                item.TypeColor = System.Windows.Media.Color.FromRgb(c.R, c.G, c.B);
            }
            foreach (Item item in dumper.InProperties.Concat(dumper.OutProperties))
            {
                item.TypeText = dumper.ConvertPropertyType(item.PropertyType);
            }
        }

        private void AddSource(FlowSourceDumper dumper, double x, double y)
        {
            if (areaControl != null)
            {
                PreProcessDumper(dumper);
                areaControl.AddSourceAt(x, y, dumper.FullName);
            }
        }

        void AreaControl_Modified()
        {
            Modified?.Invoke(this, EventArgs.Empty);
        }

        bool AreaControl_CanConvert(string arg1, string arg2)
        {
            Type from = FlowSourceDumper.GetTypeFromString(arg1),
                to = FlowSourceDumper.GetTypeFromString(arg2);
            if (from == null || to == null)
            {
                return false;
            }
            return TypeConverterManager.CanConvert(from, to);
        }

        void AreaControl_SelectionChanged(string arg1, Dictionary<string, string> arg2)
        {
            if (String.IsNullOrEmpty(arg1))
            {
                FlowPropertyPanel?.MakeEmpty();
            }
            else if (FlowPropertyPanel != null)
            {
                var source = FlowSourceObjectManager.CreateSource(arg1);
                FlowPropertyPanel.ChangeFlowSource(this, source, arg2);
            }

            SelectionChanged?.Invoke(this, EventArgs.Empty);
        }

        Source AreaControl_GetSource(string arg)
        {
            if (dumpersByTypeName.TryGetValue(arg, out FlowSourceDumper dumper))
            {
                PreProcessDumper(dumper);
                var source = new Source(dumper.AssemblyAndType)
                {
                    Name = dumper.SourceName,
                    FullName = dumper.FullName,
                    ToolTip = $"({dumper.SourceName})\n{dumper.FlowToolTipText}",
                    Warning = dumper.Warning
                };
                foreach (Item item in dumper.InEvents.Concat(dumper.OutEvents.Concat(dumper.InProperties.Concat(dumper.OutProperties))))
                {
                    source.AddItem(item.Clone());
                }
                if (sourceCache.TryGetValue(dumper.SourceName, out var sameNameSources))
                {
                    foreach (AssemblyAndTypeAndSource asm in sameNameSources)
                    {
                        source.AddSameNameSource(asm.AssemblyAndType);
                    }
                }
                return source;
            }
            return null;
        }

        void AreaControl_DragOver(object sender, DragEventArgs e)
        {
            e.Effects = GetDraggedDumper(e) != null ? DragDropEffects.Copy : DragDropEffects.None;
            e.Handled = true;
        }

        void AreaControl_Drop(object sender, DragEventArgs e)
        {
            var dumper = GetDraggedDumper(e);
            if (dumper != null)
            {
                var p = e.GetPosition(areaControl);
                AddSource(dumper, p.X, p.Y);
                e.Handled = true;
            }
        }

        // ノードのツリーからドラッグされたノード。データはノードの型の完全な名前（別のウィンドウからのドラッグでも受け取れるように文字列にする）
        private static FlowSourceDumper GetDraggedDumper(DragEventArgs e)
        {
            if (e.Data.GetDataPresent(FlowTreeView.DragDataFormat) && e.Data.GetData(FlowTreeView.DragDataFormat) is string typeName &&
                dumpersByTypeName.TryGetValue(typeName, out var dumper))
            {
                return dumper;
            }
            return null;
        }

        internal static string GetTypeName(FlowSourceDumper dumper)
        {
            return dumper.AssemblyAndType.Type.FullName;
        }

        private void AddNodeMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (e.OriginalSource is MenuItem menuItem && menuItem.DataContext is FlowNodeEntry entry && entry.Dumper != null && entry.Children.Count == 0)
            {
                AddSource(entry.Dumper, areaControl.MouseRightDownPos.X, areaControl.MouseRightDownPos.Y);
                contextMenu.IsOpen = false;
            }
        }

        private void ContextMenu_Opened(object sender, RoutedEventArgs e)
        {
            if (areaControl == null)
            {
                contextMenu.IsOpen = false;
                return;
            }
            var x = areaControl.MouseRightDownPos.X;
            var y = areaControl.MouseRightDownPos.Y;
            cutMenuItem.IsEnabled = areaControl.CanCut;
            copyMenuItem.IsEnabled = areaControl.CanCopy;
            pasteMenuItem.IsEnabled = areaControl.CanPaste;
            pasteWithLinksMenuItem.IsEnabled = areaControl.CanPaste;
            deleteMenuItem.IsEnabled = areaControl.CanRemoveSelected;
            SetVisible(addBoundCommentMenuItem, areaControl.CanAddBoundComment);
            SetVisible(removeBoundCommentMenuItem, areaControl.CanRemoveBoundComment);
            SetVisible(removeScopeMenuItem, areaControl.CanRemoveScopeAt(x, y));
            var canSelectSourceAndCommentInScopeAt = areaControl.CanSelectSourceAndCommentInScopeAt(x, y);
            SetVisible(selectInScopeMenuItem, canSelectSourceAndCommentInScopeAt);
            SetVisible(selectAllInScopeMenuItem, canSelectSourceAndCommentInScopeAt);
            SetVisible(addBreakPointMenuItem, areaControl.CanSetBreakPoint);
            SetVisible(removeBreakPointMenuItem, areaControl.CanUnsetBreakPoint);
            SetVisible(findLinkedNodeMenuItem, areaControl.CanFindLinkedItem(x, y));
            var canCopyNodeName = areaControl.CanGetNodeName(x, y);
            SetVisible(copyNodeNameMenuItem, canCopyNodeName);
            var canCopyPropertyName = areaControl.GetNodeAndPropertyName(x, y, out _, out _, out _);
            SetVisible(copyPropertyNameMenuItem, canCopyPropertyName);
            SetVisible(copyNameSeparator, canCopyNodeName || canCopyPropertyName);
        }

        private static void SetVisible(UIElement element, bool visible)
        {
            element.Visibility = visible ? Visibility.Visible : Visibility.Collapsed;
        }

        private Point MouseRightDownPos
        {
            get { return areaControl.MouseRightDownPos; }
        }

        public void AddCommentAt(double x, double y)
        {
            areaControl?.AddCommentAt(x, y);
        }

        public void Undo()
        {
            areaControl?.Undo();
        }

        public void Redo()
        {
            areaControl?.Redo();
        }

        public void Cut()
        {
            areaControl?.Cut();
        }

        public void Copy()
        {
            areaControl?.Copy();
        }

        /// <summary>
        /// 貼り付けます。座標はこのパネルの座標（WPF の単位）です。
        /// </summary>
        public void PasteAt(double x, double y)
        {
            areaControl?.PasteAt(x, y);
        }

        /// <summary>
        /// マウスカーソルの位置に貼り付けます。
        /// </summary>
        public void PasteAtCursor()
        {
            if (areaControl != null)
            {
                var p = Mouse.GetPosition(areaControl);
                areaControl.PasteAt(p.X, p.Y);
            }
        }

        public void PasteWithLinksAt(double x, double y)
        {
            areaControl?.PasteWithLinksAt(x, y);
        }

        /// <summary>
        /// マウスカーソルの位置に、リンクも含めて貼り付けます。
        /// </summary>
        public void PasteWithLinksAtCursor()
        {
            if (areaControl != null)
            {
                var p = Mouse.GetPosition(areaControl);
                areaControl.PasteWithLinksAt(p.X, p.Y);
            }
        }

        public void Delete()
        {
            areaControl?.RemoveSelected();
        }

        public void FitToView()
        {
            areaControl?.FitToView();
        }

        public void AdjustPosition()
        {
            areaControl?.AdjustPosition();
        }

        public void SaveXML(Stream stream)
        {
            areaControl?.Save(stream);
        }

        public Source[] GetSources()
        {
            if (areaControl != null)
            {
                return [.. areaControl.GetAllSourceControls().Select(s => s.CurrentSource)];
            }
            return [];
        }

        public void LoadXML(Stream stream)
        {
            // 読み込みは呼び出し元のスレッドで行うので、進み具合は別のスレッドのウィンドウで表示する
            EnsureLoaded();
            LoadingWindow.ShowLoading();
            string[] errors;
            try
            {
                areaControl.Load(stream, out errors, LoadingWindow.ChangeProgress);
            }
            finally
            {
                LoadingWindow.CloseLoading();
            }
            if (errors.Length > 0)
            {
                MessageBox.Show(String.Join("\r\n", errors));
            }
        }

        // まだ表示されていない（Loaded の前の）パネルにも読み込めるようにする
        private void EnsureLoaded()
        {
            if (areaControl == null)
            {
                FlowDrawPanel_Loaded(this, new RoutedEventArgs());
            }
        }

        public void AddBindedComment()
        {
            if (areaControl != null && areaControl.CanAddBoundComment)
            {
                areaControl.AddBoundComment();
            }
        }

        public void RemoveBindedComment()
        {
            if (areaControl != null && areaControl.CanRemoveBoundComment)
            {
                areaControl.RemoveBoundComment();
            }
        }

        public int[] GetBreakPoints()
        {
            if (areaControl != null)
            {
                return areaControl.GetBreakPoints();
            }
            return [];
        }

        public SearchResult Search(string query)
        {
            return areaControl?.Search(query);
        }

        public void SelectAndFocus(Selectable selectable)
        {
            Select(selectable);
            Focus(selectable);
        }

        public void SelectAndFocus(int id)
        {
            Select(id);
            Focus(id);
        }

        public void Select(Selectable selectable)
        {
            areaControl?.Select(selectable);
        }

        public void Select(int id)
        {
            areaControl?.Select(id);
        }

        public int[] GetSelectedSourceId()
        {
            if (areaControl != null)
            {
                return areaControl.SelectedSourceId;
            }
            return [];
        }

        public void Focus(Selectable selectable)
        {
            areaControl?.Focus(selectable);
        }

        public void Focus(int id)
        {
            areaControl?.Focus(id);
        }

        public void ShowError(int id, string text)
        {
            areaControl?.ShowError(id, text);
        }

        public void AddScopeAt(double x, double y)
        {
            areaControl?.AddScopeAt(x, y);
        }

        public void RemoveScopeAt(double x, double y)
        {
            areaControl?.RemoveScopeAt(x, y);
        }

        public void SelectInScope(double x, double y)
        {
            areaControl?.SelectSourceAndCommentInScopeAt(x, y, false);
        }

        public void SelectAllInScope(double x, double y)
        {
            areaControl?.SelectSourceAndCommentInScopeAt(x, y, true);
        }

        public void SetBreakPoint()
        {
            areaControl?.SetBreakPoint();
        }

        public void UnsetBreakPoint()
        {
            areaControl?.UnsetBreakPoint();
        }

        public void FindLinkedNode()
        {
            if (areaControl != null)
            {
                var query = areaControl.FindLinkedItemQuery(MouseRightDownPos.X, MouseRightDownPos.Y);
                SearchRequired?.Invoke(query);
            }
        }

        public void CopyNodeName()
        {
            if (areaControl != null)
            {
                var nodeName = areaControl.GetNodeName(MouseRightDownPos.X, MouseRightDownPos.Y);
                if (nodeName != null)
                {
                    Clipboard.SetText(nodeName);
                }
            }
        }

        public void CopyPropertyName()
        {
            if (areaControl != null && areaControl.GetNodeAndPropertyName(MouseRightDownPos.X, MouseRightDownPos.Y,
                out string nodeName, out string propertyName, out string propertyValue))
            {
                var copyText = propertyValue != null
                    ? String.Format("{0}.{1}={2}", nodeName, propertyName, propertyValue)
                    : String.Format("{0}.{1}", nodeName, propertyName);
                Clipboard.SetText(copyText);
            }
        }

        private void AddCommentMenuItem_Click(object sender, RoutedEventArgs e)
        {
            AddCommentAt(MouseRightDownPos.X, MouseRightDownPos.Y);
        }

        private void CutMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Cut();
        }

        private void CopyMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Copy();
        }

        private void PasteMenuItem_Click(object sender, RoutedEventArgs e)
        {
            PasteAt(MouseRightDownPos.X, MouseRightDownPos.Y);
        }

        private void PasteWithLinksMenuItem_Click(object sender, RoutedEventArgs e)
        {
            PasteWithLinksAt(MouseRightDownPos.X, MouseRightDownPos.Y);
        }

        private void DeleteMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Delete();
        }

        private void FitViewMenuItem_Click(object sender, RoutedEventArgs e)
        {
            FitToView();
        }

        private void AddBoundCommentMenuItem_Click(object sender, RoutedEventArgs e)
        {
            AddBindedComment();
        }

        private void RemoveBoundCommentMenuItem_Click(object sender, RoutedEventArgs e)
        {
            RemoveBindedComment();
        }

        private void AddScopeMenuItem_Click(object sender, RoutedEventArgs e)
        {
            AddScopeAt(MouseRightDownPos.X, MouseRightDownPos.Y);
        }

        private void RemoveScopeMenuItem_Click(object sender, RoutedEventArgs e)
        {
            RemoveScopeAt(MouseRightDownPos.X, MouseRightDownPos.Y);
        }

        private void SelectInScopeMenuItem_Click(object sender, RoutedEventArgs e)
        {
            SelectInScope(MouseRightDownPos.X, MouseRightDownPos.Y);
        }

        private void SelectAllInScopeMenuItem_Click(object sender, RoutedEventArgs e)
        {
            SelectAllInScope(MouseRightDownPos.X, MouseRightDownPos.Y);
        }

        private void AddBreakPointMenuItem_Click(object sender, RoutedEventArgs e)
        {
            SetBreakPoint();
        }

        private void RemoveBreakPointMenuItem_Click(object sender, RoutedEventArgs e)
        {
            UnsetBreakPoint();
        }

        private void FindLinkedNodeMenuItem_Click(object sender, RoutedEventArgs e)
        {
            FindLinkedNode();
        }

        private void CopyNodeNameMenuItem_Click(object sender, RoutedEventArgs e)
        {
            CopyNodeName();
        }

        private void CopyPropertyNameMenuItem_Click(object sender, RoutedEventArgs e)
        {
            CopyPropertyName();
        }

        private void UpdateLanguage()
        {
            var provider = LanguageProvider;
            addNodeMenuItem.Header = provider.AddNode;
            addCommentMenuItem.Header = provider.AddComment;
            cutMenuItem.Header = provider.Cut;
            copyMenuItem.Header = provider.Copy;
            pasteMenuItem.Header = provider.Paste;
            pasteWithLinksMenuItem.Header = provider.PasteWithLinks;
            deleteMenuItem.Header = provider.Delete;
            fitViewMenuItem.Header = provider.FitView;
            addBoundCommentMenuItem.Header = provider.AddBoundComment;
            removeBoundCommentMenuItem.Header = provider.RemoveBoundComment;
            addScopeMenuItem.Header = provider.AddScope;
            removeScopeMenuItem.Header = provider.RemoveScope;
            selectInScopeMenuItem.Header = provider.SelectInScope;
            selectAllInScopeMenuItem.Header = provider.SelectAllInScope;
            addBreakPointMenuItem.Header = provider.AddBreakPoint;
            removeBreakPointMenuItem.Header = provider.RemoveBreakPoint;
            findLinkedNodeMenuItem.Header = provider.FindLinkedNode;
            copyNodeNameMenuItem.Header = provider.CopyNodeName;
            copyPropertyNameMenuItem.Header = provider.CopyPropertyName;

            foreach (Classes.IToolTipText toolTipText in toolTipTexts)
            {
                toolTipText.UpdateLanguage(provider);
            }
            foreach (var entry in nodeEntries)
            {
                entry.NotifyToolTipChanged();
            }
        }

        /// <summary>
        /// ノードを配置する領域（タイマーを持つ）を破棄します。
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                areaControl?.Dispose();
            }
        }

        public void ChangeCurrentSourceProperty(string name, string value)
        {
            areaControl?.ChangeSelectedSourcePropertyValue(name, value);
        }

        public void ExpandAll()
        {
            areaControl?.ExpandAll();
        }

        public void CollapseAll()
        {
            areaControl?.CollapseAll();
        }
    }
}
