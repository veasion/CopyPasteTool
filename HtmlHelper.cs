using System;
using System.Collections.Generic;
using System.Text;

namespace CopyPasteTool
{
    class HtmlHelper
    {

        public static string GetDefaultJs()
        {
            return "// 旧 ie 浏览器不兼容新 js 特效处理\n" +
                "this.console = {log:function(str) {alert(str);}}\n" +
                "String.prototype.trim = String.prototype.trim || function() {\n" +
                "\treturn this.replace(/^\\s+|\\s+$/g,'');\n" +
                "};\n" +
                "String.prototype.startsWith = String.prototype.startsWith || function (str) {\n" +
                "\treturn this.indexOf(str) == 0;\n" +
                "}\n" +
                "String.prototype.endsWith = String.prototype.endsWith || function (str) {\n" +
                "\tif (str.length > this.length) {\n" +
                "\t\treturn false;\n" +
                "\t}\n" +
                "\treturn this.substring(this.length - str.length) == str;\n" +
                "}\n" +
                "Object.keys = Object.keys || function (o) {\n" +
                "\tvar k = [], p;\n" +
                "\tfor (p in o) if (Object.prototype.hasOwnProperty.call(o, p)) k.push(p);\n" +
                "\treturn k;\n" +
                "}\n" +
                "if (!this.JSON) {\n" +
                "\tthis.JSON = {};\n" +
                "\tthis.JSON.parse = function (s) { return eval(\"(\" + s + \")\"); };\n" +
                "\tthis.JSON.stringify = function() { return '不支持JSON.stringify'; }\n" +
                "}\n";
        }

        public static string GetJsCode()
        {
            return "// 自定义改变文本函数\nfunction change(text, param) {\n  return '改变后的: ' + text;\n}\n";
        }
    }
}
