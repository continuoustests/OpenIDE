using System;
using System.Linq;
using ICSharpCode.NRefactory.CSharp;
using System.Text;
using System.Collections.Generic;

namespace CSharp.Crawlers
{
	public class NRefactoryParser : ICSharpParser
	{
		private IOutputWriter _writer;
        private string _file = "";
        private string _namespace = "";
        private List<string> _types = new List<string>();

		public ICSharpParser SetOutputWriter(IOutputWriter writer) {
            _writer = writer;
            return this;
        }

        public void ParseFile(string file, Func<string> getContent) {
        	var parser = new CSharpParser();
        	var ast = parser.Parse(getContent());
            _file = file;
            _writer.AddFile(file);
        	scanNode(ast);
        }

        private void scanNode(AstNode node) {
			foreach (var child in node.Children) {
				if (child.GetType() == typeof(UsingDeclaration))
					handleUsing((UsingDeclaration)child);
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
            _writer.AddUsing(
                new Using(
                    _file,
                    usng.Namespace,
                    usng.Import.StartLocation.Line,
                    usng.Import.StartLocation.Column));
		}
		
		private void handleNamespace(NamespaceDeclaration ns) {
            var location = ns.NamespaceToken.EndLocation;
            var id = ns.Children
                .Where(x => x.GetType() == typeof(Identifier))
                .FirstOrDefault();
            if (id != null)
                location = id.StartLocation;
            _writer.AddNamespace(
                new Namespce(
                    _file,
                    ns.Name,
                    location.Line,
                    location.Column));
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
            _writer.AddEnum(
                new EnumType(
                    _file,
                    _namespace,
                    type.Name,
                    getTypeModifier(type.Modifiers),
                    type.NameToken.StartLocation.Line,
                    type.NameToken.StartLocation.Column,
                    getTypeProperties(type)));
        }

        private void addStruct(TypeDeclaration type)
        {
            _writer.AddStruct(
                new Struct(
                    _file,
                    _namespace,
                    type.Name,
                    getTypeModifier(type.Modifiers),
                    type.NameToken.StartLocation.Line,
                    type.NameToken.StartLocation.Column,
                    getTypeProperties(type)));
        }

        private void addInterface(TypeDeclaration type)
        {
            _writer.AddInterface(
                new Interface(
                    _file,
                    _namespace,
                    type.Name,
                    getTypeModifier(type.Modifiers),
                    type.NameToken.StartLocation.Line,
                    type.NameToken.StartLocation.Column,
                    getTypeProperties(type)));
        }

        private void addClass(TypeDeclaration type)
        {
            Console.WriteLine(type.BaseTypes.ToString());
            _writer.AddClass(
                new Class(
                    _file,
                    _namespace,
                    type.Name,
                    getTypeModifier(type.Modifiers),
                    type.NameToken.StartLocation.Line,
                    type.NameToken.StartLocation.Column,
                    getTypeProperties(type)));
        }

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

        private string getTypeProperties(TypeDeclaration type) {
            var json = new JSONWriter();
            var jsonBaseSection = new JSONWriter();
            foreach (var baseType in type.BaseTypes)
                jsonBaseSection.Append(signatureFrom(baseType), "");
            if (type.BaseTypes.Count > 0)
                json.AppendSection("bases", jsonBaseSection);
            getMemberProperties(type, json);
            return json.ToString();
        }

        private string getMemberProperties(EntityDeclaration type)
        {
            var json = new JSONWriter();
            getTypeModifiers(type, json);
            getTypeAttributes(type, json);
            return json.ToString();
        }

        private void getMemberProperties(EntityDeclaration type, JSONWriter json)
        {
            getTypeModifiers(type, json);
            getTypeAttributes(type, json);
        }

        private void getTypeModifiers(EntityDeclaration type, JSONWriter json) {
            if (modifiersContain(type.Modifiers, Modifiers.Abstract))
                json.Append("abstract", "1");
            if (modifiersContain(type.Modifiers, Modifiers.Sealed))
                json.Append("sealed", "1");
            if (modifiersContain(type.Modifiers, Modifiers.Partial))
                json.Append("partial", "1");
            if (modifiersContain(type.Modifiers, Modifiers.Static))
                json.Append("static", "1");
            if (modifiersContain(type.Modifiers, Modifiers.Virtual))
                json.Append("virtual", "1");
            if (modifiersContain(type.Modifiers, Modifiers.Const))
                json.Append("const", "1");
            if (modifiersContain(type.Modifiers, Modifiers.Override))
                json.Append("override", "1");
            if (modifiersContain(type.Modifiers, Modifiers.Readonly))
                json.Append("readonly", "1");
        }

        private void getTypeAttributes(EntityDeclaration type, JSONWriter json) {
            if (type.Attributes.Count == 0)
                return;
            var attribSection = new JSONWriter();
            foreach (var typeAttrib in type.Attributes) {
                foreach (var attribute in typeAttrib.Attributes) {
                    var value = "";
                    foreach (var arg in attribute.Arguments) {
                        if (value.Length > 0)
                            value += ",";
                        value += arg.ToString().Replace("\"", "");
                    }
                    attribSection.Append(signatureFrom(attribute.Type), value);
                }
            }
            json.AppendSection("attributes", attribSection);
        }

        private bool modifiersContain(Modifiers modifiers, Modifiers modifier) {
            return (modifiers & modifier) == modifier;
        }

        private void handleMethod(MethodDeclaration method) {
            var parameters = new List<Parameter>();
            foreach (var param in method.Parameters)
                parameters.Add(new Parameter(signatureFrom(param.Type), param.Name));
            var json = new JSONWriter();
            getMemberProperties(method, json);
            _writer.AddMethod(
                new Method(
                    _file,
                    getMemberNamespace(),
                    method.Name,
                    getTypeModifier(method.Modifiers),
                    method.NameToken.StartLocation.Line,
                    method.NameToken.StartLocation.Column,
                    signatureFrom(method.ReturnType),
                    parameters,
                    json));
		}

        private string getMemberNamespace() {
            var ns = _namespace;
            foreach (var type in _types)
                ns += "." + type;
            return ns;
        }

        private void handleProperty(PropertyDeclaration property) {
            _writer.AddField(
                new Field(
                    _file,
                    getMemberNamespace(),
                    property.Name,
                    getTypeModifier(property.Modifiers),
                    property.NameToken.StartLocation.Line,
                    property.NameToken.StartLocation.Column,
                    signatureFrom(property.ReturnType),
                    getMemberProperties(property)));
        }

		private void handleVariableInitializer(VariableInitializer variable) {
			if (variable.Parent.GetType() == typeof(FieldDeclaration)) {
                var field = (FieldDeclaration)variable.Parent;
                _writer.AddField(
                    new Field(
                        _file,
                        getMemberNamespace(),
                        variable.Name,
                        getTypeModifier(field.Modifiers),
                        variable.NameToken.StartLocation.Line,
                        variable.NameToken.StartLocation.Column,
                        signatureFrom(field.ReturnType),
                        getMemberProperties(field)));
            }
			/*else if (variable.Parent.GetType() == typeof(VariableDeclarationStatement))
				type = signatureFrom(variable);
			else
				type = variable.Parent.GetType().ToString();*/
		}

        private string signatureFrom(object expr) {
            if (isOfType<PrimitiveExpression>(expr))
                return ((PrimitiveExpression)expr).Value.ToString();
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
            if (isOfType<SimpleType>(expr))
                return ((SimpleType)expr).Identifier;
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
            var type = expr.GetType();
            return "";
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