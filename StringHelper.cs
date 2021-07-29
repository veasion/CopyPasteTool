using System;
using System.Collections.Generic;
using System.Text;

namespace CopyPasteTool
{
    class StringHelper
    {
        public static string LowerCamelCase(string name)
        {
            if (name == null || "".Equals(name.Trim()))
            {
                return null;
            }
            if (name.StartsWith("_"))
            {
                name = name.Substring(1, name.Length - 1);
            }
            if (name.EndsWith("_"))
            {
                name = name.Substring(0, name.Length - 1);
            }
            StringBuilder sb = new StringBuilder();
            int len = name.Length;
            if (len > 3)
            {
                name = name.Replace("-", "_");
            }
            len = name.Length;
            bool u = true;
            for (int i = 0; i < len; i++)
            {
                char c = name[i];
                if ((int)c < 97)
                {
                    if (u)
                    {
                        if (i != 0 && i + 1 < len && (int)name[i + 1] >= 97)
                        {
                            if (name[i + 1] == 's' && i + 1 == len - 1)
                            {
                                sb.Append(c.ToString().ToLower());
                            }
                            else
                            {
                                sb.Append(c);
                            }
                        }
                        else
                        {
                            sb.Append(c.ToString().ToLower());
                        }
                    }
                    else
                    {
                        u = true;
                        sb.Append(c);
                    }
                }
                else
                {
                    u = false;
                    sb.Append(c);
                }
            }
            name = sb.ToString();
            if (name.IndexOf("_") != -1)
            {
                while (name.IndexOf("__") != -1)
                {
                    name = name.Replace("__", "_");
                }
                sb = new StringBuilder();
                len = name.Length;
                for (int i = 0; i < len; i++)
                {
                    char c = name[i];
                    if (i == 0)
                    {
                        sb.Append(c.ToString().ToLower());
                    }
                    else if (c == '_')
                    {
                        if (++i < len)
                        {
                            sb.Append(name[i].ToString().ToUpper());
                        }
                    }
                    else
                    {
                        sb.Append(c);
                    }
                }
            }
            return sb.ToString();
        }
    }
}
