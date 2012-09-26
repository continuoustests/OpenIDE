using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;
using System.IO;
using CSharp.Projects;

namespace CSharp.Crawlers
{
    public class AssemblyParser
    {
        private IOutputWriter _writer;
        private FileRef _file;

        public AssemblyParser(IOutputWriter writer) {
            _writer = writer;
        }

        public void Parse(string asm) {
            _file = new FileRef(asm, null);
            AssemblyDefinition assembly;
            if (File.Exists(_file.File))
                assembly = AssemblyDefinition.ReadAssembly(_file.File);
            else
                assembly = new DefaultAssemblyResolver().Resolve(_file.File);
            assembly
                .Modules.ToList()
                .ForEach(x => 
                    x.GetTypes().ToList()
                    .ForEach(y => handleType(y)));
        }

        private void handleType(TypeDefinition type) {
            if (type.BaseType == null)
                return;
            if (type.Name.StartsWith("List"))
                Console.WriteLine("we are here");
            if (type.IsClass)
                handleClass(type);
            foreach (var child in type.NestedTypes)
                handleType(child);
        }

        private void handleClass(TypeDefinition cls) {
            var classDef = new Class(_file, cls.Namespace, cls.Name, "public", 0, 0, "");
            _writer.WriteClass(classDef);
            cls.Fields
                .Where(y => y.IsPublic && !y.IsRuntimeSpecialName && !y.IsSpecialName).ToList()
                .ForEach(x => {
                    _writer.WriteField(
                        new Field(_file,
                            x.DeclaringType.FullName, x.Name, "public",
                            0,0,x.FieldType.FullName,""));
                });
            cls.Properties
                .Where(y => !y.IsRuntimeSpecialName && !y.IsSpecialName).ToList()
                .ForEach(x =>
                    _writer.WriteField(
                        new Field(_file,
                            x.DeclaringType.FullName, x.Name, "public",
                            0,0,x.PropertyType.FullName,"")));
            cls.Methods
                .Where(y => y.IsPublic && !y.IsRuntimeSpecialName && !y.IsSpecialName).ToList()
                .ForEach(x => {
                    _writer.WriteMethod(
                        new Method(_file,
                            x.DeclaringType.FullName, x.Name, "public",
                            0,0,x.MethodReturnType.ReturnType.FullName,
                            x.Parameters.Select(z => new Parameter(z.ParameterType.FullName, z.Name)),
                            new JSONWriter()));
                });
        }
    }
}
