using System;
using System.Collections.Generic;
using System.Text;

namespace CopyPasteTool
{
    class HtmlHelper
    {
        public static string GetHtmlCode()
        {
            return GetHtmlCode(null);
        }

        public static string GetHtmlCode(string jsCode)
        {
            if (jsCode == null)
            {
                jsCode = GetJsCode();
            }
            return "<!DOCTYPE html>\n" +
                "<html lang=\"zh-CN\">\n" +
                "<head>\n" +
                "<meta http-equiv=\"X-UA-Compatible\" content=\"IE=edge,chrome=1\" />\n" +
                "</head>\n" +
                "<body></body>\n" +
                "<script type=\"text/javascript\">\n" +
                "(function(){\n" +
                "\t// 旧 ie 浏览器不兼容新 js 特效处理\n" +
                "\tthis.console = {log:function(str) {alert(str)}}\n" +
                "\tString.prototype.trim = String.prototype.trim || function() {\n" +
                "\t\treturn this.replace(/^\\s+|\\s+$/g,'');\n" +
                "\t};\n" +
                "\tString.prototype.startsWith = String.prototype.startsWith || function (str) {\n" +
                "\t\treturn this.indexOf(str) == 0;\n" +
                "\t}\n" +
                "\tString.prototype.endsWith = String.prototype.endsWith || function (str) {\n" +
                "\t\tif (str.length > this.length) {\n" +
                "\t\t\treturn false;\n" +
                "\t\t}\n" +
                "\t\treturn this.substring(this.length - str.length) == str;\n" +
                "\t}\n" +
                "\tObject.keys = Object.keys || function (o) {\n" +
                "\t\tvar k = [], p;\n" +
                "\t\tfor (p in o) if (Object.prototype.hasOwnProperty.call(o, p)) k.push(p);\n" +
                "\t\treturn k;\n" +
                "\t}\n" +
                "\tif (!this.JSON) {\n" +
                "\t\tthis.JSON = {};\n" +
                "\t\tthis.JSON.parse = function (s) { return eval(\"(\" + s + \")\"); };\n" +
                "\t\tthis.JSON.stringify = function() { return '不支持JSON.stringify'; }\n" +
                "\t}\n" +
                "})();\n" +
                "\n" +
                jsCode +
                "\n" +
                "</script>\n" +
                "</html>";
        }

        public static string GetJsCode()
        {
            return "// 自定义改变文本函数\nfunction change(text, param) {\n  return '改变后的: ' + text;\n}\n";
        }
    }
}
