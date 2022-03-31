using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace LinkComposer.SourceGenerator
{
    public class ControllerSyntaxWalker : CSharpSyntaxWalker
    {
        private readonly GeneratorExecutionContext _context;
        private readonly SemanticModel _semanticModel;

        public ControllerSyntaxWalker(GeneratorExecutionContext context, SemanticModel semanticModel)
        {
            _context = context;
            _semanticModel = semanticModel;
        }

        public ClassDeclarationSyntax Cls { get; set; }

        public string ControllerRouteAttributeValue { get; set; }

        public override void VisitClassDeclaration(ClassDeclarationSyntax node)
        {
            if (!node.Identifier.Text.EndsWith("Controller"))
                return;

            var className = node.Identifier.ToString().Replace("Controller", "ControllerLink");

            Cls = Helpers.CreateControllerLinkClass(className, _context.Compilation.AssemblyName);

            var controllerRouteAttribute = node.AttributeLists.FirstOrDefault(a => a.ToString().StartsWith("[RoutePrefix("));

            if (controllerRouteAttribute is null)
                controllerRouteAttribute = node.AttributeLists.FirstOrDefault(a => a.ToString().StartsWith("[Route("));

            ControllerRouteAttributeValue = controllerRouteAttribute?.Attributes.FirstOrDefault()?.ArgumentList?.Arguments.FirstOrDefault()?.ToFullString()
                .Replace("\"", "")
                .Replace("[controller]", node.Identifier.ToString().Replace("Controller", ""));

            base.VisitClassDeclaration(node);

            var nsUsings = SyntaxFactory.List<UsingDirectiveSyntax>()
                .Add(SyntaxFactory.UsingDirective(SyntaxFactory.IdentifierName("LinkComposer.Attributes")));

            var nsMembers = SyntaxFactory.List<MemberDeclarationSyntax>()
                .Add(Cls);

            var ns = SyntaxFactory.NamespaceDeclaration(SyntaxFactory.IdentifierName("LinkComposer.Links"),
                SyntaxFactory.List<ExternAliasDirectiveSyntax>(),
                nsUsings, nsMembers).NormalizeWhitespace();

            _context.AddSource($"{className}.g.cs", ns.ToFullString());
        }

        public override void VisitMethodDeclaration(MethodDeclarationSyntax node)
        {
            var attributes = node.AttributeLists;
            var httpAttribute = attributes.FirstOrDefault(a => a.ToString().StartsWith("[Http"));

            if (httpAttribute == null)
                return; 

            var routeAttribute = attributes.FirstOrDefault(a => a.ToString().StartsWith("[Route("));
            var routeAttributeValue = routeAttribute?.Attributes.FirstOrDefault()?.ArgumentList?.Arguments.FirstOrDefault()?.ToFullString()?.Replace("\"", "");

            var httpAttributeValue = httpAttribute.Attributes.FirstOrDefault()?.ArgumentList?.Arguments.FirstOrDefault()?.ToFullString()?.Replace("\"", "");

            var route = httpAttributeValue ?? routeAttributeValue;

            var newHtttpArributeList = Helpers.CreateLinkComposerRouteAttribute(route, ControllerRouteAttributeValue);

            var actionName = node.Identifier.ToString();

            // exclude parameters
            var actionParameters = node.ParameterList.Parameters
                .Where(ap =>
                {
                    if (ap.Type is null)
                        return false;

                    if (!_semanticModel.GetTypeInfo(ap.Type).Type.IsValueType 
                        && _semanticModel.GetTypeInfo(ap.Type).Type.SpecialType != SpecialType.System_String)
                    {
                        if (ap.AttributeLists.Any(a => a.ToString().Contains("[FromQuery]")))
                        {
                            var parameter = _semanticModel.GetDeclaredSymbol(ap);
                            var properties = parameter.Type
                                .GetMembers()
                                .Where(a => a.Kind == SymbolKind.Property)
                                .Select(a => (IPropertySymbol)a)
                                .Select(p => SyntaxFactory.PropertyDeclaration(SyntaxFactory.ParseTypeName(p.Type.Name), p.Name)
                                    .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                                    .AddAccessorListAccessors(
                                        SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration).WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)),
                                        SyntaxFactory.AccessorDeclaration(SyntaxKind.SetAccessorDeclaration).WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken))
                                    ));

                            var pmMembers = SyntaxFactory.List<MemberDeclarationSyntax>()
                                .AddRange(properties);

                            var parameterModel = SyntaxFactory.ClassDeclaration(ap.Type.ToString())
                                .WithMembers(pmMembers)
                                .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword)));

                            Cls = Cls.AddMembers(parameterModel);

                            return true;
                        }

                        return false;
                    }

                    return true;
                })
                .Where(ap => ap.Type?.ToString() != nameof(CancellationToken))
                .Select(ap =>
                {
                    return SyntaxFactory.Parameter(SyntaxFactory.List<AttributeListSyntax>(), SyntaxFactory.TokenList(), ap.Type, ap.Identifier, ap.Default);
                });

            var method = Helpers.CreateControllerLinkAction(newHtttpArributeList, actionName, actionParameters);
            Cls = Cls.AddMembers(method);

            base.VisitMethodDeclaration(node);
        }
    }
}
