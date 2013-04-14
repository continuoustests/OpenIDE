using System;
using System.Linq;
using ICSharpCode.NRefactory.CSharp;
using System.Text;
using System.Collections.Generic;
using CSharp.Projects;

namespace CSharp.Crawlers
{
	public class NRefactoryParser : ICSharpParser
	{
		private IOutputWriter _writer;
        private FileRef _file = new FileRef("", null);
        private string _namespace = "";
        private string _containingMethod = "";
        private List<string> _types = new List<string>();
        private bool _getLocalVariables = false;

		public ICSharpParser SetOutputWriter(IOutputWriter writer) {
            _writer = writer;
            return this;
        }

        public ICSharpParser ParseLocalVariables() {
            _getLocalVariables = true;
            return this;
        }

        public void ParseFile(FileRef file, Func<string> getContent)
        {
        	var parser = new CSharpParser();
        	var ast = parser.Parse(getContent());
            _file = file;
            _writer.WriteFile(file);
        	scanNode(ast);
        }

        private void scanNode(AstNode node) {
			foreach (var child in node.Children) {
				if (child.GetType() == typeof(UsingDeclaration))
					handleUsing((UsingDeclaration)child);
                else if (child.GetType() == typeof(UsingAliasDeclaration))
                    handleUsing((UsingAliasDeclaration)child);
				else if (child.GetType() == typeof(NamespaceDeclaration))
					handleNamespace((NamespaceDeclaration)child);
				else if (child.GetType() == typeof(TypeDeclaration))
					handleType((TypeDeclaration)child);
                else if (child.GetType() == typeof(MethodDeclaration))
                    handleMethod((MethodDeclaration)child);
                else if (child.GetType() == typeof(PropertyDeclaration))
                    handleProperty((PropertyDeclaration)child);
				else if (child.GetType() == typeof(VariableInitializer))
					handleVariableInitializer((VariableInitializer)child);

				scanNode(child);
                
                if (child.GetType() == typeof(UsingDeclaration))
                    _namespace = "";
                else if (child.GetType() == typeof(TypeDeclaration))
                    _types.RemoveAt(_types.Count - 1);
			}
		}

		private void handleUsing(UsingDeclaration usng) {
            _writer.WriteUsing(
                new Using(
                    _file,
                    usng.Namespace,
                    usng.Import.StartLocation.Line,
                    usng.Import.StartLocation.Column)
                    .SetEndPosition(
                        usng.SemicolonToken.EndLocation.Line,
                        usng.SemicolonToken.EndLocation.Column));
		}

        private void handleUsing(UsingAliasDeclaration alias) {
            _writer.WriteUsingAlias(
                new UsingAlias(
                    _file,
                    alias.Alias,
                    signatureFrom(alias.Import),
                    alias.Import.StartLocation.Line,
                    alias.Import.StartLocation.Column)
                    .SetEndPosition(
                        alias.SemicolonToken.EndLocation.Line,
                        alias.SemicolonToken.EndLocation.Column));
        }
		
		private void handleNamespace(NamespaceDeclaration ns) {
            var location = ns.NamespaceToken.EndLocation;
            var id = ns.Children
                .Where(x => x.GetType() == typeof(Identifier))
                .FirstOrDefault();
            if (id != null)
                location = id.StartLocation;
            _writer.WriteNamespace(
                new Namespce(
                    _file,
                    ns.Name,
                    location.Line,
                    location.Column)
                    .SetEndPosition(
                        ns.EndLocation.Line,
                        ns.EndLocation.Column));
            _namespace = ns.Name;
		}

		private void handleType(TypeDeclaration type) {
            _types.Add(type.Name);
            switch (type.ClassType) {
                case ClassType.Class:
                    addClass(type);
                    break;
                case ClassType.Interface:
                    addInterface(type);
                    break;
                case ClassType.Struct:
                    addStruct(type);
                    break;
                case ClassType.Enum:
                    addEnum(type);
                    break;
            }
		}

