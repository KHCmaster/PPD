using System;
using System.Reflection;

namespace PPDFrameworkCore
{
    public class Binding
    {
        IPropertyChanged srcObject;
        Object destObject;

        string srcPropertyLast;
        string destPropertyLast;

        PropertyInfo srcPropertyInfo;
        PropertyInfo destPropertyInfo;

        public Binding(object src, string srcProperty, object dest, string destProperty)
        {
            GetPropertyInfo(src, dest, srcProperty, destProperty);
            ChangeInitialValue();
        }

        private void GetPropertyInfo(object src, object dest, string srcProperty, string destProperty)
        {
            var lastObj = GetObjectFromString(srcProperty, src, out srcPropertyLast);

            if (lastObj != null && lastObj is IPropertyChanged)
            {
                foreach (PropertyInfo propertyInfo in lastObj.GetType().GetProperties())
                {
                    if (propertyInfo.Name == srcPropertyLast)
                    {
                        srcPropertyInfo = propertyInfo;
                        break;
                    }
                }
                srcObject = lastObj as IPropertyChanged;
                srcObject.PropertyChanged += Binding_PropertyChanged;
            }

            lastObj = GetObjectFromString(destProperty, dest, out destPropertyLast);
            if (lastObj != null)
            {
                foreach (PropertyInfo propertyInfo in lastObj.GetType().GetProperties())
                {
                    if (propertyInfo.Name == destPropertyLast)
                    {
                        destPropertyInfo = propertyInfo;
                        break;
                    }
                }
                destObject = lastObj;
            }
        }

        private Object GetObjectFromString(string exp, object first, out string last)
        {
            Object lastObj = first;
            var srcSplit = exp.Split('.');
            last = srcSplit[srcSplit.Length - 1];
            for (int i = 0; i < srcSplit.Length - 1; i++)
            {
                var propertyInfo = FindPropertyInfo(lastObj, srcSplit[i]);
                if (propertyInfo != null)
                {
                    lastObj = propertyInfo.GetValue(lastObj, null);
                }
                else
                {
                    Console.WriteLine("There is no property {0} in {1}", srcSplit[i], exp);
                    lastObj = null;
                    last = null;
                    break;
                }
            }
            return lastObj;
        }

        void Binding_PropertyChanged(string propertyName)
        {
            if (srcPropertyInfo != null && destPropertyInfo != null && propertyName == srcPropertyLast)
            {
                object srcValue = null;
                srcValue = srcPropertyInfo.GetValue(srcObject, null);

                if (srcValue != null)
                {
                    destPropertyInfo.SetValue(destObject, srcValue, null);
                }
            }
        }

        private PropertyInfo FindPropertyInfo(object obj, string propertyName)
        {
            foreach (PropertyInfo propertyInfo in obj.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static))
            {
                if (propertyInfo.Name == propertyName)
                {
                    return propertyInfo;
                }
            }
            return null;
        }

        private void ChangeInitialValue()
        {
            if (srcPropertyInfo != null && destPropertyInfo != null)
            {
                object srcValue = null;
                srcValue = srcPropertyInfo.GetValue(srcObject, null);

                if (srcValue != null)
                {
                    destPropertyInfo.SetValue(destObject, srcValue, null);
                }
            }
        }

        public void Release()
        {
            if (srcObject != null)
            {
                srcObject.PropertyChanged -= Binding_PropertyChanged;
            }
        }
    }

}
