using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Xml;

namespace FlowScriptDrawControl.Model
{
    class SelectionSerializer
    {
        private static SelectionSerializer serializer = new SelectionSerializer();

        public static SelectionSerializer Serializer
        {
            get
            {
                return serializer;
            }
        }

        public string Serialize(Selection selection)
        {
            var document = new XmlDocument();
            var root = document.AppendChild(document.CreateElement("Root"));
            root.Attributes.Append(CreateAttribute(document, "Guid", selection.Guid.ToString()));
            var sources = root.AppendChild(document.CreateElement("Sources"));
            var connections = root.AppendChild(document.CreateElement("Flows"));
            var externalConnections = root.AppendChild(document.CreateElement("ExternalFlows"));
            var comments = root.AppendChild(document.CreateElement("Comments"));
            var scopes = root.AppendChild(document.CreateElement("Scopes"));
            var scopeList = new HashSet<Scope>();
            foreach (Source source in selection.Sources)
            {
                sources.AppendChild(SerializeSource(document, source));
                foreach (Item item in source.InItems)
                {
                    if (item.InConnection != null)
                    {
                        Source connectionSource = item.InConnection.Target.Source;
                        if (Array.IndexOf(selection.Sources, connectionSource) < 0)
                        {
                            externalConnections.AppendChild(SerializeFlow(document, item.InConnection.Target, item));
                        }
                    }
                }
                foreach (Item item in source.OutItems)
                {
                    foreach (Connection connection in item.OutConnections)
                    {
                        Source connectionSource = connection.Target.Source;
                        if (Array.IndexOf(selection.Sources, connectionSource) >= 0)
                        {
                            connections.AppendChild(SerializeFlow(document, item, connection.Target));
                        }
                    }
                }
                if (source.Scope != null)
                {
                    var scopeQueue = new Queue<Scope>();
                    scopeQueue.Enqueue(source.Scope);
                    while (scopeQueue.Count > 0)
                    {
                        var scope = scopeQueue.Dequeue();
                        if (scope.Parent != null)
                        {
                            scopeQueue.Enqueue(scope.Parent);
                        }
                        scopeList.Add(scope);
                    }
                }
            }
            foreach (Comment comment in selection.Comments)
            {
                comments.AppendChild(SerializeComment(document, comment));
                if (comment.Scope != null)
                {
                    scopeList.Add(comment.Scope);
                }
            }
            foreach (Scope scope in scopeList)
            {
                scopes.AppendChild(SerializeScope(document, scope));
            }
            return document.OuterXml;
        }

        public static XmlElement SerializeSource(XmlDocument document, Source source)
        {
            var element = document.CreateElement("Source");
            element.Attributes.Append(CreateAttribute(document, "Name", source.FullName));
            element.Attributes.Append(CreateAttribute(document, "FullName", source.FullName));
            element.Attributes.Append(CreateAttribute(document, "ID", source.Id.ToString(CultureInfo.InvariantCulture)));
            element.Attributes.Append(CreateAttribute(document, "ShowAll", (!source.IsCollapsed).ToString(CultureInfo.InvariantCulture)));
            element.Attributes.Append(CreateAttribute(document, "X", source.Position.X.ToString(CultureInfo.InvariantCulture)));
            element.Attributes.Append(CreateAttribute(document, "Y", source.Position.Y.ToString(CultureInfo.InvariantCulture)));
            if (source.Scope != null)
            {
                element.Attributes.Append(CreateAttribute(document, "ScopeID", source.Scope.Id.ToString(CultureInfo.InvariantCulture)));
            }
            if (source.Comment != null)
            {
                var commentElement = SerializeBoundComment(document, source.Comment);
                element.AppendChild(commentElement);
            }
            foreach (Item item in source.InProperties)
            {
                if (item.Value != null)
                {
                    var valueElement = SerializeValue(document, item);
                    element.AppendChild(valueElement);
                }
            }
            return element;
        }

