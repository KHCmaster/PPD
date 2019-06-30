using FlowScriptControl.Classes;
using FlowScriptDrawControl.Control;
using FlowScriptDrawControl.Model;
using FlowScriptEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;

namespace FlowScriptControl.Controls
{
    public partial class FlowDrawPanel : UserControl
    {
        public event EventHandler Modified;
        public event Action<string> SearchRequired;

        private static List<AssemblyAndType> flowSources;
        private static Dictionary<string, Color> colorDictionary;
        private static Dictionary<string, string> typeAliasDictionary;
        private static bool initialized;
        private List<Classes.IToolTipText> toolTipTexts;
        private Dictionary<string, CustomToolStripMenuItem> toolStripDictionary;
        private static List<NodeFilterInfoBase> filters;
        private static Dictionary<string, AssemblyAndTypeAndSource[]> sourceCache;

        public bool IsControlFocused
        {
            get
            {
                return elementHost1.Focused;
            }
        }

        public static Dictionary<string, string> TypeAliasDictionary
        {
            get
            {
                return typeAliasDictionary;
            }
        }

        ToolTip tooltip;
        FlowAreaControl areaControl;

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

        public event EventHandler SelectionChanged;

        static FlowDrawPanel()
        {
            flowSources = new List<AssemblyAndType>();
            colorDictionary = new Dictionary<string, Color>
            {
                { "event", Color.FromArgb(0, 255, 0) }
            };
            typeAliasDictionary = new Dictionary<string, string>();
            filters = new List<NodeFilterInfoBase>
            {
                NodeFilterInfo.DefaultFilter
            };
        }

        public FlowDrawPanel()
        {
            toolTipTexts = new List<Classes.IToolTipText>();
            toolStripDictionary = new Dictionary<string, CustomToolStripMenuItem>();
            tooltip = new ToolTip();
            InitializeComponent();
        }

