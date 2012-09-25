using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSharp.Crawlers
{
    public class JSONWriter
    {
        private StringBuilder _sb = new StringBuilder();

        public JSONWriter Append(string key, string value)
        {
            append(quote(key), quote(value));
            return this;
        }

        public void AppendSection(string key, JSONWriter nodes) {
            var value = nodes.ToString();
            if (value == "")
                value = "{}";
            append(quote(key), value);
        }

        private string quote(string text) {
            return "\"" + text + "\"";
        }

        private void append(string key, string value) {
            if (_sb.Length == 0)
                _sb.Append(key + ":" + value);
            else
                _sb.Append("," + key + ":" + value);
        }

        public override string ToString() {
            if (_sb.Length == 0)
                return "";
            return "{" + _sb.ToString() + "}";
        }
    }
}
