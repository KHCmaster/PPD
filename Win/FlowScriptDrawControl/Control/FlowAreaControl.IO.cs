using FlowScriptDrawControl.Command;
using FlowScriptDrawControl.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;

namespace FlowScriptDrawControl.Control
{
    partial class FlowAreaControl
    {
        public void Load(Stream stream)
        {
            Load(stream, out string[] errors, d => Console.WriteLine("{0}%", d));
        }

        public void Load(Stream stream, out string[] errorList)
        {
            Load(stream, out errorList, d => Console.WriteLine("{0}%", d));
        }

        public void Load(Stream stream, out string[] errorList, Action<float> progressCallback)
        {
            var errors = new List<string>();
            Clear();
            try
            {
                Utility.IsLoading = true;
                var document = new XmlDocument();
                document.Load(stream);
                var collapseList = new Dictionary<Source, Source>();
                var sourceDicts = new Dictionary<int, SourceControl>();
                var nodes = SelectSingleNodeChildren(document, "Sources");
                int iter = 0;
                foreach (XmlNode node in nodes)
                {
                    try
                    {
                        var source = SelectionSerializer.DeserializeSource(node, OnGetSource);
                        if (source == null)
                        {
                            throw new Exception(String.Format("Invalid FullName. FullName:{0}", node.Attributes["FullName"].Value));
                        }
                        var temp = CreateSource(source);
                        var control = AddSourceAtImpl(temp);
                        sourceDicts.Add(source.Id, control);
                        collapseList.Add(control.CurrentSource, source);
                        if (source.Comment != null)
                        {
                            var command = new AddBoundCommentBommand(this, control);
                            command.CommentControl.CurrentComment.Text = source.Comment.Text;
                            commandManager.AddCommand(command);
                        }
                    }
                    catch (Exception e)
                    {
                        errors.Add(e.Message);
                    }
                    iter++;
                    progressCallback?.Invoke((float)iter / nodes.Count);
                }
                foreach (XmlNode node in SelectSingleNodeChildren(document, "//Flows"))
                {
                    try
                    {
                        var connectionInfo = SelectionSerializer.DeserializeFlow(node);
                        SourceControl src = sourceDicts[connectionInfo.SrcId], dest = sourceDicts[connectionInfo.DestId];
                        if (src != null && dest != null)
                        {
                            SourceItemControl srcItem = src.GetItemControlByName(connectionInfo.SrcName, true),
                                destItem = dest.GetItemControlByName(connectionInfo.DestName, false);
                            if (srcItem != null && destItem != null)
                            {
                                var command = new AddFlowCommand(this, new ArrowControl(), srcItem, destItem);
                                commandManager.AddCommand(command);
                            }
                            else
                            {
                                var message = String.Format("SrcItem or DestItem is null. SrcID:{0}, DestID:{1}, SrcName:{2}, DestName:{3}",
                                    connectionInfo.SrcId, connectionInfo.DestId,
                                    connectionInfo.SrcName, connectionInfo.DestName);
                                throw new Exception(message);
                            }
                        }
                        else
                        {
                            var message = String.Format("SrcControl or DestControl is null. SrcID:{0}, DestID:{1}, SrcName:{2}, DestName:{3}",
                                connectionInfo.SrcId, connectionInfo.DestId,
                                connectionInfo.SrcName, connectionInfo.DestName);
                            throw new Exception(message);
                        }
                    }
                    catch (Exception e)
                    {
                        errors.Add(e.Message);
                    }
                }
                foreach (KeyValuePair<Source, Source> pair in collapseList)
                {
                    pair.Key.IsCollapsed = pair.Value.IsCollapsed;
                }
                foreach (XmlNode node in SelectSingleNodeChildren(document, "//Comments"))
                {
                    var comment = SelectionSerializer.DeserializeComment(node);
                    var commentControl = AddCommentAtImpl(comment.Position.X, comment.Position.Y);
                    commentControl.CurrentComment.Text = comment.Text;
                    commentControl.CurrentComment.ScopeId = comment.ScopeId;
                }

                var parentScopeIdList = new Dictionary<ScopeControl, int>();
                var addedScopes = new List<ScopeControl>();
                foreach (XmlNode node in SelectSingleNodeChildren(document, "//Scopes"))
                {
                    var scope = SelectionSerializer.DeserializeScope(node);
                    ScopeControl scopeControl = null;
                    if (scope.ParentScopeId < 0)
                    {
                        scopeControl = AddScopeAtImpl(scope.Position, scope.Color, scope.Id, null);
                        addedScopes.Add(scopeControl);
                    }
                    else
                    {
                        scopeControl = CreateScopeControl(scope.Position, scope.Color, scope.Id);
                        parentScopeIdList.Add(scopeControl, scope.ParentScopeId);
                    }
                }
                SolveScopeDependencyAndAdd(parentScopeIdList, addedScopes.ToArray());
                foreach (SourceControl source in GetAllSourceControls())
                {
                    try
                    {
                        if (source.CurrentSource.ScopeId < 0)
                        {
                            continue;
                        }
                        var scope = GetScopeById(source.CurrentSource.ScopeId);
                        if (scope == null)
                        {
                            throw new Exception(String.Format("Scope Not Found:Scope ID:{0}, SourceID:{1}",
                                source.CurrentSource.ScopeId, source.CurrentSource.Id));
                        }
                        var command = new AddSelectableToScopeCommand(this, scope, source);
                        commandManager.AddCommand(command);
                    }
                    catch (Exception e)
                    {
                        errors.Add(e.Message);
                    }
                }
                foreach (CommentControl comment in GetAllCommentControls())
                {
                    try
                    {
                        if (comment.CurrentComment.ScopeId < 0)
                        {
                            continue;
                        }
                        var scope = GetScopeById(comment.CurrentComment.ScopeId);
                        if (scope == null)
                        {
                            throw new Exception(String.Format("Scope Not Found:Scope ID:{0}", comment.CurrentComment.ScopeId));
                        }
                        var command = new AddSelectableToScopeCommand(this, scope, comment);
                        commandManager.AddCommand(command);
                    }
                    catch (Exception e)
                    {
                        errors.Add(e.Message);
                    }
                }
            }
            finally
            {
                commandManager.Clear();
                UpdateMaxId();
                errorList = errors.ToArray();
                Utility.IsLoading = false;
            }
        }

