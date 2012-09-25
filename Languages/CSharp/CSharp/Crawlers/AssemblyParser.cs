using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;
using System.IO;

namespace CSharp.Crawlers
{
    public class AssemblyParser
    {
        private IOutputWriter _writer;
        private string _file;

        public AssemblyParser(IOutputWriter writer) {
            _writer = writer;
        }

        public void Parse(string asm) {
            _file = asm;
            AssemblyDefinition assembly;
            if (File.Exists(_file))
                assembly = AssemblyDefinition.ReadAssembly(_file);
            else
                assembly = new DefaultAssemblyResolver().Resolve(_file);
            assembly
                .Modules.ToList()
                .ForEach(x => 
                    x.GetTypes().ToList()
                    .ForEach(y => handleType(y)));
        }

        private void handleType(TypeDefinition type) {
            if (type.BaseType == null)
                return;
            if (type.IsClass)
                handleClass(type);
            foreach (var child in type.NestedTypes)
                handleType(child);
        }

        private void handleClass(TypeDefinition cls) {
            var classDef = new Class(_file, cls.Namespace, cls.Name, "public", 0, 0, "");
            _writer.AddClass(classDef);
            cls.Fields
                .Where(y => y.IsPublic && !y.IsRuntimeSpecialName && !y.IsSpecialName).ToList()
                .ForEach(x =>
                    _writer.AddField(
                        new Field(_file,
                            x.DeclaringType.FullName, x.Name, "public",
                            0,0,x.FieldType.FullName,"")));
            cls.Properties
                .Where(y => !y.IsRuntimeSpecialName && !y.IsSpecialName).ToList()
                .ForEach(x =>
                    _writer.AddField(
                        new Field(_file,
                            x.DeclaringType.FullName, x.Name, "public",
                            0,0,x.PropertyType.FullName,"")));
            cls.Methods
                .Where(y => y.IsPublic && !y.IsRuntimeSpecialName && !y.IsSpecialName).ToList()
                .ForEach(x =>
                    _writer.AddMethod(
                        new Method(_file,
                            x.DeclaringType.FullName, x.Name, "public",
                            0,0,x.MethodReturnType.ReturnType.FullName,
                            x.Parameters.Select(z => new Parameter(z.ParameterType.FullName, z.Name)),
                            new JSONWriter())));
        }
    }
}
