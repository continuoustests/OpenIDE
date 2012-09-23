using System;
using System.Linq;
using ICSharpCode.NRefactory.CSharp;

namespace CSharp.Crawlers
{
	public class NRefactoryParser : ICSharpParser
	{
		private IOutputWriter _writer;
        private string _file = "";
        private string _namespace = "";
        private string _type = "";

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
				else if (child.GetType() == typeof(FieldDeclaration))
					handleField((FieldDeclaration)child);
				else if (child.GetType() == typeof(VariableInitializer))
					handleVariableInitializer((VariableInitializer)child);
				else if (child.GetType() == typeof(MethodDeclaration))
					handleMethod((MethodDeclaration)child);
				else
                    node = node;
					//Console.WriteLine("".PadLeft(_level, '\t') + child.GetType().ToString());
				scanNode(child);
			}
		}

		private void handleUsing(UsingDeclaration usng) {
		}
		
		private void handleNamespace(NamespaceDeclaration ns) {
            var location = ns.NamespaceToken.EndLocation;
            var id = ns.Children
                .Where(x => x.GetType() == typeof(Identifier))
                .FirstOrDefault();
            if (id != null)
                location = id.StartLocation;
            _writer.AddNamespace(
                new Namespace(
                    _file,
                    ns.Name,
                    0,
                    location.Line,
                    location.Column));
            _namespace = ns.Name;
		}
		
		private void handleType(TypeDeclaration type) {
            switch (type.ClassType) {
                case ClassType.Class:
                    _writer.AddClass(
                        new Class(
                            _file,
                            _namespace,
                            type.Name,
                            0,
                            type.NameToken.StartLocation.Line,
                            type.NameToken.StartLocation.Column));
                    break;
                case ClassType.Interface:
                    _writer.AddInterface(
                        new Interface(
                            _file,
                            _namespace,
                            type.Name,
                            0,
                            type.NameToken.StartLocation.Line,
                            type.NameToken.StartLocation.Column));
                    break;
                case ClassType.Struct:
                    _writer.AddStruct(
                        new Struct(
                            _file,
                            _namespace,
                            type.Name,
                            0,
                            type.NameToken.StartLocation.Line,
                            type.NameToken.StartLocation.Column));
                    break;
                case ClassType.Enum:
                    _writer.AddEnum(
                        new EnumType(
                            _file,
                            _namespace,
                            type.Name,
                            0,
                            type.NameToken.StartLocation.Line,
                            type.NameToken.StartLocation.Column));
                    break;
            }
		}
		
		private void handleField(FieldDeclaration field) {
		}

        private void handleMethod(MethodDeclaration method) {
            var parameters = "";
            foreach (var param in method.Parameters)
                parameters += (parameters.Length == 0 ? "" : ",") + signatureFrom(param);
		}
		
		private void handleVariableInitializer(VariableInitializer variable) {
			var type = "";
			if (variable.Parent.GetType() == typeof(FieldDeclaration))
				type = signatureFrom(((FieldDeclaration)variable.Parent).ReturnType);
			else if (variable.Parent.GetType() == typeof(VariableDeclarationStatement))
				type = signatureFrom(variable);
			else
				type = variable.Parent.GetType().ToString();
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