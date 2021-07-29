using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace CopyPasteTool
{
    class CacheHelper
    {
        private static string dir = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
        private static string file = dir.Substring(0, dir.LastIndexOf("\\") + 1) + "cvCache.confg";

        public static void cacheOtherText(string text)
        {
            File.WriteAllText(file, text);
        }

        public static string getOtherText()
        {
            if (!File.Exists(file))
            {
                return "";
            }
            else
            {
                return File.ReadAllText(file);
            }
        }

    }
}