        private void FlowDrawPanel_Load(object sender, EventArgs e)
        {
            if (sourceCache == null)
            {
                sourceCache = new Dictionary<string, AssemblyAndTypeAndSource[]>();
                foreach (IGrouping<string, AssemblyAndTypeAndSource> ss in flowSources.Select(a => new AssemblyAndTypeAndSource(a)).GroupBy(a => a.Source.Name).ToArray())
                {
                    var sources = ss.ToArray();
                    Array.Sort(sources, (a1, a2) =>
                    {
                        int count1 = a1.Source.InProperties.Count + a1.Source.InMethods.Count + a1.Source.OutProperties.Count + a1.Source.OutMethods.Count;
                        int count2 = a2.Source.InProperties.Count + a2.Source.InMethods.Count + a2.Source.OutProperties.Count + a2.Source.OutMethods.Count;
                        if (count1 > count2)
                        {
                            return 1;
                        }
                        if (count1 < count2)
                        {
                            return -1;
                        }

                        if (a1.Source.InProperties.Count > a2.Source.InProperties.Count)
                        {
                            return 1;
                        }
                        if (a1.Source.InProperties.Count < a2.Source.InProperties.Count)
                        {
                            return -1;
                        }

                        if (a1.Source.OutProperties.Count > a2.Source.OutProperties.Count)
                        {
                            return 1;
                        }
                        if (a1.Source.OutProperties.Count < a2.Source.OutProperties.Count)
                        {
                            return -1;
                        }

                        if (a1.Source.InMethods.Count > a2.Source.InMethods.Count)
                        {
                            return 1;
                        }
                        if (a1.Source.InMethods.Count < a2.Source.InMethods.Count)
                        {
                            return -1;
                        }

                        if (a1.Source.OutMethods.Count > a2.Source.OutMethods.Count)
                        {
                            return 1;
                        }
                        if (a1.Source.OutMethods.Count < a2.Source.OutMethods.Count)
                        {
                            return -1;
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
                    });
                    sourceCache.Add(ss.Key, sources);
                }
            }

            foreach (KeyValuePair<string, AssemblyAndTypeAndSource[]> group in sourceCache)
            {
                foreach (AssemblyAndTypeAndSource source in group.Value)
                {
                    try
                    {
                        CreateToolStripMenuItem(source);
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

            areaControl = new FlowAreaControl();
            areaControl.DragEnter += areaControl_DragEnter;
            areaControl.Drop += areaControl_Drop;
            elementHost1.Child = areaControl;
            areaControl.SelectionChanged += areaControl_SelectionChanged;
            areaControl.CanConvert += areaControl_CanConvert;
            areaControl.Modified += areaControl_Modified;
            areaControl.GetSource += areaControl_GetSource;

            FinishInitialize();
        }

        void areaControl_Modified()
        {
            OnModified(this, EventArgs.Empty);
        }

        bool areaControl_CanConvert(string arg1, string arg2)
        {
            Type from = FlowSourceDumper.GetTypeFromString(arg1),
                to = FlowSourceDumper.GetTypeFromString(arg2);
            if (from == null || to == null)
            {
                return false;
            }
            return TypeConverterManager.CanConvert(from, to);
        }

        void areaControl_SelectionChanged(string arg1, Dictionary<string, string> arg2)
        {
            if (String.IsNullOrEmpty(arg1))
            {
                ChangeProperty();
            }
            else
            {
                ChangeProperty(arg1, arg2);
            }

            SelectionChanged?.Invoke(this, EventArgs.Empty);
        }

        Source areaControl_GetSource(string arg)
        {
            if (toolStripDictionary.ContainsKey(arg))
            {
                CustomToolStripMenuItem ctsmi = toolStripDictionary[arg];
                PreProcessDumper(ctsmi.Dumper);
                var source = new Source(ctsmi.Dumper.AssemblyAndType)
                {
                    Name = ctsmi.Dumper.SourceName,
                    FullName = ctsmi.Dumper.FullName,
                    ToolTip = $"({ctsmi.Dumper.SourceName})\n{ctsmi.Dumper.FlowToolTipText}",
                    Warning = ctsmi.Dumper.Warning
                };
                foreach (Item item in ctsmi.Dumper.InEvents.Concat(ctsmi.Dumper.OutEvents.Concat(ctsmi.Dumper.InProperties.Concat(ctsmi.Dumper.OutProperties))))
                {
                    source.AddItem(item.Clone());
                }
                foreach (KeyValuePair<string, AssemblyAndTypeAndSource[]> asms in sourceCache.Where(p => p.Key == ctsmi.Dumper.SourceName))
                {
                    foreach (AssemblyAndTypeAndSource asm in asms.Value)
                    {
                        source.AddSameNameSource(asm.AssemblyAndType);
                    }
                }
                return source;
            }
            return null;
        }

        private void OnModified(object sender, EventArgs e)
        {
            Modified?.Invoke(sender, e);
        }

        private void ChangeProperty()
        {
            if (FlowPropertyPanel != null)
            {
                FlowPropertyPanel.MakeEmpty();
            }
        }

        private void ChangeProperty(string fullName, Dictionary<string, string> properties)
        {
            if (FlowPropertyPanel != null)
            {
                var source = FlowSourceObjectManager.CreateSource(fullName);
                FlowPropertyPanel.ChangeFlowSource(this, source, properties);
            }
        }

        public static void EnumerateClass(string filePath)
        {
            foreach (AssemblyAndType asmAndType in FlowSourceEnumerator.EnumerateFromFile(filePath, new Type[] { typeof(FlowSourceObjectBase), typeof(TypeColorBase), typeof(TypeAliasBase) }))
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
            var types = new List<AssemblyAndType>();
            foreach (string dllFile in Directory.GetFiles(directoryPath, "*.dll"))
            {
                if (exceptionFileList.Any(name => name.ToLower() == Path.GetFileNameWithoutExtension(dllFile).ToLower()))
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
            RecursiveSortMenuItem(addNodeToolStripMenuItem, null);
            if (!initialized)
            {
                if (FlowTreeView != null)
                {
                    foreach (CustomToolStripMenuItem menu in addNodeToolStripMenuItem.DropDownItems)
                    {
                        FlowTreeView.ReprecateMenuToTree(menu);
                    }
                    FlowTreeView.SetFilters(filters.ToArray());
                    FlowTreeView.SelectedFilterChanged += FlowTreeView_SelectedFilterChanged;
                }
#if DEBUG
                FlowSourceDumper.WriteToText("text.ini");
                FlowSourceDumper.CheckPropertyType();
                FlowSourceDumper.CheckPropertyColor(colorDictionary);
                FlowSourceDumper.CheckEnumTypeFormatter();
                FlowSourceDumper.CheckTypeSerializer();
#endif
            }
            initialized = true;
        }

        void FlowTreeView_SelectedFilterChanged(object sender, EventArgs e)
        {
            HideMenus(addNodeToolStripMenuItem.DropDownItems, FlowTreeView.SelectedFilter);
        }

        private bool HideMenus(ToolStripItemCollection collection, NodeFilterInfoBase filterInfo)
        {
            bool hasVisibleNodes = false;
            foreach (CustomToolStripMenuItem menuItem in collection)
            {
                var hide = false;
                if (menuItem.DropDownItems.Count > 0)
                {
                    var childrenIsVisible = HideMenus(menuItem.DropDownItems, filterInfo);
                    if (!childrenIsVisible)
                    {
                        hide = true;
                    }
                    hasVisibleNodes |= childrenIsVisible;
                }
                else
                {
                    if (!IsFilteredMenu(menuItem, filterInfo))
                    {
                        hide = true;
                    }
                    else
                    {
                        hasVisibleNodes = true;
                    }
                }
                menuItem.Visible = !hide;
            }

            return hasVisibleNodes;
        }

        private bool IsFilteredMenu(CustomToolStripMenuItem node, NodeFilterInfoBase filter)
        {
            if (node.Dumper == null)
            {
                return true;
            }

            return !filter.IsHide(node.Dumper);
        }

        private static void AddTypeAlias(AssemblyAndType asmAndType)
        {
            var typeAlias = (TypeAliasBase)asmAndType.Assembly.CreateInstance(asmAndType.Type.FullName);
            foreach (KeyValuePair<Type, string> kvp in typeAlias.EnumerateAlias())
            {
                if (!typeAliasDictionary.ContainsKey(kvp.Key.FullName))
                {
                    typeAliasDictionary.Add(kvp.Key.FullName, kvp.Value);
                }
            }
        }

        private static void AddColorDefinition(AssemblyAndType asmAndType)
        {
            var temp = (TypeColorBase)asmAndType.Assembly.CreateInstance(asmAndType.Type.FullName);
            foreach (KeyValuePair<Type, Color> kvp in temp.EnumerateColors())
            {
                if (!colorDictionary.ContainsKey(kvp.Key.FullName))
                {
                    colorDictionary.Add(kvp.Key.FullName, kvp.Value);
                }
                else
                {
                    colorDictionary[kvp.Key.FullName] = kvp.Value;
                }
            }
        }
        private void CreateToolStripMenuItem(AssemblyAndTypeAndSource source)
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
            ToolStripMenuItem addTarget = addNodeToolStripMenuItem;
            for (int i = 0; i < nameSplits.Length - 1; i++)
            {
                var tsmi = FindMenu(addTarget, nameSplits[i]);
                if (tsmi == null)
                {
                    var folder = new FlowSourceFolder(String.Join(".", nameSplits, 0, i + 1));
                    toolTipTexts.Add(folder);
                    if (languageProvider != null)
                    {
                        folder.UpdateLanguage(languageProvider);
                    }
                    tsmi = new CustomToolStripMenuItem(folder, this, tooltip, nameSplits[i])
                    {
                        Name = nameSplits[i]
                    };
                    addTarget.DropDownItems.Add(tsmi);
                }
                addTarget = tsmi;
            }
            var existingMenuItem = FindMenu(addTarget, nameSplits[nameSplits.Length - 1]);
            CustomToolStripMenuItem newMenu;
            if (existingMenuItem == null)
            {
                newMenu = new CustomToolStripMenuItem(source.Dumper, this, tooltip, nameSplits[nameSplits.Length - 1]);
                addTarget.DropDownItems.Add(newMenu);
            }
            else
            {
                if (existingMenuItem.DropDownItems.Count == 0)
                {
                    var text = CreateTextFromDumper(existingMenuItem.Dumper);
                    var cloneMenu = new CustomToolStripMenuItem(existingMenuItem.Dumper, this, tooltip, text)
                    {
                        Name = nameSplits[nameSplits.Length - 1]
                    };
                    cloneMenu.Click += newMenu_Click;
                    existingMenuItem.DropDownItems.Add(cloneMenu);
                }
                var newText = CreateTextFromDumper(source.Dumper);
                newMenu = new CustomToolStripMenuItem(source.Dumper, this, tooltip, newText);
                existingMenuItem.DropDownItems.Add(newMenu);
            }
            toolStripDictionary.Add(source.AssemblyAndType.Type.FullName, newMenu);
            newMenu.Name = nameSplits[nameSplits.Length - 1];
            newMenu.Click += newMenu_Click;
        }

        private string CreateTextFromDumper(FlowSourceDumper dumper)
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

        void newMenu_Click(object sender, EventArgs e)
        {
            var ctsmi = sender as CustomToolStripMenuItem;
            FlowSourceDumper dumper = ctsmi.Dumper;
            AddSource(dumper, areaControl.MouseRightDownPos.X, areaControl.MouseRightDownPos.Y);
            contextMenuStrip1.Hide();
        }

        private void PreProcessDumper(FlowSourceDumper dumper)
        {
            foreach (Item item in dumper.InEvents.Concat(dumper.OutEvents.Concat(dumper.InProperties.Concat(dumper.OutProperties))))
            {
                Color c = colorDictionary[item.Type];
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

        private void RecursiveSortMenuItem(ToolStripMenuItem menuItem, ToolStripMenuItem parent)
        {
            foreach (ToolStripMenuItem tsmi in menuItem.DropDownItems)
            {
                RecursiveSortMenuItem(tsmi, menuItem);
            }

            if ((menuItem is CustomToolStripMenuItem tempItem && tempItem.IsFolder) || parent == null)
            {
                var list = new ArrayList(menuItem.DropDownItems);
                list.Sort(ToolStripItemComparer.Comparer);
                menuItem.DropDownItems.Clear();
                foreach (ToolStripItem item in list)
                {
                    menuItem.DropDownItems.Add(item);
                }
            }
        }

        private CustomToolStripMenuItem FindMenu(ToolStripMenuItem parent, string name)
        {
            foreach (ToolStripMenuItem tsmi in parent.DropDownItems)
            {
                if (tsmi.Name == name)
                {
                    return tsmi as CustomToolStripMenuItem;
                }
            }
            return null;
        }

        private void contextMenuStrip1_Opening(object sender, CancelEventArgs e)
        {
            切り取りToolStripMenuItem.Enabled = areaControl.CanCut;
            コピーToolStripMenuItem.Enabled = areaControl.CanCopy;
            貼り付けToolStripMenuItem.Enabled = areaControl.CanPaste;
            リンクも貼り付けToolStripMenuItem.Enabled = areaControl.CanPaste;
            削除ToolStripMenuItem.Enabled = areaControl.CanRemoveSelected;
            バインドコメントを追加ToolStripMenuItem.Visible = areaControl.CanAddBoundComment;
            バインドコメントを削除ToolStripMenuItem.Visible = areaControl.CanRemoveBoundComment;
            スコープを削除ToolStripMenuItem.Visible = areaControl.CanRemoveScopeAt(areaControl.MouseRightDownPos.X, areaControl.MouseRightDownPos.Y);
            var canSelectSourceAndCommentInScopeAt = areaControl.CanSelectSourceAndCommentInScopeAt(areaControl.MouseRightDownPos.X, areaControl.MouseRightDownPos.Y);
            スコープ内の要素を選択ToolStripMenuItem.Visible = canSelectSourceAndCommentInScopeAt;
            スコープ内の全要素を選択ToolStripMenuItem.Visible = canSelectSourceAndCommentInScopeAt;
            ブレイクポイントを設定ToolStripMenuItem.Visible = areaControl.CanSetBreakPoint;
            ブレイクポイントを解除ToolStripMenuItem.Visible = areaControl.CanUnsetBreakPoint;
            リンクしているノードを検索ToolStripMenuItem.Visible = areaControl.CanFindLinkedItem(areaControl.MouseRightDownPos.X, areaControl.MouseRightDownPos.Y);
            var canCopyNodeName = areaControl.CanGetNodeName(areaControl.MouseRightDownPos.X, areaControl.MouseRightDownPos.Y);
            ノード名をコピーToolStripMenuItem.Visible = canCopyNodeName;
            var canCopyPropertyName = areaControl.GetNodeAndPropertyName(areaControl.MouseRightDownPos.X,
                    areaControl.MouseRightDownPos.Y, out string nodeName, out string propertyName, out string propertyValue);
            プロパティ名をコピーToolStripMenuItem.Visible = canCopyPropertyName;
            toolStripSeparator3.Visible = canCopyNodeName || canCopyPropertyName;
        }

        public void AddCommentAt(double x, double y)
        {
            if (areaControl != null)
            {
                areaControl.AddCommentAt(x, y);
            }
        }

        public void Undo()
        {
            if (areaControl != null)
            {
                areaControl.Undo();
            }
        }

        public void Redo()
        {
            if (areaControl != null)
            {
                areaControl.Redo();
            }
        }

        public void Cut()
        {
            if (areaControl != null)
            {
                areaControl.Cut();
            }
        }

        public void Copy()
        {
            if (areaControl != null)
            {
                areaControl.Copy();
            }
        }

        public void PasteAt(double x, double y)
        {
            if (areaControl != null)
            {
                areaControl.PasteAt(x, y);
            }
        }

        public void PasteWithLinksAt(double x, double y)
        {
            if (areaControl != null)
            {
                areaControl.PasteWithLinksAt(x, y);
            }
        }

        public void Delete()
        {
            if (areaControl != null)
            {
                areaControl.RemoveSelected();
            }
        }

        public void FitToView()
        {
            if (areaControl != null)
            {
                areaControl.FitToView();
            }
        }

        public void AdjustPosition()
        {
            if (areaControl != null)
            {
                areaControl.AdjustPosition();
            }
        }

        public void SaveXML(Stream stream)
        {
            if (areaControl != null)
            {
                areaControl.Save(stream);
            }
        }

        public Source[] GetSources()
        {
            if (areaControl != null)
            {
                return areaControl.GetAllSourceControls().Select(s => s.CurrentSource).ToArray();
            }
            return new Source[0];
        }

        public void LoadXML(Stream stream)
        {
            LoadingForm.ShowLoading(this.FindForm());
            areaControl.Load(stream, out string[] errors, LoadingForm.ChangeProgress);
            LoadingForm.CloseLoading();
            if (errors.Length > 0)
            {
                MessageBox.Show(String.Format("{0}", String.Join("\r\n", errors)));
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
            return new int[0];
        }

        public SearchResult Search(string query)
        {
            if (areaControl != null)
            {
                return areaControl.Search(query);
            }

            return null;
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
            if (areaControl != null)
            {
                areaControl.Select(selectable);
            }
        }

        public void Select(int id)
        {
            if (areaControl != null)
            {
                areaControl.Select(id);
            }
        }

        public int[] GetSelectedSourceId()
        {
            if (areaControl != null)
            {
                return areaControl.SelectedSourceId;
            }
            return new int[0];
        }

        public void Focus(Selectable selectable)
        {
            if (areaControl != null)
            {
                areaControl.Focus(selectable);
            }
        }

        public void Focus(int id)
        {
            if (areaControl != null)
            {
                areaControl.Focus(id);
            }
        }

        public void ShowError(int id, string text)
        {
            if (areaControl != null)
            {
                areaControl.ShowError(id, text);
            }
        }

        public void AddScopeAt(double x, double y)
        {
            if (areaControl != null)
            {
                areaControl.AddScopeAt(x, y);
            }
        }

        public void RemoveScopeAt(double x, double y)
        {
            if (areaControl != null)
            {
                areaControl.RemoveScopeAt(x, y);
            }
        }

        public void SelectInScope(double x, double y)
        {
            if (areaControl != null)
            {
                areaControl.SelectSourceAndCommentInScopeAt(x, y, false);
            }
        }

        public void SelectAllInScope(double x, double y)
        {
            if (areaControl != null)
            {
                areaControl.SelectSourceAndCommentInScopeAt(x, y, true);
            }
        }

        public void SetBreakPoint()
        {
            if (areaControl != null)
            {
                areaControl.SetBreakPoint();
            }
        }

        public void UnsetBreakPoint()
        {
            if (areaControl != null)
            {
                areaControl.UnsetBreakPoint();
            }
        }

        public void FindLinkedNode()
        {
            if (areaControl != null)
            {
                var query = areaControl.FindLinkedItemQuery(
                    areaControl.MouseRightDownPos.X, areaControl.MouseRightDownPos.Y);
                SearchRequired?.Invoke(query);
            }
        }

        public void CopyNodeName()
        {
            if (areaControl != null)
            {
                var nodeName = areaControl.GetNodeName(areaControl.MouseRightDownPos.X,
                    areaControl.MouseRightDownPos.Y);
                if (nodeName != null)
                {
                    Clipboard.SetText(nodeName);
                }
            }
        }

        public void CopyPropertyName()
        {
            if (areaControl != null)
            {
                if (areaControl.GetNodeAndPropertyName(areaControl.MouseRightDownPos.X,
                    areaControl.MouseRightDownPos.Y, out string nodeName, out string propertyName, out string propertyValue))
                {
                    string copyText;
                    if (propertyValue != null)
                    {
                        copyText = String.Format("{0}.{1}={2}", nodeName, propertyName, propertyValue);
                    }
                    else
                    {
                        copyText = String.Format("{0}.{1}", nodeName, propertyName);
                    }
                    Clipboard.SetText(copyText);
                }
            }
        }

        private void コメントを追加ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AddCommentAt(areaControl.MouseRightDownPos.X, areaControl.MouseRightDownPos.Y);
        }

        private void 切り取りToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Cut();
        }

        private void コピーToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Copy();
        }

        private void 貼り付けToolStripMenuItem_Click(object sender, EventArgs e)
        {
            PasteAt(areaControl.MouseRightDownPos.X, areaControl.MouseRightDownPos.Y);
        }

        private void リンクも貼り付けToolStripMenuItem_Click(object sender, EventArgs e)
        {
            PasteWithLinksAt(areaControl.MouseRightDownPos.X, areaControl.MouseRightDownPos.Y);
        }

        private void 削除ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Delete();
        }

        private void 表示領域を合わせるToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FitToView();
        }

        private void バインドコメントを追加ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AddBindedComment();
        }

        private void バインドコメントを削除ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            RemoveBindedComment();
        }

