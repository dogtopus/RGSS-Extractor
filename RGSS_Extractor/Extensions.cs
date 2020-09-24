using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Resources;

namespace RGSS_Extractor
{
    public static class Extensions
    {
        public static bool ExtensionContains(this string path, params string[] extensionList)
        {
            if (string.IsNullOrWhiteSpace(path)) { return false; }
            IEnumerable<string> en = extensionList.Where(i => Path.GetExtension(path).Contains(i));
            return en.Any();
        }

        public static string ApplyResource(Type resourceObject, string Name)
        {
            ResourceManager resource = new ResourceManager(resourceObject);
            return resource.GetString(Name) ?? string.Empty;
        }
    }
}