        private void addEnum(TypeDeclaration type)
        {
            var enm = addEnumInfo(
                        new EnumType(
                            _file,
                            _namespace,
                            getName(type),
                            getTypeModifier(type.Modifiers),
                            type.NameToken.StartLocation.Line,
                            type.NameToken.StartLocation.Column)
                            .SetEndPosition(
                                type.RBraceToken.EndLocation.Line,
                            type.RBraceToken.EndLocation.Column),
                        type);
            _writer.WriteEnum(enm);
            foreach (var member in type.Members) {
                _writer
                    .WriteField(
                        new Field(
                            _file,
                            enm.Signature,
                            member.Name,
                            "public",
                            member.NameToken.StartLocation.Line,
                            member.NameToken.StartLocation.Column,
                            "System.Int32").AddModifiers(new[] { "static" }));
            }
        }

        private void addStruct(TypeDeclaration type)
        {
            _writer.WriteStruct(
                addTypeInfo(
                    new Struct(
                        _file,
                        _namespace,
                        getName(type),
                        getTypeModifier(type.Modifiers),
                        type.NameToken.StartLocation.Line,
                        type.NameToken.StartLocation.Column)
                        .SetEndPosition(
                            type.RBraceToken.EndLocation.Line,
                            type.RBraceToken.EndLocation.Column),
                    type));
        }

        private void addInterface(TypeDeclaration type)
        {
            _writer.WriteInterface(
                addTypeInfo(
                    new Interface(
                        _file,
                        _namespace,
                        getName(type),
                        getTypeModifier(type.Modifiers),
                        type.NameToken.StartLocation.Line,
                        type.NameToken.StartLocation.Column)
                        .SetEndPosition(
                            type.RBraceToken.EndLocation.Line,
                            type.RBraceToken.EndLocation.Column),
                    type));
        }

        private void addClass(TypeDeclaration type)
        {
            _writer.WriteClass(
                addTypeInfo(
                    new Class(
                        _file,
                        _namespace,
                        getName(type),
                        getTypeModifier(type.Modifiers),
                        type.NameToken.StartLocation.Line,
                        type.NameToken.StartLocation.Column)
                        .SetEndPosition(
                            type.RBraceToken.EndLocation.Line,
                            type.RBraceToken.EndLocation.Column),
                    type));
        }

        private string getName(TypeDeclaration type) {
            if (type.TypeParameters.Count == 0)
                return type.Name;
            return type.Name + "`" + type.TypeParameters.Count.ToString();
        }

        private T addTypeInfo<T>(TypeBase<T> type, TypeDeclaration decl) {
            getTypeAttributes(type, decl);
            foreach (var baseType in decl.BaseTypes) {
                var signature = signatureFrom(baseType);
                type.AddBaseType(signature);
            }
            return type.AddModifiers(getTypeModifiers(decl));
        }

        private EnumType addEnumInfo(EnumType type, TypeDeclaration decl) {
            getTypeAttributes(type, decl);
            return type.AddModifiers(getTypeModifiers(decl));
        }

        /*private string getTypeProperties(TypeDeclaration type) {
            var json = new JSONWriter();
            var jsonBaseSection = new JSONWriter();
            foreach (var baseType in type.BaseTypes)
                jsonBaseSection.Append(signatureFrom(baseType), "");
            if (type.BaseTypes.Count > 0)
                json.AppendSection("bases", jsonBaseSection);
            getMemberProperties(type, json);
            return json.ToString();
        }*/

        private string getTypeModifier(Modifiers modifiers)
        {
            if (modifiersContain(modifiers, Modifiers.Public))
                return "public";
            if (modifiersContain(modifiers, Modifiers.Internal))
                return "internal";
            if (modifiersContain(modifiers, Modifiers.Protected))
                return "protected";
            return "private";
        }

        private string getMemberProperties(EntityDeclaration type)
        {
            var json = new JSONWriter();
            getTypeModifiers(type);
            //getTypeAttributes(type, json);
            return json.ToString();
        }

        private void getMemberProperties(EntityDeclaration type, JSONWriter json)
        {
            getTypeModifiers(type);
            getTypeAttributes(new Class(null, "", "", "", 0, 0), type);
        }