        private void スコープを追加ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AddScopeAt(areaControl.MouseRightDownPos.X, areaControl.MouseRightDownPos.Y);
        }

        private void スコープを削除ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            RemoveScopeAt(areaControl.MouseRightDownPos.X, areaControl.MouseRightDownPos.Y);
        }

        private void スコープ内の要素を選択ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SelectInScope(areaControl.MouseRightDownPos.X, areaControl.MouseRightDownPos.Y);
        }

        private void スコープ内の全要素を選択ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SelectAllInScope(areaControl.MouseRightDownPos.X, areaControl.MouseRightDownPos.Y);
        }

        private void ブレイクポイントを設定ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SetBreakPoint();
        }

        private void ブレイクポイントを解除ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            UnsetBreakPoint();
        }

        private void リンクしているノードを検索ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FindLinkedNode();
        }

        private void ノード名をコピーToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CopyNodeName();
        }

        private void プロパティ名をコピーToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CopyPropertyName();
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

        ILanguageProvider languageProvider;
        public ILanguageProvider LanguageProvider
        {
            get { return languageProvider ?? DefaultLanguageProvider.Default; }
            set
            {
                languageProvider = value;
                UpdateLanguage();
            }
        }

        private void UpdateLanguage()
        {
            addNodeToolStripMenuItem.Text = LanguageProvider.AddNode;
            コメントを追加ToolStripMenuItem.Text = LanguageProvider.AddComment;
            切り取りToolStripMenuItem.Text = LanguageProvider.Cut;
            コピーToolStripMenuItem.Text = LanguageProvider.Copy;
            貼り付けToolStripMenuItem.Text = LanguageProvider.Paste;
            リンクも貼り付けToolStripMenuItem.Text = LanguageProvider.PasteWithLinks;
            削除ToolStripMenuItem.Text = LanguageProvider.Delete;
            表示領域を合わせるToolStripMenuItem.Text = LanguageProvider.FitView;
            バインドコメントを追加ToolStripMenuItem.Text = LanguageProvider.AddBoundComment;
            バインドコメントを削除ToolStripMenuItem.Text = LanguageProvider.RemoveBoundComment;
            スコープを追加ToolStripMenuItem.Text = LanguageProvider.AddScope;
            スコープを削除ToolStripMenuItem.Text = LanguageProvider.RemoveScope;
            スコープ内の要素を選択ToolStripMenuItem.Text = LanguageProvider.SelectInScope;
            スコープ内の全要素を選択ToolStripMenuItem.Text = LanguageProvider.SelectAllInScope;
            ブレイクポイントを設定ToolStripMenuItem.Text = LanguageProvider.AddBreakPoint;
            ブレイクポイントを解除ToolStripMenuItem.Text = LanguageProvider.RemoveBreakPoint;
            リンクしているノードを検索ToolStripMenuItem.Text = LanguageProvider.FindLinkedNode;
            ノード名をコピーToolStripMenuItem.Text = LanguageProvider.CopyNodeName;
            プロパティ名をコピーToolStripMenuItem.Text = LanguageProvider.CopyPropertyName;

            foreach (Classes.IToolTipText toolTipText in toolTipTexts)
            {
                toolTipText.UpdateLanguage(LanguageProvider);
            }
        }

        public void ChangeCurrentSourceProperty(string name, string value)
        {
            if (areaControl != null)
            {
                areaControl.ChangeSelectedSourcePropertyValue(name, value);
            }
        }

        public void ExpandAll()
        {
            if (areaControl != null)
            {
                areaControl.ExpandAll();
            }
        }

        public void CollapseAll()
        {
            if (areaControl != null)
            {
                areaControl.CollapseAll();
            }
        }

        void areaControl_Drop(object sender, System.Windows.DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(CustomTreeNode)) && !(e.Data.GetData(typeof(CustomTreeNode)) as CustomTreeNode).Data.IsFolder)
            {
                CustomTreeNode treeNode = (e.Data.GetData(typeof(CustomTreeNode)) as CustomTreeNode).Data;
                var p = e.GetPosition(areaControl);
                AddSource(treeNode.Dumper, p.X, p.Y);
            }
        }

        void areaControl_DragEnter(object sender, System.Windows.DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(CustomTreeNode)) && !(e.Data.GetData(typeof(CustomTreeNode)) as CustomTreeNode).Data.IsFolder)
            {
                e.Effects = System.Windows.DragDropEffects.Copy;
            }
            else
            {
                e.Effects = System.Windows.DragDropEffects.None;
            }
        }
    }

    class ToolStripItemComparer : System.Collections.IComparer
    {
        private static ToolStripItemComparer comparer = new ToolStripItemComparer();

        public static ToolStripItemComparer Comparer
        {
            get
            {
                return comparer;
            }
        }

        #region IComparer メンバ

        int System.Collections.IComparer.Compare(object x, object y)
        {
            var item1 = x as ToolStripItem;
            var item2 = y as ToolStripItem;
            return String.Compare(item1.Text, item2.Text);
        }

        #endregion
    }

}
