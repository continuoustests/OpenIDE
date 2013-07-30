using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSharp.Crawlers
{
    public interface IType
    {
        IEnumerable<string> BaseTypes { get; }
    }

    public abstract class TypeBase<T> : CodeItemBase<T>, IType
    {
        protected List<string> _baseTypes = new List<string>();

        public IEnumerable<string> BaseTypes { get { return _baseTypes; } }
        
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

        protected override IEnumerable<ResolveStatement> getTypeResolveStatements() {
            var statements = new List<ResolveStatement>(base.getTypeResolveStatements());
            for (int i = 0; i < _baseTypes.Count; i++) {
                int index = i;
                statements.Add(new ResolveStatement(_baseTypes[index], getNamespace(), (s) => updateBaseType(index, s)));
            }
            return statements;
        }

        private void updateBaseType(int i, string value) {
            _baseTypes[i] = value;
        }
    }

    public abstract class CodeItemBase<T>
    {
        protected T _me;
        protected List<string> _modifiers = new List<string>();
        protected List<CodeAttribute> _attributes = new List<CodeAttribute>();

        protected abstract string getNamespace();

        public long ID { get; protected set; }

        public bool IsStatic { get { return _modifiers.Contains("static"); } }

        public int EndLine { get; private set; }
        public int EndColumn { get; private set; }

        public string JSON { get { return getJSON(); } }

        public void SetID(long id) {
            ID = id;
        }

        public T SetEndPosition(int line, int column) {
            EndLine = line;
            EndColumn = column;
            return _me;
        }

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

        protected virtual IEnumerable<ResolveStatement> getTypeResolveStatements() {
            var statements = new List<ResolveStatement>();
            for (int i = 0; i < _attributes.Count; i++) {
                int index = i;
                statements.Add(new ResolveStatement(_attributes[index].Name, getNamespace(), (s) => updateAttribute(index, s)));
            }
            return statements;
        }

        private void updateAttribute(int i, string value) {
            _attributes[i].Name = value;
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

    public class CodeAttribute
    {
        public string Name { get; set; }
        public List<string> Parameters = new List<string>();

        public void AddParameter(string value) {
            Parameters.Add(value);
        }
    }

    public class ResolveStatement
    {
        public string Value { get; private set; }
        public string Namespace { get; private set; }
        public Action<string> Replace { get; private set; }

        public ResolveStatement(string value, string ns, Action<string> replace) {
            Value = value;
            Namespace = ns;
            Replace = replace;
        }
    }
}
