using System;
using System.Collections.Generic;
using System.Text;
using System.Resources;
using System.Reflection;
using System.Globalization;
using Plugin.Multilingual;
using NavigatorLibrary.Utilities;
namespace NavigatorLibrary.Models
{
    public static class PhoneInformation
    {
        private const string _resourceID = "";       



        public static List<string> GetAllLanguage()
        {
            List<string> LanguageList = new List<string>();

            return LanguageList;
        }

        public static string GetCurrentLanguage()
        {
            return "en";
        }

        public static string GetCurrentMapName(string userNaming)
        {
            return "NTUH_Yunlin";
        }

        public static string GetBuildingName(string naviGraphName)
        {
            return "NTUH Yunlin branch";
        }

        private static string GetResource(string key)
        {
            ResourceManager resourceManager = new ResourceManager(_resourceID, typeof(TranslateExtension).GetTypeInfo().Assembly);
            CultureInfo currentLanguage = CrossMultilingual.Current.CurrentCultureInfo;
            return resourceManager.GetString(key,currentLanguage);
        }
    }
}