        private List<string> getTypeModifiers(EntityDeclaration type) {
            var modifiers = new List<string>();
            if (modifiersContain(type.Modifiers, Modifiers.Abstract))
                modifiers.Add("abstract");
            if (modifiersContain(type.Modifiers, Modifiers.Sealed))
                modifiers.Add("sealed");
            if (modifiersContain(type.Modifiers, Modifiers.Partial))
                modifiers.Add("partial");
            if (modifiersContain(type.Modifiers, Modifiers.Static))
                modifiers.Add("static");
            if (modifiersContain(type.Modifiers, Modifiers.Virtual))
                modifiers.Add("virtual");
            if (modifiersContain(type.Modifiers, Modifiers.Const))
                modifiers.Add("const");
            if (modifiersContain(type.Modifiers, Modifiers.Override))
                modifiers.Add("override");
            if (modifiersContain(type.Modifiers, Modifiers.Readonly))
                modifiers.Add("readonly");
            return modifiers;
        }

        private void getTypeAttributes<T>(CodeItemBase<T> type, EntityDeclaration decl)
        {
            if (decl.Attributes.Count == 0)
                return;
            foreach (var typeAttrib in decl.Attributes) {
                foreach (var attribute in typeAttrib.Attributes) {
                    var codeAttrib = new CodeAttribute() {
                        Name = signatureFrom(attribute.Type)
                    };
                    if (!codeAttrib.Name.EndsWith("Attribute"))
                        codeAttrib.Name +=  "Attribute";
                    foreach (var arg in attribute.Arguments)
                        codeAttrib.AddParameter(arg.ToString().Replace("\"", ""));

                    type.AddAttribute(codeAttrib);
                }
            }
        }


        private bool modifiersContain(Modifiers modifiers, Modifiers modifier) {
            return (modifiers & modifier) == modifier;
        }

        private void handleMethod(MethodDeclaration method) {
            var memberNamespace = getMemberNamespace();

            var sb = new StringBuilder();
            sb.Append(memberNamespace + "." + method.Name + "(");
            int i = 0;
            foreach (var param in method.Parameters) {
                if (i != 0)
                    sb.Append(",");
                sb.Append(signatureFrom(param.Type));
                i++;
            }
            sb.Append(")");

            var parameters = new List<Parameter>();
            foreach (var param in method.Parameters) {
                var signature = signatureFrom(param.Type);
                var parameter =
                    new Parameter(
                        _file,
                        sb.ToString(),
                        param.Name,
                        "parameter",
                        param.NameToken.StartLocation.Line,
                        param.NameToken.StartLocation.Column,
                        signature);
                parameter
                    .SetEndPosition(
                        param.EndLocation.Line,
                        param.EndLocation.Column);
                parameters.Add(parameter);
            }

            var cacheMethod = 
                new Method(
                    _file,
                    memberNamespace,
                    method.Name,
                    getTypeModifier(method.Modifiers),
                    method.NameToken.StartLocation.Line,
                    method.NameToken.StartLocation.Column,
                    signatureFrom(method.ReturnType),
                    parameters)
                    .SetEndPosition(
                        method.EndLocation.Line,
                        method.EndLocation.Column);
            _writer.WriteMethod(addMemberInfo(cacheMethod, method));
            _containingMethod = cacheMethod.ToNamespaceSignature();
		}

        private string getMemberNamespace() {
            var ns = _namespace;
            foreach (var type in _types)
                ns += "." + type;
            return ns;
        }

        private void handleProperty(PropertyDeclaration property) {
            _writer.WriteField(
                addMemberInfo(
                    new Field(
                        _file,
                        getMemberNamespace(),
                        property.Name,
                        getTypeModifier(property.Modifiers),
                        property.NameToken.StartLocation.Line,
                        property.NameToken.StartLocation.Column,
                        signatureFrom(property.ReturnType))
                        .SetEndPosition(
                            property.EndLocation.Line,
                            property.EndLocation.Column),
                    property));
        }

