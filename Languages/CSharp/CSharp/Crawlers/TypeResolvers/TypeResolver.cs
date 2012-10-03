using System;
using System.Collections;
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

        public void ResolveAllUnresolved(IOutputWriter cache) {
            cache.Files.ToList()
                .ForEach(file => {
                    var partials = new List<PartialType>();
                    getPartials(cache.Classes, file, partials);
                    getPartials(cache.Interfaces, file, partials);
                    getPartials(cache.Structs, file, partials);
                    getPartials(cache.Enums, file, partials);
                    getPartials(cache.Fields, file, partials);
                    getPartials(cache.Methods, file, partials);
                    _cache.ResolveMatchingType(partials.ToArray());
                });
        }

        private static void getPartials(IEnumerable<ICodeReference> codeRefs, FileRef file, List<PartialType> partials)
        {
            codeRefs
                .Where(x => !x.AllTypesAreResolved && x.File.File == file.File).ToList()
                .ForEach(x => {
                    x.AllTypesAreResolved = true;
                    partials.AddRange(
                        x.GetResolveStatements()
                            .Select(stmnt => 
                                new PartialType(
                                    file,
                                    new Point(x.Line, x.Column),
                                    stmnt.Value,
                                    stmnt.Replace)));
                });
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
