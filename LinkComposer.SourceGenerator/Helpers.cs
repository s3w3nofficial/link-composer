using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;

namespace LinkComposer.SourceGenerator
{
    public static class Helpers
    {
        public static ClassDeclarationSyntax CreateControllerLinkClass(string className, string assemblyName)
        {
            return SyntaxFactory.ClassDeclaration(className)
                .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword)))
                .AddAttributeLists(SyntaxFactory.AttributeList(
                    SyntaxFactory.SingletonSeparatedList(
                        SyntaxFactory.Attribute(
                            SyntaxFactory.IdentifierName("LinkComposerProjectInfo"))
                                .AddArgumentListArguments(SyntaxFactory.AttributeArgument(
                                    SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(assemblyName)))))))
                .AddBaseListTypes(SyntaxFactory.SimpleBaseType(
                    SyntaxFactory.ParseTypeName("LinkComposerController")));
        }

        public static AttributeListSyntax CreateLinkComposerRouteAttribute(string route, string controllerRoute)
        {
            return SyntaxFactory.AttributeList(SyntaxFactory.SingletonSeparatedList(
                SyntaxFactory.Attribute(SyntaxFactory.IdentifierName("LinkComposerRoute"))
                .AddArgumentListArguments(SyntaxFactory.AttributeArgument(
                    SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(route ?? ""))),
                    SyntaxFactory.AttributeArgument(SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(controllerRoute ?? ""))))));
        }

        public static IEnumerable<ParameterSyntax> FilterActionParameters(SeparatedSyntaxList<ParameterSyntax> parameters)
        {
            throw new System.AccessViolationException();
        }

        public static MethodDeclarationSyntax CreateControllerLinkAction(AttributeListSyntax attributes, string actionName, IEnumerable<ParameterSyntax> parameters)
        {
            return SyntaxFactory.MethodDeclaration(
                SyntaxFactory.List(new[] { attributes }),
                SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.InternalKeyword)),
                SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.VoidKeyword)),
                null,
                SyntaxFactory.Identifier(actionName),
                null,
                SyntaxFactory.ParameterList(SyntaxFactory.SeparatedList(parameters)),
                SyntaxFactory.List<TypeParameterConstraintClauseSyntax>(),
                SyntaxFactory.Block(),
                null);
        }
    }
}
