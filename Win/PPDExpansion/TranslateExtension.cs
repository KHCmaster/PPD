using System;
using System.Windows.Markup;

namespace PPDExpansion
{
    [ContentProperty("Key")]
    [MarkupExtensionReturnType(typeof(string))]
    public class TranslateExtension : MarkupExtension
    {
        [ConstructorArgument("key")]
        public string Key
        {
            get;
            set;
        }

        [ConstructorArgument("stringFormat")]
        public string StringFormat
        {
            get;
            set;
        }

        public TranslateExtension(string key)
        {
            Key = key;
        }

        public TranslateExtension(string key, string stringFormat)
        {
            Key = key;
            StringFormat = stringFormat;
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (String.IsNullOrEmpty(StringFormat))
            {
                return Utility.Language[Key];
            }
            return String.Format(StringFormat, Utility.Language[Key]);
        }
    }
}
