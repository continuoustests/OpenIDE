using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSharp.Projects;

namespace CSharp.Crawlers.TypeResolvers
{
    public class TypeResolver
    {
        private ICacheReader _cache;

        public TypeResolver(ICacheReader cache) {
            _cache = cache;
        }

        public void Resolve(params PartialType[] types) {
            _cache.ResolveMatchingType(types);
        }
    }

    public class PartialType
    {
        public FileRef File { get; set; }
        public Point Location { get; set; }
        public string Type { get; set; }
        public Action<string> _resolve;

        public PartialType(FileRef file, Point location, string type, Action<string> resolve) {
            File = file;
            Location = location;
            Type = type;
            _resolve = resolve;
        }

        public void Resolve(string type) {
            _resolve(type);
        }
    }

    public class ResolvedType
    {
        public string Partial { get; set; }
        public string Resolved { get; set; }
        public string File { get; set; }

        public ResolvedType(string partial, string resolved, string file) {
            Partial = partial;
            Resolved = resolved;
            File = file;
        }
    }
}
