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

            base.VisitClassDeclaration(node);

            var nsUsings = SyntaxFactory.List<UsingDirectiveSyntax>()
                .Add(SyntaxFactory.UsingDirective(SyntaxFactory.IdentifierName("System")))
                .Add(SyntaxFactory.UsingDirective(SyntaxFactory.IdentifierName("System.Collections.Generic")))
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

                    var semanticType = _semanticModel.GetTypeInfo(ap.Type).Type;

                    if (semanticType is INamedTypeSymbol namedType)
                        if (namedType.TypeArguments.Length > 0
                            && namedType.TypeArguments.FirstOrDefault().TypeKind == TypeKind.Enum)
                            semanticType = namedType.TypeArguments.FirstOrDefault();

                    if (semanticType.Kind == SymbolKind.ArrayType)
                        semanticType = ((IArrayTypeSymbol)semanticType).ElementType;

                    if (ap.AttributeLists
                        .Any(a => a.ToString().Contains("[FromQuery]") || a.ToString().Contains("[FromUri]")))
                    { 
                        // if class already contains model with same nama, skip it
                        if (Cls.Members.Where(m => m is ClassDeclarationSyntax).Any(c => ((ClassDeclarationSyntax)c).Identifier.Text == semanticType.Name))
                            return true;

                        // if class already contains model with same nama, skip it
                        if (Cls.Members.Where(m => m is EnumDeclarationSyntax).Any(c => ((EnumDeclarationSyntax)c).Identifier.Text == semanticType.Name))
                            return true;

                        if (semanticType.TypeKind is TypeKind.Enum)
                        {
                            var enumDeclaration = Helpers.GetEnumFromTypeSymbol(semanticType);

                            Cls = Cls.AddMembers(enumDeclaration);
                            return true;
                        }

                        if (Helpers.IsTypeSymbolCustomClass(semanticType))
                        {
                            var res = Helpers.GetClassFromTypeSymbol(semanticType);
                            Cls = Cls
                                .AddMembers(res.cls)
                                .AddMembers(res.additional.ToArray());

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

            var sameActions = Cls.Members
                .Where(m => m is MethodDeclarationSyntax && ((MethodDeclarationSyntax)m).Identifier.Text == actionName)
                .Select(m => ((MethodDeclarationSyntax)m).ParameterList.Parameters.Select(p => p.Type.ToFullString()).ToArray())
                .Count(parameters => HasSameParams(parameters, actionParameters.Select(p => p.Type.ToFullString()).ToArray()));

            if (sameActions > 0)
                actionName += sameActions + 1;

            var method = Helpers.CreateControllerLinkAction(newHtttpArributeList, actionName, actionParameters);
            Cls = Cls.AddMembers(method);

            base.VisitMethodDeclaration(node);
        }

        private static bool HasSameParams(string[] params1, string[] params2)
        {
            if (params1.Length != params2.Length)
                return false;

            for (int i = 0; i < params1.Length; i++)
                if (params1[i] != params2[i])
                    return false;

            return true;
        }
    }
}