        public static XmlElement SerializeScope(XmlDocument document, Scope scope)
        {
            var element = document.CreateElement("Scope");
            element.Attributes.Append(CreateAttribute(document, "ID", scope.Id.ToString(CultureInfo.InvariantCulture)));
            element.Attributes.Append(CreateAttribute(document, "X", scope.Position.X.ToString(CultureInfo.InvariantCulture)));
            element.Attributes.Append(CreateAttribute(document, "Y", scope.Position.Y.ToString(CultureInfo.InvariantCulture)));
            element.Attributes.Append(CreateAttribute(document, "Color", String.Format("#{0:X2}{1:X2}{2:X2}", scope.Color.R, scope.Color.G, scope.Color.B)));
            if (scope.Parent != null)
            {
                element.Attributes.Append(CreateAttribute(document, "ScopeID", scope.Parent.Id.ToString(CultureInfo.InvariantCulture)));
            }
            return element;
        }

        public static XmlElement SerializeBoundComment(XmlDocument document, Comment comment)
        {
            var element = document.CreateElement("Comment");
            element.Attributes.Append(CreateAttribute(document, "Text", comment.Text));
            return element;
        }

        public static XmlElement SerializeValue(XmlDocument document, Item item)
        {
            var element = document.CreateElement("Value");
            element.Attributes.Append(CreateAttribute(document, "Name", item.Name));
            element.Attributes.Append(CreateAttribute(document, "Value", item.Value));
            return element;
        }

        public static XmlElement SerializeFlow(XmlDocument document, Item srcItem, Item destItem)
        {
            var element = document.CreateElement("Flow");
            element.Attributes.Append(CreateAttribute(document, "SrcID", srcItem.Source.Id.ToString(CultureInfo.InvariantCulture)));
            element.Attributes.Append(CreateAttribute(document, "SrcName", srcItem.Name));
            element.Attributes.Append(CreateAttribute(document, "DestID", destItem.Source.Id.ToString(CultureInfo.InvariantCulture)));
            element.Attributes.Append(CreateAttribute(document, "DestName", destItem.Name));
            return element;
        }

        public static XmlElement SerializeComment(XmlDocument document, Comment comment)
        {
            var element = document.CreateElement("Comment");
            element.Attributes.Append(CreateAttribute(document, "X", comment.Position.X.ToString(CultureInfo.InvariantCulture)));
            element.Attributes.Append(CreateAttribute(document, "Y", comment.Position.Y.ToString(CultureInfo.InvariantCulture)));
            element.Attributes.Append(CreateAttribute(document, "Text", comment.Text));
            if (comment.Scope != null)
            {
                element.Attributes.Append(CreateAttribute(document, "ScopeID", comment.Scope.Id.ToString(CultureInfo.InvariantCulture)));
            }
            return element;
        }

        private static XmlAttribute CreateAttribute(XmlDocument document, string name, string value)
        {
            var attribute = document.CreateAttribute(name);
            attribute.Value = value;
            return attribute;
        }

        private Selection DeserializeImpl(string xml, Func<string, Source> sourceGenerateFunc)
        {
            var document = new XmlDocument();
            document.LoadXml(xml);
            var guid = new Guid(document.DocumentElement.GetAttribute("Guid"));
            var sources = new List<Source>();
            var comments = new List<Comment>();
            var connections = new List<ConnectionInfo>();
            var externalConnections = new List<ConnectionInfo>();
            var scopes = new List<Scope>();
            foreach (XmlNode node in document.SelectSingleNode("//Sources").ChildNodes)
            {
                sources.Add(DeserializeSource(node, sourceGenerateFunc));
            }
            foreach (XmlNode node in document.DocumentElement.SelectSingleNode("//Comments").ChildNodes)
            {
                comments.Add(DeserializeComment(node));
            }
            foreach (XmlNode node in document.DocumentElement.SelectSingleNode("//Flows").ChildNodes)
            {
                connections.Add(DeserializeFlow(node));
            }
            foreach (XmlNode node in document.DocumentElement.SelectSingleNode("//ExternalFlows").ChildNodes)
            {
                externalConnections.Add(DeserializeFlow(node));
            }
            foreach (XmlNode node in document.DocumentElement.SelectSingleNode("//Scopes").ChildNodes)
            {
                scopes.Add(DeserializeScope(node));
            }
            foreach (Scope scope in scopes)
            {
                scope.Parent = scopes.FirstOrDefault(s => s.Id == scope.ParentScopeId);
            }
            return new Selection(guid, sources.ToArray(), comments.ToArray(),
                connections.ToArray(), externalConnections.ToArray(), scopes.ToArray());
        }