		private void handleVariableInitializer(VariableInitializer variable) {
			if (variable.Parent.GetType() == typeof(FieldDeclaration)) {
                var field = (FieldDeclaration)variable.Parent;
                var returnType = signatureFrom(field.ReturnType);
                _writer.WriteField(
                    addMemberInfo(
                        new Field(
                            _file,
                            getMemberNamespace(),
                            variable.Name,
                            getTypeModifier(field.Modifiers),
                            variable.NameToken.StartLocation.Line,
                            variable.NameToken.StartLocation.Column,
                            returnType)
                            .SetEndPosition(
                                variable.EndLocation.Line,
                                variable.EndLocation.Column),
                        field));
                return;
            }

            if (!_getLocalVariables)
                return;

            var type = "";
			if (variable.Parent.GetType() == typeof(VariableDeclarationStatement)) {
                var varDecl = (VariableDeclarationStatement)variable.Parent;
                type = signatureFrom(varDecl.Type);
                if (type == "var")
                    type = signatureFrom(variable);
            }
			else
				type = variable.Parent.GetType().ToString();

            _writer.WriteVariable(
                new Variable(
                    _file,
                    _containingMethod,
                    variable.Name,
                    "local",
                    variable.NameToken.StartLocation.Line,
                    variable.NameToken.StartLocation.Column,
                    type)
                    .SetEndPosition(
                        variable.EndLocation.Line,
                        variable.EndLocation.Column));
		}

        private T addMemberInfo<T>(CodeItemBase<T> type, EntityDeclaration decl)
        {
            getTypeAttributes(type, decl);
            return type.AddModifiers(getTypeModifiers(decl));
        }

        private string signatureFrom(object expr) {
            if (isOfType<PrimitiveExpression>(expr))
                return ((PrimitiveExpression)expr).Value.GetType().ToString();
            if (isOfType<VariableInitializer>(expr))
                return signatureFrom(((VariableInitializer)expr).Initializer);
            if (isOfType<InvocationExpression>(expr))
                return signatureFromType((InvocationExpression)expr);
            if (isOfType<MemberReferenceExpression>(expr)) {
                var memRef = (MemberReferenceExpression)expr;
                return signatureFrom(memRef.Target) + "." + memRef.MemberName;
            }
            if (isOfType<IdentifierExpression>(expr))
                return ((IdentifierExpression)expr).Identifier;
            if (isOfType<PrimitiveType>(expr))
                return "System." + ((PrimitiveType)expr).KnownTypeCode.ToString();
            if (isOfType<ComposedType>(expr)) {
                var composedType = (ComposedType)expr;
                var fullname = composedType.ToString();
                var simpleType = signatureFrom(composedType.BaseType);
                return fullname.Replace(composedType.BaseType.ToString(), simpleType);
            }
            if (isOfType<MemberType>(expr))
                return getName((MemberType)expr);
            if (isOfType<SimpleType>(expr))
                return getName((SimpleType)expr);
            if (isOfType<TypeReferenceExpression>(expr))
                return signatureFrom(((TypeReferenceExpression)expr).Type);
            if (isOfType<ParameterDeclaration>(expr)) {
                var param = (ParameterDeclaration)expr;
                return signatureFrom(param.Type) + " " + param.Name;
            }
            if (isOfType<ObjectCreateExpression>(expr)) {
                var create = (ObjectCreateExpression)expr;
                var parameters = "";
                foreach (var param in create.Arguments)
                    parameters += (parameters.Length == 0 ? "" : ",") + signatureFrom(param);
                return signatureFrom(create.Type) + "(" + parameters + ")";
            }
            return "";
        }

        private string getName(MemberType type)
        {
            return getName(type.MemberName, type.TypeArguments);
        }

        private string getName(SimpleType type)
        {
            return getName(type.Identifier, type.TypeArguments);
        }

	    private string getName(string name, AstNodeCollection<AstType> typeArguments)
	    {
	        if (typeArguments.Count > 0)
	            return name + "`" + typeArguments.Count.ToString();
	        return name;
	    }

	    private string signatureFromType(InvocationExpression invocationExpression) {
            var target = signatureFrom(invocationExpression.Target);
            var parameters = "";
            foreach (var param in invocationExpression.Arguments)
                parameters += (parameters.Length == 0 ? "" : ",") + signatureFrom(param);
            return target + "(" + parameters + ")";
        }

        private bool isOfType<T>(object expr) {
            return expr.GetType().Equals(typeof(T));
        }
	}
}