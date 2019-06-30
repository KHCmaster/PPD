using PPDConfiguration;
using System;
using System.Windows.Markup;

namespace PPDEditorCommon
{
    [ContentProperty("Key")]
    [MarkupExtensionReturnType(typeof(string))]
    public class TranslateExtension : MarkupExtension
    {
        internal static LanguageReader Language
        {
            get;
            set;
        }

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
            if (Language == null)
            {
                if (String.IsNullOrEmpty(StringFormat))
                {
                    return Key;
                }
                return String.Format(StringFormat, Key);
            }

            if (String.IsNullOrEmpty(StringFormat))
            {
                return Language[Key];
            }
            return String.Format(StringFormat, Language[Key]);
        }
    }
}