        public static Source DeserializeSource(XmlNode node, Func<string, Source> sourceGenerateFunc)
        {
            int scopeId = -1;
            var source = sourceGenerateFunc(node.Attributes["FullName"].Value);
            source.Position = new Point(double.Parse(node.Attributes["X"].Value, CultureInfo.InvariantCulture),
                double.Parse(node.Attributes["Y"].Value, CultureInfo.InvariantCulture));
            source.IsCollapsed = !bool.Parse(node.Attributes["ShowAll"].Value);
            source.Id = int.Parse(node.Attributes["ID"].Value, CultureInfo.InvariantCulture);
            if (node.Attributes["ScopeID"] != null)
            {
                scopeId = int.Parse(node.Attributes["ScopeID"].Value, CultureInfo.InvariantCulture);
            }
            source.ScopeId = scopeId;
            foreach (XmlNode childNode in node.ChildNodes)
            {
                switch (childNode.Name)
                {
                    case "Value":
                        string name = childNode.Attributes["Name"].Value,
                            value = childNode.Attributes["Value"].Value;
                        source.GetInPropertyByName(name).Value = value;
                        break;
                    case "Comment":
                        string text = childNode.Attributes["Text"].Value;
                        source.Comment = new Comment { Text = text };
                        break;
                }
            }
            return source;
        }

        public static Scope DeserializeScope(XmlNode node)
        {
            int parentScopeId = -1;
            var scope = new Scope
            {
                Position = new Point(double.Parse(node.Attributes["X"].Value, CultureInfo.InvariantCulture),
                double.Parse(node.Attributes["Y"].Value, CultureInfo.InvariantCulture)),
                Id = int.Parse(node.Attributes["ID"].Value, CultureInfo.InvariantCulture),
                Color = (Color)ColorConverter.ConvertFromString(node.Attributes["Color"].Value)
            };
            if (node.Attributes["ScopeID"] != null)
            {
                parentScopeId = int.Parse(node.Attributes["ScopeID"].Value, CultureInfo.InvariantCulture);
            }
            scope.ParentScopeId = parentScopeId;
            return scope;
        }

        public static Comment DeserializeComment(XmlNode node)
        {
            int scopeId = -1;
            var comment = new Comment
            {
                Position = new Point(double.Parse(node.Attributes["X"].Value, CultureInfo.InvariantCulture),
                double.Parse(node.Attributes["Y"].Value, CultureInfo.InvariantCulture)),
                Text = node.Attributes["Text"].Value
            };
            if (node.Attributes["ScopeID"] != null)
            {
                scopeId = int.Parse(node.Attributes["ScopeID"].Value, CultureInfo.InvariantCulture);
            }
            comment.ScopeId = scopeId;
            return comment;
        }

        public static ConnectionInfo DeserializeFlow(XmlNode node)
        {
            return new ConnectionInfo(
                int.Parse(node.Attributes["SrcID"].Value, CultureInfo.InvariantCulture),
                node.Attributes["SrcName"].Value,
                int.Parse(node.Attributes["DestID"].Value, CultureInfo.InvariantCulture),
                node.Attributes["DestName"].Value);
        }

        public Selection Deserialize(string xml, Func<string, Source> sourceGenerateFunc)
        {
            try
            {
                return DeserializeImpl(xml, sourceGenerateFunc);
            }
            catch
            {
                return null;
            }
        }
    }
}
