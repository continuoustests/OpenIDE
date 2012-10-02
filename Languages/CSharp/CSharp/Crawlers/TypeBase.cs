using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSharp.Crawlers
{
    public abstract class TypeBase<T> : CodeItemBase<T>
    {
        protected List<string> _baseTypes = new List<string>();

        public T AddBaseType(string baseType) {
            _baseTypes.Add(baseType);
            return _me;
        }

        protected override string getJSON() {
            var writer = new JSONWriter();
            base.getJSON(writer);
            if (_baseTypes.Count > 0) {
                var baseTypes = new JSONWriter();
                foreach (var type in _baseTypes)
                    baseTypes.Append(type, "");
                writer.AppendSection("bases", baseTypes);
            }
            return writer.ToString();
        }
    }

    public abstract class CodeItemBase<T>
    {
        protected T _me;
        protected List<string> _modifiers = new List<string>();
        protected List<CodeAttribute> _attributes = new List<CodeAttribute>();

        public string JSON { get { return getJSON(); } }

        public T AddModifiers(IEnumerable<string> modifiers) {
            _modifiers.AddRange(modifiers);
            return _me;
        }

        public T AddAttribute(CodeAttribute attrib) {
            _attributes.Add(attrib);
            return _me;
        }

        protected virtual void setThis(T me) {
            _me = me;
        }

        protected virtual string getJSON() {
            var writer = new JSONWriter();
            getJSON(writer);
            return writer.ToString();
        }

        protected void getJSON(JSONWriter writer)
        {
            foreach (var modifier in _modifiers)
                writer.Append(modifier, "1");
            if (_attributes.Count > 0)
            {
                var attributes = new JSONWriter();
                foreach (var attrib in _attributes)
                    attributes.Append(attrib.Name, getCommaString(attrib.Parameters));
                writer.AppendSection("attributes", attributes);
            }
        }

        private string getCommaString(List<string> list) {
            if (list == null)
                return "";
            var sb = new StringBuilder();
            foreach (var s in list) {
                if (sb.Length == 0)
                    sb.Append(s);
                else
                    sb.Append("," + s);
            }
            return sb.ToString();
        }
    }

    public struct CodeAttribute
    {
        public string Name;
        public List<string> Parameters;

        public void AddParameter(string value) {
            if (Parameters == null)
                Parameters = new List<string>();
            Parameters.Add(value);
        }
    }
}
