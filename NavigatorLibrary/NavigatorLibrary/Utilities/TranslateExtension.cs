using System;
using System.Reflection;
using System.Resources;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Plugin.Multilingual;

namespace NavigatorLibrary.Utilities
{
    [ContentProperty("Text")]
    public class TranslateExtension:IMarkupExtension
    {
        const string _resourceID = "IndoorNavigation.Resources.AppResources";

        static readonly Lazy<ResourceManager> _resourceManager =
            new Lazy<ResourceManager>(() =>
            new ResourceManager(_resourceID,
                                typeof(TranslateExtension).GetTypeInfo()
                                .Assembly));

        public string Text { get; set; }

        public object ProvideValue(IServiceProvider serviceProvider)
        {
            if (Text == null)
                return "";

            var ci = CrossMultilingual.Current.CurrentCultureInfo;

            var translation = _resourceManager.Value.GetString(Text, ci);

            if (translation == null)
            {
#if DEBUG
                throw new ArgumentException(
                    String.Format("Key '{0}' was not found in resources '{1}'" +
                                  "for culture '{2}'.",
                                  Text, _resourceID, ci.Name), "Text");
#else
                translation = Text; 
				// returns the key, which GETS DISPLAYED TO THE USER
#endif
            }
            return translation;
        }
    }
}
