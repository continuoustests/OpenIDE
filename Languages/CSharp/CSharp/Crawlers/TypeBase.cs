using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSharp.Crawlers
{
    public abstract class TypeBase<T>
    {
        protected T _me;
        protected List<string> _modifiers = new List<string>();

        public string JSON { get { return getJSON(); } }

        public T AddModifiers(IEnumerable<string> modifiers) {
            _modifiers.AddRange(modifiers);
            return _me;
        }

        protected void setThis(T me) {
            _me = me;
        }

        protected string getJSON() {
            var writer = new JSONWriter();
            foreach (var modifier in _modifiers)
                writer.Append(modifier, "1");
            return writer.ToString();
        }
    }
}
