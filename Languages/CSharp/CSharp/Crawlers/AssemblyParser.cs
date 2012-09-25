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

        public AssemblyParser(IOutputWriter writer) {
            _writer = writer;
        }

        public void Parse(string asm) {
            AssemblyDefinition assembly;
            if (File.Exists(asm))
                assembly = AssemblyDefinition.ReadAssembly(asm);
            else
                assembly = new DefaultAssemblyResolver().Resolve(asm);
            assembly
                .MainModule
                .GetTypes().ToList()
                .ForEach(x => 
                    _writer.AddClass(
                        new Class(
                            asm,
                            "",
                            x.FullName,
                            "",
                            0,
                            0,
                            "")));
        }
    }
}
