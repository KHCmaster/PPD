using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace FlowScriptEngine
{
    public class Serializer
    {
        XDocument document;


        public Serializer()
        {
            document = new XDocument(new XElement("Root"));
        }

        internal void Serialize(FlowSourceObjectBase source)
        {
            var sourceElem = new XElement("Source", new XAttribute("ID", source.Id));
            document.Root.Add(sourceElem);
            foreach (var property in source.OutProperties)
            {
                try
                {
                    var value = source.GetPropertyValue(property.MemberInfo.Name);
                    var elem = CreateElement(property.MemberInfo.Name);
                    sourceElem.Add(elem);
                    Serialize(elem, value);
                }
                catch
                {
                }
            }
        }

        internal void Serialize(FunctionScope scope)
        {
            var scopeElem = new XElement("Scope", new XAttribute("ID", scope.Id));
            document.Root.Add(scopeElem);
            SerializeScope(scopeElem, scope);
        }

        internal void SerializeScope(XElement element, FunctionScope scope)
        {
            if (scope.Parent != null)
            {
                var elem = CreateElement("ParentScope");
                element.Add(elem);
                SerializeScope(elem, scope.Parent);
            }
            foreach (KeyValuePair<string, object> pair in scope.Pairs)
            {
                try
                {
                    var elem = new XElement("Property", new XAttribute("Name", pair.Key));
                    element.Add(elem);
                    Serialize(elem, pair.Value);
                }
                catch
                {

                }
            }
        }

        public XElement CreateElement(string name)
        {
            return new XElement("Property", new XAttribute("Name", name));
        }

        public void Serialize(XElement element, object value)
        {
            Type type = null;
            string typeText = "";
            if (value == null)
            {
                typeText = "(null)";
            }
            else
            {
                type = value.GetType();
                typeText = type.FullName;
            }
            var valueElem = new XElement("Value", new XAttribute("Type", typeText));
            element.Add(valueElem);
            if (type.IsArray && value != null)
            {
                var elemType = type = type.GetElementType();
                var array = (Array)value;
                for (var i = 0; i < array.Length; i++)
                {
                    var newElem = CreateElement(String.Format("[{0}]", i));
                    valueElem.Add(newElem);
                    Serialize(newElem, array.GetValue(i));
                }
            }
            else
            {
                var serializer = TypeSerializerManager.GetSerializer(type);
                if (serializer == null)
                {
                    if (value == null)
                    {
                        valueElem.Value = "(null)";
                    }
                    else
                    {
                        valueElem.Value = value.ToString();
                    }
                }
                else
                {
                    serializer.Serialize(this, valueElem, value);
                }
            }
        }

        public override string ToString()
        {
            return document.ToString();
        }
    }
}
