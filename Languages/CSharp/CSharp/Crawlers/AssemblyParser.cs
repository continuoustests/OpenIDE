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
            _writer.WriteFile(_file);
            assembly
                .Modules.ToList()
                .ForEach(x => 
                    x.GetTypes().ToList()
                    .ForEach(y => handleType(y)));
        }

        private void handleType(TypeDefinition type) {
            if (type.Name == "<Module>")
                return;
            if (type.IsClass) {
                var classDef = new Class(_file, type.Namespace, type.Name, "public", 0, 0);
                _writer.WriteClass(classDef);
                handleTypeMembers(type);
            }
            if (type.IsInterface) {
                var iface = new Interface(_file, type.Namespace, type.Name, "public", 0, 0);
                _writer.WriteInterface(iface);
                handleTypeMembers(type);
            }
            if (type.IsEnum) {
                var enm = new EnumType(_file, type.Namespace, type.Name, "public", 0, 0);
                _writer.WriteEnum(enm);
                handleTypeMembers(type);
            }
            foreach (var child in type.NestedTypes)
                handleType(child);
        }

        private void handleTypeMembers(TypeDefinition cls) {
            
            cls.Fields
                .Where(y => y.IsPublic && !y.IsRuntimeSpecialName && !y.IsSpecialName).ToList()
                .ForEach(x => {
                    _writer.WriteField(
                        new Field(_file,
                            x.DeclaringType.FullName, x.Name, "public",
                            0,0,x.FieldType.FullName));
                });
            cls.Properties
                .Where(y => !y.IsRuntimeSpecialName && !y.IsSpecialName).ToList()
                .ForEach(x =>
                    _writer.WriteField(
                        new Field(_file,
                            x.DeclaringType.FullName, x.Name, "public",
                            0,0,x.PropertyType.FullName)));
            cls.Methods
                .Where(y => y.IsPublic && !y.IsRuntimeSpecialName && !y.IsSpecialName).ToList()
                .ForEach(x => {
                    _writer.WriteMethod(
                        new Method(_file,
                            x.DeclaringType.FullName, x.Name, "public",
                            0,0,x.MethodReturnType.ReturnType.FullName,
                            x.Parameters.Select(z => 
                                new Parameter(
                                    _file,
                                    x.DeclaringType.FullName + "." + x.Name,
                                    z.Name,
                                    "parameter",
                                    0,0,z.ParameterType.FullName))));
                });
        }
    }
}