        class ScopeNode
        {
            private List<ScopeNode> nexts;

            public ScopeControl ScopeControl
            {
                get;
                set;
            }

            public ScopeNode()
            {
                nexts = new List<ScopeNode>();
            }

            public void AddNext(ScopeNode scopeNode)
            {
                nexts.Add(scopeNode);
            }

            public void AddToCanvas(FlowAreaControl flowAreaControl)
            {
                foreach (ScopeNode next in nexts)
                {
                    next.AddToCanvasImpl(ScopeControl, flowAreaControl);
                }
            }

            private void AddToCanvasImpl(ScopeControl parent, FlowAreaControl flowAreaControl)
            {
                flowAreaControl.AddScopeImpl(ScopeControl, parent);
                foreach (ScopeNode next in nexts)
                {
                    next.AddToCanvasImpl(ScopeControl, flowAreaControl);
                }
            }
        }

        private void SolveScopeDependencyAndAdd(Dictionary<ScopeControl, int> parentScopeIdList, ScopeControl[] alreadyAddedScopes)
        {
            var nodes = new Dictionary<int, ScopeNode>();
            foreach (ScopeControl scope in alreadyAddedScopes)
            {
                nodes.Add(scope.CurrentScope.Id, new ScopeNode { ScopeControl = scope });
            }
            foreach (KeyValuePair<ScopeControl, int> kvp in parentScopeIdList)
            {
                var scopeNode = new ScopeNode { ScopeControl = kvp.Key };
                if (!nodes.ContainsKey(kvp.Key.CurrentScope.Id))
                {
                    nodes.Add(kvp.Key.CurrentScope.Id, scopeNode);
                }
            }
            foreach (KeyValuePair<ScopeControl, int> kvp in parentScopeIdList)
            {
                ScopeNode scopeNode = nodes[kvp.Key.CurrentScope.Id];
                nodes[kvp.Value].AddNext(scopeNode);
            }
            foreach (ScopeControl scope in alreadyAddedScopes)
            {
                nodes[scope.CurrentScope.Id].AddToCanvas(this);
            }
        }

        private XmlNodeList SelectSingleNodeChildren(XmlDocument document, string xPath)
        {
            var node = document.DocumentElement.SelectSingleNode(xPath);
            return node == null ? document.DocumentElement.SelectNodes("//ablawuasdsdgkhasfodjjdfk") : node.ChildNodes;
        }

        public void Save(Stream stream)
        {
            var document = new XmlDocument();
            var root = document.AppendChild(document.CreateElement("Root"));
            var sources = root.AppendChild(document.CreateElement("Sources"));
            var connections = root.AppendChild(document.CreateElement("Flows"));
            var comments = root.AppendChild(document.CreateElement("Comments"));
            var scopes = root.AppendChild(document.CreateElement("Scopes"));
            foreach (Source source in GetAllSourceControls().Select(s => s.CurrentSource))
            {
                sources.AppendChild(SelectionSerializer.SerializeSource(document, source));
                foreach (Item item in source.OutItems)
                {
                    foreach (Connection connection in item.OutConnections)
                    {
                        Source connectionSource = connection.Target.Source;
                        connections.AppendChild(SelectionSerializer.SerializeFlow(document, item, connection.Target));
                    }
                }
            }
            foreach (Comment comment in GetAllCommentControls().Select(c => c.CurrentComment))
            {
                comments.AppendChild(SelectionSerializer.SerializeComment(document, comment));
            }
            foreach (Scope scope in GetAllScopeControls().Select(c => c.CurrentScope))
            {
                scopes.AppendChild(SelectionSerializer.SerializeScope(document, scope));
            }
            document.Save(stream);
        }
    }
}
