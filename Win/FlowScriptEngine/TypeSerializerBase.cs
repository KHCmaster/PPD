using System;
using System.Xml.Linq;

namespace FlowScriptEngine
{
    public abstract class TypeSerializerBase
    {
        public abstract Type Type { get; }
        public abstract void Serialize(Serializer serializer, XElement element, object value);

        public void SetValue(XElement element, string value)
        {
            element.Value = value;
        }

        public void AddNewElement(Serializer serializer, XElement element, string name, object value)
        {
            var childElem = serializer.CreateElement(name);
            element.Add(childElem);
            serializer.Serialize(childElem, value);
        }

        public void SerializeWithRefrection(Serializer serializer, XElement element, object value)
        {
            foreach (var propertyInfo in value.GetType().GetProperties())
            {
                if (propertyInfo.CanRead)
                {
                    var parameters = propertyInfo.GetIndexParameters();
                    if (parameters.Length == 0 && propertyInfo.PropertyType != value.GetType())
                    {
                        AddNewElement(serializer, element, propertyInfo.Name, propertyInfo.GetValue(value, null));
                    }
                }
            }
        }
    }
}
